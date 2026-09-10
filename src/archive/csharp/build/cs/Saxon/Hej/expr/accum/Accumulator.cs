using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.accum {
    public class Accumulator : Saxon.Hej.expr.instruct.Actor {
        private Saxon.Hej.om.StructuredQName accumulatorName;
        private Saxon.Hej.trans.SimpleMode preDescentRules;
        private Saxon.Hej.trans.SimpleMode postDescentRules;
        private Saxon.Hej.expr.Expression initialValueExpression;
        private Saxon.Hej.value.SequenceType type;
        private bool streamable;
        private bool universallyApplicable;
        private int importPrecedence;
        private bool tracing;
        private Saxon.Hej.expr.instruct.SlotManager slotManagerForInitialValueExpression;
        public Accumulator()  {
            preDescentRules = new Saxon.Hej.trans.SimpleMode(new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.om.NamespaceUri.SAXON, "preDescent"));
            postDescentRules = new Saxon.Hej.trans.SimpleMode(new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.om.NamespaceUri.SAXON, "postDescent"));
            body = Saxon.Hej.expr.Literal.makeEmptySequence();
        }
        public override Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_ACCUMULATOR, getAccumulatorName());
        }
        public bool isDeclaredStreamable() {
            return streamable;
        }
        public void setDeclaredStreamable(bool streamable) {
            this.streamable = streamable;
        }
        public Saxon.Hej.om.StructuredQName getAccumulatorName() {
            return accumulatorName;
        }
        public void setAccumulatorName(Saxon.Hej.om.StructuredQName firstName) {
            this.accumulatorName = firstName;
        }
        public int getImportPrecedence() {
            return importPrecedence;
        }
        public void setImportPrecedence(int importPrecedence) {
            this.importPrecedence = importPrecedence;
        }
        public void setUniversallyApplicable(bool universal) {
            this.universallyApplicable = universal;
        }
        public bool isUniversallyApplicable() {
            return universallyApplicable;
        }
        public bool isTracing() {
            return tracing;
        }
        public void setTracing(bool tracing) {
            this.tracing = tracing;
        }
        public Saxon.Hej.expr.instruct.SlotManager getSlotManagerForInitialValueExpression() {
            return slotManagerForInitialValueExpression;
        }
        public void setSlotManagerForInitialValueExpression(Saxon.Hej.expr.instruct.SlotManager slotManagerForInitialValueExpression) {
            this.slotManagerForInitialValueExpression = slotManagerForInitialValueExpression;
        }
        public Saxon.Hej.trans.SimpleMode getPreDescentRules() {
            return preDescentRules;
        }
        public void setPreDescentRules(Saxon.Hej.trans.SimpleMode preDescentRules) {
            this.preDescentRules = preDescentRules;
        }
        public Saxon.Hej.trans.SimpleMode getPostDescentRules() {
            return postDescentRules;
        }
        public void setPostDescentRules(Saxon.Hej.trans.SimpleMode postDescentRules) {
            this.postDescentRules = postDescentRules;
        }
        public Saxon.Hej.expr.Expression getInitialValueExpression() {
            return initialValueExpression;
        }
        public void setInitialValueExpression(Saxon.Hej.expr.Expression initialValueExpression) {
            this.initialValueExpression = initialValueExpression;
        }
        public void addChildExpression(Saxon.Hej.expr.Expression expression) {
            Saxon.Hej.expr.Expression e = Saxon.Hej.expr.instruct.Block.makeBlock(getBody(), expression);
            setBody(e);
        }
        public Saxon.Hej.value.SequenceType getType() {
            return type;
        }
        public void setType(Saxon.Hej.value.SequenceType type) {
            this.type = type;
        }
        public bool isCompatible(Saxon.Hej.expr.accum.Accumulator other) {
            return getAccumulatorName().Equals(other.getAccumulatorName());
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return accumulatorName;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            export(presenter, null);
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter @out, System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,int> componentIdMap) {
            @out.startElement("accumulator");
            @out.emitAttribute("name", getObjectName());
            @out.emitAttribute("line", (getLineNumber()+""));
            @out.emitAttribute("module", getSystemId());
            @out.emitAttribute("as", type.toAlphaCode());
            @out.emitAttribute("streamable", (streamable ? "1" : "0"));
            @out.emitAttribute("slots", (getSlotManagerForInitialValueExpression().getNumberOfVariables()+""));
            if ((componentIdMap != null)) {
                @out.emitAttribute("binds", (""+getDeclaringComponent().listComponentReferences(componentIdMap)));
            }
            if (isUniversallyApplicable()) {
                @out.emitAttribute("flags", "u");
            }
            @out.setChildRole("init");
            initialValueExpression.export(@out);
            Saxon.Hej.trans.Mode.RuleAction action = r => {
                @out.startElement("accRule");
                @out.emitAttribute("slots", (((Saxon.Hej.expr.accum.AccumulatorRule)(r.getAction())).getStackFrameMap().getNumberOfVariables()+""));
                @out.emitAttribute("rank", (""+r.getRank()));
                if (((Saxon.Hej.expr.accum.AccumulatorRule)(r.getAction())).isCapturing()) {
                    @out.emitAttribute("flags", "c");
                }
                r.getPattern().export(@out);
                r.getAction().export(@out);
                @out.endElement();
            } ;
            try {
                @out.startElement("pre");
                @out.emitAttribute("slots", (preDescentRules.getStackFrameSlotsNeeded()+""));
                preDescentRules.processRules(action);
                @out.endElement();
                @out.startElement("post");
                @out.emitAttribute("slots", (postDescentRules.getStackFrameSlotsNeeded()+""));
                postDescentRules.processRules(action);
                @out.endElement();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Ejava.lang.AssertionError(e);
            }
            @out.endElement();
        }
    
    }
}