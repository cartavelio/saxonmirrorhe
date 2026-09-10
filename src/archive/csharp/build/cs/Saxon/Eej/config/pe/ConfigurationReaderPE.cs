using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.config.pe {
    public class ConfigurationReaderPE : Saxon.Hej.trans.ConfigurationReader {
        protected internal override void readExtensionElement(Saxon.Hej.om.AttributeMap atts) {
            string @namespace = null;
            string className = null;
            foreach (Saxon.Hej.om.AttributeInfo a in atts) {
                string uri = a.getNodeName().getURI();
                string attName = a.getNodeName().getLocalPart();
                string value = a.getValue();
                if (uri.Length==0) {
                    if (("namespace".Equals(attName) && (value.Length>0))) {
                        @namespace = value;
                    }
                    if (("factory".Equals(attName) && (value.Length>0))) {
                        className = value;
                    }
                }
            }
            if (((@namespace != null) && (className != null))) {
                try {
                    Saxon.Eej.xsltextn.ExtensionElementFactory factory = (Saxon.Eej.xsltextn.ExtensionElementFactory)(targetConfig.getDynamicLoader().getInstance(className, null));
                    ((Saxon.Eej.config.ProfessionalConfiguration)(targetConfig)).setExtensionElementNamespace(@namespace, factory);
                } catch (Saxon.Hej.trans.XPathException e) {
                    errorClass("extensionElement", null, className, typeof(Saxon.Eej.xsltextn.ExtensionElementFactory), e);
                }
            }
        }
    
    }
}