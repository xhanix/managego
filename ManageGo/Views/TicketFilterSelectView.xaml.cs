using System;
using System.Collections.Generic;
using FreshMvvm;
using Xamarin.Forms;

namespace ManageGo
{
    public partial class TicketFilterSelectView : ContentPage
    {

        public TicketFilterSelectView()
        {
            InitializeComponent();

        }

        internal TicketFilterSelectView(FreshBasePageModel bindingContext) : this()
        {
            this.BindingContext = bindingContext;
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {

        }
    }
}
