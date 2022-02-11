using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class IgnorableWhitespaceStripper : Saxon.Hej.@event.ProxyReceiver {
        private bool[] stripStack = new bool[100];
        private int top = 0;
        public IgnorableWhitespaceStripper(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
            bool strip = false;
            if ((type != Saxon.Hej.type.Untyped.getInstance())) {
                if (((type.isComplexType() && !(((Saxon.Hej.type.ComplexType)(type)).isSimpleContent())) && !(((Saxon.Hej.type.ComplexType)(type)).isMixedContent()))) {
                    strip = true;
                }
            }
            top++;
            if ((top>=stripStack.Length)) {
                stripStack = Saxon.Ejava.util.Arrays.copyOf(stripStack, (top*2));
            }
            stripStack[top] = strip;
        }
        public override void endElement() {
            nextReceiver.endElement();
            top--;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((!(chars.isEmpty()) && ((!(stripStack[top]) || !(Saxon.Hej.value.Whitespace.isAllWhite(chars)))))) {
                nextReceiver.characters(chars, locationId, properties);
            }
        }
        public override bool usesTypeAnnotations() {
            return true;
        }
    
    }
}