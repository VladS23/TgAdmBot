using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {

        public async void DownloadFile(string fileId, string fileName)
        {
            Stream destStream = System.IO.File.OpenWrite(fileName);
            Telegram.Bot.Types.File file = await botClient.GetFileAsync(fileId);
            botClient.DownloadFileAsync(file.FilePath,destStream,cancellationToken);
        }
    }
}
