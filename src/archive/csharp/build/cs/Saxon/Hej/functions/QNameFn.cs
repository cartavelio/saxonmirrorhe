using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class QNameFn : Saxon.Hej.functions.SystemFunction {
        public static Saxon.Hej.value.QNameValue expandedQName(Saxon.Hej.value.StringValue @namespace, Saxon.Hej.value.StringValue lexical) {
            string uri;
            if ((@namespace == null)) {
                uri = null;
            } else {
                uri = @namespace.getStringValue();
            }
            try {
                string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(lexical.getStringValue());
                if ((!(parts[0].Length==0) && !(Saxon.Hej.om.NameChecker.isValidNCName(parts[0])))) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Malformed prefix in QName: '"+parts[0])+'\''));
                    err.setErrorCode("FOCA0002");
                    throw err;
                }
                return new Saxon.Hej.value.QNameValue(parts[0], uri, parts[1], Saxon.Hej.type.BuiltInAtomicType.QNAME, true);
            } catch (Saxon.Hej.om.QNameException e) {
                throw new Saxon.Hej.trans.XPathException(e.Message, "FOCA0002");
            } catch (Saxon.Hej.trans.XPathException err) {
                if (err.getErrorCodeLocalPart().Equals("FORG0001")) {
                    err.setErrorCode("FOCA0002");
                }
                throw;
            }
        }
        public override Saxon.Hej.value.QNameValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return expandedQName((Saxon.Hej.value.StringValue)(arguments[0].head()), (Saxon.Hej.value.StringValue)(arguments[1].head()));
        }
        public override string getCompilerName() {
            return "QNameFnCompiler";
        }
    
    }
}