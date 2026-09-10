using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class KeyMap : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.ma.map.MapItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string keyName = arguments[0].head().getStringValue();
            Saxon.Hej.om.NodeInfo doc = (Saxon.Hej.om.NodeInfo)(arguments[1].head());
            Saxon.Hej.om.Item minItem = arguments[2].head();
            Saxon.Hej.om.Item maxItem = arguments[3].head();
            Saxon.Hej.str.UnicodeString min = ((minItem == null) ? null : minItem.getUnicodeStringValue());
            Saxon.Hej.str.UnicodeString max = ((maxItem == null) ? null : maxItem.getUnicodeStringValue());
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            Saxon.Hej.om.StructuredQName keyQName = Saxon.Hej.om.StructuredQName.fromLexicalQName(keyName, false, true, getRetainedStaticContext());
            Saxon.Hej.trans.KeyManager keyManager = getRetainedStaticContext().getPackageData().getKeyManager();
            Saxon.Hej.trans.KeyDefinitionSet kds = keyManager.getKeyDefinitionSet(keyQName);
            if ((kds == null)) {
                throw new Saxon.Hej.trans.XPathException((("Key definition "+keyName)+" not found"));
            }
            checkKeyDefinition(th, kds, keyName);
            Saxon.Hej.trans.KeyIndex index = keyManager.obtainIndex(kds, doc.getTreeInfo(), context);
            return new Saxon.Hej.ma.map.RangeKey(min, max, (System.Collections.Generic.SortedDictionary<Saxon.Hej.expr.sort.AtomicMatchKey,System.Object>)(index.getUnderlyingMap()));
        }
        private static void checkKeyDefinition(Saxon.Hej.type.TypeHierarchy th, Saxon.Hej.trans.KeyDefinitionSet kds, string keyName) {
            if (((kds.getCollationName() != null) && !(kds.getCollationName().Equals(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI)))) {
                throw new Saxon.Hej.trans.XPathException((("Key definition "+keyName)+" does not use codepoint collation"));
            }
            if (!(kds.isRangeKey())) {
                throw new Saxon.Hej.trans.XPathException((("Key definition "+keyName)+" is not defined as a range key"));
            }
            foreach (Saxon.Hej.trans.KeyDefinition def in kds.getKeyDefinitions()) {
                Saxon.Hej.type.ItemType it = def.getUse().getItemType();
                if ((((it != Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) && (it != Saxon.Hej.type.BuiltInAtomicType.STRING)) && !(th.isSubType(it, Saxon.Hej.type.BuiltInAtomicType.STRING)))) {
                    throw new Saxon.Hej.trans.XPathException((("Key definition "+keyName)+" has non-string key values"));
                }
            }
        }
    
    }
}