using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class UnicodeBlocks {
        private static System.Collections.Generic.IDictionary<string,Saxon.Hej.z.IntSet> blocks = null;
        public static Saxon.Hej.z.IntSet getBlock(string name) {
            if ((blocks == null)) {
                readBlocks(new Saxon.Hej.Configuration());
            }
            Saxon.Hej.z.IntSet cc = Saxon.Impl.Helpers.MapUtils.Get(blocks, name);
            if ((cc != null)) {
                return cc;
            }
            cc = Saxon.Impl.Helpers.MapUtils.Get(blocks, normalizeBlockName(name));
            return cc;
        }
        private static string normalizeBlockName(string name) {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(name.Length);
            for (int i = 0; (i<name.Length); i++) {
                char c = name[i];
                switch (c) {
                  case ' ':
                  case '\t':
                  case '\r':
                  case '\n':
                  case '_':
                    break ;
                  default:
                    fsb.append(c);
                    break ;
                }
            }
            return fsb.ToString();
        }
        private static void readBlocks(Saxon.Hej.Configuration config) { lock(typeof(UnicodeBlocks)) {
            blocks = new System.Collections.Generic.Dictionary<string, Saxon.Hej.z.IntSet>(250);
            System.IO.Stream @in = Saxon.Hej.Version.platform.locateResource("unicodeBlocks.xml", new System.Collections.Generic.List<string>());
            if ((@in == null)) {
                throw new Saxon.Hej.regex.RESyntaxException("Unable to read unicodeBlocks.xml file");
            }
            Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions();
            options.setSchemaValidationMode(Saxon.Hej.lib.Validation.SKIP);
            options.setDTDValidationMode(Saxon.Hej.lib.Validation.SKIP);
            options.setSpaceStrippingRule(Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance());
            Saxon.Hej.om.TreeInfo doc;
            try {
                doc = config.buildDocumentTree(new Saxon.Ejavax.xml.transform.stream.StreamSource(@in, "unicodeBlocks.xml"), options);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.regex.RESyntaxException(("Failed to process unicodeBlocks.xml: "+e.Message));
            }
            Saxon.Hej.tree.iter.AxisIterator iter = doc.getRootNode().iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, "", "block", config.getNamePool()));
            while (true) {
                Saxon.Hej.om.NodeInfo item = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next());
                if ((item == null)) {
                    break ;
                }
                string blockName = normalizeBlockName(item.getAttributeValue("", "name"));
                Saxon.Hej.z.IntSet range = null;
                foreach (Saxon.Hej.om.NodeInfo rangeElement in item.children(Saxon.Hej.pattern.NodeKindTest.ELEMENT)) {
                    int from = Saxon.Impl.Helpers.Utils.ParseInt(rangeElement.getAttributeValue("", "from").Substring(2), 16);
                    int to = Saxon.Impl.Helpers.Utils.ParseInt(rangeElement.getAttributeValue("", "to").Substring(2), 16);
                    Saxon.Hej.z.IntSet cr = new Saxon.Hej.z.IntBlockSet(from, to);
                    if ((range == null)) {
                        range = cr;
                    } else {
                        if ((range is Saxon.Hej.z.IntBlockSet)) {
                            range = range.mutableCopy().union(cr);
                        } else {
                            range = range.union(cr);
                        }
                    }
                }
                Saxon.Impl.Helpers.MapUtils.Put(blocks, blockName, range);
            }
        }}
    
    }
}