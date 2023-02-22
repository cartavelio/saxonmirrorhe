using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class SerializationParamsHandler {
        public static readonly string NAMESPACE = Saxon.Hej.lib.NamespaceConstant.OUTPUT;
        internal Saxon.Ejava.util.Properties properties;
        internal Saxon.Hej.serialize.CharacterMap characterMap;
        internal Saxon.Hej.s9api.Location locator;
        public SerializationParamsHandler()  {
        }
        public SerializationParamsHandler(Saxon.Ejava.util.Properties props)  {
            this.properties = props;
        }
        public void setLocator(Saxon.Hej.s9api.Location locator) {
            this.locator = locator;
        }
        public void setSerializationParams(Saxon.Hej.om.NodeInfo node) {
            if ((properties == null)) {
                properties = new Saxon.Ejava.util.Properties();
            }
            if ((node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                node = Saxon.Hej.tree.util.Navigator.getOutermostElement(node.getTreeInfo());
            }
            if ((node.getNodeKind() != Saxon.Hej.type.Type.ELEMENT)) {
                throw new Saxon.Hej.trans.XPathException("Serialization params: node must be a document or element node");
            }
            if (!(node.getLocalPart().Equals("serialization-parameters"))) {
                throw new Saxon.Hej.trans.XPathException("Serialization params: element name must be 'serialization-parameters");
            }
            if (!(node.getURI().Equals(NAMESPACE))) {
                throw new Saxon.Hej.trans.XPathException(("Serialization params: element namespace must be "+NAMESPACE));
            }
            restrictAttributes(node);
            System.Collections.Generic.ISet<Saxon.Hej.om.NodeName> nodeNames = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeName>();
            Saxon.Hej.tree.iter.AxisIterator kids = node.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            Saxon.Hej.om.NodeInfo child;
            while (((child = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next())) != null)) {
                if (!(nodeNames.Add(Saxon.Hej.om.NameOfNode.makeName(child)))) {
                    throw new Saxon.Hej.trans.XPathException(("Duplicated serialization parameter "+child.getDisplayName()), "SEPM0019");
                }
                string lname = child.getLocalPart();
                string uri = child.getURI();
                if (uri.Length==0) {
                    throw new Saxon.Hej.trans.XPathException((("Serialization parameter "+lname)+" is in no namespace"), "SEPM0017");
                }
                if (Saxon.Hej.lib.NamespaceConstant.OUTPUT.Equals(uri)) {
                    uri = "";
                }
                if (("".Equals(uri) && lname.Equals("use-character-maps"))) {
                    restrictAttributes(child);
                    Saxon.Hej.tree.iter.AxisIterator gKids = child.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                    Saxon.Hej.om.NodeInfo gChild;
                    Saxon.Hej.z.IntHashMap<string> map = new Saxon.Hej.z.IntHashMap<string>();
                    while (((gChild = ((/*1430*/Saxon.Hej.om.NodeInfo)gKids.next())) != null)) {
                        restrictAttributes(gChild, "character", "map-string");
                        if (!(((gChild.getURI().Equals(NAMESPACE) && gChild.getLocalPart().Equals("character-map"))))) {
                            if ((gChild.getURI().Equals(NAMESPACE) || gChild.getURI().Length==0)) {
                                throw new Saxon.Hej.trans.XPathException(("Invalid child of use-character-maps: "+gChild.getDisplayName()), "SEPM0017");
                            }
                        }
                        string ch = getAttribute(gChild, "character");
                        string str = getAttribute(gChild, "map-string");
                        Saxon.Hej.value.StringValue chValue = new Saxon.Hej.value.StringValue(ch);
                        if ((chValue.length() != 1)) {
                            throw new Saxon.Hej.trans.XPathException(("In the serialization parameters, the value of @character in the character map "+"must be a single Unicode character"), "SEPM0017");
                        }
                        int code = chValue.getContent().codePointAt(0);
                        string prev = map.put(code, str);
                        if ((prev != null)) {
                            throw new Saxon.Hej.trans.XPathException(("In the serialization parameters, the character map contains two entries for the character \\u"+Saxon.Impl.Helpers.Utils.intToHexString((65536+code)).Substring(1)), "SEPM0018");
                        }
                    }
                    characterMap = new Saxon.Hej.serialize.CharacterMap(Saxon.Hej.om.NameOfNode.makeName(node).getStructuredQName(), map);
                } else {
                    restrictAttributes(child, "value");
                    string value = getAttribute(child, "value");
                    try {
                        Saxon.Hej.expr.instruct.ResultDocument.setSerializationProperty(properties, uri, lname, value, child.getAllNamespaces(), false, node.getConfiguration());
                    } catch (Saxon.Hej.trans.XPathException err) {
                        if (("XQST0109".Equals(err.getErrorCodeLocalPart()) || "SEPM0016".Equals(err.getErrorCodeLocalPart()))) {
                            if ("".Equals(uri)) {
                                Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException(("Unknown serialization parameter "+Saxon.Hej.trans.Err.depict(child)), "SEPM0017");
                                e2.setLocator(locator);
                                throw e2;
                            }
                        } else {
                            throw;
                        }
                    }
                }
            }
        }
        private static void restrictAttributes(Saxon.Hej.om.NodeInfo element, params string[]  allowedNames) {
            foreach (Saxon.Hej.om.AttributeInfo att in element.attributes()) {
                Saxon.Hej.om.NodeName name = att.getNodeName();
                if (("".Equals(name.getURI()) && (Saxon.Ejava.util.Arrays.binarySearch(allowedNames, name.getLocalPart())<0))) {
                    throw new Saxon.Hej.trans.XPathException(((("In serialization parameters, attribute @"+name.getLocalPart())+" must not appear on element ")+element.getDisplayName()), "SEPM0017");
                }
            }
        }
        private static string getAttribute(Saxon.Hej.om.NodeInfo element, string localName) {
            string value = element.getAttributeValue("", localName);
            if ((value == null)) {
                throw new Saxon.Hej.trans.XPathException(((("In serialization parameters, attribute @"+localName)+" is missing on element ")+element.getDisplayName()));
            }
            return value;
        }
        public Saxon.Hej.serialize.SerializationProperties getSerializationProperties() {
            Saxon.Hej.serialize.CharacterMapIndex index = new Saxon.Hej.serialize.CharacterMapIndex();
            if ((characterMap != null)) {
                index.putCharacterMap(new Saxon.Hej.om.StructuredQName("", "", "charMap"), characterMap);
                properties.setProperty(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS, "charMap");
            }
            return new Saxon.Hej.serialize.SerializationProperties(properties, index);
        }
        public Saxon.Hej.serialize.CharacterMap getCharacterMap() {
            return characterMap;
        }
    
    }
}