using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class NumberSequenceFormatter : Saxon.Hej.expr.Expression {
        private Saxon.Hej.expr.Operand valueOp;
        private Saxon.Hej.expr.Operand formatOp;
        private Saxon.Hej.expr.Operand groupSizeOp;
        private Saxon.Hej.expr.Operand groupSeparatorOp;
        private Saxon.Hej.expr.Operand letterValueOp;
        private Saxon.Hej.expr.Operand ordinalOp;
        private readonly Saxon.Hej.expr.Operand startAtOp;
        private Saxon.Hej.expr.Operand langOp;
        private Saxon.Hej.expr.number.NumberFormatter formatter = null;
        private Saxon.Hej.lib.Numberer numberer = null;
        private readonly bool backwardsCompatible;
        public NumberSequenceFormatter(Saxon.Hej.expr.Expression value, Saxon.Hej.expr.Expression format, Saxon.Hej.expr.Expression groupSize, Saxon.Hej.expr.Expression groupSeparator, Saxon.Hej.expr.Expression letterValue, Saxon.Hej.expr.Expression ordinal, Saxon.Hej.expr.Expression startAt, Saxon.Hej.expr.Expression lang, Saxon.Hej.expr.number.NumberFormatter formatter, bool backwardsCompatible)  {
            if ((value != null)) {
                valueOp = new Saxon.Hej.expr.Operand(this, value, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            if ((format != null)) {
                formatOp = new Saxon.Hej.expr.Operand(this, format, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            if ((groupSize != null)) {
                groupSizeOp = new Saxon.Hej.expr.Operand(this, groupSize, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            if ((groupSeparator != null)) {
                groupSeparatorOp = new Saxon.Hej.expr.Operand(this, groupSeparator, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            if ((letterValue != null)) {
                letterValueOp = new Saxon.Hej.expr.Operand(this, letterValue, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            if ((ordinal != null)) {
                ordinalOp = new Saxon.Hej.expr.Operand(this, ordinal, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            startAtOp = new Saxon.Hej.expr.Operand(this, startAt, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            if ((lang != null)) {
                langOp = new Saxon.Hej.expr.Operand(this, lang, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            this.formatter = formatter;
            this.backwardsCompatible = backwardsCompatible;
            if (((formatter == null) && (format is Saxon.Hej.expr.StringLiteral))) {
                this.formatter = new Saxon.Hej.expr.number.NumberFormatter();
                this.formatter.prepare(((Saxon.Hej.expr.StringLiteral)(format)).stringify());
            }
        }
        public override Saxon.Hej.expr.Expression simplify() {
            if (((valueOp != null) && !(valueOp.getChildExpression().getItemType().isPlainType()))) {
                valueOp.setChildExpression(Saxon.Hej.expr.Atomizer.makeAtomizer(valueOp.getChildExpression(), null));
            }
            preallocateNumberer(getConfiguration());
            return base.simplify();
        }
        public void preallocateNumberer(Saxon.Hej.Configuration config) {
            if ((langOp == null)) {
                numberer = config.makeNumberer(null, null);
            } else {
                if ((langOp.getChildExpression() is Saxon.Hej.expr.StringLiteral)) {
                    string language = ((Saxon.Hej.expr.StringLiteral)(langOp.getChildExpression())).stringify();
                    if (!(language.Length==0)) {
                        Saxon.Hej.type.ValidationFailure vf = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE.validate(Saxon.Hej.str.StringView.tidy(language));
                        if ((vf != null)) {
                            langOp.setChildExpression(new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING));
                            throw new Saxon.Hej.trans.XPathException("The lang attribute must be a valid language code", "XTDE0030");
                        }
                    }
                    numberer = config.makeNumberer(language, null);
                }
            }
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandSparseList(valueOp, formatOp, groupSizeOp, groupSeparatorOp, letterValueOp, ordinalOp, startAtOp, langOp);
        }
        private bool isFixed(Saxon.Hej.expr.Operand op) {
            return ((op == null) || (op.getChildExpression() is Saxon.Hej.expr.Literal));
        }
        private bool hasFixedOperands() {
            foreach (Saxon.Hej.expr.Operand o in operands()) {
                if (!(isFixed(o))) {
                    return false;
                }
            }
            return true;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            optimizeChildren(visitor, contextInfo);
            if (hasFixedOperands()) {
                Saxon.Hej.value.StringValue val = evaluateItem(visitor.makeDynamicContext());
                Saxon.Hej.expr.StringLiteral literal = new Saxon.Hej.expr.StringLiteral(val);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, literal);
                return literal;
            } else {
                return this;
            }
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.NumberSequenceFormatter exp = new Saxon.Hej.expr.NumberSequenceFormatter(copy(valueOp, rebindings), copy(formatOp, rebindings), copy(groupSizeOp, rebindings), copy(groupSeparatorOp, rebindings), copy(letterValueOp, rebindings), copy(ordinalOp, rebindings), copy(startAtOp, rebindings), copy(langOp, rebindings), formatter, backwardsCompatible);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        private Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.Operand op, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return ((op == null) ? null : op.getChildExpression().copy(rebindings));
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.STRING;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override Saxon.Hej.value.StringValue evaluateItem(Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IList<System.Object> vec = new System.Collections.Generic.List<System.Object>(4);
            Saxon.Hej.lib.ConversionRules rules = context.getConfiguration().getConversionRules();
            string startAv = startAtOp.getChildExpression().evaluateAsString(context).ToString();
            System.Collections.Generic.IList<int> startValues = parseStartAtValue(startAv);
            Saxon.Hej.om.SequenceIterator iter = valueOp.getChildExpression().iterate(context);
            Saxon.Hej.value.AtomicValue val;
            int pos = 0;
            while (((val = (Saxon.Hej.value.AtomicValue)(iter.next())) != null)) {
                if ((backwardsCompatible && !(vec.Count==0))) {
                    break ;
                }
                int startValue = ((startValues.Count>pos) ? startValues[pos] : startValues[(startValues.Count-1)]);
                pos++;
                try {
                    Saxon.Hej.value.NumericValue num;
                    if ((val is Saxon.Hej.value.NumericValue)) {
                        num = (Saxon.Hej.value.NumericValue)(val);
                    } else {
                        num = Saxon.Hej.functions.Number_1.convert(val, context.getConfiguration());
                    }
                    if (num.isNaN()) {
                        throw new Saxon.Hej.trans.XPathException("NaN");
                    }
                    num = num.round(0);
                    if ((num.compareTo(Saxon.Hej.value.Int64Value.MAX_LONG)>0)) {
                        System.Numerics.BigInteger bi = ((Saxon.Hej.value.BigIntegerValue)(Saxon.Hej.type.Converter.convert(num, Saxon.Hej.type.BuiltInAtomicType.INTEGER, rules).asAtomic())).asBigInteger();
                        if ((startValue != 1)) {
                            bi = System.Numerics.BigInteger.Add(bi, Saxon.Impl.Helpers.BigIntegerUtils.ValueOf((startValue-1)));
                        }
                        vec.Add(bi);
                    } else {
                        if ((num.compareTo(Saxon.Hej.value.Int64Value.ZERO)<0)) {
                            throw new Saxon.Hej.trans.XPathException("The numbers to be formatted must not be negative");
                        }
                        long i = ((Saxon.Hej.value.NumericValue)(Saxon.Hej.type.Converter.convert(num, Saxon.Hej.type.BuiltInAtomicType.INTEGER, rules).asAtomic())).longValue();
                        i += (startValue-1);
                        vec.Add(i);
                    }
                } catch (Saxon.Hej.trans.XPathException err) {
                    if (backwardsCompatible) {
                        vec.Add("NaN");
                    } else {
                        vec.Add(val.getUnicodeStringValue());
                        Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(("Cannot convert supplied value to an integer. "+err.Message));
                        e.setErrorCode("XTDE0980");
                        e.setLocation(getLocation());
                        e.setXPathContext(context);
                        throw e;
                    }
                }
            }
            if ((backwardsCompatible && vec.Count==0)) {
                vec.Add("NaN");
            }
            int gpsize = 0;
            string gpseparator = "";
            string letterVal;
            string ordinalVal = null;
            if ((groupSizeOp != null)) {
                string g = groupSizeOp.getChildExpression().evaluateAsString(context).ToString();
                try {
                    gpsize = System.Int32.Parse(g);
                } catch (Saxon.Ejava.lang.NumberFormatException err) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("grouping-size must be numeric");
                    e.setXPathContext(context);
                    e.setErrorCode("XTDE0030");
                    e.setLocation(getLocation());
                    throw e;
                }
            }
            if ((groupSeparatorOp != null)) {
                gpseparator = groupSeparatorOp.getChildExpression().evaluateAsString(context).ToString();
            }
            if ((ordinalOp != null)) {
                ordinalVal = ordinalOp.getChildExpression().evaluateAsString(context).ToString();
            }
            Saxon.Hej.lib.Numberer numb = numberer;
            if ((numb == null)) {
                if ((langOp == null)) {
                    numb = context.getConfiguration().makeNumberer(null, null);
                } else {
                    string language = langOp.getChildExpression().evaluateAsString(context).ToString();
                    Saxon.Hej.type.ValidationFailure vf = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE.validate(Saxon.Hej.str.StringView.tidy(language));
                    if ((vf != null)) {
                        throw new Saxon.Hej.trans.XPathException("The lang attribute of xsl:number must be a valid language code", "XTDE0030");
                    }
                    numb = context.getConfiguration().makeNumberer(language, null);
                }
            }
            if ((letterValueOp == null)) {
                letterVal = "";
            } else {
                letterVal = letterValueOp.getChildExpression().evaluateAsString(context).ToString();
                if (!((("alphabetic".Equals(letterVal) || "traditional".Equals(letterVal))))) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("letter-value must be \"traditional\" or \"alphabetic\"");
                    e.setXPathContext(context);
                    e.setErrorCode("XTDE0030");
                    e.setLocation(getLocation());
                    throw e;
                }
            }
            Saxon.Hej.expr.number.NumberFormatter nf;
            if ((formatter == null)) {
                nf = new Saxon.Hej.expr.number.NumberFormatter();
                nf.prepare(formatOp.getChildExpression().evaluateAsString(context).ToString());
            } else {
                nf = formatter;
            }
            Saxon.Hej.str.UnicodeString s = nf.format(vec, gpsize, gpseparator, letterVal, ordinalVal, numb);
            return new Saxon.Hej.value.StringValue(s);
        }
        public System.Collections.Generic.IList<int> parseStartAtValue(string value) {
            System.Collections.Generic.IList<int> list = new System.Collections.Generic.List<int>();
            string[] tokens = System.Text.RegularExpressions.Regex.Split(value, "\\s+");
            foreach (string tok in tokens) {
                try {
                    int n = System.Int32.Parse(tok);
                    list.Add(n);
                } catch (Saxon.Ejava.lang.NumberFormatException err) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("Invalid start-at value: non-integer component {"+tok)+"}"));
                    e.setErrorCode("XTDE0030");
                    e.setLocation(getLocation());
                    throw e;
                }
            }
            if (list.Count==0) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Invalid start-at value: no numeric components found");
                e.setErrorCode("XTDE0030");
                e.setLocation(getLocation());
                throw e;
            }
            return list;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("numSeqFmt", this);
            string flags = "";
            if (backwardsCompatible) {
                flags += "1";
            }
            if (!(flags.Length==0)) {
                @out.emitAttribute("flags", flags);
            }
            if ((valueOp != null)) {
                @out.setChildRole("value");
                valueOp.getChildExpression().export(@out);
            }
            if ((formatOp != null)) {
                @out.setChildRole("format");
                formatOp.getChildExpression().export(@out);
            }
            if ((startAtOp != null)) {
                @out.setChildRole("startAt");
                startAtOp.getChildExpression().export(@out);
            }
            if ((langOp != null)) {
                @out.setChildRole("lang");
                langOp.getChildExpression().export(@out);
            }
            if ((ordinalOp != null)) {
                @out.setChildRole("ordinal");
                ordinalOp.getChildExpression().export(@out);
            }
            if ((groupSeparatorOp != null)) {
                @out.setChildRole("gpSep");
                groupSeparatorOp.getChildExpression().export(@out);
            }
            if ((groupSizeOp != null)) {
                @out.setChildRole("gpSize");
                groupSizeOp.getChildExpression().export(@out);
            }
            @out.endElement();
        }
    
    }
}