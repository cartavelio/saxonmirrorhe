using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class CaseVariants {
        private static Saxon.Hej.z.IntToIntMap monoVariants = null;
        private static Saxon.Hej.z.IntHashMap<int[]> polyVariants = null;
        static void build() {
            monoVariants = new Saxon.Hej.z.IntToIntHashMap(2500);
            polyVariants = new Saxon.Hej.z.IntHashMap<int[]>(100);
            System.IO.Stream @in = Saxon.Hej.Version.platform.locateResource("casevariants.xml", new System.Collections.Generic.List<string>());
            if ((@in == null)) {
                throw new System.Exception("Unable to read casevariants.xml file");
            }
            Saxon.Hej.Configuration config = new Saxon.Hej.Configuration();
            Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions().withSchemaValidationMode(Saxon.Hej.lib.Validation.SKIP).withDTDValidationMode(Saxon.Hej.lib.Validation.SKIP).withPleaseCloseAfterUse(true);
            Saxon.Hej.om.NodeInfo doc;
            try {
                doc = config.buildDocumentTree(new Saxon.Ejavax.xml.transform.stream.StreamSource(@in, "casevariants.xml"), options).getRootNode();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new System.Exception("Failed to build casevariants.xml", e);
            }
            Saxon.Hej.tree.iter.AxisIterator iter = doc.iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.om.NamespaceUri.NULL, "c", config.getNamePool()));
            while (true) {
                Saxon.Hej.om.NodeInfo item = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next());
                if ((item == null)) {
                    break ;
                }
                string code = item.getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "n");
                int icode = Saxon.Impl.Helpers.Utils.ParseInt(code, 16);
                string variants = item.getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "v");
                string[] vhex = System.Text.RegularExpressions.Regex.Split(variants, ",");
                int[] vint = new int[vhex.Length];
                for (int i = 0; (i<vhex.Length); i++) {
                    vint[i] = Saxon.Impl.Helpers.Utils.ParseInt(vhex[i], 16);
                }
                if ((vhex.Length == 1)) {
                    monoVariants.put(icode, vint[0]);
                } else {
                    polyVariants.put(icode, vint);
                }
            }
        }
        public static int[] getCaseVariants(int code) { lock(typeof(CaseVariants)) {
            if ((monoVariants == null)) {
                build();
            }
            int mono = monoVariants.get(code);
            if ((mono != monoVariants.getDefaultValue())) {
                return new int[]{mono};
            } else {
                int[] result = polyVariants.get(code);
                if ((result == null)) {
                    return Saxon.Hej.z.IntArraySet.EMPTY_INT_ARRAY;
                } else {
                    return result;
                }
            }
        }}
        public static int[] ROMAN_VARIANTS = {0x0130, 0x0131, 0x212A, 0x017F};
    
    }
}