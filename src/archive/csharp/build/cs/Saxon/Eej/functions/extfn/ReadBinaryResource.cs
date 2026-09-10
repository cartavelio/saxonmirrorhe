using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class ReadBinaryResource : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            try {
                string arg = arguments[0].head().getStringValue();
                System.IO.Stream stream;
                try {
                    Saxon.Ejava.net.URL url = new Saxon.Ejava.net.URL(arg);
                    stream = Saxon.Impl.Overrides.ResourceLoader.urlStream(url);
                } catch (System.Exception e) {
                    throw new Saxon.Hej.trans.XPathException(("saxon:read-binary-resource - invalid or inaccessible URL "+arg), Saxon.Hej.trans.SaxonErrorCode.SXJE0053);
                }
                System.Collections.Generic.IList<byte[]> buffers = new System.Collections.Generic.List<byte[]>();
                int len = 0;
                while (true) {
                    byte[] buffer = new byte[4096];
                    int b = stream.Read(buffer);
                    if (isFinished(b)) {
                        break ;
                    }
                    if ((b<4096)) {
                        buffer = Saxon.Ejava.util.Arrays.copyOf(buffer, b);
                    }
                    buffers.Add(buffer);
                    len += b;
                }
                byte[] result = new byte[len];
                int offset = 0;
                foreach (byte[] buffer in buffers) {
                    Saxon.Ejava.lang.System.arraycopy(buffer, 0, result, offset, buffer.Length);
                    offset += buffer.Length;
                }
                return new Saxon.Hej.value.Base64BinaryValue(result);
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.XPathException(("saxon:read-binary-resource: I/O error: "+e.Message), Saxon.Hej.trans.SaxonErrorCode.SXJE0053);
            }
        }
        private bool isFinished(int indicator) {
            return indicator == 0;
        }
    
    }
}