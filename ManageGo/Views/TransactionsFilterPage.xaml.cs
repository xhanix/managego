using System;
using System.Collections.Generic;
using Xamarin.Forms;
using FreshMvvm;

namespace ManageGo.Views
{
    public partial class TransactionsFilterPage : ContentView
    {
        public TransactionsFilterPage()
        {
            InitializeComponent();
        }

        public TransactionsFilterPage(FreshBasePageModel context) : this()
        {
            this.BindingContext = context;
        }
    }
}
