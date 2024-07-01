using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    #pragma warning disable 659
    public class DeepEqual : Saxon.Hej.functions.CollatingFunctionFixed {

        static DeepEqual() {
            Saxon.Hej.functions.OptionsParameter o = new Saxon.Hej.functions.OptionsParameter();
            o.addAllowedOption("base-uri", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("comments", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("debug", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("false-on-error", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("id-property", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("idrefs-property", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("in-scope-namespaces", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("namespace-prefixes", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("nilled-property", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("normalize-space", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("preserve-space", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.TRUE);
            o.addAllowedOption("processing-instructions", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("text-boundaries", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.TRUE);
            o.addAllowedOption("timezones", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("type-annotations", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.FALSE);
            o.addAllowedOption("type-variety", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.TRUE);
            o.addAllowedOption("typed-values", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN, Saxon.Hej.value.BooleanValue.TRUE);
            o.addAllowedOption("normalization-form", Saxon.Hej.value.SequenceType.OPTIONAL_STRING, Saxon.Hej.value.EmptySequence.getInstance());
            o.setAllowedValues("normalization-form", "FOJS0005", "NFC", "NFD", "NFKC", "NFKD");
            o.addAllowedOption("unordered-elements", Saxon.Hej.type.BuiltInAtomicType.QNAME.zeroOrMore(), Saxon.Hej.value.EmptySequence.getInstance());
            OPTION_DETAILS = o;
        }
        public static Saxon.Hej.functions.OptionsParameter OPTION_DETAILS;
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.Expression[] newArgs = new Saxon.Hej.expr.Expression[4];
            newArgs[0] = arguments[0];
            newArgs[1] = arguments[1];
            if (((arguments.Length<3) || (arguments[2] is Saxon.Hej.expr.DefaultedArgumentExpression))) {
                newArgs[2] = new Saxon.Hej.expr.StringLiteral(getRetainedStaticContext().getDefaultCollationName());
            } else {
                newArgs[2] = arguments[2];
            }
            if (((arguments.Length<4) || (arguments[3] is Saxon.Hej.expr.DefaultedArgumentExpression))) {
                newArgs[3] = Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.ma.map.DictionaryMap());
            } else {
                newArgs[3] = arguments[3];
            }
            setArity(4);
            return base.makeFunctionCall(newArgs);
        }
        public class DeepEqualOptions {
            public bool baseUriSignificant = false;
            public bool commentsSignificant = false;
            public bool debug = false;
            public bool falseOnError = false;
            public bool idSignificant = false;
            public bool idrefSignificant = false;
            public bool inScopeNamespacesSignificant = false;
            public bool namespacePrefixesSignificant = false;
            public string normalizationForm = null;
            public bool nilledSignificant = false;
            public bool normalizeSpace = false;
            public bool processingInstructionsSignificant = false;
            public bool textBoundariesSignificant = true;
            public bool timezonesSignificant = false;
            public bool typeAnnotationsSignificant = false;
            public bool typeVarietySignificant = true;
            public bool typedValuesSignificant = true;
            public System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> unorderedElements = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>();
            public bool preserveSpace = true;
            public string collationName;
            public Saxon.Hej.lib.StringCollator stringCollator;
            public Saxon.Hej.expr.sort.AtomicComparer comparer;
            private static readonly string[] booleanOptions = new string[]{"base-uri", "comments", "debug", "false-on-error", "id-property", "idrefs-property", "in-scope-namespaces", "namespace-prefixes", "nilled-property", "normalize-space", "preserve-space", "processing-instructions", "text-boundaries", "timezones", "type-annotations", "type-variety", "typed-values"};
            public DeepEqualOptions()  {
            }
            public DeepEqualOptions(Saxon.Hej.ma.map.MapItem map, string collationName, Saxon.Hej.expr.XPathContext context)  {
                System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> values = OPTION_DETAILS.processSuppliedOptions(map, context);
                foreach (string option in Saxon.Hej.functions.DeepEqual.DeepEqualOptions.booleanOptions) {
                    setBooleanOption(values, option);
                }
                Saxon.Hej.om.GroundedValue normForm = map.get(new Saxon.Hej.value.StringValue("normalization-form"));
                if ((normForm != null)) {
                    normalizationForm = normForm.getStringValue();
                }
                Saxon.Hej.om.GroundedValue listedElements = map.get(new Saxon.Hej.value.StringValue("unordered-elements"));
                if ((listedElements != null)) {
                    unorderedElements = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>();
                    foreach (Saxon.Hej.om.Item item in listedElements.asIterable()) {
                        if ((item is Saxon.Hej.value.QNameValue)) {
                            unorderedElements.Add(((Saxon.Hej.value.QNameValue)(item)).getStructuredQName());
                        }
                    }
                }
                this.collationName = collationName;
                stringCollator = context.getConfiguration().getCollation(collationName);
                if ((stringCollator == null)) {
                    throw new Saxon.Hej.trans.XPathException(("Unknown collation "+collationName), "FOCH0002");
                }
                comparer = Saxon.Hej.expr.sort.GenericAtomicComparer.makeAtomicComparer(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, stringCollator, context);
                if ((normalizeSpace || (normalizationForm != null))) {
                    comparer = new Saxon.Hej.functions.DeepEqual.NormalizingComparer(comparer, this);
                }
            }
            public static Saxon.Hej.functions.DeepEqual.DeepEqualOptions defaultOptions() {
                return new Saxon.Hej.functions.DeepEqual.DeepEqualOptions();
            }
            private void setBooleanOption(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> map, string optionName) {
                Saxon.Hej.om.Sequence value = Saxon.Impl.Helpers.MapUtils.Get(map, optionName);
                if ((value != null)) {
                    bool booleanValue = Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(value.iterate());
                    switch (optionName) {
                      case "base-uri":
                        baseUriSignificant = booleanValue;
                        return ;
                      case "comments":
                        commentsSignificant = booleanValue;
                        return ;
                      case "debug":
                        debug = booleanValue;
                        return ;
                      case "false-on-error":
                        falseOnError = booleanValue;
                        return ;
                      case "id-property":
                        idSignificant = booleanValue;
                        return ;
                      case "idrefs-property":
                        idrefSignificant = booleanValue;
                        return ;
                      case "in-scope-namespaces":
                        inScopeNamespacesSignificant = booleanValue;
                        return ;
                      case "namespace-prefixes":
                        namespacePrefixesSignificant = booleanValue;
                        return ;
                      case "nilled-property":
                        nilledSignificant = booleanValue;
                        return ;
                      case "normalize-space":
                        normalizeSpace = booleanValue;
                        return ;
                      case "preserve-space":
                        preserveSpace = booleanValue;
                        return ;
                      case "processing-instructions":
                        processingInstructionsSignificant = booleanValue;
                        return ;
                      case "text-boundaries":
                        textBoundariesSignificant = booleanValue;
                        return ;
                      case "timezones":
                        timezonesSignificant = booleanValue;
                        return ;
                      case "type-annotations":
                        typeAnnotationsSignificant = booleanValue;
                        return ;
                      case "type-variety":
                        typeVarietySignificant = booleanValue;
                        return ;
                      case "typed-values":
                        typedValuesSignificant = booleanValue;
                        return ;
                      default:
                        throw new Saxon.Ejava.lang.IllegalArgumentException();
                    }
                }
            }
        
        }
        public static bool deepEqual(Saxon.Hej.om.SequenceIterator op1, Saxon.Hej.om.SequenceIterator op2, Saxon.Hej.expr.XPathContext context, Saxon.Hej.functions.DeepEqual.DeepEqualOptions options) {
            bool result = true;
            string reason = null;
            Saxon.Hej.lib.ErrorReporter reporter = context.getErrorReporter();
            try {
                if (!(options.textBoundariesSignificant)) {
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
                    if (((item1 is Saxon.Hej.om.FunctionItem) || (item2 is Saxon.Hej.om.FunctionItem))) {
                        if (!((((item1 is Saxon.Hej.om.FunctionItem) && (item2 is Saxon.Hej.om.FunctionItem))))) {
                            reason = (("if one item is a function then both must be functions (position "+pos1)+")");
                            return false;
                        }
                        bool fe = ((Saxon.Hej.om.FunctionItem)(item1)).deepEqual40((Saxon.Hej.om.FunctionItem)(item2), context, options);
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
                            string message = deepEqual((Saxon.Hej.om.NodeInfo)(item1), (Saxon.Hej.om.NodeInfo)(item2), context, options);
                            if ((message != null)) {
                                result = false;
                                reason = ((("nodes at position "+pos1)+" differ: ")+message);
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
                                if (!(options.comparer.comparesEqual(av1, av2))) {
                                    result = false;
                                    reason = (("atomic values at position "+pos1)+" differ");
                                    break ;
                                }
                            }
                            if ((options.typeAnnotationsSignificant && !(av1.getItemType().Equals(av2.getItemType())))) {
                                result = false;
                                reason = (("atomic values at position "+pos1)+" have different type annotations");
                                break ;
                            }
                            if ((((options.namespacePrefixesSignificant && (av1 is Saxon.Hej.value.QualifiedNameValue)) && (av2 is Saxon.Hej.value.QualifiedNameValue)) && !(((Saxon.Hej.value.QualifiedNameValue)(av1)).getPrefix().Equals(((Saxon.Hej.value.QualifiedNameValue)(av2)).getPrefix())))) {
                                result = false;
                                reason = (("QName values at position "+pos1)+" have different namespace prefixes");
                                break ;
                            }
                            if ((((options.timezonesSignificant && (av1 is Saxon.Hej.value.CalendarValue)) && (av2 is Saxon.Hej.value.CalendarValue)) && (((Saxon.Hej.value.CalendarValue)(av1)).getTimezoneInMinutes() != ((Saxon.Hej.value.CalendarValue)(av2)).getTimezoneInMinutes()))) {
                                result = false;
                                reason = (("Values at position "+pos1)+" have different timezone");
                                break ;
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
                explain(reporter, reason, options, null, null);
            }
            return result;
        }
        public static string deepEqual(Saxon.Hej.om.NodeInfo n1, Saxon.Hej.om.NodeInfo n2, Saxon.Hej.expr.XPathContext context, Saxon.Hej.functions.DeepEqual.DeepEqualOptions options) {
            if (n1.Equals(n2)) {
                return null;
            }
            Saxon.Hej.lib.ErrorReporter reporter = context.getErrorReporter();
            if ((n1.getNodeKind() != n2.getNodeKind())) {
                string reason = ((("node kinds differ: comparing "+showKind(n1))+" to ")+showKind(n2));
                explain(reporter, reason, options, n1, n2);
                return reason;
            }
            if ((options.baseUriSignificant && !(Saxon.Ejava.util.Objects.Equals(n1.getBaseURI(), n2.getBaseURI())))) {
                string reason = ((("base URIs differ: comparing "+n1.getBaseURI())+" to ")+n2.getBaseURI());
                explain(reporter, reason, options, n1, n2);
                return reason;
            }
            if ((options.typeAnnotationsSignificant && !(n1.getSchemaType().Equals(n2.getSchemaType())))) {
                string reason = "nodes have different type annotations";
                explain(reporter, reason, options, n1, n2);
                return reason;
            }
            switch (n1.getNodeKind()) {
              case Saxon.Hej.type.Type.ELEMENT:
                if (!(Saxon.Hej.tree.util.Navigator.haveSameName(n1, n2))) {
                    string reason = ((("element names differ: "+Saxon.Hej.om.NameOfNode.makeName(n1).getStructuredQName().getEQName())+" != ")+Saxon.Hej.om.NameOfNode.makeName(n2).getStructuredQName().getEQName());
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                if ((options.namespacePrefixesSignificant && !(n1.getPrefix().Equals(n2.getPrefix())))) {
                    string reason = ((("element prefixes differ: "+n1.getPrefix())+" != ")+n2.getPrefix());
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                if ((options.inScopeNamespacesSignificant && !(n1.getAllNamespaces().Equals(n2.getAllNamespaces())))) {
                    string reason = ((("in-scope namespaces differ: "+n1.getAllNamespaces())+" versus ")+n2.getAllNamespaces());
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                Saxon.Hej.tree.iter.AxisIterator a1 = n1.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                Saxon.Hej.tree.iter.AxisIterator a2 = n2.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                if (!(Saxon.Hej.om.SequenceTool.sameLength(a1, a2))) {
                    string reason = "elements have different number of attributes";
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                Saxon.Hej.om.NodeInfo att1;
                a1 = n1.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                while (((att1 = ((/*1430*/Saxon.Hej.om.NodeInfo)a1.next())) != null)) {
                    Saxon.Hej.tree.iter.AxisIterator a2iter = n2.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE, new Saxon.Hej.pattern.SameNameTest(att1));
                    Saxon.Hej.om.NodeInfo att2 = ((/*1430*/Saxon.Hej.om.NodeInfo)a2iter.next());
                    if ((att2 == null)) {
                        string reason = (("one element has an attribute "+Saxon.Hej.om.NameOfNode.makeName(att1).getStructuredQName().getEQName())+", the other does not");
                        explain(reporter, reason, options, n1, n2);
                        return reason;
                    }
                    string attReason = deepEqual(att1, att2, context, options);
                    if ((attReason != null)) {
                        string reason = ((("elements have different values for the attribute "+Saxon.Hej.om.NameOfNode.makeName(att1).getStructuredQName().getEQName())+" - ")+attReason);
                        explain(reporter, reason, options, n1, n2);
                        return reason;
                    }
                }
                if (options.inScopeNamespacesSignificant) {
                    Saxon.Hej.om.NamespaceMap nm1 = n1.getAllNamespaces();
                    Saxon.Hej.om.NamespaceMap nm2 = n2.getAllNamespaces();
                    if (!(nm1.Equals(nm2))) {
                        string reason = ((("elements have different in-scope namespaces: "+nm1)+" versus ")+nm2);
                        explain(reporter, reason, options, n1, n2);
                        return reason;
                    }
                }
                if (options.typeAnnotationsSignificant) {
                    if (!(n1.getSchemaType().Equals(n2.getSchemaType()))) {
                        string reason = "elements have different type annotation";
                        explain(reporter, reason, options, n1, n2);
                        return reason;
                    }
                }
                if (options.typeVarietySignificant) {
                    if ((n1.getSchemaType().isComplexType() != n2.getSchemaType().isComplexType())) {
                        string reason = "one element has complex type, the other simple";
                        explain(reporter, reason, options, n1, n2);
                        return reason;
                    }
                    if (n1.getSchemaType().isComplexType()) {
                        Saxon.Hej.type.ComplexVariety variety1 = ((Saxon.Hej.type.ComplexType)(n1.getSchemaType())).getVariety();
                        Saxon.Hej.type.ComplexVariety variety2 = ((Saxon.Hej.type.ComplexType)(n2.getSchemaType())).getVariety();
                        if ((variety1 != variety2)) {
                            string reason = "both elements have complex type, but a different variety";
                            explain(reporter, reason, options, n1, n2);
                            return reason;
                        }
                    }
                }
                if (options.typedValuesSignificant) {
                    Saxon.Hej.type.SchemaType type1 = n1.getSchemaType();
                    Saxon.Hej.type.SchemaType type2 = n2.getSchemaType();
                    bool isSimple1 = (type1.isSimpleType() || ((Saxon.Hej.type.ComplexType)(type1)).isSimpleContent());
                    bool isSimple2 = (type2.isSimpleType() || ((Saxon.Hej.type.ComplexType)(type2)).isSimpleContent());
                    if ((options.typeVarietySignificant && (isSimple1 != isSimple2))) {
                        string reason = "one element has a simple type, the other does not";
                        explain(reporter, reason, options, n1, n2);
                        return reason;
                    }
                    if ((isSimple1 && isSimple2)) {
                        Saxon.Hej.tree.iter.AtomicIterator v1 = ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)n1.atomize().iterate());
                        Saxon.Hej.tree.iter.AtomicIterator v2 = ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)n2.atomize().iterate());
                        bool typedValueComparison = deepEqual(v1, v2, context, options);
                        return (typedValueComparison ? null : "typed values of elements differ");
                    }
                }
                if ((options.idSignificant && (n1.isId() != n2.isId()))) {
                    string reason = "one element is an ID, the other is not";
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                if ((options.idrefSignificant && (n1.isIdref() != n2.isIdref()))) {
                    string reason = "one element is an IDREF, the other is not";
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                if ((options.nilledSignificant && (n1.isNilled() != n2.isNilled()))) {
                    string reason = "one element is nilled, the other is not";
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                if (options.unorderedElements.Contains(Saxon.Hej.om.NameOfNode.makeName(n1).getStructuredQName())) {
                    return hasSameChildrenUnordered(n1, n2, options, context);
                }
                goto case Saxon.Hej.type.Type.DOCUMENT;
              case Saxon.Hej.type.Type.DOCUMENT:
                Saxon.Hej.om.SequenceIterator c1 = n1.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeSelector.of(node => !(isIgnorable(node, options))));
                Saxon.Hej.om.SequenceIterator c2 = n2.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeSelector.of(node => !(isIgnorable(node, options))));
                if (!(options.textBoundariesSignificant)) {
                    c1 = mergeAdjacentTextNodes(c1);
                    c2 = mergeAdjacentTextNodes(c2);
                }
                while (true) {
                    Saxon.Hej.om.NodeInfo d1 = (Saxon.Hej.om.NodeInfo)(c1.next());
                    Saxon.Hej.om.NodeInfo d2 = (Saxon.Hej.om.NodeInfo)(c2.next());
                    if (((d1 == null) || (d2 == null))) {
                        bool r = (d1 == d2);
                        if (!(r)) {
                            string message = (("the first operand contains a node with "+(((d1 == null) ? "fewer" : "more")))+" children than the second");
                            if (((d1 is Saxon.Hej.tree.tiny.WhitespaceTextImpl) || (d2 is Saxon.Hej.tree.tiny.WhitespaceTextImpl))) {
                                message += " (the first extra child is whitespace text)";
                            }
                            explain(reporter, message, options, n1, n2);
                            return message;
                        }
                        return null;
                    }
                    string recursiveResult = deepEqual(d1, d2, context, options);
                    if ((recursiveResult != null)) {
                        return recursiveResult;
                    }
                }
              case Saxon.Hej.type.Type.ATTRIBUTE:
                if (!(Saxon.Hej.tree.util.Navigator.haveSameName(n1, n2))) {
                    string reason = ((("attribute names differ: "+Saxon.Hej.om.NameOfNode.makeName(n1).getStructuredQName().getEQName())+" != ")+Saxon.Hej.om.NameOfNode.makeName(n1).getStructuredQName().getEQName());
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                if ((options.namespacePrefixesSignificant && !(n1.getPrefix().Equals(n2.getPrefix())))) {
                    string reason = ((("attribute prefixes differ: "+n1.getPrefix())+" != ")+n2.getPrefix());
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                if (options.typeAnnotationsSignificant) {
                    if (!(n1.getSchemaType().Equals(n2.getSchemaType()))) {
                        string reason = "attributes have different type annotations";
                        explain(reporter, reason, options, n1, n2);
                        return reason;
                    }
                }
                bool ar;
                if (options.typedValuesSignificant) {
                    ar = deepEqual(((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)n1.atomize().iterate()), ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)n2.atomize().iterate()), context, options);
                } else {
                    ar = options.comparer.comparesEqual(new Saxon.Hej.value.StringValue(n1.getUnicodeStringValue()), new Saxon.Hej.value.StringValue(n2.getUnicodeStringValue()));
                }
                if (!(ar)) {
                    string reason = "attribute values differ";
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                if ((options.idSignificant && (n1.isId() != n2.isId()))) {
                    string reason = "one attribute is an ID, the other is not";
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                if ((options.idrefSignificant && (n1.isIdref() != n2.isIdref()))) {
                    string reason = "one attribute is an IDREF, the other is not";
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                return null;
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
              case Saxon.Hej.type.Type.NAMESPACE:
                if (!(n1.getLocalPart().Equals(n2.getLocalPart()))) {
                    string reason = (Saxon.Hej.type.Type.displayTypeName(n1)+" names differ");
                    explain(reporter, reason, options, n1, n2);
                    return reason;
                }
                goto case Saxon.Hej.type.Type.TEXT;
              case Saxon.Hej.type.Type.TEXT:
              case Saxon.Hej.type.Type.COMMENT:
                bool vr = compareStrings(n1.getStringValue(), n2.getStringValue(), options, context);
                if (!(vr)) {
                    if (options.debug) {
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
                        explain(reporter, ((((((Saxon.Hej.type.Type.displayTypeName(n1)+" values differ (")+Saxon.Hej.tree.util.Navigator.getPath(n1))+", ")+Saxon.Hej.tree.util.Navigator.getPath(n2))+"): ")+message), options, n1, n2);
                        return message;
                    } else {
                        return "atomized values differ";
                    }
                }
                return null;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Unknown node kind");
            }
        }
        private static string hasSameChildrenUnordered(Saxon.Hej.om.NodeInfo e0, Saxon.Hej.om.NodeInfo e1, Saxon.Hej.functions.DeepEqual.DeepEqualOptions options, Saxon.Hej.expr.XPathContext context) {
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> children0 = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>();
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> children1 = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>();
            foreach (Saxon.Hej.om.NodeInfo c0 in e0.children()) {
                if (!(isIgnorable(c0, options))) {
                    children0.Add(c0);
                }
            }
            foreach (Saxon.Hej.om.NodeInfo c1 in e1.children()) {
                if (!(isIgnorable(c1, options))) {
                    children1.Add(c1);
                }
            }
            if ((children0.Count != children1.Count)) {
                return ((("Number of children differs: "+children0.Count)+" vs. ")+children1.Count);
            }
            System.Collections.Generic.IList<int> hashcodes1 = new System.Collections.Generic.List<int>(children1.Count);
            Saxon.Hej.z.IntSet hashSet = new Saxon.Hej.z.IntHashSet();
            foreach (Saxon.Hej.om.NodeInfo nodeInfo in children1) {
                int hash = computeHashCode(nodeInfo, options);
                hashSet.add(hash);
                hashcodes1.Add(hash);
            }
            foreach (Saxon.Hej.om.NodeInfo c0 in children0) {
                int hash = computeHashCode(c0, options);
                if (!(hashSet.contains(hash))) {
                    return "Node found among first node's children with no counterpart among the second node's children";
                }
                int found = -1;
                for (int j = 0; (j<hashcodes1.Count); j++) {
                    if (((hash == hashcodes1[j]) && (deepEqual(c0, children1[j], context, options) == null))) {
                        found = j;
                        break ;
                    }
                }
                if ((found>=0)) {
                    Saxon.Impl.Helpers.ListUtils.Remove(children1, found);
                    Saxon.Impl.Helpers.ListUtils.Remove(hashcodes1, found);
                } else {
                    return "Node found among first node's children with no counterpart among the second node's children";
                }
            }
            return null;
        }
        private static int computeHashCode(Saxon.Hej.om.NodeInfo node, Saxon.Hej.functions.DeepEqual.DeepEqualOptions options) {
            return (((node.getNodeKind()<<24)^node.getFingerprint())^((node.attributes().size()<<10)));
        }
        private static long hashCodeOfSequence(Saxon.Hej.om.GroundedValue value, System.Func<Saxon.Hej.om.Item,System.Int64> hash) {
            long h = 0;
            foreach (Saxon.Hej.om.Item it in value.asIterable()) {
                h ^= hash(it);
            }
            return h;
        }
        private static long hashCodeOfNode(Saxon.Hej.om.NodeInfo node, Saxon.Hej.functions.DeepEqual.DeepEqualOptions options, Saxon.Hej.expr.XPathContext context) {
            int kind = node.getNodeKind();
            long h = 0x7876ABCD2345DCBAL;
            h ^= (((long)(node.getFingerprint())<<25));
            if (options.namespacePrefixesSignificant) {
                h ^= (((long)(node.getPrefix().GetHashCode())<<13));
            }
            if (((kind == Saxon.Hej.type.Type.TEXT) && !(Saxon.Hej.value.Whitespace.isAllWhite(node.getUnicodeStringValue())))) {
                string s = node.getStringValue();
                if (options.normalizeSpace) {
                    s = Saxon.Hej.value.Whitespace.collapseWhitespace(s);
                }
                if ((options.normalizationForm != null)) {
                    try {
                        s = Saxon.Hej.functions.NormalizeUnicode.normalize(s, options.normalizationForm);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException(e);
                    }
                }
                h ^= ((long)(s.GetHashCode())<<5);
            }
            return h;
        }
        private static bool compareStrings(string s1, string s2, Saxon.Hej.functions.DeepEqual.DeepEqualOptions options, Saxon.Hej.expr.XPathContext context) {
            if (options.normalizeSpace) {
                s1 = Saxon.Hej.value.Whitespace.collapseWhitespace(s1);
                s2 = Saxon.Hej.value.Whitespace.collapseWhitespace(s2);
            }
            if ((options.normalizationForm != null)) {
                try {
                    s1 = Saxon.Hej.functions.NormalizeUnicode.normalize(s1, options.normalizationForm);
                    s2 = Saxon.Hej.functions.NormalizeUnicode.normalize(s2, options.normalizationForm);
                } catch (Saxon.Hej.trans.XPathException e) {
                    return false;
                }
            }
            return options.stringCollator.comparesEqual(Saxon.Hej.str.StringView.of(s1), Saxon.Hej.str.StringView.of(s2));
        }
        private static bool isIgnorable(Saxon.Hej.om.NodeInfo node, Saxon.Hej.functions.DeepEqual.DeepEqualOptions options) {
            int kind = node.getNodeKind();
            if ((kind == Saxon.Hej.type.Type.COMMENT)) {
                return !(options.commentsSignificant);
            } else {
                if ((kind == Saxon.Hej.type.Type.PROCESSING_INSTRUCTION)) {
                    return !(options.processingInstructionsSignificant);
                } else {
                    if ((kind == Saxon.Hej.type.Type.TEXT)) {
                        return ((!(options.preserveSpace)) && Saxon.Hej.value.Whitespace.isAllWhite(node.getUnicodeStringValue()));
                    }
                }
            }
            return false;
        }
        private static void explain(Saxon.Hej.lib.ErrorReporter reporter, string message, Saxon.Hej.functions.DeepEqual.DeepEqualOptions options, Saxon.Hej.om.NodeInfo n1, Saxon.Hej.om.NodeInfo n2) {
            if (options.debug) {
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
                sb.append(binding.getNamespaceUri());
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
            Saxon.Hej.om.Item arg3 = ((arguments.Length>=3) ? arguments[2].head() : null);
            string collationName = ((arg3 == null) ? getRetainedStaticContext().getDefaultCollationName() : arg3.getStringValue());
            Saxon.Hej.ma.map.MapItem options = new Saxon.Hej.ma.map.DictionaryMap();
            if ((arguments.Length>=4)) {
                Saxon.Hej.ma.map.MapItem suppliedOptions = (Saxon.Hej.ma.map.MapItem)(arguments[3].head());
                if ((suppliedOptions != null)) {
                    options = suppliedOptions;
                }
            }
            Saxon.Hej.functions.DeepEqual.DeepEqualOptions eqOptions = new Saxon.Hej.functions.DeepEqual.DeepEqualOptions(options, collationName, context);
            bool b = deepEqual(arguments[0].iterate(), arguments[1].iterate(), context, eqOptions);
            return Saxon.Hej.value.BooleanValue.get(b);
        }
        public override string getStreamerName() {
            return "DeepEqual";
        }
        private class NormalizingComparer : Saxon.Hej.expr.sort.AtomicComparer {
            private Saxon.Hej.expr.sort.AtomicComparer baseComparer;
            private Saxon.Hej.functions.DeepEqual.DeepEqualOptions options;
            public NormalizingComparer(Saxon.Hej.expr.sort.AtomicComparer baseComparer, Saxon.Hej.functions.DeepEqual.DeepEqualOptions options)  {
                this.baseComparer = baseComparer;
                this.options = options;
            }
            public Saxon.Hej.lib.StringCollator getCollator() {
                return baseComparer.getCollator();
            }
            public Saxon.Hej.expr.sort.AtomicComparer provideContext(Saxon.Hej.expr.XPathContext context) {
                baseComparer = baseComparer.provideContext(context);
                return this;
            }
            public int compareAtomicValues(Saxon.Hej.value.AtomicValue v0, Saxon.Hej.value.AtomicValue v1) {
                return baseComparer.compareAtomicValues(v0, v1);
            }
            public bool comparesEqual(Saxon.Hej.value.AtomicValue v0, Saxon.Hej.value.AtomicValue v1) {
                if (((v0 is Saxon.Hej.value.StringValue) && (v1 is Saxon.Hej.value.StringValue))) {
                    Saxon.Hej.str.UnicodeString u0 = v0.getUnicodeStringValue();
                    Saxon.Hej.str.UnicodeString u1 = v1.getUnicodeStringValue();
                    if (options.normalizeSpace) {
                        u0 = Saxon.Hej.value.Whitespace.collapseWhitespace(u0);
                        u1 = Saxon.Hej.value.Whitespace.collapseWhitespace(u1);
                    }
                    if ((options.normalizationForm != null)) {
                        try {
                            u0 = Saxon.Hej.str.StringView.of(Saxon.Hej.functions.NormalizeUnicode.normalize(u0.ToString(), options.normalizationForm));
                        } catch (Saxon.Hej.trans.XPathException e) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException(e);
                        }
                        try {
                            u1 = Saxon.Hej.str.StringView.of(Saxon.Hej.functions.NormalizeUnicode.normalize(u1.ToString(), options.normalizationForm));
                        } catch (Saxon.Hej.trans.XPathException e) {
                            throw new Saxon.Ejava.lang.IllegalArgumentException();
                        }
                    }
                    return getCollator().comparesEqual(u0, u1);
                } else {
                    return baseComparer.comparesEqual(v0, v1);
                }
            }
            public string save() {
                return null;
            }
        
        }
    
    }
}