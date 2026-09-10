using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public abstract class AnnotationParent : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        public AnnotationParent()  {
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkOnlyChildIsAnnotation();
        }
    
    }
}