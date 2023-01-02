using Qiwi.BillPayments.Client;
using Qiwi.BillPayments.Model;
using Qiwi.BillPayments.Model.In;
using Qiwi.BillPayments.Model.Out;
using Qiwi.BillPayments.Web;

namespace TgAdmBot.Payments
{
    internal class Payment
    {
        private static BillPaymentsClient billingClient = BillPaymentsClientFactory.Create(secretKey: Program.qiwiPrivate);
        public static BillResponse CreateNewBilling(decimal amount, string billingId)
        {
            try
            {
                BillResponse resp = billingClient.CreateBill(
    info: new CreateBillInfo
    {
        BillId = billingId,
        Amount = new MoneyAmount
        {
            ValueDecimal = amount,
            CurrencyEnum = CurrencyEnum.Rub
        },
        Comment = "comment",
        ExpirationDateTime = DateTime.Now.AddDays(7),
        Customer = new Customer
        {
            Email = "example@mail.org",
            Account = Guid.NewGuid().ToString(),
            Phone = "79123456789"
        },
        SuccessUrl = new Uri("https://t.me/Amalia_Tebot")
    }
);
            return resp;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }


        public static BillResponse GetBillingInfo(string billingId)
        {
            try
            {

                BillResponse response = billingClient.GetBillInfo(billingId);

                return response;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public static BillResponse RejectBilling(string billingId)
        {
            return billingClient.CancelBill(
    billId: billingId
);
        }
    }
}
