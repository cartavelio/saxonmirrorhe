package com.saxonica.ee.elab.insn;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.eval.PushEvaluator;
import com.saxonica.ee.elab.eval.UnicodeStringEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.ContextMappingIterator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.instruct.AnalyzeString;
import net.sf.saxon.expr.instruct.TailCall;
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.regex.RegexIterator;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.EmptyIterator;

public class AnalyzeStringElaborator extends PullElaborator {

    @Override
    public PullEvaluator elaborateForPull() {
        AnalyzeString insn = (AnalyzeString) getExpression();
        UnicodeStringEvaluator input = makeElaborator(insn.getSelect()).elaborateForUnicodeString(true);
        PullEvaluator matching = insn.getMatching() == null
                ? null
                : makeElaborator(insn.getMatching()).elaborateForPull();
        PullEvaluator nonMatching = insn.getNonMatching() == null
                ? null
                : makeElaborator(insn.getNonMatching()).elaborateForPull();
        RegularExpression pattern = insn.getPatternExpression();
        return context -> {
            RegularExpression re = pattern == null ? insn.getRegularExpression(context) : pattern;
            UnicodeString in = input.eval(context);
            RegexIterator iter = re.analyze(in);
            XPathContextMajor c2 = context.newContext();
            c2.setOrigin(insn);
            c2.trackFocus(iter);
            c2.setCurrentRegexIterator(iter);

            return new ContextMappingIterator(cxt -> {
                if (iter.isMatching()) {
                    if (matching != null) {
                        return matching.eval(c2);
                    }
                } else {
                    if (nonMatching != null) {
                        return nonMatching.eval(c2);
                    }
                }
                return EmptyIterator.getInstance();
            }, c2);
        };
    }

    @Override
    public PushEvaluator elaborateForPush() {
        AnalyzeString insn = (AnalyzeString) getExpression();
        UnicodeStringEvaluator input = makeElaborator(insn.getSelect()).elaborateForUnicodeString(true);
        PushEvaluator matching = insn.getMatching() == null
                ? null
                : makeElaborator(insn.getMatching()).elaborateForPush();
        PushEvaluator nonMatching = insn.getNonMatching() == null
                ? null
                : makeElaborator(insn.getNonMatching()).elaborateForPush();
        RegularExpression pattern = insn.getPatternExpression();
        return (out, context) -> {
            RegularExpression re = pattern == null ? insn.getRegularExpression(context) : pattern;
            UnicodeString in = input.eval(context);
            RegexIterator iter = re.analyze(in);
            XPathContextMajor c2 = context.newContext();
            c2.setOrigin(insn);
            FocusIterator focus = c2.trackFocus(iter);
            c2.setCurrentRegexIterator(iter);

            while(focus.next() != null) {
                if (iter.isMatching()) {
                    if (matching != null) {
                        TailCall tc = matching.process(out, c2);
                        Expression.dispatchTailCall(tc);
                    }
                } else {
                    if (nonMatching != null) {
                        TailCall tc = nonMatching.process(out, c2);
                        Expression.dispatchTailCall(tc);
                    }
                }
            }
            return null;
        };

    }
}

