using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using Java.Util;
using Java.Util.Concurrent;
using Xamarin.Forms;
using Size = Android.Util.Size;

namespace ManageGo.Droid
{

    public class CamRecorder : FrameLayout, TextureView.ISurfaceTextureListener
    {
        private const string TAG = "Camera2VideoFragment";
        private SparseIntArray ORIENTATIONS = new SparseIntArray();
        private SurfaceTexture _viewSurface;
        private Android.Util.Size _previewSize;
        private AutoFitTextureView _cameraTexture;
        public event EventHandler<bool> Busy;
        public event EventHandler<byte[]> Photo;
        /// <summary>
        /// The CameraCaptureSession for camera preview.
        /// </summary>
        private CameraCaptureSession _previewSession;
        // Button to record video
        // private Button buttonVideo;
        private Context _context;
        // AutoFitTextureView for camera preview
        //public AutoFitTextureView textureView;

        public CameraDevice cameraDevice;
        public CameraCaptureSession previewSession;
        public MediaRecorder mediaRecorder;

        //private bool isRecordingVideo;
        public Semaphore cameraOpenCloseLock = new Semaphore(1);

        // Called when the CameraDevice changes state
        private MyCameraStateCallback stateListener;
        // Handles several lifecycle events of a TextureView
        //private MySurfaceTextureListener surfaceTextureListener;

        public CaptureRequest.Builder builder;
        private CaptureRequest.Builder previewBuilder;

        private Size videoSize;



        private HandlerThread backgroundThread;
        private Handler backgroundHandler;

        public bool OpeningCamera { get; internal set; }
        public bool IsPreviewing { get; internal set; }

        private CameraManager _manager;
        private bool isRecordingVideo;

        public CamRecorder(Context context) : base(context)
        {
            var inflater = LayoutInflater.FromContext(context);
            _context = context;
            if (inflater != null)
            {
                var view = inflater.Inflate(Resource.Layout.CameraLayout, this);
                _cameraTexture = view.FindViewById<AutoFitTextureView>(Resource.Id.CameraTexture);
                _cameraTexture.SurfaceTextureListener = this;
                // textureView = view.FindViewById<AutoFitTextureView>(Resource.Id.CameraTexture);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);
                //surfaceTextureListener = new MySurfaceTextureListener(this);
                stateListener = new MyCameraStateCallback(this);
            }
        }

        private void StartBackgroundThread()
        {
            backgroundThread = new HandlerThread("CameraBackground");
            backgroundThread.Start();
            backgroundHandler = new Handler(backgroundThread.Looper);
        }

        private void StopBackgroundThread()
        {
            backgroundThread.QuitSafely();
            try
            {
                backgroundThread.Join();
                backgroundThread = null;
                backgroundHandler = null;
            }
            catch (InterruptedException e)
            {
                e.PrintStackTrace();
            }
        }

        /// <summary>
        /// Raises the surface texture destroyed event.
        /// </summary>
        /// <param name="surface">Surface.</param>
        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            if (_previewSession != null && _previewSession.IsReprocessable)
                _previewSession?.StopRepeating();
            return true;
        }

        public void openCamera()
        {
            if (null == _context || OpeningCamera)
                return;
            OpeningCamera = true;

            _manager = (CameraManager)_context.GetSystemService(Context.CameraService);
            try
            {
                if (!cameraOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
                    throw new RuntimeException("Time out waiting to lock camera opening.");
                string cameraId = _manager.GetCameraIdList()[0];
                CameraCharacteristics characteristics = _manager.GetCameraCharacteristics(cameraId);
                StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                videoSize = ChooseVideoSize(map.GetOutputSizes(Class.FromType(typeof(MediaRecorder))));
                _previewSize = map.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture)))[0];
                ///_previewSize  = ChooseOptimalSize(map.GetOutputSizes(Class.FromType(typeof(MediaRecorder))), _previewSize  .Width, _previewSize  .Height, videoSize);
                int orientation = (int)Resources.Configuration.Orientation;
                if (orientation == (int)Android.Content.Res.Orientation.Landscape)
                {
                    _cameraTexture.SetAspectRatio(_previewSize.Width, _previewSize.Height);
                }
                else
                {
                    _cameraTexture.SetAspectRatio(_previewSize.Height, _previewSize.Width);
                }
                ///configureTransform(width, height);
                mediaRecorder = new MediaRecorder();
                _manager.OpenCamera(cameraId, stateListener, null);

            }
            catch (CameraAccessException)
            {
                //Toast.MakeText(Activity, "Cannot access the camera.", ToastLength.Short).Show();
            }
            catch (NullPointerException)
            {
                //var dialog = new ErrorDialog();
                //dialog.Show(FragmentManager, "dialog");
            }
            catch (InterruptedException)
            {
                throw new RuntimeException("Interrupted while trying to lock camera opening.");
            }
        }

        private Size ChooseVideoSize(Size[] choices)
        {
            foreach (Size size in choices)
            {
                if (size.Width == size.Height * 4 / 3 && size.Width <= 1000)
                    return size;
            }
            Log.Error(TAG, "Couldn't find any suitable video size");
            return choices[choices.Length - 1];
        }

        private Android.Util.Size ChooseOptimalSize(Size[] choices, int width, int height, Size aspectRatio)
        {
            var bigEnough = new List<Size>();
            int w = aspectRatio.Width;
            int h = aspectRatio.Height;
            foreach (Size option in choices)
            {
                if (option.Height == option.Width * h / w &&
                    option.Width >= width && option.Height >= height)
                    bigEnough.Add(option);
            }

            if (bigEnough.Count > 0)
                return (Size)Collections.Min(bigEnough, new CompareSizesByArea());
            else
            {
                //Log.Error(TAG, "Couldn't find any suitable preview size");
                return choices[0];
            }
        }

        //Start the camera preview
        public void startPreview()
        {
            if (null == cameraDevice || !_cameraTexture.IsAvailable || null == _previewSize)
                return;

            try
            {
                SetUpMediaRecorder();
                IsPreviewing = true;
                SurfaceTexture texture = _cameraTexture.SurfaceTexture;
                //Assert.IsNotNull(texture);
                texture.SetDefaultBufferSize(_previewSize.Width, _previewSize.Height);
                previewBuilder = cameraDevice.CreateCaptureRequest(CameraTemplate.Record);
                var surfaces = new List<Surface>();
                var previewSurface = new Surface(texture);
                surfaces.Add(previewSurface);
                previewBuilder.AddTarget(previewSurface);

                var recorderSurface = mediaRecorder.Surface;
                surfaces.Add(recorderSurface);
                previewBuilder.AddTarget(recorderSurface);

                cameraDevice.CreateCaptureSession(surfaces, new PreviewCaptureStateCallback(this), backgroundHandler);

            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }
        }


        private void CloseCamera()
        {
            try
            {
                cameraOpenCloseLock.Acquire();
                if (null != cameraDevice)
                {

                    cameraDevice.Close();
                    cameraDevice = null;
                }
                if (null != mediaRecorder)
                {
                    mediaRecorder.Release();
                    mediaRecorder = null;
                }
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException("Interrupted while trying to lock camera closing.");
            }
            finally
            {
                cameraOpenCloseLock.Release();
            }
        }

        internal void TakePhoto()
        {
            if (_context != null && cameraDevice != null)
            {
                try
                {
                    Busy?.Invoke(this, true);


                    // Pick the best JPEG size that can be captures with this CameraDevice
                    var characteristics = _manager.GetCameraCharacteristics(cameraDevice.Id);
                    Android.Util.Size[] jpegSizes = null;
                    if (characteristics != null)
                    {
                        jpegSizes = ((StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap)).GetOutputSizes((int)ImageFormatType.Jpeg);
                    }
                    int width = 640;
                    int height = 480;

                    if (jpegSizes != null && jpegSizes.Length > 0)
                    {
                        width = jpegSizes[0].Width;
                        height = jpegSizes[0].Height;
                    }

                    // We use an ImageReader to get a JPEG from CameraDevice
                    // Here, we create a new ImageReader and prepare its Surface as an output from the camera
                    var reader = ImageReader.NewInstance(width, height, ImageFormatType.Jpeg, 1);
                    var outputSurfaces = new List<Surface>(2);
                    outputSurfaces.Add(reader.Surface);
                    outputSurfaces.Add(new Surface(_viewSurface));

                    previewBuilder = cameraDevice.CreateCaptureRequest(CameraTemplate.StillCapture);

                    previewBuilder.AddTarget(reader.Surface);
                    previewBuilder.Set(CaptureRequest.ControlMode, new Integer((int)ControlMode.Auto));

                    // Orientation
                    var windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                    SurfaceOrientation rotation = windowManager.DefaultDisplay.Rotation;

                    previewBuilder.Set(CaptureRequest.JpegOrientation, new Integer(ORIENTATIONS.Get((int)rotation)));

                    // This listener is called when an image is ready in ImageReader 
                    ImageAvailableListener readerListener = new ImageAvailableListener();

                    readerListener.Photo += (sender, e) =>
                    {

                        Photo?.Invoke(this, e);
                    };

                    // We create a Handler since we want to handle the resulting JPEG in a background thread
                    HandlerThread thread = new HandlerThread("CameraPicture");
                    thread.Start();
                    backgroundHandler = new Handler(thread.Looper);
                    reader.SetOnImageAvailableListener(readerListener, backgroundHandler);

                    var captureListener = new CameraCaptureListener();

                    captureListener.PhotoComplete += (sender, e) =>
                    {
                        Busy?.Invoke(this, false);
                    };

                    cameraDevice.CreateCaptureSession(outputSurfaces, new CameraCaptureStateListener()
                    {
                        OnConfiguredAction = (CameraCaptureSession session) =>
                        {
                            try
                            {
                                _previewSession = session;
                                session.Capture(previewBuilder.Build(), captureListener, backgroundHandler);
                            }
                            catch (CameraAccessException ex)
                            {
                                Log.WriteLine(LogPriority.Info, "Capture Session error: ", ex.ToString());
                            }
                        }
                    }, backgroundHandler);
                }
                catch (CameraAccessException error)
                {

                }
                catch (Java.Lang.Exception error)
                {

                }
            }
        }

        //Update the preview
        public void updatePreview()
        {
            if (null == cameraDevice)
                return;

            try
            {
                SetUpCaptureRequestBuilder(previewBuilder);
                HandlerThread thread = new HandlerThread("CameraPreview");
                thread.Start();
                previewSession.SetRepeatingRequest(previewBuilder.Build(), null, backgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        private void SetUpCaptureRequestBuilder(CaptureRequest.Builder _builder)
        {
            _builder.Set(CaptureRequest.ControlMode, new Java.Lang.Integer((int)ControlMode.Auto));

        }

        public void configureTransform(int viewWidth, int viewHeight)
        {
            if (null == _context || null == _previewSize || null == _cameraTexture)
                return;
            var windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            var rotation = windowManager.DefaultDisplay.Rotation;
            var matrix = new Matrix();
            var viewRect = new RectF(0, 0, viewWidth, viewHeight);
            var bufferRect = new RectF(0, 0, _previewSize.Height, _previewSize.Width);
            float centerX = viewRect.CenterX();
            float centerY = viewRect.CenterY();
            if (SurfaceOrientation.Rotation90 == rotation || SurfaceOrientation.Rotation270 == rotation)
            {
                bufferRect.Offset((centerX - bufferRect.CenterX()), (centerY - bufferRect.CenterY()));
                matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
                float scale = System.Math.Max(
                    (float)viewHeight / _previewSize.Height,
                    (float)viewHeight / _previewSize.Width);
                matrix.PostScale(scale, scale, centerX, centerY);
                matrix.PostRotate(90 * ((int)rotation - 2), centerX, centerY);
            }
            _cameraTexture.SetTransform(matrix);
        }

        private void SetUpMediaRecorder()
        {
            if (null == _context)
                return;
            mediaRecorder.SetAudioSource(AudioSource.Mic);
            mediaRecorder.SetVideoSource(VideoSource.Surface);
            mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);

            string localFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var localPath = System.IO.Path.Combine(localFolder, "video.mp4");


            // var localPath = Android.OS.Environment.ExternalStorageDirectory + "video.mp4";

            mediaRecorder.SetOutputFile(localPath);
            mediaRecorder.SetVideoEncodingBitRate(10000000);
            mediaRecorder.SetVideoFrameRate(30);
            mediaRecorder.SetVideoSize(videoSize.Width, videoSize.Height);
            mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
            mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
            var windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            int rotation = (int)windowManager.DefaultDisplay.Rotation;
            int orientation = ORIENTATIONS.Get(rotation);
            mediaRecorder.SetOrientationHint(orientation);
            mediaRecorder.Prepare();
        }



        public void StartRecordingVideo()
        {
            try
            {
                //UI

                isRecordingVideo = true;

                //Start recording
                mediaRecorder.Start();
            }
            catch (IllegalStateException e)
            {
                e.PrintStackTrace();
            }
        }

        public void stopRecordingVideo()
        {
            //UI
            isRecordingVideo = false;

            //Stop recording
            /*
            mediaRecorder.Stop ();
            mediaRecorder.Reset ();
            startPreview ();
            */

            // Workaround for https://github.com/googlesamples/android-Camera2Video/issues/2
            CloseCamera();
            openCamera();
        }

        public void ConfigureTransform(int viewWidth, int viewHeight)
        {
            if (_viewSurface != null && _previewSize != null && _context != null)
            {
                var windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

                var rotation = windowManager.DefaultDisplay.Rotation;
                var matrix = new Matrix();
                var viewRect = new RectF(0, 0, viewWidth, viewHeight);
                var bufferRect = new RectF(0, 0, _previewSize.Width, _previewSize.Height);

                var centerX = viewRect.CenterX();
                var centerY = viewRect.CenterY();

                if (rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270)
                {
                    bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
                    matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);

                    var scale = System.Math.Max((float)viewHeight / _previewSize.Height, (float)viewWidth / _previewSize.Width);
                    matrix.PostScale(scale, scale, centerX, centerY);
                    matrix.PostRotate(90 * ((int)rotation - 2), centerX, centerY);
                }

                _cameraTexture.SetTransform(matrix);
            }
        }

        public void OnLayout(int l, int t, int r, int b)
        {
            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            _cameraTexture.Measure(msw, msh);
            _cameraTexture.Layout(0, 0, r - l, b - t);
        }


        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int w, int h)
        {
            _viewSurface = surface;
            ConfigureTransform(w, h);
            startPreview();
        }



        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            ConfigureTransform(width, height);
            startPreview();
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {

        }

        private class CompareSizesByArea : Java.Lang.Object, Java.Util.IComparator
        {
            public int Compare(Java.Lang.Object lhs, Java.Lang.Object rhs)
            {
                // We cast here to ensure the multiplications won't overflow
                if (lhs is Size && rhs is Size)
                {
                    var right = (Size)rhs;
                    var left = (Size)lhs;
                    return Long.Signum((long)left.Width * left.Height -
                        (long)right.Width * right.Height);
                }
                else
                    return 0;

            }
        }


    }



    public class MyCameraStateCallback : CameraDevice.StateCallback
    {
        CamRecorder fragment;
        public MyCameraStateCallback(CamRecorder frag)
        {
            fragment = frag;
        }
        public override void OnOpened(CameraDevice camera)
        {
            fragment.cameraDevice = camera;
            fragment.startPreview();
            fragment.cameraOpenCloseLock.Release();

            // if (null != fragment._cam)
            //     fragment.configureTransform(fragment.textureView.Width, fragment.textureView.Height);
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            fragment.cameraOpenCloseLock.Release();
            camera.Close();
            fragment.cameraDevice = null;
        }


        public override void OnError(CameraDevice camera, CameraError error)
        {
            fragment.cameraOpenCloseLock.Release();
            camera.Close();
            fragment.cameraDevice = null;

        }
    }

    /*
    public class MySurfaceTextureListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
        CamRecorder fragment;
        public MySurfaceTextureListener(CamRecorder frag)
        {
            fragment = frag;
        }





        public void OnSurfaceTextureAvailable(SurfaceTexture surface_texture, int width, int height)
        {
            fragment.openCamera(width, height);
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface_texture, int width, int height)
        {
            fragment.configureTransform(width, height);
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface_texture)
        {
            return true;
        }

        publ



    ic void OnSurfaceTextureUpdated(SurfaceTexture surface_texture)
        {
        }

    }*/

    public class PreviewCaptureStateCallback : CameraCaptureSession.StateCallback
    {
        CamRecorder fragment;
        public PreviewCaptureStateCallback(CamRecorder frag)
        {
            fragment = frag;
        }
        public override void OnConfigured(CameraCaptureSession session)
        {
            fragment.previewSession = session;
            fragment.updatePreview();

        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {

        }
    }

}