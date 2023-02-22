using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public abstract class TinyParentNodeImpl : Saxon.Hej.tree.tiny.TinyNodeImpl {
        public override bool hasChildNodes() {
            return (((nodeNr+1)<tree.numberOfNodes) && (tree.depth[(nodeNr+1)]>tree.depth[nodeNr]));
        }
        public override Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return getStringValue(tree, nodeNr);
        }
        public static Saxon.Hej.str.UnicodeString getStringValue(Saxon.Hej.tree.tiny.TinyTree tree, int nodeNr) {
            int level = tree.depth[nodeNr];
            int next = (nodeNr+1);
            if ((tree.nodeKind[nodeNr] == Saxon.Hej.type.Type.TEXTUAL_ELEMENT)) {
                return Saxon.Hej.tree.tiny.TinyTextImpl.getStringValue(tree, nodeNr);
            } else {
                if ((next<tree.numberOfNodes)) {
                    if ((tree.depth[next]<=level)) {
                        return Saxon.Hej.str.EmptyUnicodeString.getInstance();
                    } else {
                        if (((tree.nodeKind[next] == Saxon.Hej.type.Type.TEXT) && ((((next+1)>=tree.numberOfNodes) || (tree.depth[(next+1)]<=level))))) {
                            return Saxon.Hej.tree.tiny.TinyTextImpl.getStringValue(tree, next);
                        }
                    }
                }
            }
            Saxon.Hej.str.UnicodeBuilder sb = null;
            while (((next<tree.numberOfNodes) && (tree.depth[next]>level))) {
                byte kind = tree.nodeKind[next];
                if (((kind == Saxon.Hej.type.Type.TEXT) || (kind == Saxon.Hej.type.Type.TEXTUAL_ELEMENT))) {
                    if ((sb == null)) {
                        sb = new Saxon.Hej.str.UnicodeBuilder();
                    }
                    sb.accept(Saxon.Hej.tree.tiny.TinyTextImpl.getStringValue(tree, next));
                } else {
                    if ((kind == Saxon.Hej.type.Type.WHITESPACE_TEXT)) {
                        if ((sb == null)) {
                            sb = new Saxon.Hej.str.UnicodeBuilder();
                        }
                        Saxon.Hej.tree.tiny.WhitespaceTextImpl.appendStringValue(tree, next, sb);
                    }
                }
                next++;
            }
            if ((sb == null)) {
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
            }
            return Saxon.Hej.str.ZenoString.of(sb.toUnicodeString());
        }
    
    }
}