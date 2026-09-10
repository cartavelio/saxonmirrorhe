using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class StandardResultDocumentResolver : Saxon.Hej.lib.ResultDocumentResolver {
        private static readonly Saxon.Hej.lib.StandardResultDocumentResolver theInstance = new Saxon.Hej.lib.StandardResultDocumentResolver();
        public static Saxon.Hej.lib.StandardResultDocumentResolver getInstance() {
            return theInstance;
        }
        public Saxon.Hej.@event.Receiver resolve(Saxon.Hej.expr.XPathContext context, string href, string baseUri, Saxon.Hej.serialize.SerializationProperties properties) {
            Saxon.Ejavax.xml.transform.stream.StreamResult result = resolve(href, baseUri);
            Saxon.Hej.lib.SerializerFactory factory = context.getConfiguration().getSerializerFactory();
            Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
            return factory.getReceiver(result, properties, pipe);
        }
        public Saxon.Ejavax.xml.transform.stream.StreamResult resolve(string href, string @base) {
            string which = "base";
            try {
                System.Uri absoluteURI;
                if (href.Length==0) {
                    if ((@base == null)) {
                        throw new Saxon.Hej.trans.XPathException("The system identifier of the principal output file is unknown");
                    }
                    absoluteURI = new System.Uri(@base, System.UriKind.RelativeOrAbsolute);
                } else {
                    which = "relative";
                    absoluteURI = new System.Uri(href, System.UriKind.RelativeOrAbsolute);
                }
                if (!(absoluteURI.IsAbsoluteUri)) {
                    if ((@base == null)) {
                        throw new Saxon.Hej.trans.XPathException("The system identifier of the principal output file is unknown");
                    }
                    which = "base";
                    System.Uri baseURI = new System.Uri(@base, System.UriKind.RelativeOrAbsolute);
                    which = "relative";
                    absoluteURI = new System.Uri(baseURI, href);
                }
                return createResult(absoluteURI);
            } catch (System.UriFormatException err) {
                throw new Saxon.Hej.trans.XPathException((("Invalid syntax for "+which)+" URI"), err);
            } catch (Saxon.Ejava.lang.IllegalArgumentException err2) {
                throw new Saxon.Hej.trans.XPathException((("Invalid "+which)+" URI syntax"), err2);
            } catch (System.Exception err6) {
                throw new Saxon.Hej.trans.XPathException("Standard result document resolver failed", err6);
            }
        }
        protected internal Saxon.Ejavax.xml.transform.stream.StreamResult createResult(System.Uri absoluteURI) {
            if ("file".Equals(absoluteURI.Scheme)) {
                return makeOutputFile(absoluteURI);
            } else {
                string details = "";
                throw new Saxon.Hej.trans.XPathException(("Failed to establish connection to non-file output destination"+((details.Length==0 ? "" : ((": "+details))))));
            }
        }
        public static Saxon.Ejavax.xml.transform.stream.StreamResult makeOutputFile(System.Uri absoluteURI) { lock(typeof(StandardResultDocumentResolver)) {
            try {
                Saxon.Ejava.io.File outputFile = new Saxon.Ejava.io.File(absoluteURI);
                if (outputFile.isDirectory()) {
                    throw new Saxon.Hej.trans.XPathException(("Cannot write to a directory: "+absoluteURI), Saxon.Hej.trans.SaxonErrorCode.SXRD0004);
                }
                if ((outputFile.exists() && !(outputFile.canWrite()))) {
                    throw new Saxon.Hej.trans.XPathException(("Cannot write to URI "+absoluteURI), Saxon.Hej.trans.SaxonErrorCode.SXRD0004);
                }
                return new Saxon.Ejavax.xml.transform.stream.StreamResult(outputFile);
            } catch (Saxon.Ejava.lang.IllegalArgumentException err) {
                throw new Saxon.Hej.trans.XPathException((((("Cannot write to URI "+absoluteURI)+" (")+err.Message)+")"));
            }
        }}
        public void close(Saxon.Ejavax.xml.transform.Result result) {
            if ((result is Saxon.Ejavax.xml.transform.stream.StreamResult)) {
                System.IO.Stream stream = ((Saxon.Ejavax.xml.transform.stream.StreamResult)(result)).getOutputStream();
                if ((stream != null)) {
                    try {
                        stream.Close();
                    } catch (System.IO.IOException err) {
                        throw new Saxon.Hej.trans.XPathException("Failed while closing output file", err);
                    }
                }
                System.IO.TextWriter writer = ((Saxon.Ejavax.xml.transform.stream.StreamResult)(result)).getWriter();
                if ((writer != null)) {
                    try {
                        writer.Close();
                    } catch (System.IO.IOException err) {
                        throw new Saxon.Hej.trans.XPathException("Failed while closing output file", err);
                    }
                }
            }
        }
    
    }
}