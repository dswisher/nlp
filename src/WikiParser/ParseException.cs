
using System;
using System.Xml;

namespace WikiParser
{
    public class ParseException : Exception
    {
        public int LineNumber { get; private set; }
        public int LinePosition { get; private set; }

        public ParseException(XmlTextReader reader, string format, params object[] args)
            : base(string.Format(format, args))
        {
            LineNumber = reader.LineNumber;
            LinePosition = reader.LinePosition;
        }


        public ParseException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }
}

