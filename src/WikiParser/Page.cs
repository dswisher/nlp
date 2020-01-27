
using System.Xml;

namespace WikiParser
{
    public class Page
    {
        public int Id { get; private set; }
        public int Namespace { get; private set; }
        public string Title { get; private set; }
        public string Text { get; private set; }
        public string Redirect { get; private set; }


        public static Page Parse(XmlTextReader reader)
        {
            reader.ExpectStartElement("page");

            var startLine = reader.LineNumber;
            var page = new Page();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "id":
                                page.Id = int.Parse(reader.ParseSimpleElement());
                                break;

                            case "ns":
                                page.Namespace = int.Parse(reader.ParseSimpleElement());
                                break;

                            case "redirect":
                                page.Redirect = reader.GetAttribute("title");
                                break;

                            case "revision":
                                page.ParseRevision(reader);
                                break;

                            case "restrictions":
                                // Ignore these things
                                reader.SkipElement();
                                break;

                            case "title":
                                page.Title = reader.ParseSimpleElement();
                                break;

                            default:
                                throw new ParseException(reader, "Unexpected element, '{0}', at top-level of <page>.", reader.Name);
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == "page")
                        {
                            return page;
                        }
                        break;
                }
            }

            throw new ParseException("The <page> element starting on line {0} is not closed!", startLine);
        }


        private void ParseRevision(XmlTextReader reader)
        {
            reader.ExpectStartElement("revision");
            var startLine = reader.LineNumber;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "id":
                            case "parentid":
                                // Ignore these things
                                reader.SkipElement();
                                break;

                            case "text":
                                Text = reader.ParseTextElement();
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == "revision")
                        {
                            return;
                        }
                        break;
                }
            }
        }
    }
}

