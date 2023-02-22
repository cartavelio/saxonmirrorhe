using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class Cardinality {
        private Cardinality()  {
        }
        public static bool allowsMany(int cardinality) {
            return (((cardinality&Saxon.Hej.expr.StaticProperty.ALLOWS_MANY)) != 0);
        }
        public static bool expectsMany(Saxon.Hej.expr.Expression expression) {
            if ((expression is Saxon.Hej.expr.VariableReference)) {
                Saxon.Hej.expr.Binding b = ((Saxon.Hej.expr.VariableReference)(expression)).getBinding();
                if ((b is Saxon.Hej.expr.LetExpression)) {
                    return expectsMany(((Saxon.Hej.expr.LetExpression)(b)).getSequence());
                }
            }
            if ((expression is Saxon.Hej.expr.Atomizer)) {
                return expectsMany(((Saxon.Hej.expr.Atomizer)(expression)).getBaseExpression());
            }
            if ((expression is Saxon.Hej.expr.FilterExpression)) {
                return expectsMany(((Saxon.Hej.expr.FilterExpression)(expression)).getSelectExpression());
            }
            return allowsMany(expression.getCardinality());
        }
        public static bool allowsZero(int cardinality) {
            return (((cardinality&Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) != 0);
        }
        public static int union(int c1, int c2) {
            int r = (c1|c2);
            if ((r == ((Saxon.Hej.expr.StaticProperty.ALLOWS_MANY|Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)))) {
                r = Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
            }
            return r;
        }
        public static int sum(int c1, int c2) {
            int mini = (min(c1)+min(c2));
            int maxi = (max(c1)+max(c2));
            return fromMinAndMax(mini, maxi);
        }
        static int min(int cardinality) {
            if (allowsZero(cardinality)) {
                return 0;
            } else {
                if ((cardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_MANY)) {
                    return 2;
                } else {
                    return 1;
                }
            }
        }
        static int max(int cardinality) {
            if (allowsMany(cardinality)) {
                return 2;
            } else {
                if ((cardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                    return 0;
                } else {
                    return 1;
                }
            }
        }
        static int fromMinAndMax(int min, int max) {
            bool zero = (min == 0);
            bool one = ((min<=1) || (max<=1));
            bool many = (max>1);
            return ((((zero ? Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO : 0))+((one ? Saxon.Hej.expr.StaticProperty.ALLOWS_ONE : 0)))+((many ? Saxon.Hej.expr.StaticProperty.ALLOWS_MANY : 0)));
        }
        public static bool subsumes(int c1, int c2) {
            return (((c1|c2)) == c1);
        }
        public static int multiply(int c1, int c2) {
            if (((c1 == Saxon.Hej.expr.StaticProperty.EMPTY) || (c2 == Saxon.Hej.expr.StaticProperty.EMPTY))) {
                return Saxon.Hej.expr.StaticProperty.EMPTY;
            }
            if ((c2 == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                return c1;
            }
            if ((c1 == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                return c2;
            }
            if (((c1 == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE) && (c2 == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE))) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            }
            if (((c1 == Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE) && (c2 == Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE))) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE;
            }
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public static string describe(int cardinality) {
            switch (cardinality) {
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE:
                return "zero or one";
              case Saxon.Hej.expr.StaticProperty.EXACTLY_ONE:
                return "exactly one";
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE:
                return "zero or more";
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE:
                return "one or more";
              case Saxon.Hej.expr.StaticProperty.EMPTY:
                return "exactly zero";
              case Saxon.Hej.expr.StaticProperty.ALLOWS_MANY:
                return "more than one";
              default:
                return ("code "+cardinality);
            }
        }
        public static string getOccurrenceIndicator(int cardinality) {
            switch (cardinality) {
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE:
                return "?";
              case Saxon.Hej.expr.StaticProperty.EXACTLY_ONE:
                return "";
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE:
                return "*";
              case Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE:
                return "+";
              case Saxon.Hej.expr.StaticProperty.ALLOWS_MANY:
                return "+";
              case Saxon.Hej.expr.StaticProperty.EMPTY:
                return "0";
              default:
                return "*";
            }
        }
        public static int fromOccurrenceIndicator(string indicator) {
            switch (indicator) {
              case "?":
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
              case "*":
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
              case "+":
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE;
              case "1":
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE;
              case "":
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE;
              case "\u00B0":
              case "0":
              default:
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO;
            }
        }
        public static string generateJavaScriptChecker(int card) {
            if ((Saxon.Hej.value.Cardinality.allowsZero(card) && Saxon.Hej.value.Cardinality.allowsMany(card))) {
                return "function c() {return true;};";
            } else {
                if ((card == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                    return "function c(n) {return n==1;};";
                } else {
                    if ((card == Saxon.Hej.expr.StaticProperty.EMPTY)) {
                        return "function c(n) {return n==0;};";
                    } else {
                        if (!(Saxon.Hej.value.Cardinality.allowsZero(card))) {
                            return "function c(n) {return n>=1;};";
                        } else {
                            return "function c(n) {return n<=1;};";
                        }
                    }
                }
            }
        }
    
    }
}