using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DialogGeneratorServer
{
    internal class Logger
    {
        public static Level LogLevel { get; set; } = Level.Info;

        public enum Level
        {
            Trace,
            Debug,
            Info,
            Warn,
            Error,
            Fatal,
            Off
        }

        private static string GetText(Level level, string message)
        {
            string className = "???";
            StackTrace stackTrace = new StackTrace();
            StackFrame[] stackFrames = stackTrace.GetFrames();
            if (stackFrames.Length >= 2)
            {
                // 1番目のフレームはGetFrames()自体のものなので、2番目のフレームを参照します
                StackFrame callingFrame = stackFrames[2];
                className = callingFrame.GetMethod().DeclaringType.FullName;
                className = Regex.Match(className, "(?<=\\.)(.+(?=\\+)|.+$)").Value;
            }
            return $"[{DateTime.Now:yyyy/MM/dd_HH:mm:ss}] [{level}]\t[{className}]\t{message}";
        }

        public static void LogDebug(string message, params object[] args)
        {
            Console.WriteLine(GetText(Level.Debug, message));
        }

        public static void Log(string message, params object[] args)
        {
            Console.WriteLine(GetText(Level.Info, message));
        }

        public static void LogError(string message, params object[] args)
        {
            Console.WriteLine(GetText(Level.Error, message));
        }
    }
}
