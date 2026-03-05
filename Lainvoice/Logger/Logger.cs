using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lainvoice.Logging
{
    public static class Logger
    {
        public static void Log(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            string fileName = Path.GetFileName(filePath);
            // on the same line, before the message, print [Lainvoice] in blue
            Console.ForegroundColor = ConsoleColor.Blue;
            Debug.Write("[Lainvoice] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Debug.Write($"[{fileName}:{lineNumber}] ");
            Console.ResetColor();
            Debug.WriteLine($"{message}");
        }
    }
}
