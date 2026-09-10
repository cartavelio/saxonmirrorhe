using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class SerializationProperties {
        internal Saxon.Ejava.util.Properties properties;
        internal Saxon.Hej.serialize.CharacterMapIndex charMapIndex;
        internal Saxon.Hej.@event.FilterFactory validationFactory;
        public SerializationProperties()  {
            this.properties = new Saxon.Ejava.util.Properties();
        }
        public SerializationProperties(Saxon.Ejava.util.Properties props)  {
            this.properties = props;
        }
        public SerializationProperties(Saxon.Ejava.util.Properties props, Saxon.Hej.serialize.CharacterMapIndex charMapIndex)  {
            this.properties = props;
            this.charMapIndex = charMapIndex;
        }
        public void setProperty(string name, string value) {
            properties.setProperty(name, value);
        }
        public string getProperty(string name) {
            return getProperties().getProperty(name);
        }
        public Saxon.Ejava.util.Properties getProperties() {
            return properties;
        }
        public Saxon.Hej.serialize.CharacterMapIndex getCharacterMapIndex() {
            return charMapIndex;
        }
        public void setValidationFactory(Saxon.Hej.@event.FilterFactory validationFactory) {
            this.validationFactory = validationFactory;
        }
        public Saxon.Hej.@event.FilterFactory getValidationFactory() {
            return validationFactory;
        }
        public Saxon.Hej.@event.SequenceNormalizer makeSequenceNormalizer(Saxon.Hej.@event.Receiver next) {
            if ((getValidationFactory() != null)) {
                next = /*Delegate.makeFilter()*/getValidationFactory()(next);
            }
            string itemSeparator = properties.getProperty(Saxon.Hej.lib.SaxonOutputKeys.ITEM_SEPARATOR);
            if (((itemSeparator == null) || "#absent".Equals(itemSeparator))) {
                return new Saxon.Hej.@event.SequenceNormalizerWithSpaceSeparator(next);
            } else {
                return new Saxon.Hej.@event.SequenceNormalizerWithItemSeparator(next, Saxon.Hej.str.StringView.of(itemSeparator));
            }
        }
        public Saxon.Hej.serialize.SerializationProperties combineWith(Saxon.Hej.serialize.SerializationProperties defaults) {
            Saxon.Hej.serialize.CharacterMapIndex charMap = this.charMapIndex;
            if (((charMap == null) || charMap.isEmpty())) {
                charMap = defaults.getCharacterMapIndex();
            }
            Saxon.Hej.@event.FilterFactory validationFactory = this.validationFactory;
            if ((validationFactory == null)) {
                validationFactory = defaults.validationFactory;
            }
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties(defaults.getProperties());
            foreach (string prop in this.getProperties().stringPropertyNames()) {
                string value = this.getProperties().getProperty(prop);
                if (((prop.Equals(Saxon.Ejavax.xml.transform.OutputKeys.CDATA_SECTION_ELEMENTS) || prop.Equals(Saxon.Hej.lib.SaxonOutputKeys.SUPPRESS_INDENTATION)) || prop.Equals(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS))) {
                    string existing = defaults.getProperty(prop);
                    if (((existing == null) || existing.Equals(value))) {
                        props.setProperty(prop, value);
                    } else {
                        props.setProperty(prop, ((existing+" ")+value));
                        if (prop.Equals(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS)) {
                            Saxon.Hej.serialize.CharacterMapIndex charMapIndex2 = charMap.copy();
                            foreach (Saxon.Hej.serialize.CharacterMap map in defaults.getCharacterMapIndex()) {
                                charMapIndex2.putCharacterMap(map.getName(), map);
                            }
                            charMap = charMapIndex2;
                        }
                    }
                } else {
                    props.setProperty(prop, value);
                }
            }
            Saxon.Hej.serialize.SerializationProperties newParams = new Saxon.Hej.serialize.SerializationProperties(props, charMap);
            newParams.setValidationFactory(validationFactory);
            return newParams;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            foreach (string k in properties.stringPropertyNames()) {
                sb.append(k).append("=").append(properties.getProperty(k)).append(" ");
            }
            if ((charMapIndex != null)) {
                foreach (Saxon.Hej.serialize.CharacterMap cm in charMapIndex) {
                    sb.append(cm.getName().getEQName()).append("={").append(cm.ToString()).append("} ");
                }
            }
            return sb.ToString();
        }
    
    }
}