using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public interface ComponentReference : Saxon.Hej.s9api.Location {
        Saxon.Eej.config.EnterpriseConfiguration getConfiguration();
        void tryToResolve(Saxon.Eej.ee.schema.SchemaCompiler compiler, bool fatal);
        bool isResolved();
        bool isDangling();
        Saxon.Hej.type.SchemaComponent getTarget();
        Saxon.Hej.om.StructuredQName getTargetComponentName();
        int getSymbolSpace();
    
    }
}