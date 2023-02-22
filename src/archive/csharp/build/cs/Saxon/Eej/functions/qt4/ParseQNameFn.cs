using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class ParseQNameFn : Saxon.Hej.functions.SystemFunction {
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
                return new Saxon.Hej.value.QNameValue(parts[0], Saxon.Hej.om.NamespaceUri.of(uri), parts[1], Saxon.Hej.type.BuiltInAtomicType.QNAME, true);
            } catch (Saxon.Hej.om.QNameException e) {
                throw new Saxon.Hej.trans.XPathException(e.Message, "FOCA0002");
            } catch (Saxon.Hej.trans.XPathException err) {
                if (err.getErrorCodeLocalPart().Equals("FORG0001")) {
                    err.setErrorCode("FOCA0002");
                }
                throw;
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue argValue = (Saxon.Hej.value.StringValue)(arguments[0].head());
            if ((argValue == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            string input = Saxon.Hej.value.Whitespace.trim(argValue.getStringValue());
            Saxon.Hej.om.StructuredQName qn = Saxon.Hej.om.StructuredQName.fromLexicalQName(input, false, false, getRetainedStaticContext());
            return new Saxon.Hej.value.QNameValue(qn, Saxon.Hej.type.BuiltInAtomicType.QNAME);
        }
        public override string getCompilerName() {
            return "QNameFnCompiler";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Eej.functions.qt4.ParseQNameFn.QNameFnElaborator();
        }
        public class QNameFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall sfc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                if ((sfc.getArity() == 2)) {
                    Saxon.Hej.expr.elab.ItemEvaluator arg0eval = sfc.getArg(0).makeElaborator().elaborateForItem();
                    Saxon.Hej.expr.elab.ItemEvaluator arg1eval = sfc.getArg(1).makeElaborator().elaborateForItem();
                    return context => expandedQName((Saxon.Hej.value.StringValue)(/*Delegate.eval()*/arg0eval(context)), (Saxon.Hej.value.StringValue)(/*Delegate.eval()*/arg1eval(context)));
                } else {
                    Saxon.Hej.expr.elab.ItemEvaluator arg0eval = sfc.getArg(0).makeElaborator().elaborateForItem();
                    Saxon.Hej.om.NamespaceResolver resolver = sfc.getRetainedStaticContext();
                    return context => {
                        Saxon.Hej.om.Item @in = /*Delegate.eval()*/arg0eval(context);
                        if ((@in == null)) {
                            return null;
                        }
                        Saxon.Hej.om.StructuredQName qn = Saxon.Hej.om.StructuredQName.fromLexicalQName(@in.getStringValue(), false, true, resolver);
                        return new Saxon.Hej.value.QNameValue(qn, Saxon.Hej.type.BuiltInAtomicType.QNAME);
                    } ;
                }
            }
        
        }
    
    }
}