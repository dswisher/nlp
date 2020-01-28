
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

using ICSharpCode.SharpZipLib.GZip;
using MwParserFromScratch;
using MwParserFromScratch.Nodes;


namespace WikiParser
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO - pull from args
            // var infile = "../../data/enwiki-20200120-pages-meta-current1.xml-p10p30303.gz";
            // var infile = "../../data/enwiki-20200120-pages-meta-current2.xml-p30304p88444.gz";
            var infile = "../../data/enwiki-20200120-pages-meta-current3.xml-p88445p200507.gz";

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
                        PrintIndexSummary(pages);

                        // DumpSummary(pages);
                        // DumpPage(pages, "ASCII");
                        // DumpPage(pages, "Alien");
                        // DumpPage(pages, "Austin (disambiguation)");

                        // DumpPage(pages, "Extrapyramidal");       // len: 112
                        // DumpPage(pages, "KISS (system)");        // len: 300
                        // DumpPage(pages, "Military of Samoa");    // len: 503
                        // DumpPage(pages, "Neelin");               // len: 806
                        // DumpPage(pages, "Sour mix");             // len: 923

                        // DumpSmallest(pages, 910);

                        // ParseAndDump(pages, "Sour mix");
                        // ParseAndDump(pages, "A");
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


        private static void PrintIndexSummary(List<Page> pages)
        {
            var keys = new Dictionary<string, int>();

            var num = 0;
            foreach (var page in pages.Where(x => x.Namespace == 0 && x.Redirect == null))
            {
                num += 1;
                var key = GetIndexBucket(page);

                if (keys.ContainsKey(key))
                {
                    keys[key] += 1;
                }
                else
                {
                    keys.Add(key, 1);
                }
            }

            var sorted = keys.OrderByDescending(x => x.Value).Take(26);

            Console.WriteLine("-> {0} pages with ns=0, redirect=null", num);

            const string fmt = "{0,5}  {1}";
            Console.WriteLine(fmt, "Count", "Bucket");
            foreach (var item in sorted)
            {
                Console.WriteLine(fmt, item.Value, item.Key);
            }
        }


        private static string GetIndexBucket(Page page)
        {
            /*
            if (page.Title.Length >= 2)
            {
                return page.Title.Substring(0, 2);
            }
            else
            {
                return page.Title + "_";
            }
            */

            return page.Title.Substring(0, 1);
        }


        private static void ParseAndDump(List<Page> pages, string title)
        {
            var page = pages.FirstOrDefault(x => x.Title == title);

            if (page == null)
            {
                Console.WriteLine("Page '{0}' not found.", title);
                return;
            }

            // TODO - dump the text and meta to a file
        }


        private static void ParseAndPrint(List<Page> pages, string title)
        {
            var page = pages.FirstOrDefault(x => x.Title == title);

            if (page == null)
            {
                Console.WriteLine("Page '{0}' not found.", title);
                return;
            }

            var parser = new WikitextParser();
            var ast = parser.Parse(page.Text);
            // PrintAst(ast, 0);
            Console.WriteLine(ast.ToPlainText());
        }


        private static string Escapse(string expr)
        {
            return expr.Replace("\r", "\\r").Replace("\n", "\\n");
        }


        private static void PrintAst(Node node, int level)
        {
            var indension = new string('.', level);
            var ns = node.ToString();
            Console.WriteLine("{0,-20} [{1}]", indension + node.GetType().Name, 
                    Escapse(ns.Substring(0, Math.Min(20, ns.Length))));
            foreach (var child in node.EnumChildren())
                PrintAst(child, level + 1);
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
