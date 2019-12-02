using System;
using System.Collections.Generic;
using FreshMvvm;
using Xamarin.Forms;

namespace ManageGo.Views
{
    public partial class EditTicketDetailsView : ContentView
    {
        public EditTicketDetailsView()
        {
            InitializeComponent();
        }

        internal EditTicketDetailsView(FreshBasePageModel bindingContext) : this()
        {
            if (bindingContext != null)
                this.BindingContext = bindingContext;
            App.MasterDetailNav.IsGestureEnabled = false;
        }
    }
}
