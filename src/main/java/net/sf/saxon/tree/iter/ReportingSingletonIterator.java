package net.sf.saxon.tree.iter;

import net.sf.saxon.expr.elab.LearningEvaluator;
import net.sf.saxon.om.Item;
import net.sf.saxon.trace.Instrumentation;

public class ReportingSingletonIterator extends SingletonIterator {

    final LearningEvaluator learningEvaluator;
    final int serialNumber;

    public ReportingSingletonIterator(Item item, LearningEvaluator learningEvaluator, int serialNumber) {
        super(item);
        this.learningEvaluator = learningEvaluator;
        this.serialNumber = serialNumber;
        Instrumentation.count("ReportingSingletonIterator");
    }

    @Override
    public Item next() {
        Item result = super.next();
        if (result != null) {
            learningEvaluator.reportCompletion(serialNumber);
        }
        Instrumentation.count("ReportingSingletonIterator.reportCompletion()");
        return result;
    }
}

