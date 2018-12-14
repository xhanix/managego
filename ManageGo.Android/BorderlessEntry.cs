using System;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using ManageGo.Controls;
using ManageGo.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(BorderlessEditor), typeof(BorderlessEditorRenderer))]
namespace ManageGo.Droid
{
    public class BorderlessEditorRenderer : EditorRenderer
    {
        public BorderlessEditorRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
                return;

            Control.Background = null;
            Control.SetSingleLine(false);
            Control.SetBackground(new ColorDrawable(Android.Graphics.Color.Transparent));
        }

        private class CustomInsertionActionModeCallback : Java.Lang.Object, ActionMode.ICallback
        {
            public bool OnCreateActionMode(ActionMode mode, IMenu menu) => false;

            public bool OnActionItemClicked(ActionMode m, IMenuItem i) => false;

            public bool OnPrepareActionMode(ActionMode mode, IMenu menu) => true;

            public void OnDestroyActionMode(ActionMode mode) { }
        }

        private class CustomSelectionActionModeCallback : Java.Lang.Object, ActionMode.ICallback
        {
            private const int CopyId = Android.Resource.Id.Copy;

            public bool OnActionItemClicked(ActionMode m, IMenuItem i) => false;

            public bool OnCreateActionMode(ActionMode mode, IMenu menu) => true;

            public void OnDestroyActionMode(ActionMode mode) { }

            public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
            {
                try
                {
                    var copyItem = menu.FindItem(CopyId);
                    var title = copyItem.TitleFormatted;
                    menu.Clear();
                    menu.Add(0, CopyId, 0, title);
                }
                catch
                {
                    // ignored
                }

                return true;
            }
        }
    }
}
