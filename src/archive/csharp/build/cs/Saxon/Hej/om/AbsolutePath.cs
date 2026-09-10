using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class AbsolutePath {
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.AbsolutePath.PathElement> path;
        private string systemId;
        public AbsolutePath(System.Collections.Generic.ICollection<Saxon.Hej.om.AbsolutePath.PathElement> path)  {
            this.path = new System.Collections.Generic.List<Saxon.Hej.om.AbsolutePath.PathElement>(path);
        }
        public void appendAttributeName(Saxon.Hej.om.NodeName attributeName) {
            if (!(path.Count==0)) {
                Saxon.Hej.om.AbsolutePath.PathElement last = path[(path.Count-1)];
                if ((last.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE)) {
                    Saxon.Impl.Helpers.ListUtils.Remove(path, (path.Count-1));
                }
            }
            Saxon.Hej.om.AbsolutePath.PathElement att = new Saxon.Hej.om.AbsolutePath.PathElement(Saxon.Hej.type.Type.ATTRIBUTE, attributeName, 1);
            path.Add(att);
        }
        public static Saxon.Hej.om.AbsolutePath pathToNode(Saxon.Hej.om.NodeInfo node) {
            System.Collections.Generic.LinkedList<Saxon.Hej.om.AbsolutePath.PathElement> list = new System.Collections.Generic.LinkedList<Saxon.Hej.om.AbsolutePath.PathElement>();
            while (((node != null) && (node.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT))) {
                Saxon.Hej.om.AbsolutePath.PathElement pe = new Saxon.Hej.om.AbsolutePath.PathElement(node.getNodeKind(), Saxon.Hej.om.NameOfNode.makeName(node), Saxon.Hej.tree.util.Navigator.getNumberSimple(node, null));
                list.AddFirst(pe);
                node = node.getParent();
            }
            return new Saxon.Hej.om.AbsolutePath(list);
        }
        public string getPathUsingPrefixes() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            foreach (Saxon.Hej.om.AbsolutePath.PathElement pe in path) {
                fsb.append('/');
                pe.addToString(fsb, 'p');
            }
            return fsb.ToString();
        }
        public string getPathUsingUris() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            foreach (Saxon.Hej.om.AbsolutePath.PathElement pe in path) {
                fsb.append('/');
                pe.addToString(fsb, 'u');
            }
            return fsb.ToString();
        }
        public string getPathUsingAbbreviatedUris() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            foreach (Saxon.Hej.om.AbsolutePath.PathElement pe in path) {
                fsb.append('/');
                pe.addToString(fsb, 's');
            }
            return fsb.ToString();
        }
        public override string ToString() {
            return getPathUsingUris();
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Hej.om.AbsolutePath) && obj.ToString().Equals(ToString()));
        }
        public override int GetHashCode() {
            return ToString().GetHashCode();
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public string getSystemId() {
            return systemId;
        }
        public class PathElement {
            internal int nodeKind;
            internal Saxon.Hej.om.NodeName name;
            internal int index;
            public PathElement(int nodeKind, Saxon.Hej.om.NodeName name, int index)  {
                this.nodeKind = nodeKind;
                this.name = name;
                this.index = index;
            }
            public int getNodeKind() {
                return nodeKind;
            }
            public Saxon.Hej.om.NodeName getName() {
                return name;
            }
            public int getIndex() {
                return index;
            }
            public void addToString(Saxon.Ejava.lang.StringBuilder fsb, char option) {
                switch (nodeKind) {
                  case Saxon.Hej.type.Type.DOCUMENT:
                    fsb.append("(/)");
                    break ;
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                    fsb.append('@');
                    if (!(name.getNamespaceUri().isEmpty())) {
                        if ((option == 'u')) {
                            fsb.append("Q{");
                            fsb.append(name.getNamespaceUri());
                            fsb.append("}");
                        } else {
                            if ((option == 'p')) {
                                string prefix = name.getPrefix();
                                if (!(prefix.Length==0)) {
                                    fsb.append(prefix);
                                    fsb.append(':');
                                }
                            } else {
                                if ((option == 's')) {
                                    fsb.append("Q{");
                                    fsb.append(Saxon.Hej.trans.Err.abbreviateURI(name.getNamespaceUri()));
                                    fsb.append("}");
                                }
                            }
                        }
                    }
                    fsb.append(getName().getLocalPart());
                    break ;
                  case Saxon.Hej.type.Type.ELEMENT:
                    if ((option == 'u')) {
                        fsb.append("Q{");
                        fsb.append(name.getNamespaceUri());
                        fsb.append("}");
                    } else {
                        if ((option == 'p')) {
                            string prefix = name.getPrefix();
                            if (!(prefix.Length==0)) {
                                fsb.append(prefix);
                                fsb.append(':');
                            }
                        } else {
                            if ((option == 's')) {
                                if (!(name.getNamespaceUri().isEmpty())) {
                                    fsb.append("Q{");
                                    fsb.append(Saxon.Hej.trans.Err.abbreviateURI(name.getNamespaceUri()));
                                    fsb.append("}");
                                }
                            }
                        }
                    }
                    fsb.append(name.getLocalPart());
                    appendPredicate(fsb);
                    break ;
                  case Saxon.Hej.type.Type.TEXT:
                    fsb.append("text()");
                    break ;
                  case Saxon.Hej.type.Type.COMMENT:
                    fsb.append("comment()");
                    appendPredicate(fsb);
                    break ;
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    fsb.append("processing-instruction(");
                    fsb.append(name.getLocalPart());
                    fsb.append(")");
                    appendPredicate(fsb);
                    break ;
                  case Saxon.Hej.type.Type.NAMESPACE:
                    fsb.append("namespace::");
                    if (name.getLocalPart().Length==0) {
                        fsb.append((("*[Q{"+Saxon.Hej.lib.NamespaceConstant.FN)+"}local-name()=\"\"]"));
                    } else {
                        fsb.append(name.getLocalPart());
                    }
                    break ;
                  default:
                    break;
                }
            }
            private void appendPredicate(Saxon.Ejava.lang.StringBuilder fsb) {
                int index = getIndex();
                if ((index != -1)) {
                    fsb.append('[');
                    fsb.append((getIndex()+""));
                    fsb.append(']');
                }
            }
        
        }
    
    }
}