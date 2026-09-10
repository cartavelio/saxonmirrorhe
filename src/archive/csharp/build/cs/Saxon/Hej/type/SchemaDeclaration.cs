using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface SchemaDeclaration {
        int getFingerprint();
        Saxon.Hej.om.StructuredQName getComponentName();
        Saxon.Hej.type.SchemaType getType();
        Saxon.Hej.pattern.NodeTest makeSchemaNodeTest();
        bool isNillable();
        bool isAbstract();
        bool hasTypeAlternatives();
    
    }
}