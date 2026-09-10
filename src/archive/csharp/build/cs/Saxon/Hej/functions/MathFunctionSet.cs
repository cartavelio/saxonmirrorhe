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
        private void reg1(string name, System.Type implementation) {
            register(name, 1, implementation, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, OPT, CARD0).arg(0, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, OPT, EMPTY);
        }
        private void init() {
            register("pi", 0, typeof(Saxon.Hej.functions.MathFunctionSet.PiFn), Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, 0);
            reg1("sin", typeof(Saxon.Hej.functions.MathFunctionSet.SinFn));
            reg1("cos", typeof(Saxon.Hej.functions.MathFunctionSet.CosFn));
            reg1("tan", typeof(Saxon.Hej.functions.MathFunctionSet.TanFn));
            reg1("asin", typeof(Saxon.Hej.functions.MathFunctionSet.AsinFn));
            reg1("acos", typeof(Saxon.Hej.functions.MathFunctionSet.AcosFn));
            reg1("atan", typeof(Saxon.Hej.functions.MathFunctionSet.AtanFn));
            reg1("sqrt", typeof(Saxon.Hej.functions.MathFunctionSet.SqrtFn));
            reg1("log", typeof(Saxon.Hej.functions.MathFunctionSet.LogFn));
            reg1("log10", typeof(Saxon.Hej.functions.MathFunctionSet.Log10Fn));
            reg1("exp", typeof(Saxon.Hej.functions.MathFunctionSet.ExpFn));
            reg1("exp10", typeof(Saxon.Hej.functions.MathFunctionSet.Exp10Fn));
            register("pow", 2, typeof(Saxon.Hej.functions.MathFunctionSet.PowFn), Saxon.Hej.type.BuiltInAtomicType.DOUBLE, OPT, CARD0).arg(0, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, OPT, EMPTY).arg(1, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, null);
            register("atan2", 2, typeof(Saxon.Hej.functions.MathFunctionSet.Atan2Fn), Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, 0).arg(0, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.DOUBLE, ONE, null);
        }
        public override string getNamespace() {
            return Saxon.Hej.lib.NamespaceConstant.MATH;
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
        public abstract class TrigFn1 : Saxon.Hej.functions.SystemFunction {
            protected internal abstract double compute(double input);
            public override Saxon.Hej.om.GroundedValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
                Saxon.Hej.value.DoubleValue @in = (Saxon.Hej.value.DoubleValue)(args[0].head());
                if ((@in == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    return new Saxon.Hej.value.DoubleValue(compute(@in.getDoubleValue()));
                }
            }
        
        }
        public class SinFn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Sin(input);
            }
        
        }
        public class CosFn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Cos(input);
            }
        
        }
        public class TanFn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Tan(input);
            }
        
        }
        public class AsinFn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Asin(input);
            }
        
        }
        public class AcosFn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Acos(input);
            }
        
        }
        public class AtanFn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Atan(input);
            }
        
        }
        public class SqrtFn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Sqrt(input);
            }
        
        }
        public class LogFn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Log(input);
            }
        
        }
        public class Log10Fn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Log10(input);
            }
        
        }
        public class ExpFn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Exp(input);
            }
        
        }
        public class Exp10Fn : Saxon.Hej.functions.MathFunctionSet.TrigFn1 {
            protected internal override double compute(double input) {
                return System.Math.Pow(10, input);
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