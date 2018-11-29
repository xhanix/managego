using System;
using System.ComponentModel;
using System.IO;
using AVFoundation;
using AVKit;
using CoreMedia;
using Foundation;
using ManageGo;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(VideoPlayer),
                          typeof(ManageGo.iOS.VideoPlayerRenderer))]
namespace ManageGo.iOS
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, UIView>
    {
        AVPlayer player;
        AVPlayerItem playerItem;
        AVPlayerViewController _playerViewController;       // solely for ViewController property

        int? NaturalTimeScale;

        public override UIViewController ViewController => _playerViewController;

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    // Create AVPlayerViewController
                    _playerViewController = new AVPlayerViewController();

                    // Set Player property to AVPlayer
                    player = new AVPlayer();

                    _playerViewController.Player = player;

                    var x = _playerViewController.View;

                    // Use the View from the controller as the native control
                    SetNativeControl(_playerViewController.View);
                }

                SetAreTransportControlsEnabled();
                SetSource();

                e.NewElement.UpdateStatus += OnUpdateStatus;
                e.NewElement.PlayRequested += OnPlayRequested;
                e.NewElement.PauseRequested += OnPauseRequested;
                e.NewElement.StopRequested += OnStopRequested;
            }

            if (e.OldElement != null)
            {
                e.OldElement.UpdateStatus -= OnUpdateStatus;
                e.OldElement.PlayRequested -= OnPlayRequested;
                e.OldElement.PauseRequested -= OnPauseRequested;
                e.OldElement.StopRequested -= OnStopRequested;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (player != null)
            {
                player.ReplaceCurrentItemWithPlayerItem(null);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == VideoPlayer.AreTransportControlsEnabledProperty.PropertyName)
            {
                SetAreTransportControlsEnabled();
            }
            else if (e.PropertyName == VideoPlayer.SourceProperty.PropertyName)
            {
                SetSource();
            }
            else if (e.PropertyName == VideoPlayer.PositionProperty.PropertyName)
            {
                TimeSpan controlPosition = ConvertTime(player.CurrentTime);

                if (Math.Abs((controlPosition - Element.Position).TotalMilliseconds) > 100)
                {
                    var scale = NaturalTimeScale ?? 1000;
                    var time = CMTime.FromSeconds(Element.Position.TotalSeconds, scale);

                    player.Seek(time, CMTime.Zero, CMTime.Zero);
                }
            }
        }

        void SetAreTransportControlsEnabled()
        {
            ((AVPlayerViewController)ViewController).ShowsPlaybackControls = Element.AreTransportControlsEnabled;
        }

        void SetSource()
        {
            AVAsset asset = null;

            if (Element.Source is UriVideoSource)
            {
                string uri = (Element.Source as UriVideoSource).Uri;

                var documents = "file://" + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                uri = Path.Combine(documents, uri);

                if (!String.IsNullOrWhiteSpace(uri))
                {
                    asset = AVAsset.FromUrl(new NSUrl(uri));
                }
            }
            else if (Element.Source is FileVideoSource)
            {
                string uri = (Element.Source as UriVideoSource).Uri;

                var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                uri = Path.Combine(documents, uri);

                if (!String.IsNullOrWhiteSpace(uri))
                {
                    var a = AVAsset.FromUrl(new NSUrl(uri));
                    asset = a;
                }
            }
            else if (Element.Source is ResourceVideoSource)
            {
                string path = (Element.Source as ResourceVideoSource).Path;

                if (!String.IsNullOrWhiteSpace(path))
                {
                    string directory = Path.GetDirectoryName(path);
                    string filename = Path.GetFileNameWithoutExtension(path);
                    string extension = Path.GetExtension(path).Substring(1);
                    NSUrl url = NSBundle.MainBundle.GetUrlForResource(filename, extension, directory);

                    asset = AVAsset.FromUrl(url);
                }
            }


            if (asset != null)
            {
                var t = asset.TracksWithMediaType("vide");
                if (t != null && t.Length > 0)
                    NaturalTimeScale = t[0]?.NaturalTimeScale;
                else
                    NaturalTimeScale = null;
                playerItem = new AVPlayerItem(asset, new NSString[] { (NSString)"preferredRate" });
            }
            else
            {
                playerItem = null;
            }

            player.ReplaceCurrentItemWithPlayerItem(playerItem);

            if (playerItem != null && Element.AutoPlay)
            {
                player.Play();
            }
        }

        // Event handler to update status
        void OnUpdateStatus(object sender, EventArgs args)
        {
            VideoStatus videoStatus = VideoStatus.NotReady;

            switch (player.Status)
            {
                case AVPlayerStatus.ReadyToPlay:
                    switch (player.TimeControlStatus)
                    {
                        case AVPlayerTimeControlStatus.Playing:
                            videoStatus = VideoStatus.Playing;
                            break;

                        case AVPlayerTimeControlStatus.Paused:
                            videoStatus = VideoStatus.Paused;
                            break;
                    }
                    break;
            }
            ((IVideoPlayerController)Element).Status = videoStatus;

            if (playerItem != null)
            {
                ((IVideoPlayerController)Element).Duration = ConvertTime(playerItem.Duration);
                ((IElementController)Element).SetValueFromRenderer(VideoPlayer.PositionProperty, ConvertTime(playerItem.CurrentTime));
            }
        }

        TimeSpan ConvertTime(CMTime cmTime)
        {
            return TimeSpan.FromSeconds(Double.IsNaN(cmTime.Seconds) ? 0 : cmTime.Seconds);

        }

        // Event handlers to implement methods
        void OnPlayRequested(object sender, EventArgs args)
        {
            player.Play();

            if (NaturalTimeScale == 2400)
            {
                player.CurrentItem.AudioTimePitchAlgorithm = AVAudioTimePitchAlgorithm.Varispeed;
                player.Rate = 0.1f;

            }
        }

        void OnPauseRequested(object sender, EventArgs args)
        {
            player.Pause();
        }

        void OnStopRequested(object sender, EventArgs args)
        {
            player.Pause();
            player.Seek(new CMTime(0, 1));
        }

    }
}
