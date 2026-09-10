package net.sf.saxon.tree.iter;

import net.sf.saxon.expr.elab.LearningEvaluator;
import net.sf.saxon.om.Item;

/**
 * This is a subclass of <code>SingletonIterator</code> that reports to
 * a <code>LearningEvaluator</code> when the single target item is actually
 * read. The <code>LearningEvaluator</code> uses this information to decide
 * whether to perform lazy or eager evaluation.
 */

public class ReportingSingletonIterator extends SingletonIterator {

    private final LearningEvaluator learningEvaluator;
    private final int serialNumber;

    /**
     * Create a <code>ReportingSingletonIterator</code>
     * @param item the single item to be delivered by the iterator
     * @param learningEvaluator the class to be notified when the item is read
     * @param serialNumber a serial number identifying this evaluation
     */

    public ReportingSingletonIterator(Item item, LearningEvaluator learningEvaluator, int serialNumber) {
        super(item);
        this.learningEvaluator = learningEvaluator;
        this.serialNumber = serialNumber;
    }

    @Override
    public Item next() {
        Item result = super.next();
        if (result != null) {
            learningEvaluator.reportCompletion(serialNumber);
        }
        return result;
    }
}

