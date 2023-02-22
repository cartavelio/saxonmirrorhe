using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class XmlResource : Saxon.Hej.lib.Resource {
        private Saxon.Hej.om.NodeInfo doc;
        private readonly Saxon.Hej.expr.XPathContext context;
        private readonly Saxon.Hej.Configuration config;
        private Saxon.Hej.resource.AbstractResourceCollection.InputDetails details;
        public readonly static Saxon.Hej.lib.ResourceFactory FACTORY = (context, details) => new Saxon.Hej.resource.XmlResource(context, details);
        public XmlResource(Saxon.Hej.om.NodeInfo doc)  {
            this.config = doc.getConfiguration();
            this.context = this.config.getConversionContext();
            this.doc = doc;
        }
        public XmlResource(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.NodeInfo doc)  {
            this.context = context;
            this.config = context.getConfiguration();
            this.doc = doc;
            if ((config != doc.getConfiguration())) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Supplied node belongs to wrong configuration");
            }
        }
        public XmlResource(Saxon.Hej.expr.XPathContext context, Saxon.Hej.resource.AbstractResourceCollection.InputDetails details)  {
            this.config = context.getConfiguration();
            this.context = context;
            this.details = details;
        }
        public string getResourceURI() {
            if ((doc == null)) {
                return details.resourceUri;
            } else {
                return doc.getSystemId();
            }
        }
        public Saxon.Hej.om.Item getItem() {
            if ((doc == null)) {
                string resourceURI = details.resourceUri;
                Saxon.Hej.lib.ParseOptions options = details.parseOptions;
                if ((options == null)) {
                    options = config.getParseOptions();
                }
                Saxon.Ejavax.xml.transform.stream.StreamSource source;
                if ((details.characterContent != null)) {
                    source = new Saxon.Ejavax.xml.transform.stream.StreamSource(new System.IO.StringReader(details.characterContent), resourceURI);
                } else {
                    if ((details.binaryContent != null)) {
                        source = new Saxon.Ejavax.xml.transform.stream.StreamSource(new System.IO.MemoryStream(details.binaryContent), resourceURI);
                    } else {
                        try {
                            System.IO.Stream stream = details.getInputStream();
                            source = new Saxon.Ejavax.xml.transform.stream.StreamSource(stream, resourceURI);
                        } catch (System.IO.IOException e) {
                            if ((details.onError == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_FAIL)) {
                                throw new Saxon.Hej.trans.XPathException(e);
                            } else {
                                if ((details.onError == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_WARNING)) {
                                    context.getController().warning(((("collection(): failed to read XML file "+details.resourceUri)+": ")+e.Message), "FODC0005", null);
                                    return null;
                                } else {
                                    return null;
                                }
                            }
                        }
                    }
                }
                try {
                    doc = config.buildDocumentTree(source, options).getRootNode();
                } catch (Saxon.Hej.trans.XPathException e) {
                    if ((details.onError == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_FAIL)) {
                        Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException(((("collection(): failed to parse XML file "+source.getSystemId())+": ")+e.Message), e.getErrorCodeLocalPart());
                        throw e2;
                    } else {
                        if ((details.onError == Saxon.Hej.functions.URIQueryParameters.ON_ERROR_WARNING)) {
                            context.getController().warning(((("collection(): failed to parse XML file "+source.getSystemId())+": ")+e.Message), e.getErrorCodeLocalPart(), null);
                        }
                    }
                    doc = null;
                } finally {
                    if (((source != null) && (source.getInputStream() != null))) {
                        try {
                            source.getInputStream().Close();
                        } catch (System.IO.IOException e) {
                        }
                    }
                }
            }
            return doc;
        }
        public string getContentType() {
            return "application/xml";
        }
    
    }
}