﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TgAdmBot.Database
{
    public enum UserRights
    {
        creator = 0,
        administrator = 1,
        moderator = 2,
        helper = 3,
        normal = 4,
    }
    public class User
    {
        public int UserId { get; set; }
        public int WarnsCount { get; set; } = 0;
        public long TelegramUserId { get; set; }
        public bool IsMuted { get; set; } = false;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        public UserRights UserRights { get; set; } = UserRights.normal;
        public Chat Chat { get; set; }
        public string Nickname { get; set; }
        public DateTime UnmuteTime { get; set; } = DateTime.Now;
        public bool IsBot { get; set; } = false;
        public int MessagesCount { get; set; } = 0;
        public int VoiceMessagesCount { get; set; } = 0;
        public int StickerMessagesCount { get; set; } = 0;
        public string LastMessage { get; set; } = "/start";

        public string GetInfo()
        {
            return (
                $"📈 Информация о {Nickname}\n"
                + $"👥 Ник : [{Nickname}](tg://user?id={TelegramUserId})" + "\n"
                + $"👑 Ранг: {UserRights.ToString()}\n"
                + $"🚫 Количество предупреждений {WarnsCount}\n"
                );
        }
        public  void UnBan()
        {
            try
            {
                //Send a request to telegram api and ban user
                using (HttpClientHandler hndl = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        string restext = $"https://api.telegram.org/bot{Program.botToken}/unbanChatMember?user_id={TelegramUserId}&chat_id={Chat.TelegramChatId}&only_if_banned=true";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }
        public void Ban()
        {
            try
            {
                using (HttpClientHandler hndl = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        string restext = $"https://api.telegram.org/bot{Program.botToken}/banChatMember?user_id={TelegramUserId}&chat_id={Chat.TelegramChatId}";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }
        public void Warn()
        {
            WarnsCount++;
            BotDatabase.db.SaveChanges();
        }
        public string WarningLimitAction()
        {
            switch (Chat.WarnsLimitAction)
            {
                case WarnsLimitAction.ban:
                    Ban();
                    return "Пользователь был удален в связи с превышением лимита предупреждений";
                case WarnsLimitAction.mute:
                    Mute();
                    return "Пользователю запрещено писать сообщения в связи с превышением лимита предупреждений";
                default:
                    return "Неизвестная ошибка";
            }
        }
        public void Mute()
        {
            try
            {
                IsMuted = true;
                BotDatabase.db.SaveChanges();
                //Send a request to telegram api and mute user
                using (HttpClientHandler hndl = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        string restext = $"https://api.telegram.org/bot{Program.botToken}/restrictChatMember?user_id={TelegramUserId}&chat_id={Chat.TelegramChatId}";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                    }
                }
            }
            catch
            {

            }

        }
        public void Unmute()
        {
            try
            {
                IsMuted = false;
                BotDatabase.db.SaveChanges();
                //Send a request to telegram api and unmute users
                using (HttpClientHandler hndl = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        string restext = $"https://api.telegram.org/bot{Program.botToken}/restrictChatMember?user_id={TelegramUserId}&chat_id={Chat.TelegramChatId}&until_date={((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() + 35}";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }
        public void UpdateStatistic(Telegram.Bot.Types.Message message)
        {
            LastActivity = DateTime.Now;
            if (message.Text != null)
            {
                MessagesCount += 1;
                LastMessage = message.Text;
            }
            if (message.Voice != null || message.VideoNote != null)
            {
                VoiceMessagesCount += 1;
            }
            if (message.Sticker != null)
            {
                StickerMessagesCount += 1;
            }
            BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == message.Chat.Id).MessagesCount += 1;
            BotDatabase.db.SaveChanges();
        }
    }
}
