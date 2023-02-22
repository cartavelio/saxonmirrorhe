using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface MutableNodeInfo : Saxon.Hej.om.NodeInfo {
        void setTypeAnnotation(Saxon.Hej.type.SchemaType type);
        void insertChildren(Saxon.Hej.om.NodeInfo[] source, bool atStart, bool inherit);
        void insertSiblings(Saxon.Hej.om.NodeInfo[] source, bool before, bool inherit);
        void setAttributes(Saxon.Hej.om.AttributeMap attributes);
        void removeAttribute(Saxon.Hej.om.NodeInfo attribute);
        void addAttribute(Saxon.Hej.om.NodeName name, Saxon.Hej.type.SimpleType attType, string value, int properties, bool inheritNamespaces);
        void removeNamespace(string prefix);
        void addNamespace(string prefix, Saxon.Hej.om.NamespaceUri uri);
        void delete();
        bool isDeleted();
        void replace(Saxon.Hej.om.NodeInfo[] replacement, bool inherit);
        void replaceStringValue(Saxon.Hej.str.UnicodeString stringValue);
        void rename(Saxon.Hej.om.NodeName newName, bool inherit);
        void addNamespace(Saxon.Hej.om.NamespaceBinding binding, bool inherit);
        void removeTypeAnnotation();
        Saxon.Hej.@event.Builder newBuilder();
    
    }
}