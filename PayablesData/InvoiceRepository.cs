using System;
using System.Data;
using System.Data.SqlClient;

namespace PayablesData
{
    public static class InvoiceRepository
    {
        public static Invoice GetInvoice(string invoiceNumber)
        {
            Invoice invoice = null;

            string selectStatement =
                "SELECT InvoiceNumber, InvoiceDate, InvoiceTotal, PaymentTotal " +
                "FROM Invoices " +
                "WHERE InvoiceNumber = @InvoiceNumber";
            SqlConnection connection = PayablesDB.GetConnection();
            SqlCommand selectCommand = new SqlCommand(selectStatement, connection);
            selectCommand.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);

            SqlDataReader reader = null;
            try
            {
                connection.Open();
                reader = selectCommand.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    invoice = new Invoice
                    {
                        InvoiceNumber = reader["InvoiceNumber"].ToString(),
                        InvoiceDate = (DateTime) reader["InvoiceDate"],
                        InvoiceTotal = (decimal) reader["InvoiceTotal"],
                        PaymentTotal = (decimal) reader["PaymentTotal"]
                    };
                }              
            }
            finally
            {
                reader?.Close();
                connection?.Close();
            }
            return invoice;
        }

        public static bool TransferPayment(Invoice fromInvoice,
            Invoice toInvoice, 
            decimal payment)
        {
            SqlTransaction transaction = null;
            SqlConnection connection = PayablesDB.GetConnection();

            SqlCommand fromCommand = CreateUpdatePaymentTotalCommand(fromInvoice, -payment, connection); //notice negative payment
            SqlCommand toCommand = CreateUpdatePaymentTotalCommand(toInvoice, payment, connection);

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                fromCommand.Transaction = transaction;
                toCommand.Transaction = transaction;

                int numberOfRowsAffected = fromCommand.ExecuteNonQuery();
                if (numberOfRowsAffected > 0)
                {
                    numberOfRowsAffected = toCommand.ExecuteNonQuery();
                    if (numberOfRowsAffected > 0)
                    {
                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                else
                {
                    transaction.Rollback();
                    return false;
                }
            }
            catch (SqlException)
            {
                transaction?.Rollback();
                throw;
            }
            finally
            {
                connection?.Close();
            }
        }

        private static SqlCommand CreateUpdatePaymentTotalCommand(Invoice fromInvoice, 
            decimal payment,
            SqlConnection connection)
        {
            SqlCommand updateCommand = new SqlCommand
            {
                Connection = connection,
                CommandText = "UPDATE Invoices " +
                              "SET PaymentTotal = PaymentTotal + @Payment " +
                              "WHERE InvoiceNumber = @InvoiceNumber " +
                              "  AND PaymentTotal = @PaymentTotal"
            };
            updateCommand.Parameters.AddWithValue("@payment", payment);
            updateCommand.Parameters.AddWithValue("@InvoiceNumber", fromInvoice.InvoiceNumber);
            updateCommand.Parameters.AddWithValue("@PaymentTotal", fromInvoice.PaymentTotal);
            return updateCommand;
        }
    }
}
