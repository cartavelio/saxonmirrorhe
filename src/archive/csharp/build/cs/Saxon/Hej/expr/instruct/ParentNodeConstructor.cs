using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public abstract class ParentNodeConstructor : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.instruct.ValidatingInstruction, Saxon.Hej.expr.instruct.InstructionWithComplexContent {
        private readonly static Saxon.Hej.expr.OperandRole SAME_FOCUS_CONTENT = new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.ABSORPTION, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        protected internal Saxon.Hej.expr.Operand contentOp;
        private Saxon.Hej.lib.ParseOptions validationOptions = null;
        protected internal bool preservingTypes = true;
        public ParentNodeConstructor()  {
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return ((validationOptions == null) ? null : validationOptions.getTopLevelType());
        }
        public Saxon.Hej.lib.ParseOptions getValidationOptions() {
            return validationOptions;
        }
        public void setValidationOptions(Saxon.Hej.lib.ParseOptions options) {
            this.validationOptions = options;
        }
        public void setValidationAction(int mode, Saxon.Hej.type.SchemaType schemaType) {
            preservingTypes = ((mode == Saxon.Hej.lib.Validation.PRESERVE) && (schemaType == null));
            if (!(preservingTypes)) {
                if ((validationOptions == null)) {
                    validationOptions = new Saxon.Hej.lib.ParseOptions();
                }
                if ((schemaType == Saxon.Hej.type.Untyped.getInstance())) {
                    validationOptions = validationOptions.withSchemaValidationMode(Saxon.Hej.lib.Validation.SKIP);
                } else {
                    validationOptions = validationOptions.withSchemaValidationMode(mode).withTopLevelType(schemaType);
                }
            }
        }
        public int getValidationAction() {
            return ((validationOptions == null) ? Saxon.Hej.lib.Validation.PRESERVE : validationOptions.getSchemaValidationMode());
        }
        public void setNoNeedToStrip() {
            preservingTypes = true;
        }
        public void setContentExpression(Saxon.Hej.expr.Expression content) {
            if ((contentOp == null)) {
                contentOp = new Saxon.Hej.expr.Operand(this, content, SAME_FOCUS_CONTENT);
            } else {
                contentOp.setChildExpression(content);
            }
        }
        public Saxon.Hej.expr.Expression getContentExpression() {
            return ((contentOp == null) ? null : contentOp.getChildExpression());
        }
        public Saxon.Hej.expr.Operand getContentOperand() {
            return contentOp;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            checkContentSequence(visitor.getStaticContext());
            return this;
        }
        public override bool allowExtractingCommonSubexpressions() {
            return false;
        }
        protected internal abstract void checkContentSequence(Saxon.Hej.expr.StaticContext env);
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            optimizeChildren(visitor, contextItemType);
            if (!(Saxon.Hej.expr.Literal.isEmptySequence(getContentExpression()))) {
                if ((getContentExpression() is Saxon.Hej.expr.instruct.Block)) {
                    setContentExpression(((Saxon.Hej.expr.instruct.Block)(getContentExpression())).mergeAdjacentTextInstructions());
                }
                if (visitor.isOptimizeForStreaming()) {
                    visitor.obtainOptimizer().makeCopyOperationsExplicit(this, contentOp);
                }
            }
            if (visitor.getStaticContext().getPackageData().isSchemaAware()) {
                Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
                if ((getValidationAction() == Saxon.Hej.lib.Validation.STRIP)) {
                    if ((getContentExpression().hasSpecialProperty(Saxon.Hej.expr.StaticProperty.ALL_NODES_UNTYPED) || (th.relationship(getContentExpression().getItemType(), Saxon.Hej.pattern.MultipleNodeKindTest.DOC_ELEM_ATTR) == Saxon.Hej.type.Affinity.DISJOINT))) {
                        setNoNeedToStrip();
                    }
                }
            } else {
                setValidationAction(Saxon.Hej.lib.Validation.STRIP, null);
                setNoNeedToStrip();
            }
            return this;
        }
        public override bool mayCreateNewNodes() {
            return true;
        }
        public override bool alwaysCreatesNewNodes() {
            return true;
        }
        public override int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet result = base.addToPathMap(pathMap, pathMapNodeSet);
            result.setReturnable(false);
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            Saxon.Hej.type.ItemType type = getItemType();
            if (((th.relationship(type, Saxon.Hej.pattern.NodeKindTest.ELEMENT) != Saxon.Hej.type.Affinity.DISJOINT) || (th.relationship(type, Saxon.Hej.pattern.NodeKindTest.DOCUMENT) != Saxon.Hej.type.Affinity.DISJOINT))) {
                result.addDescendants();
            }
            return new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(this));
        }
        public bool isPreservingTypes() {
            return preservingTypes;
        }
        public bool isLocal() {
            return Saxon.Hej.expr.parser.ExpressionTool.isLocalConstructor(this);
        }
    
    }
}