using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public class ItemWithMergeKeys {
        internal Saxon.Hej.om.Item baseItem;
        internal System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> sortKeyValues;
        internal string sourceName;
        internal ItemWithMergeKeys(Saxon.Hej.om.Item bItem, Saxon.Hej.expr.sort.SortKeyDefinitionList sKeys, string name, Saxon.Hej.expr.XPathContext context)  {
            baseItem = bItem;
            sourceName = name;
            sortKeyValues = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>(sKeys.size());
            foreach (Saxon.Hej.expr.sort.SortKeyDefinition sKey in sKeys) {
                sortKeyValues.Add((Saxon.Hej.value.AtomicValue)(sKey.getSortKey().evaluateItem(context)));
            }
        }
    
    }
}