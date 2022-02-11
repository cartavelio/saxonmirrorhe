using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDDocumentation : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        public XSDDocumentation()  {
        }
        public override void processAllAttributes() {
            prepareAttributes();
            if ((getFingerprint() == Saxon.Hej.om.StandardNames.XS_APPINFO)) {
                foreach (Saxon.Hej.om.NodeInfo child in children()) {
                    if ((child is Saxon.Eej.ee.schema.sdoc.SaxonParam)) {
                        ((Saxon.Eej.ee.schema.sdoc.SchemaElement)(child)).processAllAttributes();
                    }
                }
            }
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"source"};
            allowAttributes(atts, allowed);
            string source = atts.getValue("", "source");
            if ((getXSDSchema().getSchemaCompiler().getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                if (((source != null) && !(Saxon.Hej.lib.StandardURIChecker.getInstance().isValidURI(source)))) {
                    error("Value of source attribute must be a valid anyURI value");
                }
            }
            string lang = atts.getValue(Saxon.Hej.lib.NamespaceConstant.XML, "lang");
            if ((lang != null)) {
                Saxon.Hej.type.ValidationFailure vf = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE.validate(Saxon.Hej.str.StringView.tidy(lang));
                if ((vf != null)) {
                    error("Value of xml:lang attribute is not a valid language code");
                }
            }
        }
        public override void validateSubtree(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
    
    }
}