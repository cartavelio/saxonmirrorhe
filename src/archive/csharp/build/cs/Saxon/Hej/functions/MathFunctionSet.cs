using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class MathFunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Hej.functions.MathFunctionSet THE_INSTANCE = new Saxon.Hej.functions.MathFunctionSet();
        public static Saxon.Hej.functions.MathFunctionSet getInstance() {
            return THE_INSTANCE;
        }
        private MathFunctionSet()  {
            init();
        }
        private void reg1(string name, System.Func<System.Double,System.Double> method) {
            register(name, 1, e => e.populate(() => new Saxon.Hej.functions.MathFunctionSet.TrigFn1(method), Saxon.Hej.type.BuiltInAtomicType.DOUBLE, OPT, CARD0).arg(0, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, OPT, EMPTY));
        }
        private void init() {
            register("pi", 0, e => e.populate(() => new Saxon.Hej.functions.MathFunctionSet.PiFn(), Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, 0));
            reg1("sin", Saxon.Ejava.lang.Math.sin /* known method reference */);
            reg1("cos", Saxon.Ejava.lang.Math.cos /* known method reference */);
            reg1("tan", Saxon.Ejava.lang.Math.tan /* known method reference */);
            reg1("asin", Saxon.Ejava.lang.Math.asin /* known method reference */);
            reg1("acos", Saxon.Ejava.lang.Math.acos /* known method reference */);
            reg1("atan", Saxon.Ejava.lang.Math.atan /* known method reference */);
            reg1("sqrt", Saxon.Ejava.lang.Math.sqrt /* known method reference */);
            reg1("log", Saxon.Ejava.lang.Math.log /* known method reference */);
            reg1("log10", Saxon.Ejava.lang.Math.log10 /* known method reference */);
            reg1("exp", Saxon.Ejava.lang.Math.exp /* known method reference */);
            reg1("exp10", input => System.Math.Pow(10, input));
            register("pow", 2, e => e.populate(() => new Saxon.Hej.functions.MathFunctionSet.PowFn(), Saxon.Hej.type.BuiltInAtomicType.DOUBLE, OPT, CARD0).arg(0, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, OPT, EMPTY).arg(1, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, null));
            register("atan2", 2, e => e.populate(() => new Saxon.Hej.functions.MathFunctionSet.Atan2Fn(), Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, null));
        }
        public override Saxon.Hej.om.NamespaceUri getNamespace() {
            return Saxon.Hej.om.NamespaceUri.MATH;
        }
        public override string getConventionalPrefix() {
            return "math";
        }
        public class PiFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
                return Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.value.DoubleValue(Saxon.Ejava.lang.Math.PI));
            }
            public override Saxon.Hej.value.DoubleValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                return new Saxon.Hej.value.DoubleValue(Saxon.Ejava.lang.Math.PI);
            }
        
        }
        public class TrigFn1 : Saxon.Hej.functions.SystemFunction {
            private readonly System.Func<System.Double,System.Double> method;
            public TrigFn1(System.Func<System.Double,System.Double> method)  {
                this.method = method;
            }
            public override Saxon.Hej.om.GroundedValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
                Saxon.Hej.value.DoubleValue @in = (Saxon.Hej.value.DoubleValue)(args[0].head());
                if ((@in == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    return new Saxon.Hej.value.DoubleValue(method(@in.getDoubleValue()));
                }
            }
        
        }
        public class PowFn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.om.GroundedValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
                Saxon.Hej.value.DoubleValue x = (Saxon.Hej.value.DoubleValue)(args[0].head());
                Saxon.Hej.value.DoubleValue result;
                if ((x == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    double dx = x.getDoubleValue();
                    if ((dx == 1)) {
                        result = x;
                    } else {
                        Saxon.Hej.value.NumericValue y = (Saxon.Hej.value.NumericValue)(args[1].head());
                        System.Diagnostics.Debug.Assert((y != null));
                        double dy = y.getDoubleValue();
                        if (((dx == -1) && System.Double.IsInfinity(dy))) {
                            result = new Saxon.Hej.value.DoubleValue(1.0e0);
                        } else {
                            result = new Saxon.Hej.value.DoubleValue(System.Math.Pow(dx, dy));
                        }
                    }
                    return result;
                }
            }
        
        }
        public class Atan2Fn : Saxon.Hej.functions.SystemFunction {
            public override Saxon.Hej.value.DoubleValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.value.DoubleValue y = (Saxon.Hej.value.DoubleValue)(arguments[0].head());
                System.Diagnostics.Debug.Assert((y != null));
                Saxon.Hej.value.DoubleValue x = (Saxon.Hej.value.DoubleValue)(arguments[1].head());
                System.Diagnostics.Debug.Assert((x != null));
                double result = System.Math.Atan2(y.getDoubleValue(), x.getDoubleValue());
                return new Saxon.Hej.value.DoubleValue(result);
            }
        
        }
    
    }
}