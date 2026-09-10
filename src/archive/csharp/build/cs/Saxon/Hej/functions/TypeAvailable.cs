using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class TypeAvailable : Saxon.Hej.functions.SystemFunction {
        private bool typeAvailable(string lexicalName, Saxon.Hej.Configuration config) {
            Saxon.Hej.om.StructuredQName qName;
            try {
                if (((lexicalName.IndexOf(':')<0) && !(lexicalName.StartsWith("Q{")))) {
                    string defaultNS = getRetainedStaticContext().getURIForPrefix("", true);
                    qName = new Saxon.Hej.om.StructuredQName("", defaultNS, lexicalName);
                } else {
                    qName = Saxon.Hej.om.StructuredQName.fromLexicalQName(lexicalName, false, true, getRetainedStaticContext());
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                e.setErrorCode("XTDE1428");
                throw;
            }
            string uri = qName.getURI();
            if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.JAVA_TYPE)) {
                try {
                    string className = Saxon.Hej.type.JavaExternalObjectType.localNameToClassName(qName.getLocalPart());
                    config.getClass(className, false);
                    return true;
                } catch (Saxon.Hej.trans.XPathException err) {
                    return false;
                }
            } else {
                Saxon.Hej.type.SchemaType type = config.getSchemaType(qName);
                if ((type == null)) {
                    return false;
                }
                return (((config.getXsdVersion() != 10) || !(((type is Saxon.Hej.type.BuiltInAtomicType)))) || ((Saxon.Hej.type.BuiltInAtomicType)(type)).isAllowedInXSD10());
            }
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string lexicalQName = arguments[0].head().getStringValue();
            return Saxon.Hej.value.BooleanValue.get(typeAvailable(lexicalQName, context.getConfiguration()));
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            try {
                if ((arguments[0] is Saxon.Hej.expr.StringLiteral)) {
                    bool b = typeAvailable(((Saxon.Hej.expr.StringLiteral)(arguments[0])).stringify(), getRetainedStaticContext().getConfiguration());
                    return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get(b));
                }
            } catch (Saxon.Hej.trans.XPathException e) {
            }
            return base.makeFunctionCall(arguments);
        }
    
    }
}