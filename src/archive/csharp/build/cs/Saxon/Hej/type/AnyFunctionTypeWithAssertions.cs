using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class AnyFunctionTypeWithAssertions : Saxon.Hej.type.AnyFunctionType {
        private readonly Saxon.Hej.query.AnnotationList assertions;
        private readonly Saxon.Hej.Configuration config;
        public AnyFunctionTypeWithAssertions(Saxon.Hej.query.AnnotationList assertions, Saxon.Hej.Configuration config)  {
            this.assertions = assertions;
            this.config = config;
        }
        public override Saxon.Hej.query.AnnotationList getAnnotationAssertions() {
            return assertions;
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return ((item is Saxon.Hej.om.FunctionItem) && checkAnnotationAssertions(assertions, (Saxon.Hej.om.FunctionItem)(item), th.getConfiguration()));
        }
        private static bool checkAnnotationAssertions(Saxon.Hej.query.AnnotationList assertions, Saxon.Hej.om.FunctionItem item, Saxon.Hej.Configuration config) {
            Saxon.Hej.query.AnnotationList annotations = item.getAnnotations();
            foreach (Saxon.Hej.query.Annotation ann in assertions) {
                Saxon.Hej.lib.FunctionAnnotationHandler handler = config.getFunctionAnnotationHandler(ann.getAnnotationQName().getNamespaceUri());
                if ((handler != null)) {
                    bool ok = handler.satisfiesAssertion(ann, annotations);
                    if (!(ok)) {
                        return false;
                    }
                }
            }
            return true;
        }
    
    }
}