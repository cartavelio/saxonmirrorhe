using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface FunctionAnnotationHandler {
        Saxon.Hej.om.NamespaceUri getAssertionNamespace();
        void check(Saxon.Hej.query.AnnotationList annotations, string construct);
        bool satisfiesAssertion(Saxon.Hej.query.Annotation assertion, Saxon.Hej.query.AnnotationList annotationList);
        Saxon.Hej.type.Affinity relationship(Saxon.Hej.query.AnnotationList firstList, Saxon.Hej.query.AnnotationList secondList);
    
    }
}