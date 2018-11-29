using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ManageGo.Droid
{
    public class UICameraPreview : ViewGroup, ISurfaceHolderCallback
    {

        public SurfaceView SurfaceView { get; private set; }
        public RelativeLayout MainLayout { get; private set; }
        ImageView button;
        public ISurfaceHolder Holder { get; private set; }
        Android.Hardware.Camera.Size previewSize;
        IList<Android.Hardware.Camera.Size> supportedPreviewSizes;
        Android.Hardware.Camera camera;
        IWindowManager windowManager;
        public bool IsPreviewing { get; set; }

        public UICameraPreview(Context context)
            : base(context)
        {
            MainLayout = new RelativeLayout(context);
            SurfaceView = new SurfaceView(context);
            RelativeLayout.LayoutParams liveViewParams = new RelativeLayout.LayoutParams(
                RelativeLayout.LayoutParams.MatchParent,
                RelativeLayout.LayoutParams.MatchParent);

            MainLayout.LayoutParameters = liveViewParams;
            //surfaceView.LayoutParameters = liveViewParams;
            //mainLayout.AddView(surfaceView);
            MainLayout.SetBackgroundColor(Color.Transparent);
            button = new ImageView(context);
            button.SetBackgroundColor(Color.Transparent);
            Drawable icon = Resources.GetDrawable(Resources.GetIdentifier("photocamera", "drawable", Context.PackageName));
            button.SetImageDrawable(icon);

            MainLayout.AddView(button);
            AddView(SurfaceView);
            AddView(MainLayout);

            windowManager = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            IsPreviewing = false;
            Holder = SurfaceView.Holder;
            Holder.AddCallback(this);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
            int height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);
            SetMeasuredDimension(width, height);
            if (supportedPreviewSizes != null)
            {
                previewSize = GetOptimalPreviewSize(supportedPreviewSizes, width, height);
            }
        }



        public Android.Hardware.Camera Preview
        {
            get { return camera; }
            set
            {
                camera = value;
                if (camera != null)
                {
                    supportedPreviewSizes = Preview.GetParameters().SupportedPreviewSizes;
                    RequestLayout();
                }
            }
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            var parameters = Preview.GetParameters();
            parameters.SetPreviewSize(previewSize.Width, previewSize.Height);
            RequestLayout();

            switch (windowManager.DefaultDisplay.Rotation)
            {
                case SurfaceOrientation.Rotation0:
                    camera.SetDisplayOrientation(90);
                    break;
                case SurfaceOrientation.Rotation90:
                    camera.SetDisplayOrientation(0);
                    break;
                case SurfaceOrientation.Rotation270:
                    camera.SetDisplayOrientation(180);
                    break;
            }

            Preview.SetParameters(parameters);
            Preview.StartPreview();
            IsPreviewing = true;

        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                if (Preview != null)
                {
                    Preview.SetPreviewDisplay(holder);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(@"          ERROR: ", ex.Message);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            if (Preview != null)
            {
                Preview.StopPreview();
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            SurfaceView.Measure(msw, msh);
            MainLayout.Measure(msw, msh);
            button.Measure(msw, msh);
            SurfaceView.Layout(0, 0, r - l, b - t);
            MainLayout.Layout(0, 0, r - l, b - t);
            RelativeLayout.LayoutParams buttonParams = new RelativeLayout.LayoutParams(125, 125);
            buttonParams.AddRule(LayoutRules.AlignParentBottom);
            buttonParams.AddRule(LayoutRules.CenterHorizontal);
            buttonParams.BottomMargin = 25;
            button.LayoutParameters = buttonParams;
        }


        Android.Hardware.Camera.Size GetOptimalPreviewSize(IList<Android.Hardware.Camera.Size> sizes, int w, int h)
        {
            const double AspectTolerance = 0.1;
            double targetRatio = (double)w / h;

            if (sizes == null)
            {
                return null;
            }

            Android.Hardware.Camera.Size optimalSize = null;
            double minDiff = double.MaxValue;

            int targetHeight = h;
            foreach (Android.Hardware.Camera.Size size in sizes)
            {
                double ratio = (double)size.Width / size.Height;

                if (Math.Abs(ratio - targetRatio) > AspectTolerance)
                    continue;
                if (Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = Math.Abs(size.Height - targetHeight);
                }
            }

            if (optimalSize == null)
            {
                minDiff = double.MaxValue;
                foreach (Android.Hardware.Camera.Size size in sizes)
                {
                    if (Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = Math.Abs(size.Height - targetHeight);
                    }
                }
            }
            return optimalSize;
        }
    }
}
