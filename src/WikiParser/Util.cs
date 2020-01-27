
using System.Text;
using System.Xml;

namespace WikiParser
{
    public static class Util
    {
        public static void ExpectStartElement(this XmlTextReader reader, string elementName)
        {
            Expect(reader, XmlNodeType.Element, elementName);
        }


        public static void ExpectEndElement(this XmlTextReader reader, string elementName)
        {
            Expect(reader, XmlNodeType.EndElement, elementName);
        }


        public static void Expect(this XmlTextReader reader, XmlNodeType nodeType, string name)
        {
            if ((reader.Name != name) || (reader.NodeType != nodeType))
            {
                throw new ParseException(reader, "Expected {0} '{1}', but found {2} '{3}'.", nodeType, name, reader.NodeType, reader.Name);
            }
        }


        public static void Expect(this XmlTextReader reader, XmlNodeType nodeType)
        {
            if (reader.NodeType != nodeType)
            {
                throw new ParseException(reader, "Expected {0} node, but found {1}.", nodeType, reader.NodeType);
            }
        }


        public static void Advance(this XmlTextReader reader, XmlNodeType nodeType)
        {
            reader.Read();
            reader.Expect(nodeType);
        }


        public static void Advance(this XmlTextReader reader, XmlNodeType nodeType, string name)
        {
            reader.Read();
            reader.Expect(nodeType, name);
        }


        public static void SkipElement(this XmlTextReader reader)
        {
            var name = reader.Name;

            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.EndElement) && (reader.Name == name))
                {
                    break;
                }
            }

            reader.ExpectEndElement(name);
        }


        public static string ParseSimpleElement(this XmlTextReader reader)
        {
            var name = reader.Name;

            reader.Advance(XmlNodeType.Text);

            var content = reader.Value;

            reader.Advance(XmlNodeType.EndElement, name);

            return content;
        }


        public static string ParseTextElement(this XmlTextReader reader)
        {
            var name = reader.Name;

            var builder = new StringBuilder();

            reader.Read();
            while (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.Whitespace)
            {
                builder.Append(reader.Value);
                reader.Read();
            }

            // Text element is self-closing...always?
            // reader.ExpectEndElement(name);

            return builder.ToString();
        }
    }
}

