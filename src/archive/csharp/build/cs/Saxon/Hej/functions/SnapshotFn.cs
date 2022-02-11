using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class SnapshotFn : Saxon.Hej.functions.SystemFunction {
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            return arguments[0].getCardinality();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Sequence @in = ((arguments.Length == 0) ? context.getContextItem() : arguments[0]);
            Saxon.Hej.om.SequenceIterator iter = snapshotSequence(@in.iterate());
            return new Saxon.Hej.om.LazySequence(iter);
        }
        public static Saxon.Hej.om.SequenceIterator snapshotSequence(Saxon.Hej.om.SequenceIterator nodes) {
            return Saxon.Hej.expr.ItemMappingIterator.map(nodes, it => snapshotSingle(it));
        }
        public static Saxon.Hej.om.Item snapshotSingle(Saxon.Hej.om.Item origin) {
            if ((origin is Saxon.Hej.om.NodeInfo)) {
                if ((((Saxon.Hej.om.NodeInfo)(origin)).getParent() == null)) {
                    Saxon.Hej.tree.wrapper.VirtualCopy vc = Saxon.Hej.tree.wrapper.VirtualCopy.makeVirtualCopy((Saxon.Hej.om.NodeInfo)(origin));
                    ((/*1430*/Saxon.Hej.tree.wrapper.VirtualTreeInfo)vc.getTreeInfo()).setCopyAccumulators(true);
                    return vc;
                } else {
                    return Saxon.Hej.tree.wrapper.SnapshotNode.makeSnapshot((Saxon.Hej.om.NodeInfo)(origin));
                }
            } else {
                return origin;
            }
        }
        public static System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> makeAncestorList(Saxon.Hej.om.NodeInfo origin) {
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> ancestors = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>(20);
            Saxon.Hej.tree.iter.AxisIterator iter = origin.iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR);
            Saxon.Hej.om.NodeInfo item;
            while (((item = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                ancestors.Add(item);
            }
            return ancestors;
        }
        public static Saxon.Hej.@event.BuilderMonitor openAncestors(Saxon.Hej.om.NodeInfo origin, System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> ancestors, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NodeInfo root = origin.getRoot();
            Saxon.Hej.tree.tiny.TinyBuilder builder = new Saxon.Hej.tree.tiny.TinyBuilder(context.getController().makePipelineConfiguration());
            builder.setStatistics(context.getConfiguration().getTreeStatistics().TEMPORARY_TREE_STATISTICS);
            builder.setSystemId(root.getSystemId());
            builder.setTiming(false);
            Saxon.Hej.@event.BuilderMonitor bm = builder.getBuilderMonitor();
            bm.open();
            Saxon.Hej.om.TreeInfo source = root.getTreeInfo();
            System.Collections.Generic.IEnumerator<string> unparsedEntities = source.getUnparsedEntityNames();
            while (unparsedEntities.MoveNext()) {
                string name = unparsedEntities.Current;
                string[] properties = source.getUnparsedEntity(name);
                builder.setUnparsedEntity(name, properties[0], properties[1]);
            }
            Saxon.Hej.type.SchemaType ancestorType;
            if (context.getController().getExecutable().isSchemaAware()) {
                ancestorType = Saxon.Hej.type.AnyType.getInstance();
            } else {
                ancestorType = Saxon.Hej.type.Untyped.getInstance();
            }
            for (int i = (ancestors.Count-1); (i>=0); i--) {
                Saxon.Hej.om.NodeInfo anc = ancestors[i];
                int kind = anc.getNodeKind();
                switch (kind) {
                  case Saxon.Hej.type.Type.ELEMENT:
                    {
                        bm.startElement(Saxon.Hej.om.NameOfNode.makeName(anc), ancestorType, anc.attributes(), anc.getAllNamespaces(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                    }
                  case Saxon.Hej.type.Type.DOCUMENT:
                    {
                        bm.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                        break ;
                    }
                  default:
                    {
                        throw new Saxon.Ejava.lang.IllegalStateException(("Unknown ancestor node kind "+anc.getNodeKind()));
                    }
                }
            }
            return bm;
        }
        public static void closeAncestors(System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> ancestors, Saxon.Hej.@event.Receiver bm) {
            foreach (Saxon.Hej.om.NodeInfo anc in ancestors) {
                switch (anc.getNodeKind()) {
                  case Saxon.Hej.type.Type.ELEMENT:
                    {
                        bm.endElement();
                        break ;
                    }
                  case Saxon.Hej.type.Type.DOCUMENT:
                    {
                        bm.endDocument();
                        break ;
                    }
                  default:
                    {
                        throw new Saxon.Ejava.lang.IllegalStateException(("Unknown ancestor node kind "+anc.getNodeKind()));
                    }
                }
            }
            bm.close();
        }
        public override string getStreamerName() {
            return "SnapshotFn";
        }
    
    }
}