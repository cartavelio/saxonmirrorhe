using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class DeepEqual : Saxon.Hej.functions.CollatingFunctionFixed {
        public const int INCLUDE_NAMESPACES = 1;
        public const int INCLUDE_PREFIXES = (1<<1);
        public const int INCLUDE_COMMENTS = (1<<2);
        public const int INCLUDE_PROCESSING_INSTRUCTIONS = (1<<3);
        public const int EXCLUDE_WHITESPACE_TEXT_NODES = (1<<4);
        public const int COMPARE_STRING_VALUES = (1<<5);
        public const int COMPARE_ANNOTATIONS = (1<<6);
        public const int WARNING_IF_FALSE = (1<<7);
        public const int JOIN_ADJACENT_TEXT_NODES = (1<<8);
        public const int COMPARE_ID_FLAGS = (1<<9);
        public const int EXCLUDE_VARIETY = (1<<10);
        public static bool deepEqual(Saxon.Hej.om.SequenceIterator op1, Saxon.Hej.om.SequenceIterator op2, Saxon.Hej.expr.sort.AtomicComparer comparer, Saxon.Hej.expr.XPathContext context, int flags) {
            bool result = true;
            string reason = null;
            Saxon.Hej.lib.ErrorReporter reporter = context.getErrorReporter();
            try {
                if ((((flags&JOIN_ADJACENT_TEXT_NODES)) != 0)) {
                    op1 = mergeAdjacentTextNodes(op1);
                    op2 = mergeAdjacentTextNodes(op2);
                }
                int pos1 = 0;
                int pos2 = 0;
                while (true) {
                    Saxon.Hej.om.Item item1 = op1.next();
                    Saxon.Hej.om.Item item2 = op2.next();
                    if (((item1 == null) && (item2 == null))) {
                        break ;
                    }
                    pos1++;
                    pos2++;
                    if (((item1 == null) || (item2 == null))) {
                        result = false;
                        if ((item1 == null)) {
                            reason = (("Second sequence is longer (first sequence length = "+pos2)+")");
                        } else {
                            reason = (("First sequence is longer (second sequence length = "+pos1)+")");
                        }
                        if (((item1 is Saxon.Hej.tree.tiny.WhitespaceTextImpl) || (item2 is Saxon.Hej.tree.tiny.WhitespaceTextImpl))) {
                            reason += " (the first extra node is whitespace text)";
                        }
                        break ;
                    }
                    if (((item1 is Saxon.Hej.om.Function) || (item2 is Saxon.Hej.om.Function))) {
                        if (!((((item1 is Saxon.Hej.om.Function) && (item2 is Saxon.Hej.om.Function))))) {
                            reason = (("if one item is a function then both must be functions (position "+pos1)+")");
                            return false;
                        }
                        bool fe = ((Saxon.Hej.om.Function)(item1)).deepEquals((Saxon.Hej.om.Function)(item2), context, comparer, flags);
                        if (!(fe)) {
                            result = false;
                            reason = (("functions at position "+pos1)+" differ");
                            break ;
                        }
                        continue ;
                    }
                    if (((item1 is Saxon.Hej.value.ObjectValue<object>) || (item2 is Saxon.Hej.value.ObjectValue<object>))) {
                        if (!(item1.Equals(item2))) {
                            return false;
                        }
                        continue ;
                    }
                    if ((item1 is Saxon.Hej.om.NodeInfo)) {
                        if ((item2 is Saxon.Hej.om.NodeInfo)) {
                            if (!(deepEquals((Saxon.Hej.om.NodeInfo)(item1), (Saxon.Hej.om.NodeInfo)(item2), comparer, context, flags))) {
                                result = false;
                                reason = (("nodes at position "+pos1)+" differ");
                                break ;
                            }
                        } else {
                            result = false;
                            reason = ("comparing a node to an atomic value at position "+pos1);
                            break ;
                        }
                    } else {
                        if ((item2 is Saxon.Hej.om.NodeInfo)) {
                            result = false;
                            reason = ("comparing an atomic value to a node at position "+pos1);
                            break ;
                        } else {
                            Saxon.Hej.value.AtomicValue av1 = (Saxon.Hej.value.AtomicValue)(item1);
                            Saxon.Hej.value.AtomicValue av2 = (Saxon.Hej.value.AtomicValue)(item2);
                            if ((av1.isNaN() && av2.isNaN())) {
                            } else {
                                if (!(comparer.comparesEqual(av1, av2))) {
                                    result = false;
                                    reason = (("atomic values at position "+pos1)+" differ");
                                    break ;
                                }
                            }
                        }
                    }
                }
            } catch (Saxon.Hej.trans.UncheckedXPathException uxe) {
                throw uxe.getXPathException();
            } catch (System.InvalidCastException err) {
                result = false;
                reason = "sequences contain non-comparable values";
            }
            if (!(result)) {
                explain(reporter, reason, flags, null, null);
            }
            return result;
        }
        public static bool deepEquals(Saxon.Hej.om.NodeInfo n1, Saxon.Hej.om.NodeInfo n2, Saxon.Hej.expr.sort.AtomicComparer comparer, Saxon.Hej.expr.XPathContext context, int flags) {
            if (n1.Equals(n2)) {
                return true;
            }
            Saxon.Hej.lib.ErrorReporter reporter = context.getErrorReporter();
            if ((n1.getNodeKind() != n2.getNodeKind())) {
                explain(reporter, ((("node kinds differ: comparing "+showKind(n1))+" to ")+showKind(n2)), flags, n1, n2);
                return false;
            }
            switch (n1.getNodeKind()) {
              case Saxon.Hej.type.Type.ELEMENT:
                if (!(Saxon.Hej.tree.util.Navigator.haveSameName(n1, n2))) {
                    explain(reporter, ((("element names differ: "+Saxon.Hej.om.NameOfNode.makeName(n1).getStructuredQName().getEQName())+" != ")+Saxon.Hej.om.NameOfNode.makeName(n2).getStructuredQName().getEQName()), flags, n1, n2);
                    return false;
                }
                if ((((((flags&INCLUDE_PREFIXES)) != 0)) && !(n1.getPrefix().Equals(n2.getPrefix())))) {
                    explain(reporter, ((("element prefixes differ: "+n1.getPrefix())+" != ")+n2.getPrefix()), flags, n1, n2);
                    return false;
                }
                Saxon.Hej.tree.iter.AxisIterator a1 = n1.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                Saxon.Hej.tree.iter.AxisIterator a2 = n2.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                if (!(Saxon.Hej.om.SequenceTool.sameLength(a1, a2))) {
                    explain(reporter, "elements have different number of attributes", flags, n1, n2);
                    return false;
                }
                Saxon.Hej.om.NodeInfo att1;
                a1 = n1.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                while (((att1 = ((/*1430*/Saxon.Hej.om.NodeInfo)a1.next())) != null)) {
                    Saxon.Hej.tree.iter.AxisIterator a2iter = n2.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE, new Saxon.Hej.pattern.SameNameTest(att1));
                    Saxon.Hej.om.NodeInfo att2 = ((/*1430*/Saxon.Hej.om.NodeInfo)a2iter.next());
                    if ((att2 == null)) {
                        explain(reporter, (("one element has an attribute "+Saxon.Hej.om.NameOfNode.makeName(att1).getStructuredQName().getEQName())+", the other does not"), flags, n1, n2);
                        return false;
                    }
                    if (!(deepEquals(att1, att2, comparer, context, flags))) {
                        deepEquals(att1, att2, comparer, context, flags);
                        explain(reporter, ("elements have different values for the attribute "+Saxon.Hej.om.NameOfNode.makeName(att1).getStructuredQName().getEQName()), flags, n1, n2);
                        return false;
                    }
                }
                if ((((flags&INCLUDE_NAMESPACES)) != 0)) {
                    Saxon.Hej.om.NamespaceMap nm1 = n1.getAllNamespaces();
                    Saxon.Hej.om.NamespaceMap nm2 = n2.getAllNamespaces();
                    if (!(nm1.Equals(nm2))) {
                        explain(reporter, ((("elements have different in-scope namespaces: "+nm1)+" versus ")+nm2), flags, n1, n2);
                        return false;
                    }
                }
                if ((((flags&COMPARE_ANNOTATIONS)) != 0)) {
                    if (!(n1.getSchemaType().Equals(n2.getSchemaType()))) {
                        explain(reporter, "elements have different type annotation", flags, n1, n2);
                        return false;
                    }
                }
                if ((((flags&EXCLUDE_VARIETY)) == 0)) {
                    if ((n1.getSchemaType().isComplexType() != n2.getSchemaType().isComplexType())) {
                        explain(reporter, "one element has complex type, the other simple", flags, n1, n2);
                        return false;
                    }
                    if (n1.getSchemaType().isComplexType()) {
                        Saxon.Hej.type.ComplexVariety variety1 = ((Saxon.Hej.type.ComplexType)(n1.getSchemaType())).getVariety();
                        Saxon.Hej.type.ComplexVariety variety2 = ((Saxon.Hej.type.ComplexType)(n2.getSchemaType())).getVariety();
                        if ((variety1 != variety2)) {
                            explain(reporter, "both elements have complex type, but a different variety", flags, n1, n2);
                            return false;
                        }
                    }
                }
                if ((((flags&COMPARE_STRING_VALUES)) == 0)) {
                    Saxon.Hej.type.SchemaType type1 = n1.getSchemaType();
                    Saxon.Hej.type.SchemaType type2 = n2.getSchemaType();
                    bool isSimple1 = (type1.isSimpleType() || ((Saxon.Hej.type.ComplexType)(type1)).isSimpleContent());
                    bool isSimple2 = (type2.isSimpleType() || ((Saxon.Hej.type.ComplexType)(type2)).isSimpleContent());
                    if ((isSimple1 != isSimple2)) {
                        explain(reporter, "one element has a simple type, the other does not", flags, n1, n2);
                        return false;
                    }
                    if (isSimple1) {
                        System.Diagnostics.Debug.Assert(isSimple2);
                        Saxon.Hej.tree.iter.AtomicIterator v1 = ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)n1.atomize().iterate());
                        Saxon.Hej.tree.iter.AtomicIterator v2 = ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)n2.atomize().iterate());
                        return deepEqual(v1, v2, comparer, context, flags);
                    }
                }
                if ((((flags&COMPARE_ID_FLAGS)) != 0)) {
                    if ((n1.isId() != n2.isId())) {
                        explain(reporter, "one element is an ID, the other is not", flags, n1, n2);
                        return false;
                    }
                    if ((n1.isIdref() != n2.isIdref())) {
                        explain(reporter, "one element is an IDREF, the other is not", flags, n1, n2);
                        return false;
                    }
                }
                goto case Saxon.Hej.type.Type.DOCUMENT;
              case Saxon.Hej.type.Type.DOCUMENT:
                Saxon.Hej.tree.iter.AxisIterator c1 = n1.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                Saxon.Hej.tree.iter.AxisIterator c2 = n2.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                while (true) {
                    Saxon.Hej.om.NodeInfo d1 = ((/*1430*/Saxon.Hej.om.NodeInfo)c1.next());
                    while (((d1 != null) && isIgnorable(d1, flags))) {
                        d1 = ((/*1430*/Saxon.Hej.om.NodeInfo)c1.next());
                    }
                    Saxon.Hej.om.NodeInfo d2 = ((/*1430*/Saxon.Hej.om.NodeInfo)c2.next());
                    while (((d2 != null) && isIgnorable(d2, flags))) {
                        d2 = ((/*1430*/Saxon.Hej.om.NodeInfo)c2.next());
                    }
                    if (((d1 == null) || (d2 == null))) {
                        bool r = (d1 == d2);
                        if (!(r)) {
                            string message = (("the first operand contains a node with "+(((d1 == null) ? "fewer" : "more")))+" children than the second");
                            if (((d1 is Saxon.Hej.tree.tiny.WhitespaceTextImpl) || (d2 is Saxon.Hej.tree.tiny.WhitespaceTextImpl))) {
                                message += " (the first extra child is whitespace text)";
                            }
                            explain(reporter, message, flags, n1, n2);
                        }
                        return r;
                    }
                    if (!(deepEquals(d1, d2, comparer, context, flags))) {
                        return false;
                    }
                }
              case Saxon.Hej.type.Type.ATTRIBUTE:
                if (!(Saxon.Hej.tree.util.Navigator.haveSameName(n1, n2))) {
                    explain(reporter, ((("attribute names differ: "+Saxon.Hej.om.NameOfNode.makeName(n1).getStructuredQName().getEQName())+" != ")+Saxon.Hej.om.NameOfNode.makeName(n1).getStructuredQName().getEQName()), flags, n1, n2);
                    return false;
                }
                if ((((((flags&INCLUDE_PREFIXES)) != 0)) && !(n1.getPrefix().Equals(n2.getPrefix())))) {
                    explain(reporter, ((("attribute prefixes differ: "+n1.getPrefix())+" != ")+n2.getPrefix()), flags, n1, n2);
                    return false;
                }
                if ((((flags&COMPARE_ANNOTATIONS)) != 0)) {
                    if (!(n1.getSchemaType().Equals(n2.getSchemaType()))) {
                        explain(reporter, "attributes have different type annotations", flags, n1, n2);
                        return false;
                    }
                }
                bool ar;
                if ((((flags&COMPARE_STRING_VALUES)) == 0)) {
                    ar = deepEqual(((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)n1.atomize().iterate()), ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)n2.atomize().iterate()), comparer, context, 0);
                } else {
                    ar = comparer.comparesEqual(new Saxon.Hej.value.StringValue(n1.getUnicodeStringValue()), new Saxon.Hej.value.StringValue(n2.getUnicodeStringValue()));
                }
                if (!(ar)) {
                    explain(reporter, "attribute values differ", flags, n1, n2);
                    return false;
                }
                if ((((flags&COMPARE_ID_FLAGS)) != 0)) {
                    if ((n1.isId() != n2.isId())) {
                        explain(reporter, "one attribute is an ID, the other is not", flags, n1, n2);
                        return false;
                    }
                    if ((n1.isIdref() != n2.isIdref())) {
                        explain(reporter, "one attribute is an IDREF, the other is not", flags, n1, n2);
                        return false;
                    }
                }
                return true;
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
              case Saxon.Hej.type.Type.NAMESPACE:
                if (!(n1.getLocalPart().Equals(n2.getLocalPart()))) {
                    explain(reporter, (Saxon.Hej.type.Type.displayTypeName(n1)+" names differ"), flags, n1, n2);
                    return false;
                }
                goto case Saxon.Hej.type.Type.TEXT;
              case Saxon.Hej.type.Type.TEXT:
              case Saxon.Hej.type.Type.COMMENT:
                bool vr = comparer.comparesEqual((Saxon.Hej.value.AtomicValue)(n1.atomize()), (Saxon.Hej.value.AtomicValue)(n2.atomize()));
                if ((!(vr) && ((((flags&WARNING_IF_FALSE)) != 0)))) {
                    string v1 = n1.getStringValue();
                    string v2 = n2.getStringValue();
                    string message = "";
                    if ((v1.Length != v2.Length)) {
                        message = (((("lengths ("+v1.Length)+",")+v2.Length)+")");
                    }
                    if (((v1.Length<10) && (v2.Length<10))) {
                        message = ((((" (\""+v1)+"\" vs \"")+v2)+"\")");
                    } else {
                        int min = System.Math.Min(v1.Length, v2.Length);
                        if (v1.Substring(0, min - 0).Equals(v2.Substring(0, min - 0))) {
                            message += ((((" different at char "+min)+"(\"")+Saxon.Hej.str.StringTool.diagnosticDisplay((((v1.Length>v2.Length) ? v1 : v2)).Substring(min)))+"\")");
                        } else {
                            if ((v1[0] != v2[0])) {
                                message += (((((" different at start "+"(\"")+v1.Substring(0, System.Math.Min(v1.Length, 10) - 0))+"\", \"")+v2.Substring(0, System.Math.Min(v2.Length, 10) - 0))+"\")");
                            } else {
                                for (int i = 1; (i<min); i++) {
                                    if (!(v1.Substring(0, i - 0).Equals(v2.Substring(0, i - 0)))) {
                                        message += ((((((" different at char "+((i-1)))+"(\"")+v1.Substring((i-1), System.Math.Min(v1.Length, (i+10)) - (i-1)))+"\", \"")+v2.Substring((i-1), System.Math.Min(v2.Length, (i+10)) - (i-1)))+"\")");
                                        break ;
                                    }
                                }
                            }
                        }
                    }
                    explain(reporter, ((((((Saxon.Hej.type.Type.displayTypeName(n1)+" values differ (")+Saxon.Hej.tree.util.Navigator.getPath(n1))+", ")+Saxon.Hej.tree.util.Navigator.getPath(n2))+"): ")+message), flags, n1, n2);
                }
                return vr;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Unknown node type");
            }
        }
        private static bool isIgnorable(Saxon.Hej.om.NodeInfo node, int flags) {
            int kind = node.getNodeKind();
            if ((kind == Saxon.Hej.type.Type.COMMENT)) {
                return (((flags&INCLUDE_COMMENTS)) == 0);
            } else {
                if ((kind == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION)) {
                    return (((flags&INCLUDE_PROCESSING_INSTRUCTIONS)) == 0);
                } else {
                    if ((kind == Saxon.Hej.type.Type.TEXT)) {
                        return (((((flags&EXCLUDE_WHITESPACE_TEXT_NODES)) != 0)) && Saxon.Hej.value.Whitespace.isAllWhite(node.getUnicodeStringValue()));
                    }
                }
            }
            return false;
        }
        private static void explain(Saxon.Hej.lib.ErrorReporter reporter, string message, int flags, Saxon.Hej.om.NodeInfo n1, Saxon.Hej.om.NodeInfo n2) {
            if ((((flags&WARNING_IF_FALSE)) != 0)) {
                reporter.report(((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident((("deep-equal() "+((((n1 != null) && (n2 != null)) ? (((("comparing "+Saxon.Hej.tree.util.Navigator.getPath(n1))+" to ")+Saxon.Hej.tree.util.Navigator.getPath(n2))+": ") : ": ")))+message)).asWarning()));
            }
        }
        private static string showKind(Saxon.Hej.om.Item item) {
            if ((((item is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(item)).getNodeKind() == Saxon.Hej.type.Type.TEXT)) && Saxon.Hej.value.Whitespace.isAllWhite(item.getUnicodeStringValue()))) {
                return "whitespace text() node";
            } else {
                return Saxon.Hej.type.Type.displayTypeName(item);
            }
        }
        private static string showNamespaces(System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceBinding> bindings) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(256);
            foreach (Saxon.Hej.om.NamespaceBinding binding in bindings) {
                sb.append(binding.getPrefix());
                sb.append("=");
                sb.append(binding.getURI());
                sb.append(" ");
            }
            sb.setLength((sb.length()-1));
            return sb.ToString();
        }
        private static Saxon.Hej.om.SequenceIterator mergeAdjacentTextNodes(Saxon.Hej.om.SequenceIterator @in) {
            System.Collections.Generic.IList<Saxon.Hej.om.Item> items = new System.Collections.Generic.List<Saxon.Hej.om.Item>(20);
            bool prevIsText = false;
            Saxon.Hej.str.UnicodeBuilder textBuffer = new Saxon.Hej.str.UnicodeBuilder();
            while (true) {
                Saxon.Hej.om.Item next = @in.next();
                if ((next == null)) {
                    break ;
                }
                if (((next is Saxon.Hej.om.NodeInfo) && (((Saxon.Hej.om.NodeInfo)(next)).getNodeKind() == Saxon.Hej.type.Type.TEXT))) {
                    textBuffer.accept(next.getUnicodeStringValue());
                    prevIsText = true;
                } else {
                    if (prevIsText) {
                        Saxon.Hej.tree.util.Orphan textNode = new Saxon.Hej.tree.util.Orphan(null);
                        textNode.setNodeKind(Saxon.Hej.type.Type.TEXT);
                        textNode.setStringValue(textBuffer.toUnicodeString());
                        items.Add(textNode);
                        textBuffer.clear();
                    }
                    prevIsText = false;
                    items.Add(next);
                }
            }
            if (prevIsText) {
                Saxon.Hej.tree.util.Orphan textNode = new Saxon.Hej.tree.util.Orphan(null);
                textNode.setNodeKind(Saxon.Hej.type.Type.TEXT);
                textNode.setStringValue(textBuffer.toUnicodeString());
                items.Add(textNode);
            }
            return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(items);
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.expr.sort.GenericAtomicComparer comparer = new Saxon.Hej.expr.sort.GenericAtomicComparer(getStringCollator(), context);
            bool b = deepEqual(arguments[0].iterate(), arguments[1].iterate(), comparer, context, 0);
            return Saxon.Hej.value.BooleanValue.get(b);
        }
        public override string getStreamerName() {
            return "DeepEqual";
        }
    
    }
}