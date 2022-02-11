using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.s9api {
    public class ValidationStatistics {
        private readonly System.Collections.Generic.IList<System.Collections.Generic.IDictionary<Saxon.Hej.type.SchemaComponent,int>> collectedStats = new System.Collections.Generic.List<System.Collections.Generic.IDictionary<Saxon.Hej.type.SchemaComponent, int>>();
        private Saxon.Hej.lib.ValidationStatisticsRecipient recipient;
        public Saxon.Hej.lib.ValidationStatisticsRecipient getRecipient() {
            if ((recipient == null)) {
                recipient = new Inner_ValidationStatisticsRecipient_d66e103(this);
            }
            return recipient;
        }
    
        private class Inner_ValidationStatisticsRecipient_d66e103 : Saxon.Hej.lib.ValidationStatisticsRecipient {
            private readonly ValidationStatistics outerInstance;
            public Inner_ValidationStatisticsRecipient_d66e103(ValidationStatistics outerInstance) {
                this.outerInstance = outerInstance;
            }
            public void notifyValidationStatistics(System.Collections.Generic.IDictionary<Saxon.Hej.type.SchemaComponent,int> statistics) {
                outerInstance.collectedStats.Add(/*localparam*/statistics);
            }
        }

    }
}