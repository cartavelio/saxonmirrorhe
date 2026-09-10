using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Saxon.Api;
using System.IO;
using System.Xml;

namespace TestDriver
{
    public abstract class ResultsDocument
    {
        private String reportFilename = null;
        private String resultsDir;
        protected Spec spec;
        protected XmlWriter results;
        private StreamWriter writer;

        protected ResultsDocument(String rdir, Spec sp, String fn)
        {
            this.resultsDir = rdir;
            this.spec = sp;
            this.reportFilename = fn;
        }

        protected ResultsDocument(String rdir, Spec sp)
        {
            this.resultsDir = rdir;
            this.spec = sp;
            this.reportFilename = null;
        }

        protected void createWriter(Processor processor) {

            string fileName = resultsDir + "/results"
                + "_" + ((SpecAttr)spec.GetAttr()).fname + "_" + processor.ProductVersion + "n.xml";

            if (reportFilename != null) {
               fileName = resultsDir + "/" + reportFilename;
            }

            // Delete the file if it exists.
            if (System.IO.File.Exists(fileName)) {
                System.IO.File.Delete(fileName);
            }
            //  StreamWriter resultWriter = File.CreateText(fileName);   
            Serializer serializer =processor.NewSerializer();
              
            serializer.OutputWriter = null;
            serializer.SetOutputProperty(Serializer.METHOD, "xml");
            serializer.SetOutputProperty(Serializer.INDENT, "yes");
            //serializer.SetOutputProperty(Serializer.Property.SAXON_LINE_LENGTH, "120");
            //this.Writer = resultWriter;
           // this.results = serializer.GetResult(null);  . .GetXMLStreamWriter();
            results = new XmlTextWriter(new StreamWriter(fileName));
              
        }

        public abstract void writeResultFilePreamble(Processor processor, XdmNode catalog);



        public void writeResultFilePostamble() {
            try {
                this.results.WriteEndElement();
                this.results.Close();
            } catch (InvalidOperationException e) {
                throw new SystemException(e.Message);
            }
            if (writer != null) {
                try {
                    writer.Close();
                } catch (EncoderFallbackException e) {
                    throw new SystemException(e.Message);
                }
            }
        }

        public void startTestSetElement(XdmNode funcSetNode) {
            try {
                results.WriteStartElement("test-set");
                results.WriteAttributeString("name", funcSetNode.GetAttributeValue(new QName("name")));
            } catch (XmlException e) {
                throw new SystemException(e.Message);
            }
        }

        public void endElement() {
            try {
                results.WriteEndElement();
            } catch (Exception e) {
                throw new SaxonApiException(e);
            }
        }

        public void writeTestcaseElement(String name, String result, String comment) {
            try {
                this.results.WriteStartElement("test-case");
                this.results.WriteAttributeString("name", name);
                this.results.WriteAttributeString("result", result);
                if (comment != null) {
                    this.results.WriteAttributeString("comment", comment);
                }
                this.results.WriteEndElement();
            } catch (Exception e) {
                throw new SaxonApiException(e.Message);
            }
        }

        public string getDate() {
            return DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }
    }
}
