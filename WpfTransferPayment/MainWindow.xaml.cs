using System;
using System.Windows;
using PayablesData;

namespace WpfTransferPayment
{
    public partial class MainWindow : Window
    {
        private Invoice _fromInvoice;
        private Invoice _toInvoice;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GetFromInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (Validator.HasValue(FromInvNoTextBox))
            {
                _fromInvoice = InvoiceRepository.GetInvoice(FromInvNoTextBox.Text);
                if (_fromInvoice != null)
                {
                    GrpFrom.DataContext = _fromInvoice;
                }
                else
                {
                    MessageBox.Show("An invoice was not found with that number.",
                        "FromInvoice Not Found");
                }
            }
            else
            {
                MessageBox.Show("FromInvoice is a required Field ...");
            }
        }

        private void GetToInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Validator.HasValue(ToInvNoTextBox))
            {
                MessageBox.Show("ToInvoice is a required Field ...");
                return;
            }

            _toInvoice = InvoiceRepository.GetInvoice(ToInvNoTextBox.Text);
            if (_toInvoice != null)
            {
                GrpBoxTO.DataContext = _toInvoice;
            }
            else
            {
                MessageBox.Show("An invoice with that number was not found.",
                    "ToInvoice Not Found");
            }
        }

        private void TransferButton_Click(object sender, RoutedEventArgs e)
        {
            if (_fromInvoice == null || _toInvoice == null)
            {
                MessageBox.Show("You must get the From and To invoices " +
                                "before transferring a payment.", "Data Entry Error");
                return;
            }

            if (!Validator.HasValue(FromTrsAmountTextBox) || !Validator.IsDecimal(FromTrsAmountTextBox))
            {
                MessageBox.Show("TransferAmount must be a decimal value...");
                return;
            }

            decimal transferAmount = Convert.ToDecimal(FromTrsAmountTextBox.Text);
            if (transferAmount > _fromInvoice.PaymentTotal)
            {
                MessageBox.Show("Transfer amount cannot be more " +
                                "than the payment total.", "Data Entry Error");
                return;
            }

            if (transferAmount > _toInvoice.BalanceDue)
            {
                MessageBox.Show("Transfer amount cannot be more " +
                                "than the balance due.", "Data Entry Error");
                return;
            }

            try
            {
                if (InvoiceRepository.TransferPayment(_fromInvoice, _toInvoice, transferAmount))
                {
                    string message =
                        $"A payment of {transferAmount:c} " +
                        $"has been transferred from invoice number {_fromInvoice.InvoiceNumber} " +
                        $"to invoice number {_toInvoice.InvoiceNumber}.";
                    MessageBox.Show(message, "Transfer Complete");
                }
                else
                {
                    MessageBox.Show("The transfer was not processed. " +
                                    "Another user may have posted a payment to " +
                                    "one of the invoices.", "Transfer Not Processed");
                }
                ClearControls();
                _fromInvoice = null;
                _toInvoice = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ClearControls()
        {
            GrpFrom.DataContext = null;
            GrpBoxTO.DataContext = null;
            FromTrsAmountTextBox.Text = "";
            FromInvNoTextBox.Focus();
        }
    }
}
