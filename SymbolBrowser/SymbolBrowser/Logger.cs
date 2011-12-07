using System;
using System.IO;

namespace Microsoft.SymbolBrowser
{
    public static class Logger
    {
        private static object key = new object();
        public static void Log(string msg)
        {
            using (TextWriter tw = new StreamWriter(File.Open("temp.log", FileMode.Append)))
            {
                tw.WriteLine(string.Format("{0} {1}: {2}",
                    DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), msg));
                tw.Close();
            }
        }

    }
}
