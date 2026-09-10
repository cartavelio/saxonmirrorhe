using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex.charclass {
    public class Categories {
        public class Category : Saxon.Hej.regex.charclass.CharacterClass {
            private readonly string label;
            private readonly Saxon.Hej.z.IntPredicateProxy predicate;
            public Category(string label, Saxon.Hej.z.IntPredicateProxy predicate)  {
                this.label = label;
                this.predicate = predicate;
            }
            public bool test(int value) {
                return predicate.test(value);
            }
            public bool isDisjoint(Saxon.Hej.regex.charclass.CharacterClass other) {
                if ((other is Saxon.Hej.regex.charclass.Categories.Category)) {
                    char majorCat0 = label[0];
                    string otherLabel = ((Saxon.Hej.regex.charclass.Categories.Category)(other)).label;
                    char majorCat1 = otherLabel[0];
                    return ((majorCat0 != majorCat1) || ((((label.Length>1) && (otherLabel.Length>1)) && !(label.Equals(otherLabel)))));
                } else {
                    if ((other is Saxon.Hej.regex.charclass.InverseCharacterClass)) {
                        return other.isDisjoint(this);
                    } else {
                        if ((other is Saxon.Hej.regex.charclass.SingletonCharacterClass)) {
                            return !(test(((Saxon.Hej.regex.charclass.SingletonCharacterClass)(other)).getCodepoint()));
                        } else {
                            if ((other is Saxon.Hej.regex.charclass.IntSetCharacterClass)) {
                                Saxon.Hej.z.IntSet intSet = other.getIntSet();
                                if ((intSet.size()>100)) {
                                    return false;
                                }
                                Saxon.Hej.z.IntIterator ii = intSet.iterator();
                                while (ii.hasNext()) {
                                    if (test(ii.next())) {
                                        return false;
                                    }
                                }
                                return true;
                            } else {
                                return false;
                            }
                        }
                    }
                }
            }
            public Saxon.Hej.z.IntSet getIntSet() {
                return extent(predicate);
            }
            private static Saxon.Hej.z.IntSet extent(Saxon.Hej.z.IntPredicateProxy predicate) {
                if ((predicate is Saxon.Hej.z.IntSetPredicate)) {
                    return ((Saxon.Hej.z.IntSetPredicate)(predicate)).getIntSet();
                }
                return null;
            }
        
            /* Expanding default method union from interface IntPredicateProxy */
            public virtual Saxon.Hej.z.IntPredicateProxy union(Saxon.Hej.z.IntPredicateProxy other) {
                return Saxon.Hej.z.IntPredicateLambda.of(val => (test(val) || other.test(val)));
            }

        }
        private static System.Collections.Generic.Dictionary<string,Saxon.Hej.regex.charclass.Categories.Category> CATEGORIES = null;
        static void build() {
            CATEGORIES = new System.Collections.Generic.Dictionary<string, Saxon.Hej.regex.charclass.Categories.Category>(30);
            System.IO.Stream @in = Saxon.Hej.Version.platform.locateResource("categories.xml", new System.Collections.Generic.List<string>());
            if ((@in == null)) {
                throw new System.Exception("Unable to read categories.xml file");
            }
            Saxon.Hej.Configuration config = new Saxon.Hej.Configuration();
            Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions().withSchemaValidationMode(Saxon.Hej.lib.Validation.SKIP).withDTDValidationMode(Saxon.Hej.lib.Validation.SKIP).withTreeModel(Saxon.Hej.@event.Builder.TINY_TREE).withPleaseCloseAfterUse(true);
            Saxon.Hej.om.NodeInfo doc;
            try {
                doc = config.buildDocumentTree(new Saxon.Ejavax.xml.transform.stream.StreamSource(@in, "categories.xml"), options).getRootNode();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new System.Exception("Failed to build categories.xml", e);
            }
            int fp_name = config.getNamePool().allocateFingerprint(Saxon.Hej.om.NamespaceUri.NULL, "name");
            int fp_f = config.getNamePool().allocateFingerprint(Saxon.Hej.om.NamespaceUri.NULL, "f");
            int fp_t = config.getNamePool().allocateFingerprint(Saxon.Hej.om.NamespaceUri.NULL, "t");
            Saxon.Hej.tree.iter.AxisIterator iter = doc.iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.om.NamespaceUri.NULL, "cat", config.getNamePool()));
            for (Saxon.Hej.om.NodeInfo item; ((item = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null); ) {
                string cat = ((Saxon.Hej.tree.tiny.TinyElementImpl)(item)).getAttributeValue(fp_name);
                Saxon.Hej.z.IntRangeSet irs = new Saxon.Hej.z.IntRangeSet();
                foreach (Saxon.Hej.om.NodeInfo r in item.children(Saxon.Hej.pattern.NodeKindTest.ELEMENT)) {
                    string from = ((Saxon.Hej.tree.tiny.TinyElementImpl)(r)).getAttributeValue(fp_f);
                    string to = ((Saxon.Hej.tree.tiny.TinyElementImpl)(r)).getAttributeValue(fp_t);
                    irs.addRange(Saxon.Impl.Helpers.Utils.ParseInt(from, 16), Saxon.Impl.Helpers.Utils.ParseInt(to, 16));
                }
                Saxon.Impl.Helpers.MapUtils.Put(CATEGORIES, cat, new Saxon.Hej.regex.charclass.Categories.Category(cat, new Saxon.Hej.z.IntSetPredicate(irs)));
            }
            string c = "CLMNPSZ";
            for (int i = 0; (i<c.Length); i++) {
                char ch = c[i];
                Saxon.Hej.z.IntPredicateProxy ip = null;
                foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.regex.charclass.Categories.Category> entry in CATEGORIES) {
                    if ((entry.Key[0] == ch)) {
                        ip = ((ip == null) ? entry.Value : Saxon.Hej.z.IntUnionPredicate.makeUnion(ip, entry.Value));
                    }
                }
                string label = (ch+"");
                Saxon.Impl.Helpers.MapUtils.Put(CATEGORIES, label, new Saxon.Hej.regex.charclass.Categories.Category(label, ip));
            }
        }
        public readonly static Saxon.Hej.regex.charclass.CharacterClass ESCAPE_s = new Saxon.Hej.regex.charclass.IntSetCharacterClass(Saxon.Hej.z.IntArraySet.make(new int[]{9, 10, 13, 32}, 4));
        public readonly static Saxon.Hej.regex.charclass.CharacterClass ESCAPE_S = new Saxon.Hej.regex.charclass.InverseCharacterClass(ESCAPE_s);
        public readonly static Saxon.Hej.regex.charclass.PredicateCharacterClass ESCAPE_i = new Saxon.Hej.regex.charclass.PredicateCharacterClass(value => (Saxon.Hej.serialize.charcode.XMLCharacterData.isNCNameStart11(value) || (value == ':')));
        public readonly static Saxon.Hej.regex.charclass.CharacterClass ESCAPE_I = new Saxon.Hej.regex.charclass.InverseCharacterClass(ESCAPE_i);
        public readonly static Saxon.Hej.regex.charclass.PredicateCharacterClass ESCAPE_c = new Saxon.Hej.regex.charclass.PredicateCharacterClass(value => (Saxon.Hej.serialize.charcode.XMLCharacterData.isNCName11(value) || (value == ':')));
        public readonly static Saxon.Hej.regex.charclass.CharacterClass ESCAPE_C = new Saxon.Hej.regex.charclass.InverseCharacterClass(ESCAPE_c);
        public readonly static Saxon.Hej.regex.charclass.Categories.Category ESCAPE_d = getCategory("Nd");
        public readonly static Saxon.Hej.regex.charclass.CharacterClass ESCAPE_D = new Saxon.Hej.regex.charclass.InverseCharacterClass(ESCAPE_d);
        static Saxon.Hej.regex.charclass.Categories.Category CATEGORY_P = getCategory("P");
        static Saxon.Hej.regex.charclass.Categories.Category CATEGORY_Z = getCategory("Z");
        static Saxon.Hej.regex.charclass.Categories.Category CATEGORY_C = getCategory("C");
        public readonly static Saxon.Hej.regex.charclass.PredicateCharacterClass ESCAPE_w = new Saxon.Hej.regex.charclass.PredicateCharacterClass(value => !((((CATEGORY_P.test(value) || CATEGORY_Z.test(value)) || CATEGORY_C.test(value)))));
        public readonly static Saxon.Hej.regex.charclass.CharacterClass ESCAPE_W = new Saxon.Hej.regex.charclass.InverseCharacterClass(ESCAPE_w);
        public static Saxon.Hej.regex.charclass.Categories.Category getCategory(string cat) { lock(typeof(Categories)) {
            if ((CATEGORIES == null)) {
                build();
            }
            return Saxon.Impl.Helpers.MapUtils.Get(CATEGORIES, cat);
        }}
    
    }
}