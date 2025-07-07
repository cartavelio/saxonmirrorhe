using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class ReadBinaryResource : Saxon.Eej.functions.extfn.ResourceAccessFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            System.IO.Stream stream = null;
            try {
                string arg = arguments[0].head().getStringValue();
                stream = resolveResourceAccess(context, arg);
                if ((stream == null)) {
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
            } catch (Exception e) when (e is System.IO.IOException || e is System.UriFormatException) {
                throw new Saxon.Hej.trans.XPathException(("saxon:read-binary-resource: I/O error: "+e.Message), Saxon.Hej.trans.SaxonErrorCode.SXJE0053);
            } catch (Saxon.Hej.trans.XPathException e) {
                if ((e.getException() is System.UriFormatException)) {
                    throw new Saxon.Hej.trans.XPathException(("saxon:read-binary-resource: I/O error: "+e.Message), Saxon.Hej.trans.SaxonErrorCode.SXJE0053);
                }
                throw;
            } finally {
                if ((stream != null)) {
                    try {
                        stream.Close();
                    } catch (System.IO.IOException e) {
                    }
                }
            }
        }
        private bool isFinished(int indicator) {
            return indicator == 0;
        }
    
    }
}