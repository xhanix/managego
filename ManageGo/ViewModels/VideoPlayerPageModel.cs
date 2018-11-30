using System;
using System.IO;
using FreshMvvm;
using FormsVideoLibrary;

namespace ManageGo
{
    public class VideoPlayerPageModel : FreshBasePageModel
    {
        public FileVideoSource Source { get; private set; }
        public override void Init(object initData)
        {
            base.Init(initData);
            Source = new FileVideoSource
            {
                File = (string)initData
            };


            CurrentPage.ToolbarItems.Add(new Xamarin.Forms.ToolbarItem { Text = "USE", Command = OnUseTapped });
            CurrentPage.ToolbarItems.Add(new Xamarin.Forms.ToolbarItem { Text = "RETAKE", Command = OnRetakeTapped });
        }

        FreshAwaitCommand OnRetakeTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    //popcurrent page, show cam preview page
                    await CoreMethods.PopPageModel(data: false, modal: false, animate: false);
                    await CoreMethods.PushPageModel<TakeVideoPageModel>(data: null, modal: true, animate: false);
                    tcs?.SetResult(true);
                });
            }
        }
        FreshAwaitCommand OnUseTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    await CoreMethods.PopPageModel(data: Source.File, modal: false, animate: false);
                    tcs?.SetResult(true);
                });
            }
        }
    }
}
