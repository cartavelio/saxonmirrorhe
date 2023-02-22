using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDUnion : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition typeDefinition;
        private int numberOfMemberTypes = 0;
        public XSDUnion()  {
        }
        protected internal override void prepareAttributes() {
            typeDefinition = getContainingSimpleType();
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "memberTypes"};
            allowAttributes(atts, allowed);
            processId();
            typeDefinition.setDerivationMethodName("union");
            string memberTypes = atts.getValue("", "memberTypes");
            processMemberTypes(memberTypes);
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool foundAnnotation = false;
            bool foundSimpleType = false;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                if ((fp == Saxon.Hej.om.StandardNames.XS_ANNOTATION)) {
                    if (foundAnnotation) {
                        duplicateElement("annotation");
                    }
                    if (foundSimpleType) {
                        mustBeFirstElement("annotation");
                    }
                    foundAnnotation = true;
                } else {
                    if ((fp == Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE)) {
                        foundSimpleType = true;
                        numberOfMemberTypes++;
                    } else {
                        illegalElement(child);
                    }
                }
            }
            if ((numberOfMemberTypes == 0)) {
                string err = "A union type must have at least one member type";
                error(err);
            }
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child.getFingerprint() == Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE)) {
                    Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition memberType = ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(child)).getSimpleTypeDefinition();
                    int fp = ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(child)).getSimpleTypeDefinition().getFingerprint();
                    Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(fp, compiler.getConfiguration(), child);
                    @ref.setTarget(memberType);
                    typeDefinition.addMemberType(@ref);
                }
            }
        }
        private void processMemberTypes(string memberTypes) {
            if ((((memberTypes == null)) || (memberTypes.Length==0))) {
                return ;
            }
            Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(memberTypes, " \t\n\r", false);
            while (st.hasMoreTokens()) {
                string typeName = st.nextToken();
                int fp;
                try {
                    fp = getFingerprint(typeName, USE_DEFAULT_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                    return ;
                }
                if (((fp == Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION) || (fp == Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION))) {
                    if ((getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10)) {
                        warning((("Type "+typeName)+" is recognized by Saxon but is not interoperable"));
                    }
                } else {
                    if (((((fp == Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE) || (fp == Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE)) || (fp == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) || (fp == Saxon.Hej.om.StandardNames.XS_UNTYPED))) {
                        if ((getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10)) {
                            warning((("In XSD 1.1, type "+typeName)+" cannot be used as a member type of a union"));
                        } else {
                            error((("In XSD 1.1, type "+typeName)+" cannot be used as a member type of a union"));
                        }
                    }
                }
                Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(fp, typeDefinition.getConfiguration(), this);
                typeDefinition.addMemberType(@ref);
                numberOfMemberTypes++;
            }
        }
    
    }
}