using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class PathMaintainer : Saxon.Hej.@event.ProxyReceiver {
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.AbsolutePath.PathElement> path = new System.Collections.Generic.Stack<Saxon.Hej.om.AbsolutePath.PathElement>();
        private readonly System.Collections.Generic.Stack<System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName,int>> siblingCounters = new System.Collections.Generic.Stack<System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName, int>>();
        public PathMaintainer(Saxon.Hej.@event.Receiver next) : base (next) {
            siblingCounters.Push(new System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName, int>());
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
            System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName,int> counters = siblingCounters.Peek();
            int preceding = Saxon.Impl.Helpers.MapUtils.GetOrDefault(counters, elemName, 0);
            int index = (preceding+1);
            Saxon.Impl.Helpers.MapUtils.Put(counters, elemName, index);
            path.Push(new Saxon.Hej.om.AbsolutePath.PathElement(Saxon.Hej.type.Type.ELEMENT, elemName, index));
            siblingCounters.Push(new System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName, int>());
        }
        public override void endElement() {
            nextReceiver.endElement();
            siblingCounters.Pop();
            path.Pop();
        }
        public string getPath(bool useURIs) {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            foreach (Saxon.Hej.om.AbsolutePath.PathElement pe in path) {
                fsb.append('/');
                if (useURIs) {
                    Saxon.Hej.om.NamespaceUri uri = pe.getName().getNamespaceUri();
                    if (!(uri.isEmpty())) {
                        fsb.append('"');
                        fsb.append(uri);
                        fsb.append('"');
                    }
                } else {
                    string prefix = pe.getName().getPrefix();
                    if (!(prefix.Length==0)) {
                        fsb.append(prefix);
                        fsb.append(':');
                    }
                }
                fsb.append(pe.getName().getLocalPart());
                fsb.append('[');
                fsb.append((pe.getIndex()+""));
                fsb.append(']');
            }
            return fsb.ToString();
        }
        public Saxon.Hej.om.AbsolutePath getAbsolutePath() {
            return new Saxon.Hej.om.AbsolutePath(new List<Saxon.Hej.om.AbsolutePath.PathElement>(path.ToArray()));
        }
    
    }
}