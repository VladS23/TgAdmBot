using Qiwi.BillPayments.Model;
using Qiwi.BillPayments.Model.Out;
using Telegram.Bot;
using TgAdmBot.Database;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {
        private async Task HandleCallbackAsync(Telegram.Bot.Types.Update update, Database.User user, Database.Chat chat)
        {
            Telegram.Bot.Types.CallbackQuery query = update.CallbackQuery;
            if (query.Data.StartsWith("/prvt!"))
            {
                PrivateMessage? prvtMsg = BotDatabase.db.PrivateMessages.SingleOrDefault(p => p.Callback == query.Data);
                if (prvtMsg != null)
                {
                    long userId = chat.Users.Single(u => u.TelegramUserId == query.From.Id).TelegramUserId;
                    if (prvtMsg.Users.Where(u => u.TelegramUserId == userId).ToList().Count > 0 && prvtMsg.Mode == PrivateMessageModes.allow)
                    {
                        //allowed (allow)
                        botClient.AnswerCallbackQueryAsync(query.Id, prvtMsg.Text, true);
                    }
                    else if (prvtMsg.Users.Where(u => u.TelegramUserId == userId).ToList().Count == 0 && prvtMsg.Mode == PrivateMessageModes.disallow)
                    {
                        //allowed (disallow)
                        botClient.AnswerCallbackQueryAsync(query.Id, prvtMsg.Text, true);
                    }
                    else
                    {
                        botClient.AnswerCallbackQueryAsync(query.Id, "Вам недоступен просмотр данного сообщения!", true);
                    }
                }
                else
                {
                    botClient.AnswerCallbackQueryAsync(query.Id, "Извините, сообщение было утеряно(", true);
                }
            }else if (query.Data.StartsWith("/bill!pay!")){
                Billing? billing = BotDatabase.db.Billings.SingleOrDefault(b=>b.callback== query.Data);
                if (billing != null)
                {
                    bool updated = billing.Update();
                    if (billing.billingInfo?.Status.ValueEnum== BillStatusEnum.Paid)
                    {
                        if (updated)
                        {
                            user.Balance = user.Balance + billing.amount;
                            BotDatabase.db.SaveChanges();
                        }
                        botClient.AnswerCallbackQueryAsync(query.Id, "Оплата успешно произведена!", true);
                    }
                    else
                    {
                        botClient.AnswerCallbackQueryAsync(query.Id, "Оплата ещё ожидается. Рекомендуем повторить попытку проверки платежа через 15 минут. Максимальное время ожидания: 2ч.", true);
                    }
                }
                else
                {
                    if (query.Data.Length> "/bill!pay!".Length)
                    {
                        string billId = query.Data.Substring("/bill!pay!".Length);
                        BillResponse? billResp = Payments.Payment.GetBillingInfo(billId);
                        if (billResp==null)
                        {
                            botClient.AnswerCallbackQueryAsync(query.Id, "Оплата ещё ожидается. Рекомендуем повторить попытку проверки платежа через 15 минут. Максимальное время ожидания: 2ч.", true);
                        }
                        else
                        {
                            if (billResp.Status?.ValueEnum==BillStatusEnum.Paid)
                            {
                                botClient.AnswerCallbackQueryAsync(query.Id, "Оплата успешно произведена! Если Вы оплатили счёт, но средства не были зачислены на баланс, то обратитесь к разработчикам: https://github.com/VladS23/TgAdmBot/issues/new", true);
                            }
                            else
                            {
                                botClient.AnswerCallbackQueryAsync(query.Id, "Оплата ещё ожидается. Рекомендуем повторить попытку проверки платежа через 15 минут. Максимальное время ожидания: 2ч.", true);
                            }
                        }
                    }
                    else
                    {
                        botClient.AnswerCallbackQueryAsync(query.Id, "Оплата не найдена! Если Вы оплатили счёт, но средства не были зачислены на баланс, то обратитесь к разработчикам: https://github.com/VladS23/TgAdmBot/issues/new", true);
                    }
                    
                }
            }
        }
        

    }
}
