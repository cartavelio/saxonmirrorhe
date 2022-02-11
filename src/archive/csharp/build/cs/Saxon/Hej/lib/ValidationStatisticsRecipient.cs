using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface ValidationStatisticsRecipient {
        void notifyValidationStatistics(System.Collections.Generic.IDictionary<Saxon.Hej.type.SchemaComponent,int> statistics);
    
    }
}