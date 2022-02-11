using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface UnionType : Saxon.Hej.type.ItemType, Saxon.Hej.type.CastingTarget {
        Saxon.Hej.om.StructuredQName getTypeName();
        Saxon.Hej.om.StructuredQName getStructuredQName();
        bool containsListType();
        System.Collections.Generic.IList<Saxon.Hej.type.PlainType> getPlainMemberTypes();
        Saxon.Hej.value.SequenceType getResultTypeOfCast();
        Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules);
        Saxon.Hej.type.ValidationFailure checkAgainstFacets(Saxon.Hej.value.AtomicValue value, Saxon.Hej.lib.ConversionRules rules);
        string getDescription();
    
    }
}