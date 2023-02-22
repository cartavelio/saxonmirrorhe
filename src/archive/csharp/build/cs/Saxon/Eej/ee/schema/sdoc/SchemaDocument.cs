using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class SchemaDocument {
        private readonly Saxon.Hej.om.TreeInfo document;
        public SchemaDocument(Saxon.Hej.om.TreeInfo doc)  {
            document = doc;
        }
        public Saxon.Hej.om.TreeInfo getDocument() {
            return document;
        }
        public bool isSourceSchemaDocument() {
            return (getXSDSchema() != null);
        }
        public bool isCompiledSchemaDocument() {
            Saxon.Hej.tree.iter.AxisIterator iter = document.getRootNode().iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(Saxon.Hej.type.Type.ELEMENT));
            Saxon.Hej.om.NodeInfo node = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next());
            return node.getURI().Equals(Saxon.Hej.lib.NamespaceConstant.SCM);
        }
        public Saxon.Eej.ee.schema.sdoc.XSDSchema getXSDSchema() {
            Saxon.Hej.tree.iter.AxisIterator iter = document.getRootNode().iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(Saxon.Hej.type.Type.ELEMENT));
            Saxon.Hej.om.NodeInfo node = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next());
            return ((node is Saxon.Eej.ee.schema.sdoc.XSDSchema) ? (Saxon.Eej.ee.schema.sdoc.XSDSchema)(node) : null);
        }
        public string getTargetNamespace() {
            Saxon.Eej.ee.schema.sdoc.XSDSchema root = getXSDSchema();
            return ((root == null) ? null : root.getTargetNamespace());
        }
    
    }
}