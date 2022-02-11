using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class JSONSerializer : Saxon.Hej.@event.SequenceWriter, Saxon.Hej.@event.ReceiverWithOutputProperties {
        private bool allowDuplicateKeys = false;
        private string nodeOutputMethod = "xml";
        private int level = 0;
        private int topLevelCount = 0;
        private int maxLineLength = 80;
        private readonly Saxon.Hej.serialize.JSONEmitter emitter;
        private Saxon.Ejava.util.Properties outputProperties;
        private Saxon.Hej.serialize.charcode.CharacterSet characterSet;
        private bool isIndenting;
        private System.Collections.Generic.Comparer<Saxon.Hej.value.AtomicValue> propertySorter;
        private bool unfailing = false;
        public JSONSerializer(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.JSONEmitter emitter, Saxon.Ejava.util.Properties outputProperties) : base (pipe) {
            setOutputProperties(outputProperties);
            this.emitter = emitter;
        }
        public void setOutputProperties(Saxon.Ejava.util.Properties details) {
            this.outputProperties = details;
            if ("yes".Equals(details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.ALLOW_DUPLICATE_NAMES))) {
                allowDuplicateKeys = true;
            }
            if ("yes".Equals(details.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.INDENT))) {
                isIndenting = true;
            }
            if ("yes".Equals(details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.UNFAILING))) {
                unfailing = true;
                allowDuplicateKeys = true;
            }
            string jnom = details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.JSON_NODE_OUTPUT_METHOD);
            if ((jnom != null)) {
                nodeOutputMethod = jnom;
            }
            string max = details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.LINE_LENGTH);
            if ((max != null)) {
                try {
                    maxLineLength = Saxon.Impl.Helpers.Utils.ParseInt(max);
                } catch (Saxon.Ejava.lang.NumberFormatException err) {
                }
            }
        }
        public void setPropertySorter(System.Collections.Generic.Comparer<Saxon.Hej.value.AtomicValue> sorter) {
            this.propertySorter = sorter;
        }
        public Saxon.Ejava.util.Properties getOutputProperties() {
            return outputProperties;
        }
        public void setNormalizationForm(Saxon.Ejava.text.Normalizer.Form form) {
            emitter.setNormalizationForm(form);
        }
        public void setCharacterMap(Saxon.Hej.serialize.CharacterMap map) {
            emitter.setCharacterMap(map);
        }
        public override void write(Saxon.Hej.om.Item item) {
            if (((level == 0) && (++topLevelCount>=2))) {
                throw new Saxon.Hej.trans.XPathException("JSON output method cannot handle sequences of two or more items", "SERE0023");
            }
            if ((item is Saxon.Hej.value.AtomicValue)) {
                emitter.writeAtomicValue((Saxon.Hej.value.AtomicValue)(item));
            } else {
                if ((item is Saxon.Hej.ma.map.MapItem)) {
                    System.Collections.Generic.ISet<string> keys = null;
                    if (!(allowDuplicateKeys)) {
                        keys = new System.Collections.Generic.HashSet<string>();
                    }
                    bool oneLiner = (!(isIndenting) || isOneLinerMap((Saxon.Hej.ma.map.MapItem)(item)));
                    emitter.startMap(oneLiner);
                    System.Collections.Generic.List<Saxon.Hej.value.AtomicValue> keyList = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                    foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((Saxon.Hej.ma.map.MapItem)(item)).keyValuePairs()) {
                        keyList.Add(pair.key);
                    }
                    if ((propertySorter != null)) {
                        Saxon.Impl.Helpers.ListUtils.Sort(keyList, propertySorter);
                    }
                    foreach (Saxon.Hej.value.AtomicValue key in keyList) {
                        string stringKey = key.getStringValue();
                        emitter.writeKey(stringKey);
                        if ((!(allowDuplicateKeys) && !(keys.Add(stringKey)))) {
                            throw new Saxon.Hej.trans.XPathException((("Key value \""+stringKey)+"\" occurs more than once in JSON map"), "SERE0022");
                        }
                        Saxon.Hej.om.Sequence value = ((Saxon.Hej.ma.map.MapItem)(item)).get(key);
                        writeSequence(value.materialize());
                    }
                    emitter.endMap();
                } else {
                    if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                        bool oneLiner = (!(isIndenting) || isOneLinerArray((Saxon.Hej.ma.arrays.ArrayItem)(item)));
                        emitter.startArray(oneLiner);
                        foreach (Saxon.Hej.om.Sequence member in ((Saxon.Hej.ma.arrays.ArrayItem)(item)).members()) {
                            writeSequence(member.materialize());
                        }
                        emitter.endArray();
                    } else {
                        if ((item is Saxon.Hej.om.NodeInfo)) {
                            string s = serializeNode((Saxon.Hej.om.NodeInfo)(item));
                            emitter.writeAtomicValue(new Saxon.Hej.value.StringValue(s));
                        } else {
                            if (unfailing) {
                                Saxon.Hej.str.UnicodeString s = item.getUnicodeStringValue();
                                emitter.writeAtomicValue(new Saxon.Hej.value.StringValue(s));
                            } else {
                                throw new Saxon.Hej.trans.XPathException(("JSON output method cannot handle an item of type "+item.GetType()), "SERE0021");
                            }
                        }
                    }
                }
            }
        }
        private bool isOneLinerArray(Saxon.Hej.ma.arrays.ArrayItem array) {
            int totalSize = 0;
            if ((array.arrayLength()<2)) {
                return true;
            }
            foreach (Saxon.Hej.om.Sequence member in array.members()) {
                if (!(((member is Saxon.Hej.value.AtomicValue)))) {
                    return false;
                }
                totalSize += ((int)(((Saxon.Hej.value.AtomicValue)(member)).getUnicodeStringValue().estimatedLength())+1);
                if ((totalSize>maxLineLength)) {
                    return false;
                }
            }
            return true;
        }
        private bool isOneLinerMap(Saxon.Hej.ma.map.MapItem map) {
            long totalSize = 0;
            if ((map.size()<2)) {
                return true;
            }
            foreach (Saxon.Hej.ma.map.KeyValuePair entry in map.keyValuePairs()) {
                if ((entry.value is Saxon.Hej.value.AtomicValue)) {
                    totalSize += (((int)(entry.key.getUnicodeStringValue().estimatedLength())+((Saxon.Hej.value.AtomicValue)(entry.value)).getUnicodeStringValue().estimatedLength())+4);
                } else {
                    if ((entry.value.getLength() == 0)) {
                        totalSize += ((int)(entry.key.getUnicodeStringValue().estimatedLength())+6);
                    } else {
                        return false;
                    }
                }
                if ((totalSize>maxLineLength)) {
                    return false;
                }
            }
            return true;
        }
        private string serializeNode(Saxon.Hej.om.NodeInfo node) {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            props.setProperty("method", nodeOutputMethod);
            props.setProperty("indent", "no");
            props.setProperty("omit-xml-declaration", "yes");
            Saxon.Hej.query.QueryResult.serialize(node, new Saxon.Ejavax.xml.transform.stream.StreamResult(sw), props);
            return sw.ToString().Trim();
        }
        private void writeSequence(Saxon.Hej.om.GroundedValue seq) {
            int len = seq.getLength();
            if ((len == 0)) {
                emitter.writeAtomicValue(null);
            } else {
                if ((len == 1)) {
                    level++;
                    write(seq.head());
                    level--;
                } else {
                    throw new Saxon.Hej.trans.XPathException(((("JSON serialization: cannot handle a sequence of length "+len)+" ")+Saxon.Hej.trans.Err.depictSequence(seq)), "SERE0023");
                }
            }
        }
        public override void close() {
            if ((topLevelCount == 0)) {
                emitter.writeAtomicValue(null);
            }
            emitter.close();
            base.close();
        }
    
    }
}