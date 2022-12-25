using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgAdmBot.Logger
{
    public enum LogType
    {
        output,
        info,
        error
    }
    internal class Log
    {
        public string text;
        public LogType type;
        public DateTime time;

        public Log(string text, LogType logType=LogType.output)
        {
            this.text = text;
            this.type= logType;
            this.time = DateTime.Now;
            Logger.AddLog(this);
        }
    }
}
