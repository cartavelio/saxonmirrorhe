using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.util {
    public sealed class Navigator {
        private Navigator()  {
        }
        public static string getAttributeValue(Saxon.Hej.om.NodeInfo element, string uri, string localName) {
            return element.getAttributeValue(uri, localName);
        }
        public static string getInheritedAttributeValue(Saxon.Hej.om.NodeInfo element, string uri, string localName) {
            Saxon.Hej.om.NodeInfo node = element;
            while ((node != null)) {
                string value = node.getAttributeValue(uri, localName);
                if ((value == null)) {
                    node = node.getParent();
                } else {
                    return value;
                }
            }
            return null;
        }
        public static Saxon.Hej.om.StructuredQName getNodeName(Saxon.Hej.om.NodeInfo node) {
            if ((node.getLocalPart() != null)) {
                return new Saxon.Hej.om.StructuredQName(node.getPrefix(), node.getURI(), node.getLocalPart());
            } else {
                return null;
            }
        }
        public static Saxon.Hej.om.NodeInfo getOutermostElement(Saxon.Hej.om.TreeInfo doc) {
            return ((/*1430*/Saxon.Hej.om.NodeInfo)doc.getRootNode().iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT).next());
        }
        public static string getBaseURI(Saxon.Hej.om.NodeInfo node) {
            return getBaseURI(node, n => {
                Saxon.Hej.om.NodeInfo parent = n.getParent();
                return ((parent == null) || !(parent.getSystemId().Equals(n.getSystemId())));
            } );
        }
        public static string getBaseURI(Saxon.Hej.om.NodeInfo node, System.Predicate<Saxon.Hej.om.NodeInfo> isTopElementWithinEntity) {
            string xmlBase = ((node is Saxon.Hej.tree.tiny.TinyElementImpl) ? ((Saxon.Hej.tree.tiny.TinyElementImpl)(node)).getAttributeValue(Saxon.Hej.om.StandardNames.XML_BASE) : node.getAttributeValue(Saxon.Hej.lib.NamespaceConstant.XML, "base"));
            if ((xmlBase != null)) {
                System.Uri baseURI;
                try {
                    baseURI = new System.Uri(xmlBase, System.UriKind.RelativeOrAbsolute);
                    if (baseURI.IsAbsoluteUri) {
                        return xmlBase;
                    } else {
                        Saxon.Hej.om.NodeInfo parentNode = node.getParent();
                        if ((parentNode == null)) {
                            System.Uri base2 = new System.Uri(node.getSystemId(), System.UriKind.RelativeOrAbsolute);
                            System.Uri resolved = (xmlBase.Length==0 ? base2 : new System.Uri(base2, baseURI));
                            return resolved.ToString();
                        }
                        string startSysId = node.getSystemId();
                        if ((startSysId == null)) {
                            return null;
                        }
                        string parentSysId = parentNode.getSystemId();
                        bool isTopWithinEntity = false;
                        if ((node is Saxon.Hej.tree.tiny.TinyElementImpl)) {
                            isTopWithinEntity = ((Saxon.Hej.tree.tiny.TinyElementImpl)(node)).getTree().isTopWithinEntity(((Saxon.Hej.tree.tiny.TinyElementImpl)(node)).getNodeNumber());
                        } else {
                            isTopWithinEntity = !(startSysId.Equals(parentSysId));
                        }
                        System.Uri @base = new System.Uri((isTopElementWithinEntity(node) ? startSysId : parentNode.getBaseURI()), System.UriKind.RelativeOrAbsolute);
                        baseURI = (xmlBase.Length==0 ? @base : new System.Uri(@base, baseURI));
                    }
                } catch (System.UriFormatException e) {
                    return xmlBase;
                }
                return baseURI.ToString();
            }
            string startSystemId = node.getSystemId();
            if ((startSystemId == null)) {
                return null;
            }
            Saxon.Hej.om.NodeInfo parent = node.getParent();
            if ((parent == null)) {
                return startSystemId;
            }
            string parentSystemId = parent.getSystemId();
            if ((startSystemId.Equals(parentSystemId) || parentSystemId.Length==0)) {
                return parent.getBaseURI();
            } else {
                return startSystemId;
            }
        }
        public static string getPath(Saxon.Hej.om.NodeInfo node) {
            return getPath(node, null);
        }
        public static string getPath(Saxon.Hej.om.NodeInfo node, Saxon.Hej.expr.XPathContext context) {
            if ((node == null)) {
                return "";
            }
            string pre;
            bool streamed = node.getConfiguration().isStreamedNode(node);
            Saxon.Hej.om.NodeInfo parent = node.getParent();
            switch (node.getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return "/";
              case Saxon.Hej.type.Type.ELEMENT:
                if ((parent == null)) {
                    return node.getDisplayName();
                } else {
                    pre = getPath(parent, context);
                    if (pre.Equals("/")) {
                        return ('/'+node.getDisplayName());
                    } else {
                        return (((pre+'/')+node.getDisplayName())+((streamed ? "" : (("["+getNumberSimple(node, context))+"]"))));
                    }
                }
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return ((getPath(parent, context)+"/@")+node.getDisplayName());
              case Saxon.Hej.type.Type.TEXT:
                pre = getPath(parent, context);
                return ((((pre.Equals("/") ? "" : pre))+"/text()")+((streamed ? "" : (("["+getNumberSimple(node, context))+"]"))));
              case Saxon.Hej.type.Type.COMMENT:
                pre = getPath(parent, context);
                return ((((pre.Equals("/") ? "" : pre))+"/comment()")+((streamed ? "" : (("["+getNumberSimple(node, context))+"]"))));
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                pre = getPath(parent, context);
                return ((((pre.Equals("/") ? "" : pre))+"/processing-instruction()")+((streamed ? "" : (("["+getNumberSimple(node, context))+"]"))));
              case Saxon.Hej.type.Type.NAMESPACE:
                string test = node.getLocalPart();
                if (test.Length==0) {
                    test = "*[not(local-name()]";
                }
                return ((getPath(parent, context)+"/namespace::")+test);
              default:
                return "";
            }
        }
        public static Saxon.Hej.om.AbsolutePath getAbsolutePath(Saxon.Hej.om.NodeInfo node) {
            bool streamed = node.getConfiguration().isStreamedNode(node);
            System.Collections.Generic.LinkedList<Saxon.Hej.om.AbsolutePath.PathElement> path = new System.Collections.Generic.LinkedList<Saxon.Hej.om.AbsolutePath.PathElement>();
            string sysId = node.getSystemId();
            while (((node != null) && (node.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT))) {
                path.AddFirst(new Saxon.Hej.om.AbsolutePath.PathElement(node.getNodeKind(), Saxon.Hej.om.NameOfNode.makeName(node), (streamed ? -1 : getNumberSimple(node, null))));
                node = node.getParent();
            }
            Saxon.Hej.om.AbsolutePath a = new Saxon.Hej.om.AbsolutePath(path);
            a.setSystemId(sysId);
            return a;
        }
        public static bool haveSameName(Saxon.Hej.om.NodeInfo n1, Saxon.Hej.om.NodeInfo n2) {
            if ((n1.hasFingerprint() && n2.hasFingerprint())) {
                return (n1.getFingerprint() == n2.getFingerprint());
            } else {
                return (n1.getLocalPart().Equals(n2.getLocalPart()) && n1.getURI().Equals(n2.getURI()));
            }
        }
        public static int getNumberSimple(Saxon.Hej.om.NodeInfo node, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.pattern.NodeTest same;
            if (node.getLocalPart().Length==0) {
                same = Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(node.getNodeKind());
            } else {
                same = new Saxon.Hej.pattern.SameNameTest(node);
            }
            Saxon.Hej.Controller controller = ((context == null) ? null : context.getController());
            Saxon.Hej.tree.iter.AxisIterator preceding = node.iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING, same);
            int i = 1;
            while (true) {
                Saxon.Hej.om.NodeInfo prev = ((/*1430*/Saxon.Hej.om.NodeInfo)preceding.next());
                if ((prev == null)) {
                    break ;
                }
                if ((controller != null)) {
                    int memo = controller.getRememberedNumber(prev);
                    if ((memo>0)) {
                        memo += i;
                        controller.setRememberedNumber(node, memo);
                        return memo;
                    }
                }
                i++;
            }
            if ((controller != null)) {
                controller.setRememberedNumber(node, i);
            }
            return i;
        }
        public static int getNumberSingle(Saxon.Hej.om.NodeInfo node, Saxon.Hej.pattern.Pattern count, Saxon.Hej.pattern.Pattern from, Saxon.Hej.expr.XPathContext context) {
            if (((count == null) && (from == null))) {
                return getNumberSimple(node, context);
            }
            bool knownToMatch = false;
            if ((count == null)) {
                if (node.getLocalPart().Length==0) {
                    count = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(node.getNodeKind()));
                } else {
                    count = new Saxon.Hej.pattern.NodeTestPattern(new Saxon.Hej.pattern.SameNameTest(node));
                }
                knownToMatch = true;
            }
            Saxon.Hej.om.NodeInfo target = node;
            if (!(knownToMatch)) {
                while (true) {
                    if (count.matches(target, context)) {
                        if ((from == null)) {
                            break ;
                        } else {
                            Saxon.Hej.om.NodeInfo anc = target;
                            while (!(from.matches(anc, context))) {
                                anc = anc.getParent();
                                if ((anc == null)) {
                                    return 0;
                                }
                            }
                            break ;
                        }
                    } else {
                        if (((from != null) && from.matches(target, context))) {
                            return 0;
                        } else {
                            target = target.getParent();
                            if ((target == null)) {
                                return 0;
                            }
                        }
                    }
                }
            }
            Saxon.Hej.om.SequenceIterator preceding = target.iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING, getNodeTestForPattern(count));
            bool alreadyChecked = (count is Saxon.Hej.pattern.NodeTestPattern);
            int i = 1;
            while (true) {
                Saxon.Hej.om.NodeInfo p = (Saxon.Hej.om.NodeInfo)(preceding.next());
                if ((p == null)) {
                    return i;
                }
                if ((alreadyChecked || count.matches(p, context))) {
                    i++;
                }
            }
        }
        public static int getNumberAny(Saxon.Hej.expr.Expression inst, Saxon.Hej.om.NodeInfo node, Saxon.Hej.pattern.Pattern count, Saxon.Hej.pattern.Pattern from, Saxon.Hej.expr.XPathContext context, bool hasVariablesInPatterns) {
            Saxon.Hej.om.NodeInfo memoNode = null;
            int memoNumber = 0;
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            bool memoise = (!(hasVariablesInPatterns) && (from == null));
            if (memoise) {
                System.Object[] memo = (System.Object[])(controller.getUserData(inst.getLocation(), "xsl:number"));
                if ((memo != null)) {
                    memoNode = (Saxon.Hej.om.NodeInfo)(memo[0]);
                    memoNumber = (int)(memo[1]);
                }
            }
            int num = 0;
            if ((count == null)) {
                if (node.getLocalPart().Length==0) {
                    count = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(node.getNodeKind()));
                } else {
                    count = new Saxon.Hej.pattern.NodeTestPattern(new Saxon.Hej.pattern.SameNameTest(node));
                }
                num = 1;
            } else {
                if (count.matches(node, context)) {
                    num = 1;
                }
            }
            Saxon.Hej.pattern.NodeTest filter;
            if ((from == null)) {
                filter = getNodeTestForPattern(count);
            } else {
                if (((from.getUType() == Saxon.Hej.type.UType.ELEMENT) && (count.getUType() == Saxon.Hej.type.UType.ELEMENT))) {
                    filter = Saxon.Hej.pattern.NodeKindTest.ELEMENT;
                } else {
                    filter = Saxon.Hej.pattern.AnyNodeTest.getInstance();
                }
            }
            if (((from != null) && from.matches(node, context))) {
                return num;
            }
            Saxon.Hej.om.SequenceIterator preceding = node.iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR, filter);
            while (true) {
                Saxon.Hej.om.NodeInfo prev = (Saxon.Hej.om.NodeInfo)(preceding.next());
                if ((prev == null)) {
                    break ;
                }
                if (count.matches(prev, context)) {
                    if (((num == 1) && prev.Equals(memoNode))) {
                        num = (memoNumber+1);
                        break ;
                    }
                    num++;
                }
                if (((from != null) && from.matches(prev, context))) {
                    break ;
                }
            }
            if (memoise) {
                System.Object[] memo = new System.Object[2];
                memo[0] = node;
                memo[1] = num;
                controller.setUserData(inst.getLocation(), "xsl:number", memo);
            }
            return num;
        }
        public static System.Collections.Generic.IList<System.Int64> getNumberMulti(Saxon.Hej.om.NodeInfo node, Saxon.Hej.pattern.Pattern count, Saxon.Hej.pattern.Pattern from, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.List<System.Int64> v = new System.Collections.Generic.List<System.Int64>(5);
            if ((count == null)) {
                if (node.getLocalPart().Length==0) {
                    count = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(node.getNodeKind()));
                } else {
                    count = new Saxon.Hej.pattern.NodeTestPattern(new Saxon.Hej.pattern.SameNameTest(node));
                }
            }
            Saxon.Hej.om.NodeInfo curr = node;
            while (true) {
                if (count.matches(curr, context)) {
                    int num = getNumberSingle(curr, count, null, context);
                    v.Insert(0, (long)(num));
                }
                if (((from != null) && from.matches(curr, context))) {
                    break ;
                }
                curr = curr.getParent();
                if ((curr == null)) {
                    break ;
                }
            }
            return v;
        }
        private static Saxon.Hej.pattern.NodeTest getNodeTestForPattern(Saxon.Hej.pattern.Pattern pattern) {
            Saxon.Hej.type.ItemType type = pattern.getItemType();
            if ((type is Saxon.Hej.pattern.NodeTest)) {
                return (Saxon.Hej.pattern.NodeTest)(type);
            } else {
                if (pattern.getUType().overlaps(Saxon.Hej.type.UType.ANY_NODE)) {
                    return Saxon.Hej.pattern.AnyNodeTest.getInstance();
                } else {
                    return Saxon.Hej.type.ErrorType.getInstance();
                }
            }
        }
        public static void copy(Saxon.Hej.om.NodeInfo node, Saxon.Hej.@event.Receiver @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            switch (node.getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
                {
                    @out.startDocument(Saxon.Hej.om.CopyOptions.getStartDocumentProperties(copyOptions));
                    foreach (Saxon.Hej.om.NodeInfo child in node.children()) {
                        child.copy(@out, copyOptions, locationId);
                    }
                    @out.endDocument();
                    break ;
                }
              case Saxon.Hej.type.Type.ELEMENT:
                {
                    Saxon.Hej.type.SchemaType annotation = ((((copyOptions&Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS)) != 0) ? node.getSchemaType() : Saxon.Hej.type.Untyped.getInstance());
                    Saxon.Hej.om.NamespaceMap ns = (Saxon.Hej.om.CopyOptions.includes(copyOptions, Saxon.Hej.om.CopyOptions.ALL_NAMESPACES) ? node.getAllNamespaces() : Saxon.Hej.om.NamespaceMap.emptyMap());
                    @out.startElement(Saxon.Hej.om.NameOfNode.makeName(node), annotation, node.attributes(), ns, locationId, (Saxon.Hej.@event.ReceiverOption.BEQUEATH_INHERITED_NAMESPACES_ONLY|Saxon.Hej.@event.ReceiverOption.NAMESPACE_OK));
                    foreach (Saxon.Hej.om.NodeInfo child in node.children()) {
                        child.copy(@out, copyOptions, locationId);
                    }
                    @out.endElement();
                    return ;
                }
              case Saxon.Hej.type.Type.ATTRIBUTE:
                {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Cannot copy attribute to Receiver");
                }
              case Saxon.Hej.type.Type.TEXT:
                {
                    Saxon.Hej.str.UnicodeString value = node.getUnicodeStringValue();
                    if ((value.length() != 0)) {
                        @out.characters(value, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    }
                    return ;
                }
              case Saxon.Hej.type.Type.COMMENT:
                {
                    @out.comment(node.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                }
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                {
                    @out.processingInstruction(node.getLocalPart(), node.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                }
              case Saxon.Hej.type.Type.NAMESPACE:
                {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Cannot copy namespace to Receiver");
                }
              default:
                break;
            }
        }
        public static void copy(Saxon.Hej.om.NodeInfo node, Saxon.Hej.@event.Outputter @out, int copyOptions, Saxon.Hej.s9api.Location locationId) {
            bool keepTypes = (((copyOptions&Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS)) != 0);
            switch (node.getNodeKind()) {
              case Saxon.Hej.type.Type.DOCUMENT:
                {
                    @out.startDocument(Saxon.Hej.om.CopyOptions.getStartDocumentProperties(copyOptions));
                    foreach (Saxon.Hej.om.NodeInfo child in node.children()) {
                        copy(child, @out, copyOptions, locationId);
                    }
                    @out.endDocument();
                    break ;
                }
              case Saxon.Hej.type.Type.ELEMENT:
                {
                    Saxon.Hej.type.SchemaType annotation = (keepTypes ? node.getSchemaType() : Saxon.Hej.type.Untyped.getInstance());
                    @out.startElement(Saxon.Hej.om.NameOfNode.makeName(node), annotation, locationId, (Saxon.Hej.@event.ReceiverOption.DISINHERIT_NAMESPACES|Saxon.Hej.@event.ReceiverOption.NAMESPACE_OK));
                    if ((((copyOptions&Saxon.Hej.om.CopyOptions.ALL_NAMESPACES)) != 0)) {
                        foreach (Saxon.Hej.om.NamespaceBinding ns in node.getAllNamespaces()) {
                            @out.@namespace(ns.getPrefix(), ns.getURI(), Saxon.Hej.@event.ReceiverOption.NONE);
                        }
                    }
                    foreach (Saxon.Hej.om.AttributeInfo attr in node.attributes()) {
                        Saxon.Hej.type.SimpleType attType = (keepTypes ? attr.getType() : Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
                        @out.attribute(attr.getNodeName(), attType, attr.getValue(), attr.getLocation(), attr.getProperties());
                    }
                    foreach (Saxon.Hej.om.NodeInfo child in node.children()) {
                        copy(child, @out, copyOptions, locationId);
                    }
                    @out.endElement();
                    return ;
                }
              case Saxon.Hej.type.Type.ATTRIBUTE:
                {
                    Saxon.Hej.type.SimpleType attType = (keepTypes ? (Saxon.Hej.type.SimpleType)(node.getSchemaType()) : Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
                    @out.attribute(Saxon.Hej.om.NameOfNode.makeName(node), attType, node.getStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                }
              case Saxon.Hej.type.Type.TEXT:
                {
                    Saxon.Hej.str.UnicodeString value = node.getUnicodeStringValue();
                    if ((value.length() != 0)) {
                        @out.characters(value, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    }
                    return ;
                }
              case Saxon.Hej.type.Type.COMMENT:
                {
                    @out.comment(node.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                }
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                {
                    @out.processingInstruction(node.getLocalPart(), node.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                }
              case Saxon.Hej.type.Type.NAMESPACE:
                {
                    @out.@namespace(node.getLocalPart(), node.getStringValue(), Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                }
              default:
                break;
            }
        }
        public static int compareOrder(Saxon.Hej.tree.wrapper.SiblingCountingNode first, Saxon.Hej.tree.wrapper.SiblingCountingNode second) {
            if (first.Equals(second)) {
                return 0;
            }
            Saxon.Hej.om.NodeInfo firstParent = first.getParent();
            if ((firstParent == null)) {
                return -1;
            }
            Saxon.Hej.om.NodeInfo secondParent = second.getParent();
            if ((secondParent == null)) {
                return +(1);
            }
            if (firstParent.Equals(secondParent)) {
                int cat1 = nodeCategories[first.getNodeKind()];
                int cat2 = nodeCategories[second.getNodeKind()];
                if ((cat1 == cat2)) {
                    return (first.getSiblingPosition()-second.getSiblingPosition());
                } else {
                    return (cat1-cat2);
                }
            }
            int depth1 = 0;
            int depth2 = 0;
            Saxon.Hej.om.NodeInfo p1 = first;
            Saxon.Hej.om.NodeInfo p2 = second;
            while ((p1 != null)) {
                depth1++;
                p1 = p1.getParent();
            }
            while ((p2 != null)) {
                depth2++;
                p2 = p2.getParent();
            }
            p1 = first;
            while ((depth1>depth2)) {
                p1 = p1.getParent();
                System.Diagnostics.Debug.Assert((p1 != null));
                if (p1.Equals(second)) {
                    return +(1);
                }
                depth1--;
            }
            p2 = second;
            while ((depth2>depth1)) {
                p2 = p2.getParent();
                System.Diagnostics.Debug.Assert((p2 != null));
                if (p2.Equals(first)) {
                    return -1;
                }
                depth2--;
            }
            while (true) {
                Saxon.Hej.om.NodeInfo par1 = p1.getParent();
                Saxon.Hej.om.NodeInfo par2 = p2.getParent();
                if (((par1 == null) || (par2 == null))) {
                    throw new Saxon.Ejava.lang.NullPointerException("Node order comparison - internal error");
                }
                if (par1.Equals(par2)) {
                    if (((p1.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE) && (p2.getNodeKind() != Saxon.Hej.type.Type.ATTRIBUTE))) {
                        return -1;
                    }
                    if (((p1.getNodeKind() != Saxon.Hej.type.Type.ATTRIBUTE) && (p2.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE))) {
                        return +(1);
                    }
                    return (((Saxon.Hej.tree.wrapper.SiblingCountingNode)(p1)).getSiblingPosition()-((Saxon.Hej.tree.wrapper.SiblingCountingNode)(p2)).getSiblingPosition());
                }
                p1 = par1;
                p2 = par2;
            }
        }
        public static int comparePosition(Saxon.Hej.om.NodeInfo first, Saxon.Hej.om.NodeInfo second) {
            if (((((first.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE) || (first.getNodeKind() == Saxon.Hej.type.Type.NAMESPACE)) || (second.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE)) || (second.getNodeKind() == Saxon.Hej.type.Type.NAMESPACE))) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException();
            }
            if (first.Equals(second)) {
                return Saxon.Hej.om.AxisInfo.SELF;
            }
            Saxon.Hej.om.NodeInfo firstParent = first.getParent();
            if ((firstParent == null)) {
                return Saxon.Hej.om.AxisInfo.ANCESTOR;
            }
            Saxon.Hej.om.NodeInfo secondParent = second.getParent();
            if ((secondParent == null)) {
                return Saxon.Hej.om.AxisInfo.DESCENDANT;
            }
            if (firstParent.Equals(secondParent)) {
                if ((first.compareOrder(second)<0)) {
                    return Saxon.Hej.om.AxisInfo.PRECEDING;
                } else {
                    return Saxon.Hej.om.AxisInfo.FOLLOWING;
                }
            }
            int depth1 = 0;
            int depth2 = 0;
            Saxon.Hej.om.NodeInfo p1 = first;
            Saxon.Hej.om.NodeInfo p2 = second;
            while ((p1 != null)) {
                depth1++;
                p1 = p1.getParent();
            }
            while ((p2 != null)) {
                depth2++;
                p2 = p2.getParent();
            }
            p1 = first;
            while ((depth1>depth2)) {
                p1 = p1.getParent();
                System.Diagnostics.Debug.Assert((p1 != null));
                if (p1.Equals(second)) {
                    return Saxon.Hej.om.AxisInfo.DESCENDANT;
                }
                depth1--;
            }
            p2 = second;
            while ((depth2>depth1)) {
                p2 = p2.getParent();
                System.Diagnostics.Debug.Assert((p2 != null));
                if (p2.Equals(first)) {
                    return Saxon.Hej.om.AxisInfo.ANCESTOR;
                }
                depth2--;
            }
            if ((first.compareOrder(second)<0)) {
                return Saxon.Hej.om.AxisInfo.PRECEDING;
            } else {
                return Saxon.Hej.om.AxisInfo.FOLLOWING;
            }
        }
        private static readonly int[] nodeCategories = {-1, 3, 2, 3, -1, -1, -1, 3, 3, 0, -1, -1, -1, 1};
        public static void appendSequentialKey(Saxon.Hej.tree.wrapper.SiblingCountingNode node, Saxon.Ejava.lang.StringBuilder sb, bool addDocNr) {
            if (addDocNr) {
                sb.append('w');
                sb.append(node.getTreeInfo().getDocumentNumber());
            }
            if ((node.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT)) {
                Saxon.Hej.om.NodeInfo parent = node.getParent();
                if ((parent != null)) {
                    appendSequentialKey((Saxon.Hej.tree.wrapper.SiblingCountingNode)(parent), sb, false);
                }
                if ((node.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE)) {
                    sb.append('A');
                }
            }
            sb.append(alphaKey(node.getSiblingPosition()));
        }
        public static string alphaKey(int value) {
            if ((value<1)) {
                return "a";
            }
            if ((value<10)) {
                return ("b"+value);
            }
            if ((value<100)) {
                return ("c"+value);
            }
            if ((value<1000)) {
                return ("d"+value);
            }
            if ((value<10000)) {
                return ("e"+value);
            }
            if ((value<100000)) {
                return ("f"+value);
            }
            if ((value<1000000)) {
                return ("g"+value);
            }
            if ((value<10000000)) {
                return ("h"+value);
            }
            if ((value<100000000)) {
                return ("i"+value);
            }
            if ((value<1000000000)) {
                return ("j"+value);
            }
            return ("k"+value);
        }
        public static bool isAncestorOrSelf(Saxon.Hej.om.NodeInfo a, Saxon.Hej.om.NodeInfo d) {
            int k = a.getNodeKind();
            if (((k != Saxon.Hej.type.Type.ELEMENT) && (k != Saxon.Hej.type.Type.DOCUMENT))) {
                return a.Equals(d);
            }
            if ((a is Saxon.Hej.tree.tiny.TinyNodeImpl)) {
                if ((d is Saxon.Hej.tree.tiny.TinyNodeImpl)) {
                    return ((Saxon.Hej.tree.tiny.TinyNodeImpl)(a)).isAncestorOrSelf((Saxon.Hej.tree.tiny.TinyNodeImpl)(d));
                } else {
                    if ((d is Saxon.Hej.tree.tiny.TinyTextualElement.TinyTextualElementText)) {
                        return (a.Equals(d) || isAncestorOrSelf(a, d.getParent()));
                    } else {
                        if ((d.getNodeKind() == Saxon.Hej.type.Type.NAMESPACE)) {
                        } else {
                            if ((d is Saxon.Hej.tree.wrapper.VirtualCopy)) {
                            } else {
                                return false;
                            }
                        }
                    }
                }
            }
            Saxon.Hej.om.NodeInfo p = d;
            while ((p != null)) {
                if (a.Equals(p)) {
                    return true;
                }
                p = p.getParent();
            }
            return false;
        }
        public static Saxon.Hej.pattern.NodeTest nodeTestFromPredicate(Saxon.Hej.pattern.NodePredicate predicate) {
            if ((predicate is Saxon.Hej.pattern.NodeTest)) {
                return (Saxon.Hej.pattern.NodeTest)(predicate);
            } else {
                return Saxon.Hej.pattern.NodeSelector.of(it => predicate.test(it) /* converted from instance method reference */);
            }
        }
        public static Saxon.Hej.tree.iter.AxisIterator filteredSingleton(Saxon.Hej.om.NodeInfo node, Saxon.Hej.pattern.NodePredicate nodeTest) {
            if (((node != null) && nodeTest.test(node))) {
                return Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator(node);
            } else {
                return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
            }
        }
        public static int getSiblingPosition(Saxon.Hej.om.NodeInfo node, Saxon.Hej.pattern.NodeTest nodeTest, int max) {
            if (((node is Saxon.Hej.tree.wrapper.SiblingCountingNode) && (nodeTest is Saxon.Hej.pattern.AnyNodeTest))) {
                return ((Saxon.Hej.tree.wrapper.SiblingCountingNode)(node)).getSiblingPosition();
            }
            Saxon.Hej.tree.iter.AxisIterator prev = node.iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING, nodeTest);
            int count = 1;
            while ((((/*1430*/Saxon.Hej.om.NodeInfo)prev.next()) != null)) {
                if ((++count>max)) {
                    return count;
                }
            }
            return count;
        }
        public class ChildrenAsIterable : System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> {
            private readonly Saxon.Hej.om.NodeInfo parent;
            private Saxon.Hej.pattern.NodePredicate filter = null;
            public ChildrenAsIterable(Saxon.Hej.om.NodeInfo parent)  {
                this.parent = parent;
            }
            public ChildrenAsIterable(Saxon.Hej.om.NodeInfo parent, Saxon.Hej.pattern.NodePredicate filter)  {
                this.parent = parent;
                this.filter = filter;
            }
            public System.Collections.Generic.IEnumerator<Saxon.Hej.om.NodeInfo> GetEnumerator() {
                Saxon.Hej.tree.iter.AxisIterator basis;
                if ((filter == null)) {
                    basis = parent.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                } else {
                    basis = parent.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, filter);
                }
                return new Saxon.Hej.tree.jiter.WrappingJavaIterator<Saxon.Hej.om.NodeInfo>(basis);
            }
            // Extra method to make the class IEnumerable as well as IEnumerable<T>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        
        }
        public class AxisFilter : Saxon.Hej.tree.iter.AxisIterator {
            private readonly Saxon.Hej.tree.iter.AxisIterator @base;
            private readonly Saxon.Hej.pattern.NodePredicate nodeTest;
            public AxisFilter(Saxon.Hej.tree.iter.AxisIterator @base, Saxon.Hej.pattern.NodePredicate test)  {
                this.@base = @base;
                nodeTest = test;
            }
            public Saxon.Hej.om.Item next() {
                while (true) {
                    Saxon.Hej.om.NodeInfo next = ((/*1430*/Saxon.Hej.om.NodeInfo)@base.next());
                    if ((next == null)) {
                        return null;
                    }
                    if (nodeTest.test(next)) {
                        return next;
                    }
                }
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public virtual void close() {
            }

        }
        public class EmptyTextFilter : Saxon.Hej.tree.iter.AxisIterator {
            private readonly Saxon.Hej.tree.iter.AxisIterator @base;
            public EmptyTextFilter(Saxon.Hej.tree.iter.AxisIterator @base)  {
                this.@base = @base;
            }
            public Saxon.Hej.om.Item next() {
                while (true) {
                    Saxon.Hej.om.NodeInfo next = ((/*1430*/Saxon.Hej.om.NodeInfo)@base.next());
                    if ((next == null)) {
                        return null;
                    }
                    if (!((((next.getNodeKind() == Saxon.Hej.type.Type.TEXT) && (next.getUnicodeStringValue().length() == 0))))) {
                        return next;
                    }
                }
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public virtual void close() {
            }

        }
        public sealed class AncestorEnumeration : Saxon.Hej.tree.iter.AxisIterator {
            private readonly bool includeSelf;
            private bool atStart;
            private Saxon.Hej.om.NodeInfo current;
            public AncestorEnumeration(Saxon.Hej.om.NodeInfo start, bool includeSelf)  {
                this.includeSelf = includeSelf;
                current = start;
                atStart = true;
            }
            public Saxon.Hej.om.Item next() {
                if (atStart) {
                    atStart = false;
                    if (includeSelf) {
                        return current;
                    }
                }
                return current = ((current == null) ? null : current.getParent());
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public void close() {
            }

        }
        public sealed class DescendantEnumeration : Saxon.Hej.tree.iter.AxisIterator {
            private Saxon.Hej.om.SequenceIterator children = null;
            private Saxon.Hej.tree.iter.AxisIterator descendants = null;
            private readonly Saxon.Hej.om.NodeInfo start;
            private readonly bool includeSelf;
            private readonly bool forwards;
            private bool atEnd = false;
            public DescendantEnumeration(Saxon.Hej.om.NodeInfo start, bool includeSelf, bool forwards)  {
                this.start = start;
                this.includeSelf = includeSelf;
                this.forwards = forwards;
            }
            public Saxon.Hej.om.Item next() {
                if ((descendants != null)) {
                    Saxon.Hej.om.NodeInfo nextd = ((/*1430*/Saxon.Hej.om.NodeInfo)descendants.next());
                    if ((nextd != null)) {
                        return nextd;
                    } else {
                        descendants = null;
                    }
                }
                if ((children != null)) {
                    Saxon.Hej.om.NodeInfo n = (Saxon.Hej.om.NodeInfo)(children.next());
                    if ((n != null)) {
                        if (n.hasChildNodes()) {
                            if (forwards) {
                                descendants = new Saxon.Hej.tree.util.Navigator.DescendantEnumeration(n, false, true);
                                return n;
                            } else {
                                descendants = new Saxon.Hej.tree.util.Navigator.DescendantEnumeration(n, true, false);
                                return ((/*1430*/Saxon.Hej.om.NodeInfo)next());
                            }
                        } else {
                            return n;
                        }
                    } else {
                        if ((forwards || !(includeSelf))) {
                            return null;
                        } else {
                            atEnd = true;
                            children = null;
                            return start;
                        }
                    }
                } else {
                    if (atEnd) {
                        return null;
                    } else {
                        if (start.hasChildNodes()) {
                            children = start.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                            if (!(forwards)) {
                                children = Saxon.Hej.functions.Reverse.getReverseIterator(children);
                            }
                        } else {
                            children = Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                        }
                        if ((forwards && includeSelf)) {
                            return start;
                        } else {
                            return ((/*1430*/Saxon.Hej.om.NodeInfo)next());
                        }
                    }
                }
            }
            public void advance() {
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public void close() {
            }

        }
        public sealed class FollowingEnumeration : Saxon.Hej.tree.iter.AxisIterator {
            private readonly Saxon.Hej.tree.iter.AxisIterator ancestorEnum;
            private Saxon.Hej.tree.iter.AxisIterator siblingEnum;
            private Saxon.Hej.tree.iter.AxisIterator descendEnum = null;
            public FollowingEnumeration(Saxon.Hej.om.NodeInfo start)  {
                ancestorEnum = new Saxon.Hej.tree.util.Navigator.AncestorEnumeration(start, false);
                switch (start.getNodeKind()) {
                  case Saxon.Hej.type.Type.ELEMENT:
                  case Saxon.Hej.type.Type.TEXT:
                  case Saxon.Hej.type.Type.COMMENT:
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    siblingEnum = start.iterateAxis(Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING);
                    break ;
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                  case Saxon.Hej.type.Type.NAMESPACE:
                    Saxon.Hej.om.NodeInfo parent = start.getParent();
                    if ((parent == null)) {
                        siblingEnum = Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                    } else {
                        siblingEnum = parent.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                    }
                    break ;
                  default:
                    siblingEnum = Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                    break ;
                }
            }
            public Saxon.Hej.om.Item next() {
                if ((descendEnum != null)) {
                    Saxon.Hej.om.NodeInfo nextd = ((/*1430*/Saxon.Hej.om.NodeInfo)descendEnum.next());
                    if ((nextd != null)) {
                        return nextd;
                    } else {
                        descendEnum = null;
                    }
                }
                if ((siblingEnum != null)) {
                    Saxon.Hej.om.NodeInfo nexts = ((/*1430*/Saxon.Hej.om.NodeInfo)siblingEnum.next());
                    if ((nexts != null)) {
                        if (nexts.hasChildNodes()) {
                            descendEnum = new Saxon.Hej.tree.util.Navigator.DescendantEnumeration(nexts, false, true);
                        } else {
                            descendEnum = null;
                        }
                        return nexts;
                    } else {
                        descendEnum = null;
                        siblingEnum = null;
                    }
                }
                Saxon.Hej.om.NodeInfo nexta = ((/*1430*/Saxon.Hej.om.NodeInfo)ancestorEnum.next());
                if ((nexta != null)) {
                    if ((nexta.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                        siblingEnum = Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                    } else {
                        siblingEnum = nexta.iterateAxis(Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING);
                    }
                    return ((/*1430*/Saxon.Hej.om.NodeInfo)next());
                } else {
                    return null;
                }
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public void close() {
            }

        }
        public sealed class PrecedingEnumeration : Saxon.Hej.tree.iter.AxisIterator {
            private readonly Saxon.Hej.tree.iter.AxisIterator ancestorEnum;
            private Saxon.Hej.tree.iter.AxisIterator siblingEnum;
            private Saxon.Hej.tree.iter.AxisIterator descendEnum = null;
            private readonly bool includeAncestors;
            public PrecedingEnumeration(Saxon.Hej.om.NodeInfo start, bool includeAncestors)  {
                this.includeAncestors = includeAncestors;
                ancestorEnum = new Saxon.Hej.tree.util.Navigator.AncestorEnumeration(start, false);
                switch (start.getNodeKind()) {
                  case Saxon.Hej.type.Type.ELEMENT:
                  case Saxon.Hej.type.Type.TEXT:
                  case Saxon.Hej.type.Type.COMMENT:
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    siblingEnum = start.iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING);
                    break ;
                  default:
                    siblingEnum = Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                    break ;
                }
            }
            public Saxon.Hej.om.Item next() {
                if ((descendEnum != null)) {
                    Saxon.Hej.om.NodeInfo nextd = ((/*1430*/Saxon.Hej.om.NodeInfo)descendEnum.next());
                    if ((nextd != null)) {
                        return nextd;
                    } else {
                        descendEnum = null;
                    }
                }
                if ((siblingEnum != null)) {
                    Saxon.Hej.om.NodeInfo nexts = ((/*1430*/Saxon.Hej.om.NodeInfo)siblingEnum.next());
                    if ((nexts != null)) {
                        if (nexts.hasChildNodes()) {
                            descendEnum = new Saxon.Hej.tree.util.Navigator.DescendantEnumeration(nexts, true, false);
                            return ((/*1430*/Saxon.Hej.om.NodeInfo)next());
                        } else {
                            descendEnum = null;
                            return nexts;
                        }
                    } else {
                        descendEnum = null;
                        siblingEnum = null;
                    }
                }
                Saxon.Hej.om.NodeInfo nexta = ((/*1430*/Saxon.Hej.om.NodeInfo)ancestorEnum.next());
                if ((nexta != null)) {
                    if ((nexta.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                        siblingEnum = Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                    } else {
                        siblingEnum = nexta.iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING);
                    }
                    if (!(includeAncestors)) {
                        return ((/*1430*/Saxon.Hej.om.NodeInfo)next());
                    } else {
                        return nexta;
                    }
                } else {
                    return null;
                }
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public void close() {
            }

        }
    
    }
}