using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ManageGo
{
    public partial class TransactionsPage : ContentPage
    {
        public event EventHandler<Models.BankTransaction> OnTransactionAppeared;
        public TransactionsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            TransactionsList.ItemsSource = null;
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            TransactionsList.HasUnevenRows = !TransactionsList.HasUnevenRows;
            TransactionsList.HasUnevenRows = !TransactionsList.HasUnevenRows;
        }


        void Handle_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            OnTransactionAppeared?.Invoke(this, (Models.BankTransaction)e.Item);
        }



        public void DataLoaded()
        {
            if (this.BindingContext != null)
            {
                TransactionsList.ItemsSource = ((TransactionsPageModel)BindingContext).FetchedTransactions;
            }
            TransactionsList.HasUnevenRows = !TransactionsList.HasUnevenRows;
            TransactionsList.HasUnevenRows = !TransactionsList.HasUnevenRows;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            TransactionsList.ItemsSource = null;
        }

    }
}
