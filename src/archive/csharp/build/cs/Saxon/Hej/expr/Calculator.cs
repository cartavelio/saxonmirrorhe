using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public abstract class Calculator {

        static Calculator() {
            def(Saxon.Hej.om.StandardNames.XS_DOUBLE, Saxon.Hej.om.StandardNames.XS_DOUBLE, DOUBLE_DOUBLE, "DOUBLE_DOUBLE");
            def(Saxon.Hej.om.StandardNames.XS_DOUBLE, Saxon.Hej.om.StandardNames.XS_FLOAT, DOUBLE_FLOAT, "DOUBLE_FLOAT");
            def(Saxon.Hej.om.StandardNames.XS_DOUBLE, Saxon.Hej.om.StandardNames.XS_DECIMAL, DOUBLE_DECIMAL, "DOUBLE_DECIMAL");
            def(Saxon.Hej.om.StandardNames.XS_DOUBLE, Saxon.Hej.om.StandardNames.XS_INTEGER, DOUBLE_INTEGER, "DOUBLE_INTEGER");
            def(Saxon.Hej.om.StandardNames.XS_FLOAT, Saxon.Hej.om.StandardNames.XS_DOUBLE, FLOAT_DOUBLE, "FLOAT_DOUBLE");
            def(Saxon.Hej.om.StandardNames.XS_FLOAT, Saxon.Hej.om.StandardNames.XS_FLOAT, FLOAT_FLOAT, "FLOAT_FLOAT");
            def(Saxon.Hej.om.StandardNames.XS_FLOAT, Saxon.Hej.om.StandardNames.XS_DECIMAL, FLOAT_DECIMAL, "FLOAT_DECIMAL");
            def(Saxon.Hej.om.StandardNames.XS_FLOAT, Saxon.Hej.om.StandardNames.XS_INTEGER, FLOAT_INTEGER, "FLOAT_INTEGER");
            def(Saxon.Hej.om.StandardNames.XS_DECIMAL, Saxon.Hej.om.StandardNames.XS_DOUBLE, DECIMAL_DOUBLE, "DECIMAL_DOUBLE");
            def(Saxon.Hej.om.StandardNames.XS_DECIMAL, Saxon.Hej.om.StandardNames.XS_FLOAT, DECIMAL_FLOAT, "DECIMAL_FLOAT");
            def(Saxon.Hej.om.StandardNames.XS_DECIMAL, Saxon.Hej.om.StandardNames.XS_DECIMAL, DECIMAL_DECIMAL, "DECIMAL_DECIMAL");
            def(Saxon.Hej.om.StandardNames.XS_DECIMAL, Saxon.Hej.om.StandardNames.XS_INTEGER, DECIMAL_INTEGER, "DECIMAL_INTEGER");
            def(Saxon.Hej.om.StandardNames.XS_INTEGER, Saxon.Hej.om.StandardNames.XS_DOUBLE, INTEGER_DOUBLE, "INTEGER_DOUBLE");
            def(Saxon.Hej.om.StandardNames.XS_INTEGER, Saxon.Hej.om.StandardNames.XS_FLOAT, INTEGER_FLOAT, "INTEGER_FLOAT");
            def(Saxon.Hej.om.StandardNames.XS_INTEGER, Saxon.Hej.om.StandardNames.XS_DECIMAL, INTEGER_DECIMAL, "INTEGER_DECIMAL");
            def(Saxon.Hej.om.StandardNames.XS_INTEGER, Saxon.Hej.om.StandardNames.XS_INTEGER, INTEGER_INTEGER, "INTEGER_INTEGER");
            def(Saxon.Hej.om.StandardNames.XS_DATE_TIME, Saxon.Hej.om.StandardNames.XS_DATE_TIME, DATETIME_DATETIME, "DATETIME_DATETIME");
            def(Saxon.Hej.om.StandardNames.XS_DATE_TIME, Saxon.Hej.om.StandardNames.XS_DURATION, DATETIME_DURATION, "DATETIME_DURATION");
            def(Saxon.Hej.om.StandardNames.XS_DURATION, Saxon.Hej.om.StandardNames.XS_DATE_TIME, DURATION_DATETIME, "DURATION_DATETIME");
            def(Saxon.Hej.om.StandardNames.XS_DURATION, Saxon.Hej.om.StandardNames.XS_DURATION, DURATION_DURATION, "DURATION_DURATION");
            def(Saxon.Hej.om.StandardNames.XS_DURATION, Saxon.Hej.om.StandardNames.XS_DOUBLE, DURATION_NUMERIC, "DURATION_NUMERIC");
            def(Saxon.Hej.om.StandardNames.XS_DURATION, Saxon.Hej.om.StandardNames.XS_FLOAT, DURATION_NUMERIC, "DURATION_NUMERIC");
            def(Saxon.Hej.om.StandardNames.XS_DURATION, Saxon.Hej.om.StandardNames.XS_DECIMAL, DURATION_NUMERIC, "DURATION_NUMERIC");
            def(Saxon.Hej.om.StandardNames.XS_DURATION, Saxon.Hej.om.StandardNames.XS_INTEGER, DURATION_NUMERIC, "DURATION_NUMERIC");
            def(Saxon.Hej.om.StandardNames.XS_DOUBLE, Saxon.Hej.om.StandardNames.XS_DURATION, NUMERIC_DURATION, "NUMERIC_DURATION");
            def(Saxon.Hej.om.StandardNames.XS_FLOAT, Saxon.Hej.om.StandardNames.XS_DURATION, NUMERIC_DURATION, "NUMERIC_DURATION");
            def(Saxon.Hej.om.StandardNames.XS_DECIMAL, Saxon.Hej.om.StandardNames.XS_DURATION, NUMERIC_DURATION, "NUMERIC_DURATION");
            def(Saxon.Hej.om.StandardNames.XS_INTEGER, Saxon.Hej.om.StandardNames.XS_DURATION, NUMERIC_DURATION, "NUMERIC_DURATION");
        }
        public const int PLUS = 0;
        public const int MINUS = 1;
        public const int TIMES = 2;
        public const int DIV = 3;
        public const int MOD = 4;
        public const int IDIV = 5;
        public static int getTokenFromOperator(int @operator) {
            return tokens[@operator];
        }
        private static readonly int[] tokens = new int[]{Saxon.Hej.expr.parser.Token.PLUS, Saxon.Hej.expr.parser.Token.MINUS, Saxon.Hej.expr.parser.Token.MULT, Saxon.Hej.expr.parser.Token.DIV, Saxon.Hej.expr.parser.Token.MOD, Saxon.Hej.expr.parser.Token.IDIV};
        public string code() {
            string name = this.GetType().Name;
            return name.Replace("Any", "a").Replace("Double", "d").Replace("Float", "f").Replace("Decimal", "c").Replace("Integer", "i").Replace("Numeric", "n").Replace("DateTime", "t").Replace("Duration", "u").Replace("Plus", "+").Replace("Minus", "-").Replace("Times", "*").Replace("Div", "/").Replace("Idiv", "~").Replace("Mod", "%");
        }
        public readonly static Saxon.Hej.expr.Calculator[] ANY_ANY = {new Saxon.Hej.expr.Calculator.AnyPlusAny(), new Saxon.Hej.expr.Calculator.AnyMinusAny(), new Saxon.Hej.expr.Calculator.AnyTimesAny(), new Saxon.Hej.expr.Calculator.AnyDivAny(), new Saxon.Hej.expr.Calculator.AnyModAny(), new Saxon.Hej.expr.Calculator.AnyIdivAny()};
        public readonly static Saxon.Hej.expr.Calculator[] DOUBLE_DOUBLE = {new Saxon.Hej.expr.Calculator.DoublePlusDouble(), new Saxon.Hej.expr.Calculator.DoubleMinusDouble(), new Saxon.Hej.expr.Calculator.DoubleTimesDouble(), new Saxon.Hej.expr.Calculator.DoubleDivDouble(), new Saxon.Hej.expr.Calculator.DoubleModDouble(), new Saxon.Hej.expr.Calculator.DoubleIdivDouble()};
        public interface DoubleOpDouble {
        
        }
        public readonly static Saxon.Hej.expr.Calculator[] DOUBLE_FLOAT = DOUBLE_DOUBLE;
        public readonly static Saxon.Hej.expr.Calculator[] DOUBLE_DECIMAL = DOUBLE_DOUBLE;
        public readonly static Saxon.Hej.expr.Calculator[] DOUBLE_INTEGER = DOUBLE_DOUBLE;
        public readonly static Saxon.Hej.expr.Calculator[] FLOAT_DOUBLE = DOUBLE_DOUBLE;
        public readonly static Saxon.Hej.expr.Calculator[] FLOAT_FLOAT = {new Saxon.Hej.expr.Calculator.FloatPlusFloat(), new Saxon.Hej.expr.Calculator.FloatMinusFloat(), new Saxon.Hej.expr.Calculator.FloatTimesFloat(), new Saxon.Hej.expr.Calculator.FloatDivFloat(), new Saxon.Hej.expr.Calculator.FloatModFloat(), new Saxon.Hej.expr.Calculator.FloatIdivFloat()};
        public readonly static Saxon.Hej.expr.Calculator[] FLOAT_DECIMAL = FLOAT_FLOAT;
        public readonly static Saxon.Hej.expr.Calculator[] FLOAT_INTEGER = FLOAT_FLOAT;
        public readonly static Saxon.Hej.expr.Calculator[] DECIMAL_DOUBLE = DOUBLE_DOUBLE;
        public readonly static Saxon.Hej.expr.Calculator[] DECIMAL_FLOAT = FLOAT_FLOAT;
        public readonly static Saxon.Hej.expr.Calculator[] DECIMAL_DECIMAL = {new Saxon.Hej.expr.Calculator.DecimalPlusDecimal(), new Saxon.Hej.expr.Calculator.DecimalMinusDecimal(), new Saxon.Hej.expr.Calculator.DecimalTimesDecimal(), new Saxon.Hej.expr.Calculator.DecimalDivDecimal(), new Saxon.Hej.expr.Calculator.DecimalModDecimal(), new Saxon.Hej.expr.Calculator.DecimalIdivDecimal()};
        public readonly static Saxon.Hej.expr.Calculator[] DECIMAL_INTEGER = DECIMAL_DECIMAL;
        public readonly static Saxon.Hej.expr.Calculator[] INTEGER_DOUBLE = DOUBLE_DOUBLE;
        public readonly static Saxon.Hej.expr.Calculator[] INTEGER_FLOAT = FLOAT_FLOAT;
        public readonly static Saxon.Hej.expr.Calculator[] INTEGER_DECIMAL = DECIMAL_DECIMAL;
        public readonly static Saxon.Hej.expr.Calculator[] INTEGER_INTEGER = {new Saxon.Hej.expr.Calculator.IntegerPlusInteger(), new Saxon.Hej.expr.Calculator.IntegerMinusInteger(), new Saxon.Hej.expr.Calculator.IntegerTimesInteger(), new Saxon.Hej.expr.Calculator.IntegerDivInteger(), new Saxon.Hej.expr.Calculator.IntegerModInteger(), new Saxon.Hej.expr.Calculator.IntegerIdivInteger()};
        public readonly static Saxon.Hej.expr.Calculator[] DATETIME_DATETIME = {null, new Saxon.Hej.expr.Calculator.DateTimeMinusDateTime(), null, null, null, null};
        public readonly static Saxon.Hej.expr.Calculator[] DATETIME_DURATION = {new Saxon.Hej.expr.Calculator.DateTimePlusDuration(), new Saxon.Hej.expr.Calculator.DateTimeMinusDuration(), null, null, null, null};
        public readonly static Saxon.Hej.expr.Calculator[] DURATION_DATETIME = {new Saxon.Hej.expr.Calculator.DurationPlusDateTime(), null, null, null, null, null};
        public readonly static Saxon.Hej.expr.Calculator[] DURATION_DURATION = {new Saxon.Hej.expr.Calculator.DurationPlusDuration(), new Saxon.Hej.expr.Calculator.DurationMinusDuration(), null, new Saxon.Hej.expr.Calculator.DurationDivDuration(), null, null};
        public readonly static Saxon.Hej.expr.Calculator[] DURATION_NUMERIC = {null, null, new Saxon.Hej.expr.Calculator.DurationTimesNumeric(), new Saxon.Hej.expr.Calculator.DurationDivNumeric(), null, null};
        public readonly static Saxon.Hej.expr.Calculator[] NUMERIC_DURATION = {null, null, new Saxon.Hej.expr.Calculator.NumericTimesDuration(), null, null, null};
        private static readonly Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.Calculator[]> table = new Saxon.Hej.z.IntHashMap<Saxon.Hej.expr.Calculator[]>(100);
        private static readonly Saxon.Hej.z.IntHashMap<string> nameTable = new Saxon.Hej.z.IntHashMap<string>(100);
        private static void def(int typeA, int typeB, Saxon.Hej.expr.Calculator[] calculatorSet, string setName) {
            int key = ((((typeA&unchecked((int)0xffff)))<<16)|((typeB&unchecked((int)0xffff))));
            table.put(key, calculatorSet);
            nameTable.put(key, setName);
            if ((typeA == Saxon.Hej.om.StandardNames.XS_DURATION)) {
                def(Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION, typeB, calculatorSet, setName);
                def(Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION, typeB, calculatorSet, setName);
            }
            if ((typeB == Saxon.Hej.om.StandardNames.XS_DURATION)) {
                def(typeA, Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION, calculatorSet, setName);
                def(typeA, Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION, calculatorSet, setName);
            }
            if ((typeA == Saxon.Hej.om.StandardNames.XS_DATE_TIME)) {
                def(Saxon.Hej.om.StandardNames.XS_DATE, typeB, calculatorSet, setName);
                def(Saxon.Hej.om.StandardNames.XS_TIME, typeB, calculatorSet, setName);
            }
            if ((typeB == Saxon.Hej.om.StandardNames.XS_DATE_TIME)) {
                def(typeA, Saxon.Hej.om.StandardNames.XS_DATE, calculatorSet, setName);
                def(typeA, Saxon.Hej.om.StandardNames.XS_TIME, calculatorSet, setName);
            }
            if ((typeA == Saxon.Hej.om.StandardNames.XS_DOUBLE)) {
                def(Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC, typeB, calculatorSet, setName);
            }
            if ((typeB == Saxon.Hej.om.StandardNames.XS_DOUBLE)) {
                def(typeA, Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC, calculatorSet, setName);
            }
        }
        public static Saxon.Hej.expr.Calculator getCalculator(int typeA, int typeB, int @operator, bool mustResolve) {
            int key = ((((typeA&unchecked((int)0xffff)))<<16)|((typeB&unchecked((int)0xffff))));
            Saxon.Hej.expr.Calculator[] set = table.get(key);
            if ((set == null)) {
                if (mustResolve) {
                    return null;
                } else {
                    return ANY_ANY[@operator];
                }
            } else {
                return set[@operator];
            }
        }
        public static Saxon.Hej.expr.Calculator reconstructCalculator(string code) {
            int typeA = typeFromCode(code[0]);
            int typeB = typeFromCode(code[2]);
            int @operator = operatorFromCode(code[1]);
            return getCalculator(typeA, typeB, @operator, false);
        }
        private static int typeFromCode(char code) {
            switch (code) {
              case 'a':
                return Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE;
              case 'd':
                return Saxon.Hej.om.StandardNames.XS_DOUBLE;
              case 'i':
                return Saxon.Hej.om.StandardNames.XS_INTEGER;
              case 'f':
                return Saxon.Hej.om.StandardNames.XS_FLOAT;
              case 'c':
                return Saxon.Hej.om.StandardNames.XS_DECIMAL;
              case 'n':
                return Saxon.Hej.om.StandardNames.XS_NUMERIC;
              case 't':
                return Saxon.Hej.om.StandardNames.XS_DATE_TIME;
              case 'u':
                return Saxon.Hej.om.StandardNames.XS_DURATION;
              default:
                throw new Saxon.Ejava.lang.AssertionError();
            }
        }
        public static int operatorFromCode(char code) {
            switch (code) {
              case '+':
                return PLUS;
              case '-':
                return MINUS;
              case '*':
                return TIMES;
              case '/':
                return DIV;
              case '~':
                return IDIV;
              case '%':
                return MOD;
              default:
                throw new Saxon.Ejava.lang.AssertionError();
            }
        }
        public static string getCalculatorSetName(int typeA, int typeB) {
            int key = ((((typeA&unchecked((int)0xffff)))<<16)|((typeB&unchecked((int)0xffff))));
            return nameTable.get(key);
        }
        public abstract Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c);
        public abstract Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB);
        public class AnyPlusAny : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                Saxon.Hej.expr.Calculator calc = getCalculator(a.getItemType().getPrimitiveType(), b.getItemType().getPrimitiveType(), PLUS, true);
                if ((calc == null)) {
                    throw new Saxon.Hej.trans.XPathException((((("Unsuitable types for + operation ("+Saxon.Hej.type.Type.displayTypeName(a))+", ")+Saxon.Hej.type.Type.displayTypeName(b))+")"), "XPTY0004", c);
                } else {
                    return calc.compute(a, b, c);
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
        
        }
        public class AnyMinusAny : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                Saxon.Hej.expr.Calculator calc = getCalculator(a.getItemType().getPrimitiveType(), b.getItemType().getPrimitiveType(), MINUS, true);
                if ((calc == null)) {
                    throw new Saxon.Hej.trans.XPathException((((("Unsuitable types for - operation ("+Saxon.Hej.type.Type.displayTypeName(a))+", ")+Saxon.Hej.type.Type.displayTypeName(b))+")"), "XPTY0004", c);
                } else {
                    return calc.compute(a, b, c);
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
        
        }
        public class AnyTimesAny : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                Saxon.Hej.expr.Calculator calc = getCalculator(a.getItemType().getPrimitiveType(), b.getItemType().getPrimitiveType(), TIMES, true);
                if ((calc == null)) {
                    throw new Saxon.Hej.trans.XPathException((((("Unsuitable types for * operation ("+Saxon.Hej.type.Type.displayTypeName(a))+", ")+Saxon.Hej.type.Type.displayTypeName(b))+")"), "XPTY0004", c);
                } else {
                    return calc.compute(a, b, c);
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
        
        }
        public class AnyDivAny : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                Saxon.Hej.expr.Calculator calc = getCalculator(a.getItemType().getPrimitiveType(), b.getItemType().getPrimitiveType(), DIV, true);
                if ((calc == null)) {
                    throw new Saxon.Hej.trans.XPathException((((("Unsuitable types for div operation ("+Saxon.Hej.type.Type.displayTypeName(a))+", ")+Saxon.Hej.type.Type.displayTypeName(b))+")"), "XPTY0004", c);
                } else {
                    return calc.compute(a, b, c);
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
        
        }
        public class AnyModAny : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                Saxon.Hej.expr.Calculator calc = getCalculator(a.getItemType().getPrimitiveType(), b.getItemType().getPrimitiveType(), MOD, true);
                if ((calc == null)) {
                    throw new Saxon.Hej.trans.XPathException((((("Unsuitable types for mod operation ("+Saxon.Hej.type.Type.displayTypeName(a))+", ")+Saxon.Hej.type.Type.displayTypeName(b))+")"), "XPTY0004", c);
                } else {
                    return calc.compute(a, b, c);
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
        
        }
        public class AnyIdivAny : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                Saxon.Hej.expr.Calculator calc = getCalculator(a.getItemType().getPrimitiveType(), b.getItemType().getPrimitiveType(), IDIV, true);
                if ((calc == null)) {
                    throw new Saxon.Hej.trans.XPathException((((("Unsuitable types for idiv operation ("+Saxon.Hej.type.Type.displayTypeName(a))+", ")+Saxon.Hej.type.Type.displayTypeName(b))+")"), "XPTY0004", c);
                } else {
                    return calc.compute(a, b, c);
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
        
        }
        public class DoublePlusDouble : Saxon.Hej.expr.Calculator, Saxon.Hej.expr.Calculator.DoubleOpDouble {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return new Saxon.Hej.value.DoubleValue((((Saxon.Hej.value.NumericValue)(a)).getDoubleValue()+((Saxon.Hej.value.NumericValue)(b)).getDoubleValue()));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            }
        
        }
        public class DoubleMinusDouble : Saxon.Hej.expr.Calculator, Saxon.Hej.expr.Calculator.DoubleOpDouble {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return new Saxon.Hej.value.DoubleValue((((Saxon.Hej.value.NumericValue)(a)).getDoubleValue()-((Saxon.Hej.value.NumericValue)(b)).getDoubleValue()));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            }
        
        }
        public class DoubleTimesDouble : Saxon.Hej.expr.Calculator, Saxon.Hej.expr.Calculator.DoubleOpDouble {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return new Saxon.Hej.value.DoubleValue((((Saxon.Hej.value.NumericValue)(a)).getDoubleValue()*((Saxon.Hej.value.NumericValue)(b)).getDoubleValue()));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            }
        
        }
        public class DoubleDivDouble : Saxon.Hej.expr.Calculator, Saxon.Hej.expr.Calculator.DoubleOpDouble {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return new Saxon.Hej.value.DoubleValue((((Saxon.Hej.value.NumericValue)(a)).getDoubleValue()/((Saxon.Hej.value.NumericValue)(b)).getDoubleValue()));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            }
        
        }
        public class DoubleModDouble : Saxon.Hej.expr.Calculator, Saxon.Hej.expr.Calculator.DoubleOpDouble {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return new Saxon.Hej.value.DoubleValue((((Saxon.Hej.value.NumericValue)(a)).getDoubleValue()%((Saxon.Hej.value.NumericValue)(b)).getDoubleValue()));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DOUBLE;
            }
        
        }
        private class DoubleIdivDouble : Saxon.Hej.expr.Calculator, Saxon.Hej.expr.Calculator.DoubleOpDouble {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                double A = ((Saxon.Hej.value.NumericValue)(a)).getDoubleValue();
                double B = ((Saxon.Hej.value.NumericValue)(b)).getDoubleValue();
                if ((B == 0.0)) {
                    throw new Saxon.Hej.trans.XPathException("Integer division by zero", "FOAR0001", c);
                }
                if ((System.Double.IsNaN(A) || System.Double.IsInfinity(A))) {
                    throw new Saxon.Hej.trans.XPathException("First operand of idiv is NaN or infinity", "FOAR0002", c);
                }
                if (System.Double.IsNaN(B)) {
                    throw new Saxon.Hej.trans.XPathException("Second operand of idiv is NaN", "FOAR0002", c);
                }
                return Saxon.Hej.value.IntegerValue.fromDouble((A/B)).asAtomic();
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
            }
        
        }
        public class FloatPlusFloat : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return new Saxon.Hej.value.FloatValue((((Saxon.Hej.value.NumericValue)(a)).getFloatValue()+((Saxon.Hej.value.NumericValue)(b)).getFloatValue()));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.FLOAT;
            }
        
        }
        public class FloatMinusFloat : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return new Saxon.Hej.value.FloatValue((((Saxon.Hej.value.NumericValue)(a)).getFloatValue()-((Saxon.Hej.value.NumericValue)(b)).getFloatValue()));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.FLOAT;
            }
        
        }
        public class FloatTimesFloat : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return new Saxon.Hej.value.FloatValue((((Saxon.Hej.value.NumericValue)(a)).getFloatValue()*((Saxon.Hej.value.NumericValue)(b)).getFloatValue()));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.FLOAT;
            }
        
        }
        public class FloatDivFloat : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return new Saxon.Hej.value.FloatValue((((Saxon.Hej.value.NumericValue)(a)).getFloatValue()/((Saxon.Hej.value.NumericValue)(b)).getFloatValue()));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.FLOAT;
            }
        
        }
        public class FloatModFloat : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return new Saxon.Hej.value.FloatValue((((Saxon.Hej.value.NumericValue)(a)).getFloatValue()%((Saxon.Hej.value.NumericValue)(b)).getFloatValue()));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.FLOAT;
            }
        
        }
        public class FloatIdivFloat : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.IntegerValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                float A = ((Saxon.Hej.value.NumericValue)(a)).getFloatValue();
                float B = ((Saxon.Hej.value.NumericValue)(b)).getFloatValue();
                if ((B == 0.0)) {
                    throw new Saxon.Hej.trans.XPathException("Integer division by zero", "FOAR0001", c);
                }
                if ((System.Single.IsNaN(A) || System.Single.IsInfinity(A))) {
                    throw new Saxon.Hej.trans.XPathException("First operand of idiv is NaN or infinity", "FOAR0002", c);
                }
                if (System.Single.IsNaN(B)) {
                    throw new Saxon.Hej.trans.XPathException("Second operand of idiv is NaN", "FOAR0002", c);
                }
                float quotient = (A/B);
                if (System.Single.IsInfinity(quotient)) {
                    return new Saxon.Hej.expr.Calculator.DecimalIdivDecimal().compute(new Saxon.Hej.value.BigDecimalValue(((Saxon.Hej.value.NumericValue)(a)).getDecimalValue()), new Saxon.Hej.value.BigDecimalValue(((Saxon.Hej.value.NumericValue)(b)).getDecimalValue()), c);
                } else {
                    return (Saxon.Hej.value.IntegerValue)(Saxon.Hej.type.Converter.FloatToInteger.INSTANCE.convert(new Saxon.Hej.value.FloatValue(quotient)).asAtomic());
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
            }
        
        }
        public class DecimalPlusDecimal : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                if (((a is Saxon.Hej.value.IntegerValue) && (b is Saxon.Hej.value.IntegerValue))) {
                    return ((Saxon.Hej.value.IntegerValue)(a)).plus((Saxon.Hej.value.IntegerValue)(b));
                } else {
                    return new Saxon.Hej.value.BigDecimalValue((((Saxon.Hej.value.NumericValue)(a)).getDecimalValue()+((Saxon.Hej.value.NumericValue)(b)).getDecimalValue()));
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
            }
        
        }
        public class DecimalMinusDecimal : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                if (((a is Saxon.Hej.value.IntegerValue) && (b is Saxon.Hej.value.IntegerValue))) {
                    return ((Saxon.Hej.value.IntegerValue)(a)).minus((Saxon.Hej.value.IntegerValue)(b));
                } else {
                    return new Saxon.Hej.value.BigDecimalValue((((Saxon.Hej.value.NumericValue)(a)).getDecimalValue()-((Saxon.Hej.value.NumericValue)(b)).getDecimalValue()));
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
            }
        
        }
        public class DecimalTimesDecimal : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                if (((a is Saxon.Hej.value.IntegerValue) && (b is Saxon.Hej.value.IntegerValue))) {
                    return ((Saxon.Hej.value.IntegerValue)(a)).times((Saxon.Hej.value.IntegerValue)(b));
                } else {
                    return new Saxon.Hej.value.BigDecimalValue((((Saxon.Hej.value.NumericValue)(a)).getDecimalValue()*((Saxon.Hej.value.NumericValue)(b)).getDecimalValue()));
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
            }
        
        }
        public class DecimalDivDecimal : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return decimalDivide((Saxon.Hej.value.NumericValue)(a), (Saxon.Hej.value.NumericValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
            }
        
        }
        public static Saxon.Hej.value.BigDecimalValue decimalDivide(Saxon.Hej.value.NumericValue a, Saxon.Hej.value.NumericValue b) {
            if ((b.signum() == 0)) {
                throw new Saxon.Hej.trans.XPathException("Decimal divide by zero", "FOAR0001");
            }
            Singulink.Numerics.BigDecimal A = a.getDecimalValue();
            Singulink.Numerics.BigDecimal B = b.getDecimalValue();
            Singulink.Numerics.BigDecimal result = internalDecimalDivide(A, B);
            return new Saxon.Hej.value.BigDecimalValue(result);
        }
        private static Singulink.Numerics.BigDecimal internalDecimalDivide(Singulink.Numerics.BigDecimal A, Singulink.Numerics.BigDecimal B) {
            return Singulink.Numerics.BigDecimal.Divide(A, B, 18, Singulink.Numerics.RoundingMode.MidpointToZero);
        }
        public class DecimalModDecimal : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                if (((a is Saxon.Hej.value.IntegerValue) && (b is Saxon.Hej.value.IntegerValue))) {
                    return ((Saxon.Hej.value.IntegerValue)(a)).mod((Saxon.Hej.value.IntegerValue)(b));
                }
                if ((((Saxon.Hej.value.NumericValue)(b)).signum() == 0)) {
                    throw new Saxon.Hej.trans.XPathException("Decimal modulo zero", "FOAR0001", c);
                }
                Singulink.Numerics.BigDecimal A = ((Saxon.Hej.value.NumericValue)(a)).getDecimalValue();
                Singulink.Numerics.BigDecimal B = ((Saxon.Hej.value.NumericValue)(b)).getDecimalValue();
                return new Saxon.Hej.value.BigDecimalValue(Saxon.Impl.Helpers.BigDecimalUtils.Remainder(A, B));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
            }
        
        }
        public class DecimalIdivDecimal : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.IntegerValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                if (((a is Saxon.Hej.value.IntegerValue) && (b is Saxon.Hej.value.IntegerValue))) {
                    return ((Saxon.Hej.value.IntegerValue)(a)).idiv((Saxon.Hej.value.IntegerValue)(b));
                }
                Singulink.Numerics.BigDecimal A = ((Saxon.Hej.value.NumericValue)(a)).getDecimalValue();
                Singulink.Numerics.BigDecimal B = ((Saxon.Hej.value.NumericValue)(b)).getDecimalValue();
                if ((B.Sign == 0)) {
                    throw new Saxon.Hej.trans.XPathException("Integer division by zero", "FOAR0001", c);
                }
                System.Numerics.BigInteger quot = ((System.Numerics.BigInteger)Saxon.Impl.Helpers.BigDecimalUtils.Idiv(A, B));
                return Saxon.Hej.value.BigIntegerValue.makeIntegerValue(quot);
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
            }
        
        }
        public class IntegerPlusInteger : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.IntegerValue)(a)).plus((Saxon.Hej.value.IntegerValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
            }
        
        }
        public class IntegerMinusInteger : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.IntegerValue)(a)).minus((Saxon.Hej.value.IntegerValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
            }
        
        }
        public class IntegerTimesInteger : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.IntegerValue)(a)).times((Saxon.Hej.value.IntegerValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
            }
        
        }
        public class IntegerDivInteger : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.IntegerValue)(a)).div((Saxon.Hej.value.IntegerValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
            }
        
        }
        public class IntegerModInteger : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.IntegerValue)(a)).mod((Saxon.Hej.value.IntegerValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
            }
        
        }
        public class IntegerIdivInteger : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.IntegerValue)(a)).idiv((Saxon.Hej.value.IntegerValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.INTEGER;
            }
        
        }
        private class DateTimeMinusDateTime : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.CalendarValue)(a)).subtract((Saxon.Hej.value.CalendarValue)(b), c);
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION;
            }
        
        }
        private class DateTimePlusDuration : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.CalendarValue)(a)).add((Saxon.Hej.value.DurationValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return typeA;
            }
        
        }
        private class DateTimeMinusDuration : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.CalendarValue)(a)).add(((Saxon.Hej.value.DurationValue)(b)).negate());
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return typeA;
            }
        
        }
        private class DurationPlusDateTime : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.CalendarValue)(b)).add((Saxon.Hej.value.DurationValue)(a));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return typeB;
            }
        
        }
        private class DurationPlusDuration : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.DurationValue)(a)).add((Saxon.Hej.value.DurationValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return typeA;
            }
        
        }
        private class DurationMinusDuration : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.DurationValue)(a)).subtract((Saxon.Hej.value.DurationValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return typeA;
            }
        
        }
        private class DurationDivDuration : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                return ((Saxon.Hej.value.DurationValue)(a)).divide((Saxon.Hej.value.DurationValue)(b));
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
            }
        
        }
        private class DurationTimesNumeric : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                if ((b is Saxon.Hej.value.Int64Value)) {
                    return ((Saxon.Hej.value.DurationValue)(a)).multiply(((Saxon.Hej.value.Int64Value)(b)).longValue());
                } else {
                    if ((b is Saxon.Hej.value.DecimalValue)) {
                        return ((Saxon.Hej.value.DurationValue)(a)).multiply(((Saxon.Hej.value.DecimalValue)(b)).getDecimalValue());
                    } else {
                        return ((Saxon.Hej.value.DurationValue)(a)).multiply(((Saxon.Hej.value.NumericValue)(b)).getDoubleValue());
                    }
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return typeA;
            }
        
        }
        private class NumericTimesDuration : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                if ((a is Saxon.Hej.value.Int64Value)) {
                    return ((Saxon.Hej.value.DurationValue)(b)).multiply(((Saxon.Hej.value.Int64Value)(a)).longValue());
                } else {
                    return ((Saxon.Hej.value.DurationValue)(b)).multiply(((Saxon.Hej.value.NumericValue)(a)).getDoubleValue());
                }
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return typeB;
            }
        
        }
        private class DurationDivNumeric : Saxon.Hej.expr.Calculator {
            public override Saxon.Hej.value.AtomicValue compute(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b, Saxon.Hej.expr.XPathContext c) {
                double d = (1.0/((Saxon.Hej.value.NumericValue)(b)).getDoubleValue());
                return ((Saxon.Hej.value.DurationValue)(a)).multiply(d);
            }
            public override Saxon.Hej.type.AtomicType getResultType(Saxon.Hej.type.AtomicType typeA, Saxon.Hej.type.AtomicType typeB) {
                return typeA;
            }
        
        }
    
    }
}