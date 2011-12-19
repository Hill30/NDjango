using System;
using System.IO;

namespace Microsoft.SymbolBrowser
{
    public static class Logger
    {
        private static object key = new object();
        private const string LOG_FILE_NAME = "temp.log";
        
        public static void DeleteLogFile(){
            if(File.Exists(LOG_FILE_NAME))
                File.Delete(LOG_FILE_NAME);
        }

        public static void Log(string msg)
        {
            
                using (TextWriter tw = new StreamWriter(File.Open(LOG_FILE_NAME, FileMode.Append)))
                {
                    tw.WriteLine(string.Format("{0} {1}: {2}",
                        DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), msg));
                    tw.Close();
                }
            
        
        }

    }
}
