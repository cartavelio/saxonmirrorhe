using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class DataURIScheme {
        public static Saxon.Hej.lib.Resource decode(System.Uri uri) {
            System.Diagnostics.Debug.Assert(uri.Scheme.Equals("data"));
            string path = uri.GetComponents(System.UriComponents.PathAndQuery, System.UriFormat.Unescaped);
            int comma = path.IndexOf(',');
            if ((comma<0)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Missing comma in data URI");
            }
            string header = path.Substring(0, comma - 0);
            string content = path.Substring((comma+1));
            bool isBase64 = header.EndsWith(";base64");
            string contentType = header.Substring(0, (isBase64 ? (comma-7) : comma) - 0);
            if (isBase64) {
                try {
                    byte[] octets = Saxon.Hej.value.Base64BinaryValue.decode(Saxon.Hej.str.StringView.tidy(content));
                    Saxon.Hej.resource.BinaryResource resource = new Saxon.Hej.resource.BinaryResource(uri.ToString(), contentType, octets);
                    resource.setData(octets);
                    return resource;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException(e.Message);
                }
            } else {
                string encoding = getEncoding(contentType);
                if ((encoding == null)) {
                    encoding = "US-ASCII";
                }
                byte[] utf8content = stringToUTF8(content);
                Saxon.Hej.resource.AbstractResourceCollection.InputDetails details = new Saxon.Hej.resource.AbstractResourceCollection.InputDetails();
                details.resourceUri = uri.ToString();
                details.contentType = getMediaType(contentType);
                details.encoding = encoding;
                details.binaryContent = utf8content;
                details.onError = Saxon.Hej.functions.URIQueryParameters.ON_ERROR_FAIL;
                details.parseOptions = new Saxon.Hej.lib.ParseOptions();
                return new Saxon.Hej.resource.UnparsedTextResource(details);
            }
        }
        private static string getMediaType(string contentType) {
            int semicolon = contentType.IndexOf(';');
            if ((semicolon<0)) {
                return contentType;
            } else {
                return contentType.Substring(0, semicolon - 0);
            }
        }
        private static string getEncoding(string contentType) {
            string[] parts = System.Text.RegularExpressions.Regex.Split(contentType, ";");
            foreach (string part in parts) {
                if (part.StartsWith("charset=")) {
                    return part.Substring(8);
                }
            }
            return null;
        }
        private static byte[] stringToUTF8(string input) {
            return System.Text.UTF8Encoding.UTF8.GetBytes(input);
        }
    
    }
}