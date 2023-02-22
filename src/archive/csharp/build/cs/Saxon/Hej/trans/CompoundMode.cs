using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class CompoundMode : Saxon.Hej.trans.Mode {
        private readonly Saxon.Hej.trans.Mode @base;
        private readonly Saxon.Hej.trans.SimpleMode overrides;
        private readonly int overridingPrecedence;
        public CompoundMode(Saxon.Hej.trans.Mode @base, Saxon.Hej.trans.SimpleMode overrides) : base (@base.getModeName()) {
            if (!(@base.getModeName().Equals(overrides.getModeName()))) {
                throw new Saxon.Ejava.lang.AssertionError("Base and overriding modes must have the same name");
            }
            if (@base.getModeName().Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME)) {
                throw new Saxon.Ejava.lang.AssertionError("Cannot override an unnamed mode");
            }
            if (@base.getModeName().Equals(Saxon.Hej.trans.Mode.OMNI_MODE)) {
                throw new Saxon.Ejava.lang.AssertionError("Cannot override mode='#all'");
            }
            this.@base = @base;
            this.overrides = overrides;
            this.mustBeTyped = @base.mustBeTyped;
            this.mustBeUntyped = @base.mustBeUntyped;
            this.overridingPrecedence = (@base.getMaxPrecedence()+1);
        }
        public override Saxon.Hej.trans.rules.BuiltInRuleSet getBuiltInRuleSet() {
            return @base.getBuiltInRuleSet();
        }
        public override Saxon.Hej.trans.SimpleMode getActivePart() {
            return overrides;
        }
        public override bool isEmpty() {
            return (@base.isEmpty() && overrides.isEmpty());
        }
        public override int getMaxPrecedence() {
            return overridingPrecedence;
        }
        public override int getMaxRank() {
            return overrides.getMaxRank();
        }
        public override void computeRankings(int start) {
            overrides.computeRankings((@base.getMaxRank()+1));
        }
        public override void processRules(Saxon.Hej.trans.Mode.RuleAction action) {
            overrides.processRules(action);
            @base.processRules(action);
        }
        public override System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> getExplicitNamespaces(Saxon.Hej.om.NamePool pool) {
            System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri> r = new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>();
            Saxon.Impl.Helpers.SetUtils.AddAll(r, @base.getExplicitNamespaces(pool));
            Saxon.Impl.Helpers.SetUtils.AddAll(r, overrides.getExplicitNamespaces(pool));
            return r;
        }
        public override void allocateAllBindingSlots(Saxon.Hej.style.StylesheetPackage pack) {
            if (!(bindingSlotsAllocated)) {
                System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> baseBindings = @base.getDeclaringComponent().getComponentBindings();
                System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> newBindings = new System.Collections.Generic.List<Saxon.Hej.expr.ComponentBinding>(baseBindings);
                Saxon.Hej.expr.Component comp = getDeclaringComponent();
                comp.setComponentBindings(newBindings);
                Saxon.Hej.trans.SimpleMode.forceAllocateAllBindingSlots(pack, overrides, newBindings);
                bindingSlotsAllocated = true;
            }
        }
        public override Saxon.Hej.trans.rules.Rule getRule(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.trans.rules.Rule r = overrides.getRule(item, context);
            if ((r == null)) {
                r = @base.getRule(item, context);
            }
            return r;
        }
        public override int getStackFrameSlotsNeeded() {
            return System.Math.Max(@base.getStackFrameSlotsNeeded(), overrides.getStackFrameSlotsNeeded());
        }
        public override Saxon.Hej.trans.rules.Rule getRule(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context, System.Predicate<Saxon.Hej.trans.rules.Rule> filter) {
            Saxon.Hej.trans.rules.Rule r = overrides.getRule(item, context, filter);
            if ((r == null)) {
                r = @base.getRule(item, context, filter);
            }
            return r;
        }
        public override void exportTemplateRules(Saxon.Hej.trace.ExpressionPresenter presenter) {
            overrides.exportTemplateRules(presenter);
            @base.exportTemplateRules(presenter);
        }
        public override void explainTemplateRules(Saxon.Hej.trace.ExpressionPresenter presenter) {
            overrides.explainTemplateRules(presenter);
            @base.explainTemplateRules(presenter);
        }
    
    }
}