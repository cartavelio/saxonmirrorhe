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
            if (((elementName.hasURI(Saxon.Hej.lib.NamespaceConstant.XSLT) || !(allowExtensions)) || (parent is Saxon.Hej.style.DataElement))) {
                return base.makeElementNode(parent, elementName, elementType, isNilled, attlist, namespaces, pipe, location, sequence);
            }
            System.Type extensionClass = null;
            string uri = elementName.getURI();
            Saxon.Eej.xsltextn.ExtensionElementFactory eef = ((Saxon.Eej.config.ProfessionalConfiguration)(config)).getExtensionElementFactory(uri);
            if ((eef != null)) {
                string localName = elementName.getLocalPart();
                extensionClass = eef.getExtensionClass(localName);
            }
            if ((extensionClass == null)) {
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
            System.Type requiredClass = extensionClass;
            if (allowExtensions) {
                temp = base.makeStyleElement(extensionClass);
                temp.setCompilation(getCompilation());
            } else {
                temp = new Saxon.Hej.style.AbsentExtensionElement();
                Saxon.Hej.trans.XmlProcessingIncident reason = new Saxon.Hej.trans.XmlProcessingIncident("Extension instructions are disabled");
                temp.setValidationError(reason, Saxon.Hej.style.StyleElement.OnFailure.REPORT_IF_INSTANTIATED);
                requiredClass = typeof(Saxon.Hej.style.AbsentExtensionElement);
            }
            temp.setNamespaceMap(namespaces);
            temp.initialise(elementName, elementType, attlist, parent, sequence);
            temp.setLocation(baseURI, lineNumber, columnNumber);
            temp.processStandardAttributes(Saxon.Hej.lib.NamespaceConstant.XSLT);
            if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.SAXON)) {
                if (!(((toplevel || temp.isExtensionNamespace(Saxon.Hej.lib.NamespaceConstant.SAXON))))) {
                    Saxon.Hej.trans.XmlProcessingIncident te = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(((("saxon:"+localname)+" is not being treated as an extension instruction because the namespace ")+" http://saxon.sf.net/ has not been declared in extension-element-prefixes"), Saxon.Hej.trans.SaxonErrorCode.SXWN9008, temp).asWarning());
                    pipe.getErrorReporter().report(te);
                    requiredClass = typeof(Saxon.Hej.style.LiteralResultElement);
                }
            }
            Saxon.Hej.style.StyleElement node;
            if (requiredClass.Equals(temp.GetType())) {
                node = temp;
            } else {
                node = makeStyleElement(requiredClass);
                node.substituteFor(temp);
            }
            return node;
        }
        public override bool isElementAvailable(string uri, string localName, bool instructionsOnly) {
            bool b = base.isElementAvailable(uri, localName, instructionsOnly);
            if (b) {
                return true;
            }
            if (!(allowExtensions)) {
                return false;
            }
            Saxon.Eej.xsltextn.ExtensionElementFactory factory = ((Saxon.Eej.config.ProfessionalConfiguration)(config)).getExtensionElementFactory(uri);
            if ((factory == null)) {
                return false;
            }
            try {
                System.Type actualClass = factory.getExtensionClass(localName);
                Saxon.Hej.style.StyleElement s = (Saxon.Hej.style.StyleElement)(System.Activator.CreateInstance(actualClass));
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