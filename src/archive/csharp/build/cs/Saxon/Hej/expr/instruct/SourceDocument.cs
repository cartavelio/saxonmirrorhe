using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class SourceDocument : Saxon.Hej.expr.instruct.Instruction {
        protected internal Saxon.Hej.expr.Operand hrefOp;
        protected internal Saxon.Hej.expr.Operand bodyOp;
        protected internal Saxon.Hej.lib.ParseOptions parseOptions;
        protected internal System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators;
        public SourceDocument(Saxon.Hej.expr.Expression hrefExp, Saxon.Hej.expr.Expression body, Saxon.Hej.lib.ParseOptions options)  {
            hrefOp = new Saxon.Hej.expr.Operand(this, hrefExp, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            bodyOp = new Saxon.Hej.expr.Operand(this, body, new Saxon.Hej.expr.OperandRole(Saxon.Hej.expr.OperandRole.HAS_SPECIAL_FOCUS_RULES, Saxon.Hej.expr.OperandUsage.TRANSMISSION));
            this.parseOptions = options;
            this.accumulators = options.getApplicableAccumulators();
        }
        public override string getExpressionName() {
            return "xsl:source-document";
        }
        public virtual string getExportTag() {
            return "sourceDoc";
        }
        public Saxon.Hej.expr.Expression getHref() {
            return hrefOp.getChildExpression();
        }
        public void setHref(Saxon.Hej.expr.Expression href) {
            hrefOp.setChildExpression(href);
        }
        public Saxon.Hej.expr.Expression getBody() {
            return bodyOp.getChildExpression();
        }
        public void setBody(Saxon.Hej.expr.Expression body) {
            bodyOp.setChildExpression(body);
        }
        public void setUsedAccumulators(System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> used) {
            accumulators = used;
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            return operandList(hrefOp, bodyOp);
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            hrefOp.typeCheck(visitor, contextInfo);
            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:stream/href", 0);
            Saxon.Hej.expr.parser.TypeChecker tc = visitor.getConfiguration().getTypeChecker(false);
            hrefOp.setChildExpression(tc.staticTypeCheck(hrefOp.getChildExpression(), Saxon.Hej.value.SequenceType.SINGLE_STRING, role, visitor));
            Saxon.Hej.expr.parser.ContextItemStaticInfo newType = getConfiguration().makeContextItemStaticInfo(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, false);
            newType.setContextPostureStriding();
            bodyOp.typeCheck(visitor, newType);
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            Saxon.Hej.expr.parser.ContextItemStaticInfo newType = getConfiguration().makeContextItemStaticInfo(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, false);
            newType.setContextPostureStriding();
            hrefOp.optimize(visitor, contextItemType);
            bodyOp.optimize(visitor, newType);
            return this;
        }
        public override bool mayCreateNewNodes() {
            return !(getBody().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED));
        }
        public override int computeDependencies() {
            int dependencies = 0;
            dependencies |= getHref().getDependencies();
            dependencies |= (getBody().getDependencies()&~(Saxon.Hej.expr.StaticProperty.DEPENDS_ON_FOCUS));
            return dependencies;
        }
        protected internal override int computeSpecialProperties() {
            Saxon.Hej.expr.Expression body = getBody();
            if ((((body.getSpecialProperties()&Saxon.Hej.expr.StaticProperty.ALL_NODES_NEWLY_CREATED)) != 0)) {
                return (Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.PEER_NODESET);
            }
            return base.computeSpecialProperties();
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            Saxon.Hej.trace.ExpressionPresenter.ExportOptions options = (Saxon.Hej.trace.ExpressionPresenter.ExportOptions)(@out.getOptions());
            if (("JS".Equals(options.target) && (options.targetVersion == 1))) {
                throw new Saxon.Hej.trans.XPathException("xsl:source-document is not supported in Saxon-JS 1.*", Saxon.Hej.trans.SaxonErrorCode.SXJS0001);
            }
            @out.startElement(getExportTag(), this);
            int validation = parseOptions.getSchemaValidationMode();
            if (((validation != Saxon.Hej.lib.Validation.SKIP) && (validation != Saxon.Hej.lib.Validation.BY_TYPE))) {
                @out.emitAttribute("validation", (validation+""));
            }
            Saxon.Hej.type.SchemaType schemaType = parseOptions.getTopLevelType();
            if ((schemaType != null)) {
                @out.emitAttribute("schemaType", schemaType.getStructuredQName());
            }
            Saxon.Hej.om.SpaceStrippingRule xsltStripSpace = ((getPackageData() is Saxon.Hej.style.StylesheetPackage) ? ((Saxon.Hej.style.StylesheetPackage)(getPackageData())).getSpaceStrippingRule() : null);
            string flags = "";
            if ((parseOptions.getSpaceStrippingRule() == xsltStripSpace)) {
                flags += "s";
            } else {
                if ((parseOptions.getSpaceStrippingRule() == Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance())) {
                    flags += "S";
                }
            }
            if (parseOptions.isLineNumbering()) {
                flags += "l";
            }
            if (parseOptions.isExpandAttributeDefaults()) {
                flags += "a";
            }
            if ((parseOptions.getDTDValidationMode() == Saxon.Hej.lib.Validation.STRICT)) {
                flags += "d";
            }
            if (parseOptions.isXIncludeAware()) {
                flags += "i";
            }
            @out.emitAttribute("flags", flags);
            if (((accumulators != null) && !(accumulators.Count==0))) {
                Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
                foreach (Saxon.Hej.expr.accum.Accumulator acc in accumulators) {
                    if ((fsb.length() != 0)) {
                        fsb.append(" ");
                    }
                    fsb.append(acc.getAccumulatorName().getEQName());
                }
                @out.emitAttribute("accum", fsb.ToString());
            }
            @out.setChildRole("href");
            getHref().export(@out);
            @out.setChildRole("body");
            getBody().export(@out);
            @out.endElement();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.instruct.SourceDocument exp = new Saxon.Hej.expr.instruct.SourceDocument(getHref().copy(rebindings), getBody().copy(rebindings), parseOptions);
            exp.setRetainedStaticContext(getRetainedStaticContext());
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override Saxon.Hej.expr.instruct.TailCall processLeavingTail(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            try {
                push(output, context);
            } catch (Saxon.Hej.trans.QuitParsingException q) {
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(getLocation());
                if ((e.getErrorCodeQName() == null)) {
                    e.setErrorCode("FODC0002");
                }
                throw;
            }
            return null;
        }
        public virtual void push(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            string href = hrefOp.getChildExpression().evaluateAsString(context).ToString();
            Saxon.Hej.om.NodeInfo doc = Saxon.Hej.functions.DocumentFn.makeDoc(href, getStaticBaseURIString(), getPackageData(), parseOptions, context, getLocation(), false);
            if ((doc != null)) {
                Saxon.Hej.Controller controller = context.getController();
                if (((accumulators != null) && (controller is Saxon.Hej.trans.XsltController))) {
                    ((Saxon.Hej.trans.XsltController)(controller)).getAccumulatorManager().setApplicableAccumulators(doc.getTreeInfo(), accumulators);
                }
                Saxon.Hej.expr.XPathContext c2 = context.newMinorContext();
                c2.setCurrentIterator(new Saxon.Hej.tree.iter.ManualIterator(doc));
                bodyOp.getChildExpression().process(output, c2);
            }
        }
    
    }
}