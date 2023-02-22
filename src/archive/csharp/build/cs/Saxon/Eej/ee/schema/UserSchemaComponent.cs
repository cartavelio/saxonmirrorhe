using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public interface UserSchemaComponent : Saxon.Hej.type.SchemaComponent {
        bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler);
        void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler);
        bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler);
        void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler);
    
    }
}