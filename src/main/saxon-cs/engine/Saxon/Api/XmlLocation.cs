using System;

namespace Saxon.Api {
    /// <summary>
    /// Xml location. An implementation of <c>IXmlLocation</c>.
    /// </summary>
    internal class XmlLocation : IXmlLocation
    {
        private Uri baseUri;
        private int lineNumber;
        public Uri BaseUri
        {
            get { return baseUri; }
            set { baseUri = value; }
        }
        public int LineNumber
        {
            get { return lineNumber; }
            set { lineNumber = value; }
        }
    }
}