using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public abstract class TreeModel {
        public readonly static Saxon.Hej.om.TreeModel TINY_TREE = new Saxon.Hej.om.TreeModel.TinyTree();
        public readonly static Saxon.Hej.om.TreeModel TINY_TREE_CONDENSED = new Saxon.Hej.om.TreeModel.TinyTreeCondensed();
        public readonly static Saxon.Hej.om.TreeModel LINKED_TREE = new Saxon.Hej.om.TreeModel.LinkedTree();
        public abstract Saxon.Hej.@event.Builder makeBuilder(Saxon.Hej.@event.PipelineConfiguration pipe);
        public virtual int getSymbolicValue() {
            return Saxon.Hej.@event.Builder.UNSPECIFIED_TREE_MODEL;
        }
        public static Saxon.Hej.om.TreeModel getTreeModel(int symbolicValue) {
            switch (symbolicValue) {
              case Saxon.Hej.@event.Builder.TINY_TREE:
                return Saxon.Hej.om.TreeModel.TINY_TREE;
              case Saxon.Hej.@event.Builder.TINY_TREE_CONDENSED:
                return Saxon.Hej.om.TreeModel.TINY_TREE_CONDENSED;
              case Saxon.Hej.@event.Builder.LINKED_TREE:
                return Saxon.Hej.om.TreeModel.LINKED_TREE;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("tree model "+symbolicValue));
            }
        }
        public bool isMutable() {
            return false;
        }
        public virtual bool isSchemaAware() {
            return false;
        }
        public virtual string getName() {
            return ToString();
        }
        private class TinyTree : Saxon.Hej.om.TreeModel {
            public override Saxon.Hej.@event.Builder makeBuilder(Saxon.Hej.@event.PipelineConfiguration pipe) {
                Saxon.Hej.tree.tiny.TinyBuilder builder = new Saxon.Hej.tree.tiny.TinyBuilder(pipe);
                builder.setStatistics(pipe.getConfiguration().getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
                return builder;
            }
            public override int getSymbolicValue() {
                return Saxon.Hej.@event.Builder.TINY_TREE;
            }
            public override bool isSchemaAware() {
                return true;
            }
            public override string getName() {
                return "TinyTree";
            }
        
        }
        private class TinyTreeCondensed : Saxon.Hej.om.TreeModel {
            public override Saxon.Hej.@event.Builder makeBuilder(Saxon.Hej.@event.PipelineConfiguration pipe) {
                Saxon.Hej.tree.tiny.TinyBuilderCondensed tbc = new Saxon.Hej.tree.tiny.TinyBuilderCondensed(pipe);
                tbc.setStatistics(pipe.getConfiguration().getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
                return tbc;
            }
            public override int getSymbolicValue() {
                return Saxon.Hej.@event.Builder.TINY_TREE_CONDENSED;
            }
            public override bool isSchemaAware() {
                return true;
            }
            public override string getName() {
                return "TinyTreeCondensed";
            }
        
        }
        private class LinkedTree : Saxon.Hej.om.TreeModel {
            public override Saxon.Hej.@event.Builder makeBuilder(Saxon.Hej.@event.PipelineConfiguration pipe) {
                return new Saxon.Hej.tree.linked.LinkedTreeBuilder(pipe);
            }
            public override int getSymbolicValue() {
                return Saxon.Hej.@event.Builder.LINKED_TREE;
            }
            public override bool isSchemaAware() {
                return true;
            }
            public override string getName() {
                return "LinkedTree";
            }
        
        }
    
    }
}