using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.config.pe {
    public class StyleNodeFactoryPE : Saxon.Hej.style.StyleNodeFactory, Saxon.Hej.tree.linked.NodeFactory {
        protected internal bool allowExtensions;
        public StyleNodeFactoryPE(Saxon.Hej.Configuration config, Saxon.Hej.style.Compilation compilation) : base (config, compilation) {
            allowExtensions = config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS);
        }
        public override Saxon.Hej.tree.linked.ElementImpl makeElementNode(Saxon.Hej.om.NodeInfo parent, Saxon.Hej.om.NodeName elementName, Saxon.Hej.type.SchemaType elementType, bool isNilled, Saxon.Hej.om.AttributeMap attlist, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.s9api.Location location, int sequence) {
            bool toplevel = (parent is Saxon.Hej.style.XSLModuleRoot);
            if (((elementName.hasURI(Saxon.Hej.om.NamespaceUri.XSLT) || !(allowExtensions)) || (parent is Saxon.Hej.style.DataElement))) {
                return base.makeElementNode(parent, elementName, elementType, isNilled, attlist, namespaces, pipe, location, sequence);
            }
            Saxon.Hej.style.StyleElement extensionElement = null;
            Saxon.Hej.om.NamespaceUri uri = elementName.getNamespaceUri();
            Saxon.Eej.xsltextn.ExtensionElementCreator eef = ((Saxon.Eej.config.ProfessionalConfiguration)(config)).getExtensionElementCreator(uri);
            if ((eef != null)) {
                string localName = elementName.getLocalPart();
                extensionElement = /*Delegate.createExtensionElement()*/eef(localName);
            }
            if ((extensionElement == null)) {
                Saxon.Hej.tree.linked.ElementImpl instr = base.makeElementNode(parent, elementName, elementType, isNilled, attlist, namespaces, pipe, location, sequence);
                if ((instr is Saxon.Hej.style.StyleElement)) {
                    ((Saxon.Hej.style.StyleElement)(instr)).setCompilation(getCompilation());
                }
                return instr;
            }
            string baseURI = null;
            int lineNumber = -1;
            int columnNumber = -1;
            baseURI = location.getSystemId();
            lineNumber = location.getLineNumber();
            columnNumber = location.getColumnNumber();
            string localname = elementName.getLocalPart();
            Saxon.Hej.style.StyleElement temp;
            if (allowExtensions) {
                temp = extensionElement;
                temp.setCompilation(getCompilation());
            } else {
                temp = new Saxon.Hej.style.AbsentExtensionElement();
                Saxon.Hej.trans.XmlProcessingIncident reason = new Saxon.Hej.trans.XmlProcessingIncident("Extension instructions are disabled");
                temp.setValidationError(reason, Saxon.Hej.style.StyleElement.OnFailure.REPORT_IF_INSTANTIATED);
            }
            temp.setNamespaceMap(namespaces);
            temp.initialise(elementName, elementType, attlist, parent, sequence);
            temp.setLocation(baseURI, lineNumber, columnNumber);
            temp.processStandardAttributes(Saxon.Hej.om.NamespaceUri.XSLT);
            if (uri.Equals(Saxon.Hej.om.NamespaceUri.SAXON)) {
                if (!(((toplevel || temp.isExtensionNamespace(Saxon.Hej.om.NamespaceUri.SAXON))))) {
                    Saxon.Hej.trans.XmlProcessingIncident te = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(((("saxon:"+localname)+" is not being treated as an extension instruction because the namespace ")+" http://saxon.sf.net/ has not been declared in extension-element-prefixes"), Saxon.Hej.trans.SaxonErrorCode.SXWN9008, temp).asWarning());
                    pipe.getErrorReporter().report(te);
                    temp = new Saxon.Hej.style.LiteralResultElement();
                }
            }
            return temp;
        }
        public override bool isElementAvailable(Saxon.Hej.om.NamespaceUri uri, string localName, bool instructionsOnly) {
            bool b = base.isElementAvailable(uri, localName, instructionsOnly);
            if (b) {
                return true;
            }
            if (!(allowExtensions)) {
                return false;
            }
            Saxon.Eej.xsltextn.ExtensionElementCreator factory = ((Saxon.Eej.config.ProfessionalConfiguration)(config)).getExtensionElementCreator(uri);
            if ((factory == null)) {
                return false;
            }
            try {
                Saxon.Hej.style.StyleElement s = /*Delegate.createExtensionElement()*/factory(localName);
                return (!(instructionsOnly) || s.isInstruction());
            } catch (System.Exception e) {
                return false;
            }
        }
        public override Saxon.Hej.expr.accum.AccumulatorRegistry makeAccumulatorManager() {
            return new Saxon.Hej.expr.accum.AccumulatorRegistry();
        }
    
    }
}