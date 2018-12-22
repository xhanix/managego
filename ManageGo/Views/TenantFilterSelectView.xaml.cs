using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ManageGo
{
    public partial class TenantFilterSelectView : ContentView
    {
        public TenantFilterSelectView()
        {
            InitializeComponent();
        }

        internal TenantFilterSelectView(BaseDetailPage bindingContext) : this()
        {
            this.BindingContext = bindingContext;
        }
    }
}
