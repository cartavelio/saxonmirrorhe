using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface FunctionItemType : Saxon.Hej.type.ItemType {
        Saxon.Hej.value.SequenceType[] getArgumentTypes();
        Saxon.Hej.value.SequenceType getResultType();
        Saxon.Hej.type.Affinity relationship(Saxon.Hej.type.FunctionItemType other, Saxon.Hej.type.TypeHierarchy th);
        Saxon.Hej.query.AnnotationList getAnnotationAssertions();
        Saxon.Hej.expr.Expression makeFunctionSequenceCoercer(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.RoleDiagnostic role);
        bool isMapType();
        bool isArrayType();
    
    }
}