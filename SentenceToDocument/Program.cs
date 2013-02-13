using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SentenceToDocument
{
    class Program
    {
        private const string META = @"C:\Users\Eli\Documents\sentences.meta";
        private const string DOCNAMES = @"C:\Users\Eli\Desktop\output\documentNames.txt";
        private const string SENTENCES = @"C:\Users\Eli\Documents\sentences.text";
        private const string OUTPUTDIR = @"C:\Users\Eli\Desktop\output\";
        private const string LASTDOC = @"C:\Users\Eli\Desktop\output\lastdoc.txt";

        public const string DATEPATT = @"hh:mm:ss";

        private const bool REBUILDNAMES = false;

        static void BuildDocumentNames()
        {
            string line;
            string[] met;

            var docs = new HashSet<string>();

            using (StreamReader meta = new StreamReader(META))
            {

                while ((line = meta.ReadLine()) != null)
                {
                    met = line.Split('\t');
                    docs.Add(met[2]);
                }
            }

            using (StreamWriter outfile = new StreamWriter(DOCNAMES))
            {
                foreach (var doc in docs)
                {
                    outfile.WriteLine(doc);
                }
            }
        }

        static void WriteDocName(string doc)
        {
            File.WriteAllText(LASTDOC, doc);
        }

        static void Main(string[] args)
        {
            if (REBUILDNAMES)
            {
                // Build a file that has all of the document names in it
                BuildDocumentNames();
            }

            // Count the total docs so we can provide a percentage
            var totalDocs = 0;
            using (var reader = File.OpenText(DOCNAMES))
            {
                while (reader.ReadLine() != null)
                {
                    totalDocs++;
                }
            }

            var currentDocIndex = 0;

            var currentDocs = new List<string>();

            int num = 0;
            int c = 0;

            // We want to go through the document names
            // grab the next 10
            // then go through all the files finding the lines in those 10 documents and writing that file
            using (StreamReader docNames = new StreamReader(DOCNAMES))
            {
                if (File.Exists(LASTDOC))
                {
                    string lastDoc;
                    using (StreamReader last = new StreamReader(LASTDOC))
                    {
                        lastDoc = last.ReadLine();
                    }

                    // Keep moving ahead until we reach where we were at
                    while ((docNames.ReadLine() != lastDoc))
                    {
                        currentDocIndex++;
                    }
                }

                Logger.WriteLine("Starting");
                Logger.WriteLine(String.Format("{0} - %{1}", DateTime.Now.ToShortTimeString(), (((float)currentDocIndex / totalDocs) * 100)));

                string doc;
                while ((doc = docNames.ReadLine()) != null)
                {
                    // Balance out the above read line
                    currentDocIndex++;


                    currentDocs = new List<string>();
                    currentDocs.Add(doc);
                    for (int i = 0; i < 6000; i++)
                    {
                        if (docNames.EndOfStream)
                            break;
                        c++;
                        currentDocs.Add(docNames.ReadLine());
                        currentDocIndex++;
                    }

                    num++;

                    var files = new Dictionary<string, SortedDictionary<int, string>>();

                    string line;
                    string sentence;
                    string[] met;
                    using (StreamReader sentences = new StreamReader(SENTENCES))
                    {
                        using (StreamReader meta = new StreamReader(META))
                        {
                            while ((line = sentences.ReadLine()) != null)
                            {
                                met = meta.ReadLine().Split('\t');
                                sentence = line.Split('\t')[1];

                                // If this line isn't one of the documents we are looking at,
                                // go to the next set
                                if (!currentDocs.Contains(met[2]))
                                    continue;

                                if (!files.ContainsKey(met[2]))
                                {
                                    files[met[2]] = new SortedDictionary<int, string>();
                                }

                                files[met[2]].Add(int.Parse(met[3]), sentence);
                            }
                        }
                    }

                    foreach (var file in files)
                    {
                        using (StreamWriter outfile = new StreamWriter(@OUTPUTDIR + file.Key + ".txt"))
                        {
                            foreach (var str in file.Value)
                            {
                                outfile.WriteLine("{1}", str.Key, str.Value);
                            }
                            File.WriteAllText(LASTDOC, file.Key);
                        }
                    }
                    Logger.WriteLine(String.Format("{0} - %{1}", DateTime.Now.ToShortTimeString(), (((float)currentDocIndex / totalDocs) * 100)));
                    // We should have at most 10 now

                }
            }

            Logger.WriteLine("Finished!");
            Console.ReadKey();
            return;
        }
    }

    public class Logger
    {
        private const string LOG = @"C:\Users\Eli\Desktop\output\log.txt";

        public static void WriteLine(string text)
        {
            using (StreamWriter outfile = new StreamWriter(LOG, true))
            {
                outfile.WriteLine(text);
                Debug.WriteLine(text);
                Console.WriteLine(text);
            }
        }
    }
}
