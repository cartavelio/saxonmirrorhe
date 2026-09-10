using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class VirtualUntypedCopy : Saxon.Hej.tree.wrapper.VirtualCopy {
        public static Saxon.Hej.tree.wrapper.VirtualCopy makeVirtualUntypedTree(Saxon.Hej.om.NodeInfo original, Saxon.Hej.om.NodeInfo root) {
            Saxon.Hej.tree.wrapper.VirtualCopy vc;
            while (((original is Saxon.Hej.tree.wrapper.VirtualUntypedCopy) && (original.getParent() == null))) {
                original = ((Saxon.Hej.tree.wrapper.VirtualUntypedCopy)(original)).original;
                root = ((Saxon.Hej.tree.wrapper.VirtualUntypedCopy)(root)).original;
            }
            vc = new Saxon.Hej.tree.wrapper.VirtualUntypedCopy(original, root);
            Saxon.Hej.Configuration config = original.getConfiguration();
            Saxon.Hej.tree.wrapper.VirtualTreeInfo doc = new Saxon.Hej.tree.wrapper.VirtualTreeInfo(config, vc);
            vc.tree = doc;
            return vc;
        }
        protected internal VirtualUntypedCopy(Saxon.Hej.om.NodeInfo @base, Saxon.Hej.om.NodeInfo root) : base (@base, root) {
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.ELEMENT:
                return Saxon.Hej.type.Untyped.getInstance();
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
              default:
                return base.getSchemaType();
            }
        }
        public override Saxon.Hej.om.AtomicSequence atomize() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.ELEMENT:
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return new Saxon.Hej.value.UntypedAtomicValue(getUnicodeStringValue());
              default:
                return base.atomize();
            }
        }
        public override void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            base.copy(@out, (copyOptions&~(Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS)), locationId);
        }
        protected internal override Saxon.Hej.tree.wrapper.VirtualCopy wrap(Saxon.Hej.om.NodeInfo node) {
            Saxon.Hej.tree.wrapper.VirtualUntypedCopy vc = new Saxon.Hej.tree.wrapper.VirtualUntypedCopy(node, root);
            vc.tree = tree;
            return vc;
        }
        public override bool isNilled() {
            return false;
        }
    
    }
}