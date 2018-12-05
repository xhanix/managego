using System;
using System.Collections.Generic;
using System.IO;
using FreshMvvm;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace ManageGo
{
    public class MGWebViewPageModel : FreshBasePageModel
    {
        public ImageSource ImageSource { get; private set; }

        public string HtmlString { get; private set; }
        public string FilePath { get; private set; } = String.Empty;
        public bool MediaIsForAttachment { get; set; }
        public override void Init(object initData)
        {
            base.Init(initData);
            //this.ToolbarItems = new System.Collections.ObjectModel.ObservableCollection<ToolbarItem>();

            if (initData is string)
            {
                var path = initData as string;
                var extension = Path.GetExtension(path).Replace(".", "").ToLower();
                if (extension == "mp4" || extension == "mov" || extension == "wmv" || extension == "avi")
                {
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        var lines = Convert.ToBase64String(System.IO.File.ReadAllBytes(initData as string));
                        HtmlString = $"<video width=\"100%\" playsinline autoplay controls muted src=\"data:video/mp4;base64,{lines}\" type=\"video/*\" />";
                    }
                    else if (Device.RuntimePlatform == Device.Android)
                    {
                        HtmlString = $"<video width=\"100%\" playsinline autoplay controls muted src=\"{path}\" type=\"video/*\" />";

                    }
                }
                else
                {
                    var _path = initData as string;
                    if (_path.StartsWith("file", StringComparison.InvariantCulture))
                        _path = _path.Replace("file:///", "");
                    FilePath = _path;
                }

            }

            else if (initData is Tuple<string, bool>)
            {
                var tup = initData as Tuple<string, bool>;
                var path = tup.Item1;
                var extension = Path.GetExtension(path).Replace(".", "").ToLower();
                MediaIsForAttachment = tup.Item2;
                if (extension == "mp4" || extension == "mov" || extension == "wmv" || extension == "avi")
                {
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        var lines = Convert.ToBase64String(System.IO.File.ReadAllBytes(path));
                        HtmlString = $"<video width=\"100%\" playsinline autoplay controls muted src=\"data:video/mp4;base64,{lines}\" type=\"video/*\" />";
                    }
                    else if (Device.RuntimePlatform == Device.Android)
                    {
                        var folder = Path.GetDirectoryName(path);

                        var lines = Convert.ToBase64String(System.IO.File.ReadAllBytes(folder + "/Video_181129-041459.mp4"));//(System.IO.File.ReadAllBytes(path));
                        HtmlString = $"<video width=\"100%\" playsinline autoplay controls muted src=\"data:video/mp4;base64,{lines}\" type=\"video/*\" />";
                    }
                }
                else
                {
                    var _path = ((Tuple<string, bool>)initData).Item1 as string;
                    if (_path.StartsWith("file", StringComparison.InvariantCulture))
                        _path = _path.Replace("file:///", "/");
                    FilePath = _path;
                }

            }

            //Add context toolbars
            if (!MediaIsForAttachment)
            {
                CurrentPage.ToolbarItems.Add(new ToolbarItem { Text = "Share", Command = OnShareTapped });
            }
            else
            {
                CurrentPage.ToolbarItems.Add(new ToolbarItem { Text = "USE", Command = OnUseButtonTapped });
                CurrentPage.ToolbarItems.Add(new ToolbarItem { Text = "RETAKE", Command = OnCancelButtonTapped });
            }
        }


        public FreshAwaitCommand OnCancelButtonTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    //CrossShareFile.Current.ShareLocalFile(FilePath, "Share file");
                    await CoreMethods.PopPageModel(data: false, modal: true);
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnUseButtonTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    //CrossShareFile.Current.ShareLocalFile(FilePath, "Share file");
                    await CoreMethods.PopPageModel(data: true, modal: true);
                    tcs?.SetResult(true);
                });
            }
        }


        public FreshAwaitCommand OnShareTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
               {
                   //CrossShareFile.Current.ShareLocalFile(FilePath, "Share file");
                   DependencyService.Get<IShareFile>().ShareLocalFile(FilePath, "Share file");
                   tcs?.SetResult(true);
               });
            }
        }

    }
}
