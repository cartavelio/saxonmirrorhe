using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class BinaryResource : Saxon.Hej.lib.Resource {
        private readonly string href;
        private readonly string contentType;
        private byte[] data;
        private Saxon.Ejava.net.URLConnection connection = null;
        public static readonly Saxon.Hej.lib.ResourceFactory FACTORY = (context, details) => new Saxon.Hej.resource.BinaryResource(details);
        public BinaryResource(Saxon.Hej.resource.AbstractResourceCollection.InputDetails @in)  {
            this.contentType = @in.contentType;
            this.href = @in.resourceUri;
            this.data = @in.binaryContent;
        }
        public BinaryResource(string href, string contentType, byte[] content)  {
            this.contentType = contentType;
            this.href = href;
            this.data = content;
        }
        public static byte[] encode(string s, string encoding) {
            return Saxon.Impl.Helpers.StringUtils.encode(s, encoding);
        }
        public static string decode(byte[] value, string encoding) {
            return decode(value, 0, value.Length, encoding);
        }
        public static string decode(byte[] value, int offset, int len, string encoding) {
            return Saxon.Impl.Helpers.StringUtils.decode(value, offset, len, encoding);
        }
        public static void error(string message) {
            throw new Saxon.Hej.trans.XPathException(message);
        }
        public void setData(byte[] data) {
            this.data = data;
        }
        public byte[] getData() {
            return data;
        }
        public string getResourceURI() {
            return href;
        }
        private byte[] readBinaryFromConn(Saxon.Ejava.net.URLConnection con) {
            System.IO.Stream raw = null;
            this.connection = con;
            try {
                raw = connection.getInputStream();
                long contentLength = connection.getContentLengthLong();
                if ((contentLength>System.Int32.MaxValue)) {
                    throw new Saxon.Hej.trans.XPathException("Cannot handle binary resources longer than 2G octets");
                }
                System.IO.Stream @in = new System.IO.BufferedStream(raw);
                if ((contentLength<0)) {
                    byte[] result = readBinaryFromStream(@in, connection.getURL().getPath());
                    @in.Close();
                    return result;
                } else {
                    byte[] data = new byte[(int)(contentLength)];
                    int bytesRead = 0;
                    int offset = 0;
                    while ((offset<contentLength)) {
                        bytesRead = @in.Read(data, offset, (data.Length-offset));
                        if ((bytesRead == -1)) {
                            break ;
                        }
                        offset += bytesRead;
                    }
                    @in.Close();
                    if ((offset != contentLength)) {
                        throw new Saxon.Hej.trans.XPathException((((("Only read "+offset)+" bytes; Expected ")+contentLength)+" bytes"));
                    }
                    return data;
                }
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        public static byte[] readBinaryFromStream(System.IO.Stream @in, string path) {
            System.IO.MemoryStream buffer = new System.IO.MemoryStream();
            int nRead;
            byte[] data = new byte[16384];
            try {
                while (((nRead = @in.Read(data, 0, data.Length)) != -1)) {
                    buffer.Write(data, 0, nRead);
                }
                buffer.Flush();
                return buffer.ToArray();
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.XPathException(((("Failed to read: "+path)+" ")+e));
            }
        }
        public Saxon.Hej.om.Item getItem() {
            if ((data != null)) {
                return new Saxon.Hej.value.Base64BinaryValue(data);
            } else {
                if ((connection != null)) {
                    data = readBinaryFromConn(connection);
                    return new Saxon.Hej.value.Base64BinaryValue(data);
                } else {
                    try {
                        connection = Saxon.Impl.Overrides.ResourceLoader.urlConnection(Saxon.Impl.Helpers.UriUtils.ToURL(new System.Uri(href, System.UriKind.RelativeOrAbsolute)));
                        data = readBinaryFromConn(connection);
                        return new Saxon.Hej.value.Base64BinaryValue(data);
                    } catch (Exception e) when (e is System.UriFormatException || e is System.IO.IOException) {
                        throw new Saxon.Hej.trans.XPathException(e);
                    }
                }
            }
        }
        public string getContentType() {
            return contentType;
        }
    
    }
}