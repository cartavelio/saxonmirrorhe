using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Translate : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable, Saxon.Hej.functions.StatefulSystemFunction {
        private Saxon.Hej.z.IntToIntMap staticMap = null;
        public override Saxon.Hej.expr.Expression fixArguments(params Saxon.Hej.expr.Expression[]  arguments) {
            if (((arguments[1] is Saxon.Hej.expr.StringLiteral) && (arguments[2] is Saxon.Hej.expr.StringLiteral))) {
                staticMap = buildMap(((Saxon.Hej.expr.StringLiteral)(arguments[1])).getGroundedValue(), ((Saxon.Hej.expr.StringLiteral)(arguments[2])).getGroundedValue());
            }
            return null;
        }
        public Saxon.Hej.z.IntToIntMap getStaticMap() {
            return staticMap;
        }
        public static Saxon.Hej.value.StringValue translate(Saxon.Hej.value.StringValue sv0, Saxon.Hej.value.StringValue sv1, Saxon.Hej.value.StringValue sv2) {
            if (((sv0.length()*sv1.length())>1000)) {
                return translateUsingMap(sv0, buildMap(sv1, sv2));
            }
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder(sv0.length32());
            long s2len = sv2.length();
            Saxon.Hej.z.IntIterator iter = sv0.codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                long j = sv1.getContent().indexOf(c, 0);
                if ((j<s2len)) {
                    sb.append(((j<0) ? c : sv2.getContent().codePointAt(j)));
                }
            }
            return new Saxon.Hej.value.StringValue(sb.toUnicodeString());
        }
        private static Saxon.Hej.z.IntToIntMap buildMap(Saxon.Hej.value.StringValue arg1, Saxon.Hej.value.StringValue arg2) {
            Saxon.Hej.z.IntToIntMap map = new Saxon.Hej.z.IntToIntHashMap(arg1.length32(), 0.5);
            Saxon.Hej.z.IntIterator iter = arg1.codePoints();
            long arg2len = arg2.length();
            long i = 0;
            while (iter.hasNext()) {
                int ch = iter.next();
                if (!(map.contains(ch))) {
                    map.put(ch, ((i>=arg2len) ? -1 : arg2.getContent().codePointAt(i)));
                }
                i++;
            }
            return map;
        }
        public static Saxon.Hej.value.StringValue translateUsingMap(Saxon.Hej.value.StringValue @in, Saxon.Hej.z.IntToIntMap map) {
            Saxon.Hej.str.UnicodeBuilder builder = new Saxon.Hej.str.UnicodeBuilder(@in.length32());
            Saxon.Hej.z.IntIterator iter = @in.codePoints();
            while (iter.hasNext()) {
                int c = iter.next();
                int newchar = map.get(c);
                if ((newchar == System.Int32.MaxValue)) {
                    newchar = c;
                }
                if ((newchar != -1)) {
                    builder.append(newchar);
                }
            }
            return new Saxon.Hej.value.StringValue(builder.toUnicodeString());
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue sv0 = (Saxon.Hej.value.StringValue)(arguments[0].head());
            if ((sv0 == null)) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
            if ((staticMap != null)) {
                return translateUsingMap(sv0, staticMap);
            } else {
                Saxon.Hej.value.StringValue sv1 = (Saxon.Hej.value.StringValue)(arguments[1].head());
                Saxon.Hej.value.StringValue sv2 = (Saxon.Hej.value.StringValue)(arguments[2].head());
                return translate(sv0, sv1, sv2);
            }
        }
        public override string getCompilerName() {
            return "TranslateCompiler";
        }
        public Saxon.Hej.functions.SystemFunction copy() {
            Saxon.Hej.functions.Translate copy = (Saxon.Hej.functions.Translate)(Saxon.Hej.functions.SystemFunction.makeFunction(getFunctionName().getLocalPart(), getRetainedStaticContext(), getArity()));
            copy.staticMap = staticMap;
            return copy;
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.Translate.TranslateFnElaborator();
        }
        public class TranslateFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.Translate fn = (Saxon.Hej.functions.Translate)(fnc.getTargetFunction());
                Saxon.Hej.expr.elab.ItemEvaluator arg0Eval = fnc.getArg(0).makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator arg1Eval = fnc.getArg(1).makeElaborator().elaborateForItem();
                Saxon.Hej.expr.elab.ItemEvaluator arg2Eval = fnc.getArg(2).makeElaborator().elaborateForItem();
                Saxon.Hej.z.IntToIntMap staticMap = fn.getStaticMap();
                if ((staticMap != null)) {
                    return context => {
                        Saxon.Hej.value.StringValue s0 = (Saxon.Hej.value.StringValue)(/*Delegate.eval()*/arg0Eval(context));
                        if (((s0 == null) || s0.isEmpty())) {
                            return Saxon.Hej.value.StringValue.EMPTY_STRING;
                        }
                        return translateUsingMap(s0, staticMap);
                    } ;
                } else {
                    return context => {
                        Saxon.Hej.value.StringValue s0 = (Saxon.Hej.value.StringValue)(/*Delegate.eval()*/arg0Eval(context));
                        if (((s0 == null) || s0.isEmpty())) {
                            return Saxon.Hej.value.StringValue.EMPTY_STRING;
                        }
                        Saxon.Hej.value.StringValue s1 = (Saxon.Hej.value.StringValue)(/*Delegate.eval()*/arg1Eval(context));
                        Saxon.Hej.value.StringValue s2 = (Saxon.Hej.value.StringValue)(/*Delegate.eval()*/arg2Eval(context));
                        return translate(s0, s1, s2);
                    } ;
                }
            }
        
        }
    
    }
}