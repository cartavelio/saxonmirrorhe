using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XdmDestination : Saxon.Hej.s9api.AbstractDestination {
        internal Saxon.Hej.om.TreeModel treeModel = Saxon.Hej.om.TreeModel.TINY_TREE;
        internal Saxon.Hej.@event.Builder builder;
        public XdmDestination()  {
        }
        public void setBaseURI(System.Uri baseURI) {
            if (!(baseURI.IsAbsoluteUri)) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Supplied base URI must be absolute");
            }
            setDestinationBaseURI(baseURI);
        }
        public System.Uri getBaseURI() {
            return getDestinationBaseURI();
        }
        public void setTreeModel(Saxon.Hej.om.TreeModel model) {
            this.treeModel = model;
        }
        public Saxon.Hej.om.TreeModel getTreeModel() {
            return treeModel;
        }
        public override Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
            Saxon.Hej.om.TreeModel model = treeModel;
            if ((model == null)) {
                int m = pipe.getParseOptions().getTreeModel();
                if ((m != Saxon.Hej.@event.Builder.UNSPECIFIED_TREE_MODEL)) {
                    model = Saxon.Hej.om.TreeModel.getTreeModel(m);
                }
                if ((model == null)) {
                    model = Saxon.Hej.om.TreeModel.TINY_TREE;
                }
            }
            builder = model.makeBuilder(pipe);
            string systemId = ((getBaseURI() == null) ? null : getBaseURI().OriginalString);
            if ((systemId != null)) {
                builder.setUseEventLocation(false);
                builder.setBaseURI(systemId);
            }
            Saxon.Hej.@event.SequenceNormalizer sn = @params.makeSequenceNormalizer(builder);
            sn.setSystemId(systemId);
            sn.onClose(helper.getListeners());
            return sn;
        }
        public override void close() {
        }
        public Saxon.Api.XdmNode getXdmNode() {
            if ((builder == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("The document has not yet been built");
            }
            Saxon.Hej.om.NodeInfo node = builder.getCurrentRoot();
            return ((node == null) ? null : (Saxon.Api.XdmNode)(Saxon.Api.XdmValue.Wrap(node)));
        }
        public void reset() {
            builder = null;
        }
    
    }
}