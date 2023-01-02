using Qiwi.BillPayments.Model;
using Qiwi.BillPayments.Model.Out;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TgAdmBot.BotSpace;
using TgAdmBot.Payments;

namespace TgAdmBot.Database
{
    public enum BillingType
    {
        VipAccess
    }

    public class Billing
    {
        public int Id { get; set; }
        public BillingType type { get; set; }
        public string callback { get; set; }
        public string billingId { get; set; }
        public decimal amount { get; set; }
        public DateTime? payTime { get; set; }
        public long payerId { get; set; }
        public string PayFormLink { get; set; }
        [NotMapped]
        public BillResponse billingInfo { get; set; } = null;

        public static Billing CreateBilling(decimal amount, BillingType type, long payerId)
        {
            string billingId = Guid.NewGuid().ToString();
            BillResponse info = Payment.CreateNewBilling(amount, billingId);
            Billing billing = new Billing
            {
                callback = $"/bill!pay!{info.BillId}",
                billingId = info.BillId,
                type = type,
                amount = amount,
                payTime = null,
                payerId = payerId,
                PayFormLink = info.PayUrl.ToString(),
                billingInfo = info
            };
            BotDatabase.db.Add(billing);
            BotDatabase.db.SaveChanges();
            return billing;
        }

        /// <summary>
        /// Проверка изменения статуса оплаты
        /// </summary>
        /// <returns>True если статус изменился</returns>
        public bool Update()
        {
            BillResponse? billResponse = Payment.GetBillingInfo(billingId);
            if (billResponse!=null)
            {
                if (billResponse?.Status.ValueEnum != this.billingInfo?.Status.ValueEnum)
                {
                    this.billingInfo = billResponse;
                    if (billResponse.Status.ValueEnum == BillStatusEnum.Paid)
                    {
                        this.payTime = billResponse.Status.ChangedDateTime;
                    }
                    BotDatabase.db.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void Reject()
        {
            this.billingInfo = Payment.RejectBilling(this.billingId);
        }
    }
}
