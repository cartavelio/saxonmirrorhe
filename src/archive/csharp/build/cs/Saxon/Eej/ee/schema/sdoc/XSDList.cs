using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDList : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition typeDefinition;
        private bool foundItemType = false;
        public XSDList()  {
        }
        protected internal override void prepareAttributes() {
            typeDefinition = getContainingSimpleType();
            typeDefinition.setDerivationMethodName("list");
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "itemType"};
            allowAttributes(atts, allowed);
            processId();
            string itemTypeName = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "itemType");
            if ((itemTypeName != null)) {
                foundItemType = true;
                int itemTypeFingerprint;
                try {
                    itemTypeFingerprint = getFingerprint(itemTypeName, USE_DEFAULT_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                    return ;
                }
                if (((itemTypeFingerprint == Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION) || (itemTypeFingerprint == Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION))) {
                    warning((("Type "+itemTypeName)+" is recognized by Saxon but is not interoperable"));
                } else {
                    if ((((itemTypeFingerprint == Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE) || (itemTypeFingerprint == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) || (itemTypeFingerprint == Saxon.Hej.om.StandardNames.XS_UNTYPED))) {
                        error((("Type "+itemTypeName)+" cannot be used as an item type"));
                    }
                }
                Saxon.Eej.ee.schema.TypeReference typeRef = new Saxon.Eej.ee.schema.TypeReference(itemTypeFingerprint, (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
                typeDefinition.setItemTypeReference(typeRef);
            }
            string separator = atts.getValue(Saxon.Hej.om.NamespaceUri.SAXON, "separator");
            if ((separator != null)) {
                System.Collections.Generic.IList<string> warnings = new System.Collections.Generic.List<string>();
                try {
                    Saxon.Hej.regex.RegularExpression regex = getConfiguration().compileRegularExpression(Saxon.Hej.str.StringView.tidy(separator), "", "XSD11", warnings);
                    if (regex.matches(Saxon.Hej.str.EmptyUnicodeString.getInstance())) {
                        error("The regular expression in saxon:separator must not be one that matches a zero-length string");
                    }
                    foreach (string message in warnings) {
                        warning(message);
                    }
                    typeDefinition.setTokenSeparator(regex);
                } catch (Saxon.Hej.trans.XPathException e) {
                    error(("Error in saxon:separator regular expression: "+e.Message));
                }
            }
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool foundAnnotation = false;
            bool foundSimpleType = false;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                switch (fp) {
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    {
                        if (foundSimpleType) {
                            mustBeFirstElement("annotation");
                        }
                        if (foundAnnotation) {
                            duplicateElement("annotation");
                        }
                        foundAnnotation = true;
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                    {
                        if (foundItemType) {
                            mutuallyExclusiveElementAndAttribute("simpleType", "itemType");
                        }
                        if (foundSimpleType) {
                            duplicateElement("simpleType");
                        }
                        foundSimpleType = true;
                        break ;
                    }
                  default:
                    {
                        illegalElement(child);
                        break ;
                    }
                }
            }
            if ((!(foundItemType) && !(foundSimpleType))) {
                string err = ("A <list> element must either have an @itemType attribute "+"or a <simpleType> child");
                error(err);
            }
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child.getFingerprint() == Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE)) {
                    Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition itemType = ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(child)).getSimpleTypeDefinition();
                    Saxon.Eej.ee.schema.TypeReference typeRef = new Saxon.Eej.ee.schema.TypeReference(itemType.getFingerprint(), (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), child);
                    typeRef.setTarget(itemType);
                    typeDefinition.setItemTypeReference(typeRef);
                }
            }
        }
    
    }
}