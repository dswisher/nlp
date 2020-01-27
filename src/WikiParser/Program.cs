
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using ICSharpCode.SharpZipLib.GZip;


namespace WikiParser
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO - pull from args
            var infile = "../../data/enwiki-20200120-pages-meta-current1.xml-p10p30303.gz";

            ParseLines(infile);
        }


        private static void ParseLines(string infile)
        {
            Console.WriteLine("Opening {0}...", infile);

            using (var fs = new FileStream(infile, FileMode.Open, FileAccess.Read))
            using (var stream = new GZipInputStream(fs))
            using (var reader = new XmlTextReader(stream))
            {
                // Advance to the first token
                reader.Read();

                try
                {
                    Console.WriteLine("Parsing...");
                    var watch = Stopwatch.StartNew();
                    var pages = ParseMediaWiki(reader);
                    watch.Stop();
                    Console.WriteLine("Found {0} pages in {1}.", pages.Count, watch.Elapsed);

                    if (pages.Count > 0)
                    {
                        // Console.WriteLine("First title: '{0}'.", pages[0].Title);
                        // Console.WriteLine("First redir: '{0}'.", pages[0].Redirect);

                        const string fmt = "{0,3}  {1,10}  {2,-50}";
                        Console.WriteLine(fmt, "NS", "ID", "Title");
                        foreach (var page in pages.Take(20))
                        {
                            Console.WriteLine(fmt, page.Namespace, page.Id, "'" + page.Title, "'");
                        }
                    }
                }
                catch (ParseException ex)
                {
                    Console.WriteLine("Line {0}: {1}", ex.LineNumber, ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unhandled exception:");
                    Console.WriteLine(ex.ToString());
                }
            }
        }


        private static List<Page> ParseMediaWiki(XmlTextReader reader)
        {
            reader.ExpectStartElement("mediawiki");

            var pages = new List<Page>();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "page")
                        {
                            pages.Add(Page.Parse(reader));
                            reader.ExpectEndElement("page");
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == "mediawiki")
                        {
                            return pages;
                        }
                        break;
                }
            }

            return pages;
        }
    }
}
