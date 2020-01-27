
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
            // var infile = "../../data/enwiki-20200120-pages-meta-current2.xml-p30304p88444.gz";
            // var infile = "../../data/enwiki-20200120-pages-meta-current3.xml-p88445p200507.gz";

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
                        // DumpSummary(pages);
                        // DumpPage(pages, "ASCII");
                        // DumpPage(pages, "Alien");
                        // DumpPage(pages, "Austin (disambiguation)");

                        // DumpPage(pages, "Extrapyramidal");       // 112 bytes
                        // DumpPage(pages, "KISS (system)");        // 300 bytes
                        // DumpPage(pages, "Military of Samoa");    // 503 bytes
                        // DumpPage(pages, "Neelin");               // 806 bytes

                        DumpSmallest(pages, 910);
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


        private static void DumpSmallest(List<Page> pages, int minSize = 0)
        {
            var smallestSize = int.MaxValue;
            Page smallestPage = null;

            foreach (var page in pages.Where(x => x.Namespace == 0 && x.Redirect == null && x.Text.Length >= minSize && !x.Text.Contains("{{disambig", StringComparison.OrdinalIgnoreCase)))
            {
                if (page.Text.Length < smallestSize)
                {
                    smallestPage = page;
                    smallestSize = page.Text.Length;
                }
            }

            DumpPage(smallestPage);
        }


        private static void DumpPage(List<Page> pages, string title)
        {
            var page = pages.FirstOrDefault(x => x.Title == title);

            if (page == null)
            {
                Console.WriteLine("Page '{0}' not found.", title);
                return;
            }

            DumpPage(page);
        }


        private static void DumpPage(Page page)
        {
            Console.WriteLine("Page: '{0}'", page.Title);
            Console.WriteLine("Len:  {0}", page.Text.Length);
            Console.WriteLine(page.Text);
        }


        private static void DumpSummary(List<Page> pages)
        {
            const string fmt = "{0,3}  {1,10}  {2,6}  {3,-50}{4}";
            Console.WriteLine(fmt, "NS", "ID", "TextLen", "Title", string.Empty);
            foreach (var page in pages.Where(x => x.Redirect == null && x.Namespace == 0).Take(40))
            {
                var redir = string.IsNullOrEmpty(page.Redirect) ? string.Empty : " -> '" + page.Redirect + "'";
                Console.WriteLine(fmt, page.Namespace, page.Id, page.Text.Length, "'" + page.Title + "'", redir);
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
