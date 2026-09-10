using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class CharacterMapExpander : Saxon.Hej.@event.ProxyReceiver {
        private Saxon.Hej.serialize.CharacterMap charMap;
        private bool useNullMarkers = true;
        public CharacterMapExpander(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public void setCharacterMap(Saxon.Hej.serialize.CharacterMap map) {
            charMap = map;
        }
        public Saxon.Hej.serialize.CharacterMap getCharacterMap() {
            return charMap;
        }
        public void setUseNullMarkers(bool use) {
            useNullMarkers = use;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> atts2 = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(attributes.size());
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                Saxon.Hej.str.UnicodeString oldValue = Saxon.Hej.str.StringView.of(att.getValue()).tidy();
                if (!(Saxon.Hej.@event.ReceiverOption.contains(att.getProperties(), Saxon.Hej.@event.ReceiverOption.DISABLE_CHARACTER_MAPS))) {
                    Saxon.Hej.str.UnicodeString mapped = charMap.map(oldValue, useNullMarkers);
                    if ((mapped != oldValue)) {
                        int p2 = (((att.getProperties()|Saxon.Hej.@event.ReceiverOption.USE_NULL_MARKERS))&~(Saxon.Hej.@event.ReceiverOption.NO_SPECIAL_CHARS));
                        atts2.Add(new Saxon.Hej.om.AttributeInfo(att.getNodeName(), att.getType(), mapped.ToString(), att.getLocation(), p2));
                    } else {
                        atts2.Add(att);
                    }
                } else {
                    atts2.Add(att);
                }
            }
            nextReceiver.startElement(elemName, type, Saxon.Hej.om.SequenceTool.attributeMapFromList(atts2), namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.DISABLE_CHARACTER_MAPS))) {
                Saxon.Hej.str.UnicodeString mapped = charMap.map(chars, useNullMarkers);
                if ((mapped != chars)) {
                    properties = (((properties|Saxon.Hej.@event.ReceiverOption.USE_NULL_MARKERS))&~(Saxon.Hej.@event.ReceiverOption.NO_SPECIAL_CHARS));
                }
                nextReceiver.characters(mapped, locationId, properties);
            } else {
                nextReceiver.characters(chars, locationId, properties);
            }
        }
    
    }
}