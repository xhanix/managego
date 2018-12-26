using System;
using System.Collections.Generic;
using FreshMvvm;
using Xamarin.Forms;

namespace ManageGo.Views
{
    public partial class PaymentFilterView : ContentView
    {
        public PaymentFilterView()
        {
            InitializeComponent();
        }

        internal PaymentFilterView(FreshBasePageModel bindingContext) : this()
        {
            this.BindingContext = bindingContext;
        }

    }
}
