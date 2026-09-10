using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface SchemaComponent {
        Saxon.Hej.type.SchemaValidationStatus getValidationStatus();
        int getRedefinitionLevel();
        Saxon.Hej.om.FunctionItem getComponentAsFunction();
    
    }
}