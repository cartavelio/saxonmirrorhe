using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface SimpleType : Saxon.Hej.type.SchemaType, Saxon.Hej.type.HyperType {
        /* implicit =====
        bool isAtomicType();
        ===== implicit */
        bool isListType();
        bool isUnionType();
        bool isBuiltInType();
        Saxon.Hej.type.SchemaType getBuiltInBaseType();
        Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules);
        Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules);
        int getWhitespaceAction();
        Saxon.Hej.str.UnicodeString preprocess(Saxon.Hej.str.UnicodeString input);
        Saxon.Hej.str.UnicodeString postprocess(Saxon.Hej.str.UnicodeString input);
    
    }
}