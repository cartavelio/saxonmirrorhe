using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class Stripper : Saxon.Hej.@event.ProxyReceiver {
        public readonly static Saxon.Hej.@event.Stripper.StripRuleTarget STRIP = new Saxon.Hej.@event.Stripper.StripRuleTarget();
        public readonly static Saxon.Hej.@event.Stripper.StripRuleTarget PRESERVE = new Saxon.Hej.@event.Stripper.StripRuleTarget();
        protected internal Saxon.Hej.om.SpaceStrippingRule rule;
        public Stripper(Saxon.Hej.om.SpaceStrippingRule rule, Saxon.Hej.@event.Receiver next) : base (next) {
            System.Diagnostics.Debug.Assert((rule != null));
            this.rule = rule;
        }
        private int[] stripStack = new int[100];
        private int top = 0;
        private int isSpacePreserving(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType type) {
            return rule.isSpacePreserving(name, type);
        }
        public const int ALWAYS_PRESERVE = 0x01;
        public const int ALWAYS_STRIP = 0x02;
        public const int STRIP_DEFAULT = 0x00;
        public const int PRESERVE_PARENT = 0x04;
        public const int SIMPLE_CONTENT = 0x08;
        public const int ASSERTIONS_EXIST = 0x10;
        public override void open() {
            top = 0;
            stripStack[top] = ALWAYS_PRESERVE;
            base.open();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
            int preserveParent = stripStack[top];
            int preserve = (preserveParent&((PRESERVE_PARENT|ASSERTIONS_EXIST)));
            int elementStrip = isSpacePreserving(elemName, type);
            if ((elementStrip == ALWAYS_PRESERVE)) {
                preserve |= ALWAYS_PRESERVE;
            } else {
                if ((elementStrip == ALWAYS_STRIP)) {
                    preserve |= ALWAYS_STRIP;
                }
            }
            if ((type != Saxon.Hej.type.Untyped.getInstance())) {
                if ((preserve == 0)) {
                    if ((type.isSimpleType() || ((Saxon.Hej.type.ComplexType)(type)).isSimpleContent())) {
                        preserve |= SIMPLE_CONTENT;
                    }
                }
                if (((type is Saxon.Hej.type.ComplexType) && ((Saxon.Hej.type.ComplexType)(type)).hasAssertions())) {
                    preserve |= ASSERTIONS_EXIST;
                }
            }
            top++;
            if ((top>=stripStack.Length)) {
                stripStack = Saxon.Ejava.util.Arrays.copyOf(stripStack, (top*2));
            }
            stripStack[top] = preserve;
            string xmlSpace = attributes.getValue(Saxon.Hej.om.NamespaceUri.XML, "space");
            if ((xmlSpace != null)) {
                if (Saxon.Hej.value.Whitespace.trim(xmlSpace).Equals("preserve")) {
                    stripStack[top] |= PRESERVE_PARENT;
                } else {
                    stripStack[top] &= ~(PRESERVE_PARENT);
                }
            }
        }
        public override void endElement() {
            nextReceiver.endElement();
            top--;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((((((((((stripStack[top]&((((ALWAYS_PRESERVE|PRESERVE_PARENT)|SIMPLE_CONTENT)|ASSERTIONS_EXIST)))) != 0)) && (((stripStack[top]&ALWAYS_STRIP)) == 0))) || !(Saxon.Hej.value.Whitespace.isAllWhite(chars)))) && !(chars.isEmpty()))) {
                nextReceiver.characters(chars, locationId, properties);
            }
        }
        public override bool usesTypeAnnotations() {
            return true;
        }
        public class StripRuleTarget : Saxon.Hej.trans.rules.RuleTarget {
            public void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            }
            public void registerRule(Saxon.Hej.trans.rules.Rule rule) {
            }
        
        }
    
    }
}