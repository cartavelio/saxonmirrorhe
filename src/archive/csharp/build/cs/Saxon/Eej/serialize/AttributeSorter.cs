using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.serialize {
    public class AttributeSorter : Saxon.Hej.@event.ProxyReceiver {
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.NodeName,int> knownAttributes;
        private const int MIDDLE_GROUND = 10000000;
        public AttributeSorter(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public void setOutputProperties(Saxon.Ejava.util.Properties details) {
            string attOrder;
            if ("yes".Equals(details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.CANONICAL))) {
                attOrder = "";
            } else {
                attOrder = details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER);
                if ((attOrder == null)) {
                    attOrder = "";
                }
            }
            knownAttributes = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName, int>();
            int pos = 0;
            Saxon.Ejava.util.StringTokenizer st2 = new Saxon.Ejava.util.StringTokenizer(attOrder, " \t\n\r", false);
            while (st2.hasMoreTokens()) {
                string expandedName = st2.nextToken();
                if (expandedName.Equals("*")) {
                    pos += MIDDLE_GROUND;
                } else {
                    Saxon.Hej.om.StructuredQName sq = Saxon.Hej.om.StructuredQName.fromClarkName(expandedName);
                    Saxon.Impl.Helpers.MapUtils.Put(knownAttributes, new Saxon.Hej.om.FingerprintedQName("", sq.getURI(), sq.getLocalPart()), pos++);
                }
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributesIn, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if ((attributesIn.size()<=1)) {
                nextReceiver.startElement(elemName, type, attributesIn, namespaces, location, properties);
            } else {
                System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo> attributes = attributesIn.asList();
                Saxon.Impl.Helpers.ListUtils.Sort(attributes, (a, b) => {
    Saxon.Hej.om.NodeName n0 = a.getNodeName();
    Saxon.Hej.om.NodeName n1 = b.getNodeName();
    int r0 = Saxon.Impl.Helpers.MapUtils.GetOrDefault(knownAttributes, n0, System.Int32.MinValue);
    int r1 = Saxon.Impl.Helpers.MapUtils.GetOrDefault(knownAttributes, n1, System.Int32.MinValue);
    if ((r0 != System.Int32.MinValue)) {
        if ((r1 == System.Int32.MinValue)) {
            return r0.CompareTo(MIDDLE_GROUND);
        } else {
            return r0.CompareTo(r1);
        }
    } else {
        if ((r1 != System.Int32.MinValue)) {
            return MIDDLE_GROUND.CompareTo(r1);
        } else {
            int x = System.String.Compare(n0.getURI(), n1.getURI(), System.StringComparison.Ordinal);
            if ((x == 0)) {
                x = System.String.Compare(n0.getLocalPart(), n1.getLocalPart(), System.StringComparison.Ordinal);
            }
            return x;
        }
    }
} );
                Saxon.Hej.om.AttributeMap sorted = Saxon.Hej.om.SequenceTool.attributeMapFromList(attributes);
                nextReceiver.startElement(elemName, type, sorted, namespaces, location, properties);
            }
        }
    
    }
}