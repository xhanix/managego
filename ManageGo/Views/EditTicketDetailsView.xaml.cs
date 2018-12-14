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
            this.BindingContext = bindingContext;
        }
    }
}
