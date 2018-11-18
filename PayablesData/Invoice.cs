using System;

namespace PayablesData
{
    public class Invoice
    {
        public string InvoiceNumber { get; set; }

        public DateTime InvoiceDate { get; set; }

        public decimal InvoiceTotal { get; set; }

        public decimal PaymentTotal { get; set; }

        public decimal CreditTotal { get; set; }

        public DateTime DueDate { get; set; }

        public decimal BalanceDue => InvoiceTotal - PaymentTotal - CreditTotal; // same as  get { return InvoiceTotal - PaymentTotal - CreditTotal; }
    }
 }
