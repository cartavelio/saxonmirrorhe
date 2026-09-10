using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    #pragma warning disable 659
    public class SortKeyDefinition : Saxon.Hej.expr.PseudoExpression {
        protected internal Saxon.Hej.expr.Operand sortKey;
        protected internal Saxon.Hej.expr.Operand order;
        protected internal Saxon.Hej.expr.Operand dataTypeExpression = null;
        protected internal Saxon.Hej.expr.Operand caseOrder;
        protected internal Saxon.Hej.expr.Operand language;
        protected internal Saxon.Hej.expr.Operand collationName = null;
        protected internal Saxon.Hej.expr.Operand stable = null;
        protected internal Saxon.Hej.lib.StringCollator collation;
        protected internal string baseURI;
        protected internal bool emptyLeast = true;
        protected internal bool backwardsCompatible = false;
        protected internal bool setContextForSortKey = false;
        private Saxon.Hej.expr.sort.AtomicComparer finalComparator = null;
        public SortKeyDefinition()  {
            order = new Saxon.Hej.expr.Operand(this, new Saxon.Hej.expr.StringLiteral("ascending"), Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            caseOrder = new Saxon.Hej.expr.Operand(this, new Saxon.Hej.expr.StringLiteral("#default"), Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            language = new Saxon.Hej.expr.Operand(this, new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING), Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public override bool isLiftable(bool forStreaming) {
            return false;
        }
        public void setSortKey(Saxon.Hej.expr.Expression exp, bool setContext) {
            Saxon.Hej.expr.OperandRole opRole;
            if (setContext) {
                opRole = new Saxon.Hej.expr.OperandRole((Saxon.Hej.expr.OperandRole.HAS_SPECIAL_FOCUS_RULES|Saxon.Hej.expr.OperandRole.HIGHER_ORDER), Saxon.Hej.expr.OperandUsage.TRANSMISSION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            } else {
                opRole = Saxon.Hej.expr.OperandRole.ATOMIC_SEQUENCE;
            }
            sortKey = new Saxon.Hej.expr.Operand(this, exp, opRole);
            setContextForSortKey = setContext;
        }
        public Saxon.Hej.expr.Expression getSortKey() {
            return sortKey.getChildExpression();
        }
        public Saxon.Hej.expr.Operand getSortKeyOperand() {
            return sortKey;
        }
        public bool isSetContextForSortKey() {
            return setContextForSortKey;
        }
        public void setOrder(Saxon.Hej.expr.Expression exp) {
            order.setChildExpression(exp);
        }
        public Saxon.Hej.expr.Expression getOrder() {
            return order.getChildExpression();
        }
        public void setDataTypeExpression(Saxon.Hej.expr.Expression exp) {
            if ((exp == null)) {
                dataTypeExpression = null;
            } else {
                if ((dataTypeExpression == null)) {
                    dataTypeExpression = new Saxon.Hej.expr.Operand(this, exp, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
                }
                dataTypeExpression.setChildExpression(exp);
            }
        }
        public Saxon.Hej.expr.Expression getDataTypeExpression() {
            return ((dataTypeExpression == null) ? null : dataTypeExpression.getChildExpression());
        }
        public void setCaseOrder(Saxon.Hej.expr.Expression exp) {
            caseOrder.setChildExpression(exp);
        }
        public Saxon.Hej.expr.Expression getCaseOrder() {
            return caseOrder.getChildExpression();
        }
        public void setLanguage(Saxon.Hej.expr.Expression exp) {
            language.setChildExpression(exp);
        }
        public Saxon.Hej.expr.Expression getLanguage() {
            return language.getChildExpression();
        }
        public void setCollationNameExpression(Saxon.Hej.expr.Expression collationNameExpr) {
            if ((collationNameExpr == null)) {
                collationName = null;
            } else {
                if ((collationName == null)) {
                    collationName = new Saxon.Hej.expr.Operand(this, collationNameExpr, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
                }
                collationName.setChildExpression(collationNameExpr);
            }
        }
        public Saxon.Hej.expr.Expression getCollationNameExpression() {
            return ((collationName == null) ? null : collationName.getChildExpression());
        }
        public void setCollation(Saxon.Hej.lib.StringCollator collation) {
            this.collation = collation;
        }
        public Saxon.Hej.lib.StringCollator getCollation() {
            return collation;
        }
        public void setBaseURI(string baseURI) {
            this.baseURI = baseURI;
        }
        public string getBaseURI() {
            return baseURI;
        }
        public void setStable(Saxon.Hej.expr.Expression stableExpr) {
            if ((stableExpr == null)) {
                stableExpr = new Saxon.Hej.expr.StringLiteral("yes");
            }
            if ((stable == null)) {
                stable = new Saxon.Hej.expr.Operand(this, stableExpr, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            stable.setChildExpression(stableExpr);
        }
        public Saxon.Hej.expr.Expression getStable() {
            return stable.getChildExpression();
        }
        public void setBackwardsCompatible(bool compatible) {
            backwardsCompatible = compatible;
        }
        public bool isBackwardsCompatible() {
            return backwardsCompatible;
        }
        public void setEmptyLeast(bool emptyLeast) {
            this.emptyLeast = emptyLeast;
        }
        public bool getEmptyLeast() {
            return emptyLeast;
        }
        public bool isFixed() {
            return ((((((order.getChildExpression() is Saxon.Hej.expr.Literal) && (((dataTypeExpression == null) || (dataTypeExpression.getChildExpression() is Saxon.Hej.expr.Literal)))) && (caseOrder.getChildExpression() is Saxon.Hej.expr.Literal)) && (language.getChildExpression() is Saxon.Hej.expr.Literal)) && (((stable == null) || (stable.getChildExpression() is Saxon.Hej.expr.Literal)))) && (((collationName == null) || (collationName.getChildExpression() is Saxon.Hej.expr.Literal))));
        }
        public override Saxon.Hej.expr.sort.SortKeyDefinition copy(Saxon.Hej.expr.parser.RebindingMap rm) {
            Saxon.Hej.expr.sort.SortKeyDefinition sk2 = new Saxon.Hej.expr.sort.SortKeyDefinition();
            sk2.setSortKey(copy(sortKey.getChildExpression(), rm), true);
            sk2.setOrder(copy(order.getChildExpression(), rm));
            sk2.setDataTypeExpression(((dataTypeExpression == null) ? null : copy(dataTypeExpression.getChildExpression(), rm)));
            sk2.setCaseOrder(copy(caseOrder.getChildExpression(), rm));
            sk2.setLanguage(copy(language.getChildExpression(), rm));
            sk2.setStable(copy(((stable == null) ? null : stable.getChildExpression()), rm));
            sk2.setCollationNameExpression(((collationName == null) ? null : copy(collationName.getChildExpression(), rm)));
            sk2.collation = collation;
            sk2.emptyLeast = emptyLeast;
            sk2.baseURI = baseURI;
            sk2.backwardsCompatible = backwardsCompatible;
            sk2.finalComparator = finalComparator;
            sk2.setContextForSortKey = setContextForSortKey;
            return sk2;
        }
        private Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.Expression @in, Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return ((@in == null) ? null : @in.copy(rebindings));
        }
        public override Saxon.Hej.expr.sort.SortKeyDefinition typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            foreach (Saxon.Hej.expr.Operand o in checkedOperands()) {
                if (o.hasSameFocus()) {
                    o.typeCheck(visitor, contextItemType);
                }
            }
            Saxon.Hej.expr.Expression lang = getLanguage();
            if (((lang is Saxon.Hej.expr.StringLiteral) && !(((Saxon.Hej.expr.StringLiteral)(lang)).getString().isEmpty()))) {
                Saxon.Hej.type.ValidationFailure vf = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE.validate(((Saxon.Hej.expr.StringLiteral)(lang)).getGroundedValue().getUnicodeStringValue());
                if ((vf != null)) {
                    throw new Saxon.Hej.trans.XPathException("The lang attribute of xsl:sort must be a valid language code", "XTDE0030");
                }
            }
            return this;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.IList<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(8);
            list.Add(sortKey);
            list.Add(order);
            if ((dataTypeExpression != null)) {
                list.Add(dataTypeExpression);
            }
            list.Add(caseOrder);
            list.Add(language);
            if ((stable != null)) {
                list.Add(stable);
            }
            if ((collationName != null)) {
                list.Add(collationName);
            }
            return list;
        }
        public override int getImplementationMethod() {
            return 0;
        }
        public Saxon.Hej.expr.sort.AtomicComparer makeComparator(Saxon.Hej.expr.XPathContext context) {
            string orderX = order.getChildExpression().evaluateAsString(context).ToString();
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.expr.sort.AtomicComparer atomicComparer;
            Saxon.Hej.lib.StringCollator stringCollator;
            if ((collation != null)) {
                stringCollator = collation;
            } else {
                if ((collationName != null)) {
                    string cname = collationName.getChildExpression().evaluateAsString(context).ToString();
                    System.Uri collationURI;
                    try {
                        collationURI = new System.Uri(cname, System.UriKind.RelativeOrAbsolute);
                        if (!(collationURI.IsAbsoluteUri)) {
                            if ((baseURI == null)) {
                                throw new Saxon.Hej.trans.XPathException("Collation URI is relative, and base URI is unknown");
                            } else {
                                System.Uri @base = new System.Uri(baseURI, System.UriKind.RelativeOrAbsolute);
                                collationURI = new System.Uri(@base, collationURI);
                            }
                        }
                    } catch (System.UriFormatException err) {
                        throw new Saxon.Hej.trans.XPathException(((("Collation name "+cname)+" is not a valid URI: ")+err));
                    }
                    stringCollator = context.getConfiguration().getCollation(collationURI.ToString());
                    if ((stringCollator == null)) {
                        throw new Saxon.Hej.trans.XPathException(("Unknown collation "+collationURI), "XTDE1035");
                    }
                } else {
                    string caseOrderX = caseOrder.getChildExpression().evaluateAsString(context).ToString();
                    string languageX = language.getChildExpression().evaluateAsString(context).ToString();
                    string uri = "http://saxon.sf.net/collation";
                    bool firstParam = true;
                    Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
                    if (!(languageX.Length==0)) {
                        Saxon.Hej.type.ValidationFailure vf = Saxon.Hej.type.StringConverter.StringToLanguage.INSTANCE.validate(Saxon.Hej.str.StringView.of(languageX).tidy());
                        if ((vf != null)) {
                            throw new Saxon.Hej.trans.XPathException("The lang attribute of xsl:sort must be a valid language code", "XTDE0030");
                        }
                        props.setProperty("lang", languageX);
                        uri += ("?lang="+languageX);
                        firstParam = false;
                    }
                    if (!(caseOrderX.Equals("#default"))) {
                        props.setProperty("case-order", caseOrderX);
                        uri += ((((firstParam ? "?" : ";"))+"case-order=")+caseOrderX);
                        firstParam = false;
                    }
                    stringCollator = Saxon.Hej.Version.platform.makeCollation(config, props, uri);
                }
            }
            if ((dataTypeExpression == null)) {
                atomicComparer = Saxon.Hej.expr.sort.AtomicSortComparer.makeSortComparer(stringCollator, sortKey.getChildExpression().getItemType().getAtomizedItemType().getPrimitiveType(), context);
                if (!(emptyLeast)) {
                    atomicComparer = new Saxon.Hej.expr.sort.EmptyGreatestComparer(atomicComparer);
                }
            } else {
                string dataType = dataTypeExpression.getChildExpression().evaluateAsString(context).ToString();
                switch (dataType) {
                  case "text":
                    atomicComparer = Saxon.Hej.expr.sort.AtomicSortComparer.makeSortComparer(stringCollator, Saxon.Hej.om.StandardNames.XS_STRING, context);
                    atomicComparer = new Saxon.Hej.expr.sort.TextComparer(atomicComparer);
                    break ;
                  case "number":
                    atomicComparer = ((context.getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10) ? Saxon.Hej.expr.sort.NumericComparer.getInstance() : Saxon.Hej.expr.sort.NumericComparer11.getInstance());
                    break ;
                  default:
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("data-type on xsl:sort must be 'text' or 'number'");
                    err.setErrorCode("XTDE0030");
                    throw err;
                }
            }
            if ((stable != null)) {
                Saxon.Hej.value.StringValue stableVal = (Saxon.Hej.value.StringValue)(stable.getChildExpression().evaluateItem(context));
                string s = Saxon.Hej.value.Whitespace.trim(stableVal.getStringValue());
                if ((((((s.Equals("yes") || s.Equals("no")) || s.Equals("true")) || s.Equals("false")) || s.Equals("1")) || s.Equals("0"))) {
                } else {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Value of 'stable' on xsl:sort must be yes|no|true|false|1|0");
                    err.setErrorCode("XTDE0030");
                    throw err;
                }
            }
            switch (orderX) {
              case "ascending":
                return atomicComparer;
              case "descending":
                return new Saxon.Hej.expr.sort.DescendingComparer(atomicComparer);
              default:
                Saxon.Hej.trans.XPathException err1 = new Saxon.Hej.trans.XPathException("order must be 'ascending' or 'descending'");
                err1.setErrorCode("XTDE0030");
                throw err1;
            }
        }
        public void setFinalComparator(Saxon.Hej.expr.sort.AtomicComparer comp) {
            finalComparator = comp;
        }
        public Saxon.Hej.expr.sort.AtomicComparer getFinalComparator() {
            return finalComparator;
        }
        public Saxon.Hej.expr.sort.SortKeyDefinition fix(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.sort.SortKeyDefinition newSKD = this.copy(new Saxon.Hej.expr.parser.RebindingMap());
            newSKD.setLanguage(new Saxon.Hej.expr.StringLiteral(this.getLanguage().evaluateAsString(context)));
            newSKD.setOrder(new Saxon.Hej.expr.StringLiteral(this.getOrder().evaluateAsString(context)));
            if ((collationName != null)) {
                newSKD.setCollationNameExpression(new Saxon.Hej.expr.StringLiteral(this.getCollationNameExpression().evaluateAsString(context)));
            }
            newSKD.setCaseOrder(new Saxon.Hej.expr.StringLiteral(this.getCaseOrder().evaluateAsString(context)));
            if ((dataTypeExpression != null)) {
                newSKD.setDataTypeExpression(new Saxon.Hej.expr.StringLiteral(this.getDataTypeExpression().evaluateAsString(context)));
            }
            newSKD.setSortKey(new Saxon.Hej.expr.ContextItemExpression(), true);
            return newSKD;
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.expr.sort.SortKeyDefinition)) {
                Saxon.Hej.expr.sort.SortKeyDefinition s2 = (Saxon.Hej.expr.sort.SortKeyDefinition)(other);
                return (((((Saxon.Ejava.util.Objects.Equals(getSortKey(), s2.getSortKey()) && Saxon.Ejava.util.Objects.Equals(getOrder(), s2.getOrder())) && Saxon.Ejava.util.Objects.Equals(getLanguage(), s2.getLanguage())) && Saxon.Ejava.util.Objects.Equals(getDataTypeExpression(), s2.getDataTypeExpression())) && Saxon.Ejava.util.Objects.Equals(getStable(), s2.getStable())) && Saxon.Ejava.util.Objects.Equals(getCollationNameExpression(), s2.getCollationNameExpression()));
            } else {
                return false;
            }
        }
        protected internal override int computeHashCode() {
            int h = 0;
            h ^= getOrder().GetHashCode();
            h ^= getCaseOrder().GetHashCode();
            h ^= getLanguage().GetHashCode();
            if ((getDataTypeExpression() != null)) {
                h ^= getDataTypeExpression().GetHashCode();
            }
            if ((getStable() != null)) {
                h ^= getStable().GetHashCode();
            }
            if ((getCollationNameExpression() != null)) {
                h ^= getCollationNameExpression().GetHashCode();
            }
            return h;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("sortKey", this);
            if ((finalComparator != null)) {
                @out.emitAttribute("comp", finalComparator.save());
            }
            @out.setChildRole("select");
            sortKey.getChildExpression().export(@out);
            @out.setChildRole("order");
            order.getChildExpression().export(@out);
            if ((dataTypeExpression != null)) {
                @out.setChildRole("dataType");
                dataTypeExpression.getChildExpression().export(@out);
            }
            @out.setChildRole("lang");
            language.getChildExpression().export(@out);
            @out.setChildRole("caseOrder");
            caseOrder.getChildExpression().export(@out);
            if ((stable != null)) {
                @out.setChildRole("stable");
                stable.getChildExpression().export(@out);
            }
            if ((collationName != null)) {
                @out.setChildRole("collation");
                collationName.getChildExpression().export(@out);
            }
            @out.endElement();
        }
    
    }
}