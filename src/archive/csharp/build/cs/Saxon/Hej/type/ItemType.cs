using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface ItemType {
        Saxon.Hej.om.Genre getGenre();
        bool isAtomicType();
        bool isPlainType();
        bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th);
        Saxon.Hej.type.ItemType getPrimitiveItemType();
        int getPrimitiveType();
        Saxon.Hej.type.UType getUType();
        double getDefaultPriority();
        double getNormalizedDefaultPriority();
        Saxon.Hej.type.PlainType getAtomizedItemType();
        bool isAtomizable(Saxon.Hej.type.TypeHierarchy th);
        string getBasicAlphaCode();
        string getFullAlphaCode();
        string toExportString();
        Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th);
    
    }
}