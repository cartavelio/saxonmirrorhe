using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public interface ValidatingInstruction {
        Saxon.Hej.type.SchemaType getSchemaType();
        int getValidationAction();
    
    }
}