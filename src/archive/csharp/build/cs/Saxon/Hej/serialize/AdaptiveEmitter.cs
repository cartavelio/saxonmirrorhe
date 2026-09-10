using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class AdaptiveEmitter : Saxon.Hej.@event.SequenceWriter, Saxon.Hej.@event.ReceiverWithOutputProperties {
        private readonly Saxon.Hej.str.UnicodeWriter writer;
        private Saxon.Hej.serialize.CharacterMap characterMap;
        private Saxon.Ejava.util.Properties outputProperties;
        private string itemSeparator = "\n";
        private bool started = false;
        public AdaptiveEmitter(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.str.UnicodeWriter writer) : base (pipe) {
            this.writer = writer;
        }
        public void setOutputProperties(Saxon.Ejava.util.Properties props) {
            outputProperties = props;
            string sep = props.getProperty(Saxon.Hej.lib.SaxonOutputKeys.ITEM_SEPARATOR);
            if (((sep != null) && !("#absent".Equals(sep)))) {
                itemSeparator = sep;
            }
        }
        public void setNormalizationForm(Saxon.Ejava.text.Normalizer.Form normalizationForm) {
        }
        public void setCharacterMap(Saxon.Hej.serialize.CharacterMap map) {
            this.characterMap = map;
        }
        public Saxon.Ejava.util.Properties getOutputProperties() {
            return outputProperties;
        }
        private void emit(string s) {
            try {
                writer.write(s);
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        private void emit(Saxon.Hej.str.UnicodeString s) {
            try {
                writer.write(s);
            } catch (System.IO.IOException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        public override void write(Saxon.Hej.om.Item item) {
            if (started) {
                emit(itemSeparator);
            } else {
                started = true;
            }
            serializeItem(item);
        }
        private void serializeItem(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Hej.value.AtomicValue)) {
                emit(serializeAtomicValue((Saxon.Hej.value.AtomicValue)(item)));
            } else {
                if ((item is Saxon.Hej.om.NodeInfo)) {
                    serializeNode((Saxon.Hej.om.NodeInfo)(item));
                } else {
                    if ((item is Saxon.Hej.ma.map.MapItem)) {
                        serializeMap((Saxon.Hej.ma.map.MapItem)(item));
                    } else {
                        if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                            serializeArray((Saxon.Hej.ma.arrays.ArrayItem)(item));
                        } else {
                            if ((item is Saxon.Hej.om.Function)) {
                                serializeFunction((Saxon.Hej.om.Function)(item));
                            }
                        }
                    }
                }
            }
        }
        static Saxon.Hej.regex.ARegularExpression QUOTES = Saxon.Hej.regex.ARegularExpression.compile("\"", "");
        private string serializeAtomicValue(Saxon.Hej.value.AtomicValue value) {
            switch (value.getPrimitiveType().getFingerprint()) {
              case Saxon.Hej.om.StandardNames.XS_STRING:
              case Saxon.Hej.om.StandardNames.XS_ANY_URI:
              case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                {
                    Saxon.Hej.str.UnicodeString s = value.getUnicodeStringValue();
                    s = QUOTES.replace(s, Saxon.Hej.str.BMPString.of("\"\""));
                    if ((characterMap != null)) {
                        s = characterMap.map(s, false);
                    }
                    return (("\""+s.ToString())+"\"");
                }
              case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
                return (value.effectiveBooleanValue() ? "true()" : "false()");
              case Saxon.Hej.om.StandardNames.XS_DECIMAL:
              case Saxon.Hej.om.StandardNames.XS_INTEGER:
                return value.getStringValue();
              case Saxon.Hej.om.StandardNames.XS_DOUBLE:
                return Saxon.Hej.functions.FormatNumber.formatExponential((Saxon.Hej.value.DoubleValue)(value));
              case Saxon.Hej.om.StandardNames.XS_FLOAT:
              case Saxon.Hej.om.StandardNames.XS_DURATION:
              case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
              case Saxon.Hej.om.StandardNames.XS_DATE:
              case Saxon.Hej.om.StandardNames.XS_TIME:
              case Saxon.Hej.om.StandardNames.XS_G_YEAR_MONTH:
              case Saxon.Hej.om.StandardNames.XS_G_MONTH:
              case Saxon.Hej.om.StandardNames.XS_G_MONTH_DAY:
              case Saxon.Hej.om.StandardNames.XS_G_YEAR:
              case Saxon.Hej.om.StandardNames.XS_G_DAY:
              case Saxon.Hej.om.StandardNames.XS_HEX_BINARY:
              case Saxon.Hej.om.StandardNames.XS_BASE64_BINARY:
                return (((value.getPrimitiveType().getDisplayName()+"(\"")+value.getUnicodeStringValue())+"\")");
              case Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION:
              case Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION:
                return (("xs:duration(\""+value.getUnicodeStringValue())+"\")");
              case Saxon.Hej.om.StandardNames.XS_QNAME:
              case Saxon.Hej.om.StandardNames.XS_NOTATION:
                return ((Saxon.Hej.value.QualifiedNameValue)(value)).getStructuredQName().getEQName();
              default:
                return "***";
            }
        }
        private void serializeFunction(Saxon.Hej.om.Function fn) {
            Saxon.Hej.om.StructuredQName fname = fn.getFunctionName();
            if (((fname == null) || fname.hasURI(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS))) {
                emit("(anonymous-function)");
            } else {
                if (fname.hasURI(Saxon.Hej.lib.NamespaceConstant.FN)) {
                    emit(("fn:"+fname.getLocalPart()));
                } else {
                    if (fname.hasURI(Saxon.Hej.lib.NamespaceConstant.MATH)) {
                        emit(("math:"+fname.getLocalPart()));
                    } else {
                        if (fname.hasURI(Saxon.Hej.lib.NamespaceConstant.MAP_FUNCTIONS)) {
                            emit(("map:"+fname.getLocalPart()));
                        } else {
                            if (fname.hasURI(Saxon.Hej.lib.NamespaceConstant.ARRAY_FUNCTIONS)) {
                                emit(("array:"+fname.getLocalPart()));
                            } else {
                                if (fname.hasURI(Saxon.Hej.lib.NamespaceConstant.SCHEMA)) {
                                    emit(("xs:"+fname.getLocalPart()));
                                } else {
                                    emit(fname.getEQName());
                                }
                            }
                        }
                    }
                }
            }
            emit(("#"+fn.getArity()));
        }
        private void serializeNode(Saxon.Hej.om.NodeInfo node) {
            switch (node.getNodeKind()) {
              case Saxon.Hej.type.Type.ATTRIBUTE:
                emit(node.getDisplayName());
                emit("=\"");
                emit(node.getUnicodeStringValue());
                emit("\"");
                break ;
              case Saxon.Hej.type.Type.NAMESPACE:
                emit((node.getLocalPart().Length==0 ? "xmlns" : ("xmlns:"+node.getLocalPart())));
                emit("=\"");
                emit(node.getUnicodeStringValue());
                emit("\"");
                break ;
              default:
                System.IO.StringWriter sw = new System.IO.StringWriter();
                Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties(outputProperties);
                props.setProperty("method", "xml");
                props.setProperty("indent", "no");
                if ((props.getProperty("omit-xml-declaration") == null)) {
                    props.setProperty("omit-xml-declaration", "no");
                }
                props.setProperty(Saxon.Hej.lib.SaxonOutputKeys.UNFAILING, "yes");
                Saxon.Hej.serialize.CharacterMapIndex cmi = null;
                if ((characterMap != null)) {
                    cmi = new Saxon.Hej.serialize.CharacterMapIndex();
                    cmi.putCharacterMap(characterMap.getName(), characterMap);
                }
                Saxon.Hej.serialize.SerializationProperties sProps = new Saxon.Hej.serialize.SerializationProperties(props, cmi);
                Saxon.Hej.query.QueryResult.serialize(node, new Saxon.Ejavax.xml.transform.stream.StreamResult(sw), sProps);
                emit(sw.ToString().Trim());
                break ;
            }
        }
        private void serializeArray(Saxon.Hej.ma.arrays.ArrayItem array) {
            emit("[");
            bool first = true;
            foreach (Saxon.Hej.om.Sequence seq in array.members()) {
                if (first) {
                    first = false;
                } else {
                    emit(",");
                }
                outputInternalSequence(seq);
            }
            emit("]");
        }
        private void serializeMap(Saxon.Hej.ma.map.MapItem map) {
            emit("map{");
            bool first = true;
            foreach (Saxon.Hej.ma.map.KeyValuePair pair in map.keyValuePairs()) {
                if (first) {
                    first = false;
                } else {
                    emit(",");
                }
                serializeItem(pair.key);
                emit(":");
                Saxon.Hej.om.Sequence value = pair.value;
                outputInternalSequence(value);
            }
            emit("}");
        }
        private void outputInternalSequence(Saxon.Hej.om.Sequence value) {
            bool first = true;
            Saxon.Hej.om.Item it;
            Saxon.Hej.om.SequenceIterator iter = value.iterate();
            bool omitParens = ((value is Saxon.Hej.om.GroundedValue) && (((Saxon.Hej.om.GroundedValue)(value)).getLength() == 1));
            if (!(omitParens)) {
                emit("(");
            }
            while (((it = iter.next()) != null)) {
                if (!(first)) {
                    emit(",");
                }
                first = false;
                serializeItem(it);
            }
            if (!(omitParens)) {
                emit(")");
            }
        }
        public override void close() {
            base.close();
            if ((writer != null)) {
                try {
                    writer.close();
                } catch (System.IO.IOException e) {
                    throw new Saxon.Hej.trans.XPathException(e);
                }
            }
        }
    
    }
}