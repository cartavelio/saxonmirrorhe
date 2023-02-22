using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class UnparsedTextResource : Saxon.Hej.lib.Resource {
        private string contentType;
        private string encoding;
        private readonly string href;
        private string unparsedText = null;
        public UnparsedTextResource(Saxon.Hej.resource.AbstractResourceCollection.InputDetails details)  {
            this.href = details.resourceUri;
            this.contentType = details.contentType;
            this.encoding = details.encoding;
            if ((details.characterContent != null)) {
                unparsedText = details.characterContent;
            } else {
                if ((details.binaryContent != null)) {
                    if ((details.encoding == null)) {
                        try {
                            System.IO.Stream @is = new System.IO.MemoryStream(details.binaryContent);
                            details.encoding = Saxon.Hej.resource.EncodingDetector.inferStreamEncoding(@is, null);
                            @is.Close();
                        } catch (System.IO.IOException e) {
                            throw new Saxon.Hej.trans.XPathException(e);
                        }
                    }
                    this.unparsedText = makeString(details.binaryContent, details.encoding);
                }
            }
        }
        public UnparsedTextResource(string uri, string content)  {
            this.href = uri;
            this.unparsedText = content;
        }
        private string makeString(byte[] bytes, string encoding) {
            return System.Text.Encoding.GetEncoding(encoding).GetString(bytes);
        }
        public readonly static Saxon.Hej.lib.ResourceFactory FACTORY = (config, details) => new Saxon.Hej.resource.UnparsedTextResource(details);
        public string getResourceURI() {
            return href;
        }
        public string getEncoding() {
            return encoding;
        }
        public string getContent() {
            if ((unparsedText == null)) {
                try {
                    System.IO.Stream stream = Saxon.Impl.Overrides.ResourceLoader.urlStream(new Saxon.Ejava.net.URL(href));
                    string enc = encoding;
                    if ((enc == null)) {
                        enc = Saxon.Hej.resource.EncodingDetector.inferStreamEncoding(stream, null);
                    }
                    unparsedText = Saxon.Hej.resource.CatalogCollection.makeStringFromStream(stream, enc);
                } catch (System.IO.IOException e) {
                    throw new Saxon.Hej.trans.XPathException(e);
                }
            }
            return unparsedText;
        }
        public Saxon.Hej.om.Item getItem(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.value.StringValue(getContent());
        }
        public string getContentType() {
            return ((contentType == null) ? "text/plain" : contentType);
        }
    
    }
}