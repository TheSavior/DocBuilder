using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GoldStandardParser
{
    class Program
    {
        private const string GOLD_XML = @"C:\Users\Eli\Documents\tac_2010_kbp_evaluation_entity_linking_queries.xml";
        private const string GOLD_TAB = @"C:\Users\Eli\Documents\tac_2010_kbp_evaluation_entity_linking_query_types.tab";
        private const string ENTITY_LOOKUP = @"C:\Users\Eli\Documents\GitHub\NEL\entityLookup.txt";
        private const string ENTITY_LOOKUP_NEW = @"C:\Users\Eli\Documents\GitHub\NEL\entityLookupNew.txt";
        private const string OUTPUT = @"C:\Users\Eli\Documents\doc_gold.txt";

        static void Main(string[] args)
        {
            int docs = 0;
            int entities = 0;

            // Dictionary of QueryId => EntityId
            var tabs = 
                (from line in File.ReadLines(GOLD_TAB)
                let pieces = line.Split('\t')
                let entityId = pieces[1]
                select new {QueryId = pieces[0], EntityId = entityId})
                .ToDictionary(item => item.QueryId, item => item.EntityId);

            using (XmlReader reader = XmlReader.Create(new StreamReader(GOLD_XML)))
            {
                /* Our document looks like:
                
                <kbpentlink>
                  <query id="EL000156">
                    <name>Annapolis</name>
                    <docid>eng-WL-11-174611-12986418</docid>
                  </query>
                  <query id="EL000176">
                    <name>Ariz</name>
                    <docid>eng-WL-11-174611-12972197</docid>
                  </query>
                  <query id="EL000281">
                    <name>Baltimore City</name>
                    <docid>eng-NG-31-143594-10249189</docid>
                  </query>
                </kbpentlink>
                 * 
                 * We want a list of names that are associated with each docid.
                 * The ones that start with eng come from the web and not newswire text
                 * We only care about newswire text
                */
                XElement root = XElement.Load(reader);
                var items =
                    from el in root.Elements("query")
                    let docid = (string)el.Element("docid")
                    let entityId = tabs[((string)el.Attribute("id"))]
                    where !docid.StartsWith("eng-") && !entityId.StartsWith("NIL")
                    group entityId by docid into ents
                    orderby ents.Key
                    select new { Document = ents.Key, Entities = ents};

                docs = items.Count();

                HashSet<String> lookup = new HashSet<string>();

                using (StreamWriter outfile = new StreamWriter(OUTPUT))
                {
                    foreach (var item in items)
                    {
                        foreach(var str in item.Entities) {
                                lookup.Add(str);
                        }
                        entities += item.Entities.Count();

                        outfile.Write(string.Format("{0}\t{1}\n", item.Document, String.Join("\t", item.Entities)));
                    }
                }

                LimitLookup(lookup);
            }

            Console.Write(string.Format("Done. {0} documents and {1} entities", docs, entities));
            Console.ReadKey();
        }

        public static void LimitLookup(HashSet<String> ents) {
            var tabs =
                (from line in File.ReadLines(ENTITY_LOOKUP)
                 let pieces = line.Split('\t')
                 let entityId = pieces[0]
                 where ents.Contains(entityId)
                 orderby entityId
                 select line);

            File.WriteAllLines(ENTITY_LOOKUP_NEW, tabs);
        }
    }
}
