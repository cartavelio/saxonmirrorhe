using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface NodeInfo : Saxon.Hej.om.Item, Saxon.Hej.s9api.Location {
        Saxon.Hej.om.TreeInfo getTreeInfo();
        Saxon.Hej.Configuration getConfiguration();
        int getNodeKind();
        bool isSameNodeInfo(Saxon.Hej.om.NodeInfo other);
        string getBaseURI();
        int compareOrder(Saxon.Hej.om.NodeInfo other);
        bool hasFingerprint();
        int getFingerprint();
        string getLocalPart();
        string getURI();
        string getDisplayName();
        string getPrefix();
        Saxon.Hej.type.SchemaType getSchemaType();
        /* implicit =====
        Saxon.Hej.om.AtomicSequence atomize();
        ===== implicit */
        Saxon.Hej.om.NodeInfo getParent();
        Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber);
        Saxon.Hej.tree.iter.AxisIterator iterateAxis(int axisNumber, Saxon.Hej.pattern.NodePredicate predicate);
        string getAttributeValue(string uri, string local);
        Saxon.Hej.om.NodeInfo getRoot();
        bool hasChildNodes();
        System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children();
        System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children(Saxon.Hej.pattern.NodePredicate filter);
        Saxon.Hej.om.AttributeMap attributes();
        void generateId(Saxon.Ejava.lang.StringBuilder buffer);
        void copy(Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId);
        Saxon.Hej.lib.ActiveSource asActiveSource();
        void setSystemId(string systemId);
        Saxon.Hej.om.NamespaceBinding[] getDeclaredNamespaces(Saxon.Hej.om.NamespaceBinding[] buffer);
        Saxon.Hej.om.NamespaceMap getAllNamespaces();
        bool isId();
        bool isIdref();
        bool isNilled();
    
    }
}