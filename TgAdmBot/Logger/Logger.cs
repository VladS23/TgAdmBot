namespace TgAdmBot.Logger
{
    internal class Logger
    {
        private static Queue<Log> logsQueue = new Queue<Log>();
        private static Thread loggerThread;
        private static string logsDirectory = "logs";
        private static string errorLogsPath = Path.Combine(Directory.GetCurrentDirectory(), logsDirectory, "error.log");
        private static string infoLogsPath = Path.Combine(Directory.GetCurrentDirectory(), logsDirectory, "info.log");
        private static string outputLogsPath = Path.Combine(Directory.GetCurrentDirectory(), logsDirectory, "output.log");


        public static void AddLog(Log logObject)
        {
            logsQueue.Enqueue(logObject);
            if (logsQueue.Count < 2)
            {
                loggerThread = new Thread(WriteLogs);
                loggerThread.Start();
            }
        }

        public static void PrepareLogsFolders()
        {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), logsDirectory)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), logsDirectory));
            }
            File.OpenWrite(errorLogsPath).Close();
            File.OpenWrite(infoLogsPath).Close();
            File.OpenWrite(outputLogsPath).Close();
        }

        private static void WriteLogs()
        {
            using StreamWriter outputLogs = File.AppendText(outputLogsPath);
            using StreamWriter infoLogs = File.AppendText(infoLogsPath);
            using StreamWriter errorLogs = File.AppendText(errorLogsPath);
            while (logsQueue.Count > 0)
            {
                Log logObject = logsQueue.Peek();
                switch (logObject.type)
                {
                    case LogType.output:
                        outputLogs.Write($"\n{logObject.time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK")}\n{logObject.text}\n{new String('=', 30)}");
                        break;
                    case LogType.info:
                        infoLogs.Write($"\n{logObject.time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK")}\n{logObject.text}\n{new String('=', 30)}");
                        break;
                    case LogType.error:
                        errorLogs.Write($"\n{logObject.time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK")}\n{logObject.text}\n{new String('=', 30)}");
                        break;
                    default:
                        errorLogs.Write($"\n{logObject.time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK")}\nNO LOG TYPE PROVIDED\n{logObject.text}\n{new String('=', 30)}");
                        break;
                }
                logsQueue.Dequeue();
            }

        }
    }
}
