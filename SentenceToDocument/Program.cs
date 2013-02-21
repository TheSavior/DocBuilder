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
        private const string OUTPUTDIR = @"C:\Users\Eli\Desktop\output\docs\";
        private const string LASTDOC = @"C:\Users\Eli\Desktop\output\lastdoc.txt";

        static float lastPrintedPercent = 0;

        public static void PrintProgress(int currentDoc, int totalDocs)
        {
            float status = (((float)currentDoc / totalDocs) * 100);
            if ((status - lastPrintedPercent) > .1)
            {
                Console.CursorLeft = 0;
                Logger.Write(String.Format("{0} - %{1}", DateTime.Now.ToLongTimeString(), status));
                lastPrintedPercent = status;
            }
            
        }

        static void Main(string[] args)
        {
            VerifyDocumentNames();
            var totalDocs = getDocCount();

            // Where are we starting?
            var resumeIndex = getResumeIndex();
            var currentDocIndex = 0;

            Logger.WriteLine("Starting");
            PrintProgress(currentDocIndex, totalDocs);

            using (StreamReader sentences = new StreamReader(SENTENCES))
            {
                using (StreamReader meta = new StreamReader(META))
                {
                    List<string> lines = new List<string>();

                    string metaStr;
                    string[] met;
                    string sentence;

                    // Start at the 0th document
                    string prevDoc = meta.ReadLine().Split('\t')[2];

                    while ((metaStr = meta.ReadLine()) != null)
                    {
                        met = metaStr.Split('\t');
                        sentence = sentences.ReadLine().Split('\t')[1];

                        // Seek ahead
                        if (currentDocIndex < resumeIndex) {
                            if (prevDoc != met[2]) 
                            {
                                currentDocIndex++;
                                prevDoc = met[2];
                            }
                            continue;
                        }

                        // If this line is one in our current doc
                        if (prevDoc == met[2])
                        {
                            lines.Add(sentence);
                        }
                        else
                        {
                            // We are starting a new document

                            // Output all the lines from the last doc
                            using (StreamWriter outfile = new StreamWriter(@OUTPUTDIR + met[2] + ".txt"))
                            {
                                foreach (var line in lines)
                                {
                                    outfile.WriteLine(line);
                                }
                            }

                            // Set the next docname
                            prevDoc = met[2];
                            currentDocIndex++;

                            PrintProgress(currentDocIndex, totalDocs);

                            // Save to the file we use to resume the name of the doc we just finished
                            File.WriteAllText(LASTDOC, currentDocIndex.ToString());

                            // Clear the sentences
                            lines = new List<string>();
                            lines.Add(sentence);
                        }
                    }
                }
            }

            Logger.WriteLine("Finished!");
            Console.ReadKey();
            return;
        }

        // Verifies that the entire sentences.meta file contains documents in groups, ordered by location
        static void VerifyOrder()
        {
            string line;
            string[] met;

            var docs = new HashSet<string>();
            var lastDoc = string.Empty;
            var loc = 0;

            using (StreamReader meta = new StreamReader(META))
            {
                while ((line = meta.ReadLine()) != null)
                {
                    met = line.Split('\t');

                    if (lastDoc != met[2])
                    {
                        loc = 0;
                        lastDoc = met[2];
                        if (docs.Contains(met[2]))
                        {
                            Console.WriteLine(met[2] + " already exists!");
                            return;
                        }
                        else
                        {
                            docs.Add(met[2]);
                        }
                    }
                    else
                    {
                        int curloc = int.Parse(met[3]);
                        if (loc > curloc)
                        {
                            Console.WriteLine("greater!");
                            return;
                        }
                        else
                        {
                            loc = curloc;
                        }
                    }
                }
            }
        }

        public static int getDocCount()
        {
            // Count the total docs so we can provide a percentage
            var totalDocs = 0;
            using (var reader = File.OpenText(DOCNAMES))
            {
                while (reader.ReadLine() != null)
                {
                    totalDocs++;
                }
            }

            return totalDocs;
        }

        public static int getResumeIndex()
        {
            int index = 0;
            if (File.Exists(LASTDOC))
            {
                var line = File.ReadAllLines(LASTDOC)[0];
                // Try to parse the contents of line
                int.TryParse(line, out index);
            }

            return index;
        }

        public static string getDocAtIndex(int index)
        {
            /*
            // This increments docIndex and sets lastDoc to be where we need to start

            using (StreamReader docNames = new StreamReader(DOCNAMES))
            {
                // Keep moving ahead until we reach where we were at
                while ((docNames.ReadLine() != lastDoc))
                {
                    currentDocIndex++;
                }
            }
             */
            return string.Empty;
        }

        static void VerifyDocumentNames()
        {
            if (!File.Exists(DOCNAMES))
            {
                Console.WriteLine("Rebuilding Names");
                // Build a file that has all of the document names in it

                string line;
                string[] met;

                var curDoc = string.Empty;

                using (StreamWriter outfile = new StreamWriter(DOCNAMES))
                {
                    using (StreamReader meta = new StreamReader(META))
                    {
                        while ((line = meta.ReadLine()) != null)
                        {
                            met = line.Split('\t');
                            if (met[2] != curDoc)
                            {
                                curDoc = met[2];
                                outfile.WriteLine(curDoc);
                            }
                        }
                    }
                }

                Console.WriteLine("Rebuilding Names Complete!");
            }
        }

        static void WriteDocName(string doc)
        {
            File.WriteAllText(LASTDOC, doc);
        }
    }
}
