using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public abstract class BinaryEmitter : Saxon.Hej.serialize.Emitter {
        protected internal System.IO.Stream outputStream;
        private bool mustClose;
        public BinaryEmitter(Saxon.Ejavax.xml.transform.stream.StreamResult streamResult, Saxon.Hej.@event.PipelineConfiguration pipe)  {
            setPipelineConfiguration(pipe);
            this.outputStream = makeOutputStream(streamResult);
        }
        public override void open() {
        }
        public override void startDocument(int properties) {
        }
        public override void endDocument() {
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
        }
        public override void endElement() {
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            System.Diagnostics.Debug.Assert((outputStream != null));
            byte[] bytes = textNodeToBinary(chars.tidy());
            try {
                outputStream.Write(bytes);
            } catch (System.IO.IOException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
        }
        protected internal abstract byte[] textNodeToBinary(Saxon.Hej.str.UnicodeString chars);
        public override void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void close() {
            try {
                if ((mustClose && (outputStream != null))) {
                    outputStream.Close();
                }
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        public override bool usesTypeAnnotations() {
            return false;
        }
        protected internal System.IO.Stream makeOutputStream(Saxon.Ejavax.xml.transform.stream.StreamResult streamResult) {
            if ((streamResult.getOutputStream() != null)) {
                return streamResult.getOutputStream();
            }
            string uriString = streamResult.getSystemId();
            if ((uriString == null)) {
                throw new Saxon.Hej.trans.XPathException("BinaryEmitter result has no system ID or output stream defined", Saxon.Hej.trans.SaxonErrorCode.SXRD0004);
            }
            try {
                Saxon.Ejava.io.File file = Saxon.Hej.serialize.ExpandedStreamResult.makeWritableOutputFile(uriString);
                outputStream = new System.IO.FileStream(file.getPath(), System.IO.FileMode.Create);
                streamResult.setOutputStream(outputStream);
                mustClose = true;
            } catch (Exception fnf) when (fnf is Saxon.Ejava.io.FileNotFoundException || fnf is System.UriFormatException || fnf is Saxon.Ejava.lang.IllegalArgumentException) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Unable to write to output destination", fnf);
                err.setErrorCode(Saxon.Hej.trans.SaxonErrorCode.SXRD0004);
                throw err;
            }
            return outputStream;
        }
    
    }
}