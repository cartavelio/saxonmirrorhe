using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface AtomicType : Saxon.Hej.type.SimpleType, Saxon.Hej.type.PlainType, Saxon.Hej.type.CastingTarget {
        Saxon.Hej.type.ValidationFailure validate(Saxon.Hej.value.AtomicValue primValue, Saxon.Hej.str.UnicodeString lexicalValue, Saxon.Hej.lib.ConversionRules rules);
        bool isOrdered(bool optimistic);
        bool isAbstract();
        bool isPrimitiveType();
        Saxon.Hej.type.BuiltInAtomicType getPrimitiveAtomicType();
        /* implicit =====
        bool isIdType();
        ===== implicit */
        /* implicit =====
        bool isIdRefType();
        ===== implicit */
        /* implicit =====
        bool isBuiltInType();
        ===== implicit */
        /* implicit =====
        Saxon.Hej.om.StructuredQName getTypeName();
        ===== implicit */
        Saxon.Hej.type.StringConverter getStringConverter(Saxon.Hej.lib.ConversionRules rules);
    
    }
}