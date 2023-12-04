using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class Rule {
        protected internal Saxon.Hej.pattern.Pattern pattern;
        protected internal Saxon.Hej.trans.rules.RuleTarget action;
        protected internal int precedence;
        protected internal int minImportPrecedence;
        protected internal double priority;
        protected internal Saxon.Hej.trans.rules.Rule next;
        protected internal int sequence;
        protected internal int part;
        private bool alwaysMatches;
        private int rank;
        protected internal Rule()  {
        }
        public Rule(Saxon.Hej.pattern.Pattern p, Saxon.Hej.trans.rules.RuleTarget o, int prec, int min, double prio, int seq, int part)  {
            pattern = p;
            action = o;
            precedence = prec;
            minImportPrecedence = min;
            priority = prio;
            next = null;
            sequence = seq;
            this.part = part;
            o.registerRule(this);
        }
        public int getSequence() {
            return sequence;
        }
        public int getPartNumber() {
            return part;
        }
        public void setAction(Saxon.Hej.trans.rules.RuleTarget action) {
            this.action = action;
        }
        public Saxon.Hej.trans.rules.RuleTarget getAction() {
            return action;
        }
        public Saxon.Hej.trans.rules.Rule getNext() {
            return next;
        }
        public void setNext(Saxon.Hej.trans.rules.Rule next) {
            this.next = next;
        }
        public Saxon.Hej.pattern.Pattern getPattern() {
            return pattern;
        }
        public void setPattern(Saxon.Hej.pattern.Pattern pattern) {
            this.pattern = pattern;
        }
        public int getPrecedence() {
            return precedence;
        }
        public int getMinImportPrecedence() {
            return minImportPrecedence;
        }
        public double getPriority() {
            return priority;
        }
        public void setAlwaysMatches(bool matches) {
            alwaysMatches = matches;
        }
        public bool isAlwaysMatches() {
            return alwaysMatches;
        }
        public void setRank(int rank) {
            this.rank = rank;
        }
        public int getRank() {
            return rank;
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter @out, bool modeStreamable) {
            Saxon.Hej.trans.rules.RuleTarget target = getAction();
            Saxon.Hej.expr.instruct.TemplateRule template = null;
            if ((target is Saxon.Hej.expr.instruct.TemplateRule)) {
                template = (Saxon.Hej.expr.instruct.TemplateRule)(target);
                int s = @out.startElement("templateRule");
                @out.emitAttribute("prec", (getPrecedence()+""));
                @out.emitAttribute("prio", (getPriority()+""));
                @out.emitAttribute("seq", (getSequence()+""));
                if ((part != 0)) {
                    @out.emitAttribute("part", (""+part));
                }
                @out.emitAttribute("rank", (""+getRank()));
                @out.emitAttribute("minImp", (getMinImportPrecedence()+""));
                @out.emitAttribute("slots", (template.getStackFrameMap().getNumberOfVariables()+""));
                @out.emitAttribute("matches", pattern.getItemType().getFullAlphaCode());
                template.explainProperties(@out);
                exportOtherProperties(@out);
                @out.setChildRole("match");
                getPattern().export(@out);
                if ((template.getBody() != null)) {
                    @out.setChildRole("action");
                    template.getBody().export(@out);
                }
                int e = @out.endElement();
                if ((s != e)) {
                    throw new Saxon.Ejava.lang.IllegalStateException(("exported expression tree unbalanced in template at line "+(((template != null) ? ((template.getLineNumber()+" of ")+template.getSystemId()) : ""))));
                }
            } else {
                target.export(@out);
            }
        }
        public virtual void exportOtherProperties(Saxon.Hej.trace.ExpressionPresenter @out) {
        }
        public int compareRank(Saxon.Hej.trans.rules.Rule other) {
            return (rank-other.rank);
        }
        public int compareComputedRank(Saxon.Hej.trans.rules.Rule other) {
            if ((precedence == other.precedence)) {
                return priority.CompareTo(other.priority);
            } else {
                if ((precedence<other.precedence)) {
                    return -1;
                } else {
                    return +(1);
                }
            }
        }
        public bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContextMajor context) {
            return (alwaysMatches || pattern.matchesItem(item, context));
        }
    
    }
}