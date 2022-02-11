using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public interface ComponentWithValueConstraint {
        Saxon.Eej.ee.schema.ValueConstraint getValueConstraint();
        void setValueConstraint(Saxon.Eej.ee.schema.ValueConstraint vc);
        Saxon.Eej.ee.schema.ValueConstraint getFixedValueConstraint();
        Saxon.Eej.ee.schema.ValueConstraint getDefaultValueConstraint();
        Saxon.Hej.om.Sequence getValueConstraintAsFunction(Saxon.Eej.ee.schema.ComponentWithValueConstraint att);
        Saxon.Hej.str.UnicodeString getFixedValueLexicalForm();
        Saxon.Hej.om.AtomicSequence getFixedValue();
        Saxon.Hej.str.UnicodeString getDefaultValueLexicalForm();
        Saxon.Hej.om.AtomicSequence getDefaultValue();
        Saxon.Hej.om.NamespaceResolver getNamespaceResolver();
    
    }
}