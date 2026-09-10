using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    public class ReportingSingletonIterator : Saxon.Hej.tree.iter.SingletonIterator {
        private readonly Saxon.Hej.expr.elab.LearningEvaluator learningEvaluator;
        private readonly int serialNumber;
        public ReportingSingletonIterator(Saxon.Hej.om.Item item, Saxon.Hej.expr.elab.LearningEvaluator learningEvaluator, int serialNumber) : base (item) {
            this.learningEvaluator = learningEvaluator;
            this.serialNumber = serialNumber;
        }
        public override Saxon.Hej.om.Item next() {
            Saxon.Hej.om.Item result = base.next();
            if ((result != null)) {
                learningEvaluator.reportCompletion(serialNumber);
            }
            return result;
        }
    
    }
}