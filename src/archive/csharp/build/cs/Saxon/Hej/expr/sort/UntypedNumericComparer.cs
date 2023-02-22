using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class UntypedNumericComparer : Saxon.Hej.expr.sort.AtomicComparer {
        private Saxon.Hej.lib.ConversionRules rules = Saxon.Hej.lib.ConversionRules.DEFAULT;
        private static readonly double[][] bounds = new double[][]{new double[]{1, 0e0, 0e1, 0e2, 0e3, 0e4, 0e5, 0e6, 0e7, 0e8, 0e9, 0e10}, new double[]{1, 1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10}, new double[]{1, 2e0, 2e1, 2e2, 2e3, 2e4, 2e5, 2e6, 2e7, 2e8, 2e9, 2e10}, new double[]{1, 3e0, 3e1, 3e2, 3e3, 3e4, 3e5, 3e6, 3e7, 3e8, 3e9, 3e10}, new double[]{1, 4e0, 4e1, 4e2, 4e3, 4e4, 4e5, 4e6, 4e7, 4e8, 4e9, 4e10}, new double[]{1, 5e0, 5e1, 5e2, 5e3, 5e4, 5e5, 5e6, 5e7, 5e8, 5e9, 5e10}, new double[]{1, 6e0, 6e1, 6e2, 6e3, 6e4, 6e5, 6e6, 6e7, 6e8, 6e9, 6e10}, new double[]{1, 7e0, 7e1, 7e2, 7e3, 7e4, 7e5, 7e6, 7e7, 7e8, 7e9, 7e10}, new double[]{1, 8e0, 8e1, 8e2, 8e3, 8e4, 8e5, 8e6, 8e7, 8e8, 8e9, 8e10}, new double[]{1, 9e0, 9e1, 9e2, 9e3, 9e4, 9e5, 9e6, 9e7, 9e8, 9e9, 9e10}, new double[]{1, 10e0, 10e1, 10e2, 10e3, 10e4, 10e5, 10e6, 10e7, 10e8, 10e9, 10e10}};
        public static bool quickCompare(Saxon.Hej.value.StringValue a0, Saxon.Hej.value.NumericValue a1, int @operator, Saxon.Hej.lib.ConversionRules rules) {
            int comp = quickComparison(a0, a1, rules);
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.FEQ:
                return (comp == 0);
              case Saxon.Hej.expr.parser.Token.FLE:
                return (comp<=0);
              case Saxon.Hej.expr.parser.Token.FLT:
                return (comp<0);
              case Saxon.Hej.expr.parser.Token.FGE:
                return (comp>=0);
              case Saxon.Hej.expr.parser.Token.FGT:
                return (comp>0);
              case Saxon.Hej.expr.parser.Token.FNE:
              default:
                return (comp != 0);
            }
        }
        private static int quickComparison(Saxon.Hej.value.StringValue a0, Saxon.Hej.value.NumericValue a1, Saxon.Hej.lib.ConversionRules rules) {
            double d1 = a1.getDoubleValue();
            Saxon.Hej.str.UnicodeString cs = Saxon.Hej.value.Whitespace.trim(a0.getUnicodeStringValue());
            bool simple = true;
            int wholePartLength = 0;
            int firstDigit = -1;
            int decimalPoints = 0;
            int sign = '?';
            for (int i = 0; (i<cs.length()); i++) {
                int c = cs.codePointAt(i);
                if (((c>='0') && (c<='9'))) {
                    if ((firstDigit<0)) {
                        firstDigit = (c-'0');
                    }
                    if ((decimalPoints == 0)) {
                        wholePartLength++;
                    }
                } else {
                    if ((c == '-')) {
                        if ((((sign != '?') || (wholePartLength>0)) || (decimalPoints>0))) {
                            simple = false;
                            break ;
                        }
                        sign = c;
                    } else {
                        if ((c == '.')) {
                            if ((decimalPoints>0)) {
                                simple = false;
                                break ;
                            }
                            decimalPoints = 1;
                        } else {
                            simple = false;
                            break ;
                        }
                    }
                }
            }
            if ((firstDigit<0)) {
                simple = false;
            }
            if (((simple && (wholePartLength>0)) && (wholePartLength<=10))) {
                double lowerBound = bounds[firstDigit][wholePartLength];
                double upperBound = bounds[(firstDigit+1)][wholePartLength];
                if ((sign == '-')) {
                    double temp = lowerBound;
                    lowerBound = -(upperBound);
                    upperBound = -(temp);
                }
                if ((upperBound<d1)) {
                    return -1;
                }
                if ((lowerBound>d1)) {
                    return +(1);
                }
            }
            if ((((simple && (decimalPoints == 0)) && (wholePartLength<=15)) && (a1 is Saxon.Hej.value.Int64Value))) {
                long l0 = System.Int64.Parse(cs.ToString());
                return l0.CompareTo(a1.longValue());
            } else {
                Saxon.Hej.type.ConversionResult result;
                lock (a0) {
                    result = Saxon.Hej.type.BuiltInAtomicType.DOUBLE.getStringConverter(rules).convertString(a0.getUnicodeStringValue());
                }
                Saxon.Hej.value.AtomicValue av = result.asAtomic();
                return ((Saxon.Hej.value.DoubleValue)(av)).getDoubleValue().CompareTo(d1);
            }
        }
        public int compareAtomicValues(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            try {
                return quickComparison((Saxon.Hej.value.StringValue)(a), (Saxon.Hej.value.NumericValue)(b), rules);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.expr.sort.ComparisonException(e);
            }
        }
        public Saxon.Hej.lib.StringCollator getCollator() {
            return null;
        }
        public Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context) {
            rules = context.getConfiguration().getConversionRules();
            return this;
        }
        public bool comparesEqual(Saxon.Hej.value.AtomicValue a, Saxon.Hej.value.AtomicValue b) {
            return (compareAtomicValues(a, b) == 0);
        }
        public string save() {
            return "QUNC";
        }
    
    }
}