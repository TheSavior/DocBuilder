using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentenceToDocument
{
    public class Logger
    {
        private const string LOG = @"C:\Users\Eli\Desktop\output\log.txt";

        public static void WriteLine(string text)
        {
            Write(text + "\n");
        }

        public static void Write(string text)
        {
            using (StreamWriter outfile = new StreamWriter(LOG, true))
            {
                outfile.WriteLine(text);
                Console.Write(text);
            }
        }
    }
}
