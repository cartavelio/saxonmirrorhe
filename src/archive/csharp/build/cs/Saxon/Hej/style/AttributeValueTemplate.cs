using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class AttributeValueTemplate {
        private AttributeValueTemplate()  {
        }
        public static Saxon.Hej.expr.Expression make(string avt, Saxon.Hej.expr.StaticContext env) {
            int languageLevel = env.getXPathVersion();
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> components = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(5);
            int i0, i1, i8, i9;
            int len = avt.Length;
            int last = 0;
            while ((last<len)) {
                i0 = avt.IndexOf("{", last);
                i1 = avt.IndexOf("{{", last);
                i8 = avt.IndexOf("}", last);
                i9 = avt.IndexOf("}}", last);
                if (((((i0<0) || (len<i0))) && (((i8<0) || (len<i8))))) {
                    addStringComponent(components, avt, last, len);
                    break ;
                } else {
                    if (((i8>=0) && (((i0<0) || (i8<i0))))) {
                        if ((i8 != i9)) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Closing curly brace in attribute value template \""+avt.Substring(0, len - 0))+"\" must be doubled"));
                            err.setErrorCode("XTSE0370");
                            err.setIsStaticError(true);
                            throw err;
                        }
                        addStringComponent(components, avt, last, (i8+1));
                        last = (i8+2);
                    } else {
                        if (((i1>=0) && (i1 == i0))) {
                            addStringComponent(components, avt, last, (i1+1));
                            last = (i1+2);
                        } else {
                            if ((i0>=0)) {
                                if ((i0>last)) {
                                    addStringComponent(components, avt, last, i0);
                                }
                                Saxon.Hej.expr.Expression exp;
                                Saxon.Hej.expr.parser.XPathParser parser = env.getConfiguration().newExpressionParser("XP", false, languageLevel);
                                parser.setLanguage(Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XPATH, 31);
                                parser.setAllowAbsentExpression(true);
                                exp = parser.parse(avt, (i0+1), Saxon.Hej.expr.parser.Token.RCURLY, env);
                                exp.setRetainedStaticContext(env.makeRetainedStaticContext());
                                exp = exp.simplify();
                                last = (parser.getTokenizer().currentTokenStartOffset+1);
                                if ((((env is Saxon.Hej.style.ExpressionContext) && (((Saxon.Hej.style.ExpressionContext)(env)).getStyleElement() is Saxon.Hej.style.XSLAnalyzeString)) && isIntegerOrIntegerPair(exp))) {
                                    env.issueWarning((((("Found {"+showIntegers(exp))+"} in regex attribute: perhaps {{")+showIntegers(exp))+"}} was intended? (The attribute is an AVT, so curly braces should be doubled)"), exp.getLocation());
                                }
                                if (env.isInBackwardsCompatibleMode()) {
                                    components.Add(makeFirstItem(exp, env));
                                } else {
                                    components.Add(Saxon.Hej.style.XSLLeafNodeConstructor.makeSimpleContentConstructor(exp, new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.SINGLE_SPACE), env).simplify());
                                }
                            } else {
                                throw new Saxon.Ejava.lang.IllegalStateException("Internal error parsing AVT");
                            }
                        }
                    }
                }
            }
            Saxon.Hej.expr.Expression result;
            if (components.Count==0) {
                result = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
            } else {
                if ((components.Count == 1)) {
                    result = components[0].simplify();
                } else {
                    Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[components.Count];
                    args = components.ToArray();
                    Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall("concat", new Saxon.Hej.expr.parser.RetainedStaticContext(env), args);
                    result = fn.simplify();
                }
            }
            result.setLocation(env.getContainingLocation());
            return result;
        }
        private static bool isIntegerOrIntegerPair(Saxon.Hej.expr.Expression exp) {
            if ((exp is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue val = ((Saxon.Hej.expr.Literal)(exp)).getGroundedValue();
                if ((val is Saxon.Hej.value.IntegerValue)) {
                    return true;
                }
                if ((val.getLength() == 2)) {
                    return ((val.itemAt(0) is Saxon.Hej.value.IntegerValue) && (val.itemAt(1) is Saxon.Hej.value.IntegerValue));
                }
            }
            return false;
        }
        private static string showIntegers(Saxon.Hej.expr.Expression exp) {
            if ((exp is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue val = ((Saxon.Hej.expr.Literal)(exp)).getGroundedValue();
                if ((val is Saxon.Hej.value.IntegerValue)) {
                    return val.ToString();
                }
                if ((val.getLength() == 2)) {
                    if (((val.itemAt(0) is Saxon.Hej.value.IntegerValue) && (val.itemAt(1) is Saxon.Hej.value.IntegerValue))) {
                        return ((val.itemAt(0).ToString()+",")+val.itemAt(1).ToString());
                    }
                }
            }
            return "";
        }
        private static void addStringComponent(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> components, string avt, int start, int end) {
            if ((start<end)) {
                components.Add(new Saxon.Hej.expr.StringLiteral(avt.Substring(start, end - start)));
            }
        }
        public static Saxon.Hej.expr.Expression makeFirstItem(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.StaticContext env) {
            if (Saxon.Hej.expr.Literal.isEmptySequence(exp)) {
                return exp;
            }
            Saxon.Hej.type.TypeHierarchy th = env.getConfiguration().getTypeHierarchy();
            if (!(exp.getItemType().isPlainType())) {
                exp = Saxon.Hej.expr.Atomizer.makeAtomizer(exp, null);
            }
            if (Saxon.Hej.value.Cardinality.allowsMany(exp.getCardinality())) {
                exp = Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(exp);
            }
            if (!(th.isSubType(exp.getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING))) {
                exp = new Saxon.Hej.expr.AtomicSequenceConverter(exp, Saxon.Hej.type.BuiltInAtomicType.STRING);
                ((Saxon.Hej.expr.AtomicSequenceConverter)(exp)).allocateConverterStatically(env.getConfiguration(), false);
            }
            return exp;
        }
    
    }
}