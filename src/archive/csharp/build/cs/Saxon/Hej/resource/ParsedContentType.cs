using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class ParsedContentType {
        public bool isXmlMediaType;
        public string encoding;
        public ParsedContentType(string contentType)  {
            string mediaType;
            int pos = contentType.IndexOf(';');
            if ((pos>=0)) {
                mediaType = contentType.Substring(0, pos - 0);
            } else {
                mediaType = contentType;
            }
            mediaType = mediaType.Trim();
            isXmlMediaType = (((mediaType.StartsWith("application/") || mediaType.StartsWith("text/"))) && ((mediaType.EndsWith("/xml") || mediaType.EndsWith("+xml"))));
            string charset = "";
            pos = contentType.ToLower().IndexOf("charset");
            if ((pos>=0)) {
                pos = contentType.IndexOf('=', (pos+7));
                if ((pos>=0)) {
                    charset = contentType.Substring((pos+1));
                }
                if (((pos = charset.IndexOf(';'))>0)) {
                    charset = charset.Substring(0, pos - 0);
                }
                if (((pos = charset.IndexOf('('))>0)) {
                    charset = charset.Substring(0, pos - 0);
                }
                if (((pos = charset.IndexOf('"'))>0)) {
                    charset = charset.Substring((pos+1), charset.IndexOf('"', (pos+2)) - (pos+1));
                }
                encoding = charset.Trim();
            }
        }
    
    }
}