using System;
using System.Collections.Generic;
using System.Text;

namespace CollectionCreator.Helpers
{
    public static class Logger
    {
        public static void Write(string message)
        {
            var fullMsg = $"[{DateTime.Now.ToString("hh:mm:ss")}] {message}";
            Console.WriteLine(fullMsg);
        }
    }
}
