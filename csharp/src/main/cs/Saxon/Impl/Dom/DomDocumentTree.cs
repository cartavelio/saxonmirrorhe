
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Saxon.Api;
using Saxon.Hej.functions;
using Saxon.Hej.om;
using Saxon.Hej.pattern;
using Saxon.Hej.pattern.nodetest;
using Saxon.Hej.tree.iter;
using Saxon.Hej.type.gnode;


namespace Saxon.Impl.Dom {
    /*
     * Wraps a .NET DOM tree. This is derived from the Java code in the Saxon .NET product,
     * but for SaxonCS is maintained as native C# code.
     */
    public class DomDocumentTree : Saxon.Hej.om.GenericTreeInfo {
        public DomDocumentTree(XmlDocument doc, string baseURI, Saxon.Hej.Configuration config) : base (config) {
            setRootNode(wrap(doc));
            setSystemId(baseURI);
        }
        public DomNodeWrapper wrap(XmlNode node) {
            return DomNodeWrapper.makeWrapper(node, this);
        }
        public override Saxon.Hej.om.NodeInfo selectID(string id, bool getParent) {
            XmlNode node = (XmlNode)((DomNodeWrapper)getRootNode()).getUnderlyingNode();
            if (node is XmlDocument document) {
                XmlElement target = document.GetElementById(id);
                if (target != null) {
                    return wrap(target);
                }
                NodeInfo rootNode = wrap(document);
                SequenceIterator descendants = rootNode.iterateDescendantAxis(NodeKindType.ELEMENT);
                for (NodeInfo n; (n = (NodeInfo)descendants.next()) != null;) {
                    if (id.Equals(n.getAttributeValue(NamespaceUri.XML, "id"))) {
                        return n;
                    }
                }
            }
            return null;
        }

        public override System.Collections.Generic.IEnumerator<string> getUnparsedEntityNames() {
            XmlNode node = (XmlNode)((DomNodeWrapper)getRootNode()).getUnderlyingNode();
            List<string> result = new();
            if (node is XmlDocument document) {
                XmlDocumentType docType = document.DocumentType;
                if (docType != null) {
                    result.AddRange(
                        from XmlEntity entity in docType.Entities 
                        where entity.NotationName != null 
                        select entity.Name);
                }
            }
            return result.GetEnumerator();
        }

        public override string[] getUnparsedEntity(string name) {
            XmlNode node = (XmlNode)((DomNodeWrapper)getRootNode()).getUnderlyingNode();
            if (node is XmlDocument document) {
                XmlDocumentType docType = document.DocumentType;
                if (docType != null) {
                    foreach (XmlEntity entity in docType.Entities) {
                        if (entity.NotationName != null && entity.Name.Equals(name)) {
                            string sysId = entity.SystemId;
                            if (sysId != null && document.BaseURI != null) {
                                sysId = ResolveURI.makeAbsolute(sysId, document.BaseURI).ToString();
                            }
                            return new[] { sysId, entity.PublicId };
                        }
                    }
                }
            }

            return null;
        }
    }
}

// Copyright (c) 2021 - 2026 Saxonica Limited