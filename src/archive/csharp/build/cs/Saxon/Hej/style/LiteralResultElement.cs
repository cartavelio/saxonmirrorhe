using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class LiteralResultElement : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.om.NodeName resultNodeName;
        private Saxon.Hej.om.NodeName[] attributeNames;
        private Saxon.Hej.expr.Expression[] attributeValues;
        private int numberOfAttributes;
        private bool toplevel;
        private Saxon.Hej.om.NamespaceMap retainedNamespaces = Saxon.Hej.om.NamespaceMap.emptyMap();
        private Saxon.Hej.om.StructuredQName[] attributeSets;
        private Saxon.Hej.type.SchemaType schemaType = null;
        private int validation = Saxon.Hej.lib.Validation.STRIP;
        private bool inheritNamespaces = true;
        private static readonly Saxon.Hej.z.IntHashSet STANDARD_ATTRIBUTES = Saxon.Hej.z.IntHashSet.of(Saxon.Hej.om.StandardNames.XSL_USE_ATTRIBUTE_SETS, Saxon.Hej.om.StandardNames.XSL_DEFAULT_COLLATION, Saxon.Hej.om.StandardNames.XSL_DEFAULT_MODE, Saxon.Hej.om.StandardNames.XSL_DEFAULT_VALIDATION, Saxon.Hej.om.StandardNames.XSL_EXTENSION_ELEMENT_PREFIXES, Saxon.Hej.om.StandardNames.XSL_EXCLUDE_RESULT_PREFIXES, Saxon.Hej.om.StandardNames.XSL_EXPAND_TEXT, Saxon.Hej.om.StandardNames.XSL_VERSION, Saxon.Hej.om.StandardNames.XSL_XPATH_DEFAULT_NAMESPACE, Saxon.Hej.om.StandardNames.XSL_TYPE, Saxon.Hej.om.StandardNames.XSL_USE_WHEN, Saxon.Hej.om.StandardNames.XSL_VALIDATION);
        public LiteralResultElement()  {
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        public override bool isInstruction() {
            return true;
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            int num = atts.size();
            if ((num == 0)) {
                numberOfAttributes = 0;
            } else {
                attributeNames = new Saxon.Hej.om.NodeName[num];
                attributeValues = new Saxon.Hej.expr.Expression[num];
                numberOfAttributes = 0;
                foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                    Saxon.Hej.om.NodeName name = att.getNodeName();
                    int fp = name.getFingerprint();
                    string attURI = name.getURI();
                    if (attURI.Equals(Saxon.Hej.lib.NamespaceConstant.XSLT)) {
                        if (!(STANDARD_ATTRIBUTES.contains(fp))) {
                            if ((fp == Saxon.Hej.om.StandardNames.XSL_INHERIT_NAMESPACES)) {
                                inheritNamespaces = processBooleanAttribute("xsl:inherit-namespaces", att.getValue());
                            } else {
                                if (!(forwardsCompatibleModeIsEnabled())) {
                                    compileError(("Unknown XSLT attribute "+Saxon.Hej.trans.Err.wrap(name.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE)), "XTSE0805");
                                }
                            }
                        }
                    } else {
                        attributeNames[numberOfAttributes] = name;
                        Saxon.Hej.expr.Expression exp = makeAttributeValueTemplate(att.getValue(), att);
                        attributeValues[numberOfAttributes] = exp;
                        numberOfAttributes++;
                    }
                }
                if ((numberOfAttributes<attributeNames.Length)) {
                    attributeNames = Saxon.Ejava.util.Arrays.copyOf(attributeNames, numberOfAttributes);
                    attributeValues = Saxon.Ejava.util.Arrays.copyOf(attributeValues, numberOfAttributes);
                }
            }
            resultNodeName = getNodeName();
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            toplevel = ((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLStylesheet));
            resultNodeName = getNodeName();
            string elementURI = getURI();
            if (toplevel) {
                if (elementURI.Length==0) {
                    compileError("Top level elements must have a non-null namespace URI", "XTSE0130");
                }
            } else {
                retainedNamespaces = getAllNamespaces();
                Saxon.Hej.style.PrincipalStylesheetModule sheet = getPrincipalStylesheetModule();
                if (sheet.hasNamespaceAliases()) {
                    Saxon.Hej.om.NamespaceMap aliasedNamespaces = retainedNamespaces;
                    foreach (Saxon.Hej.om.NamespaceBinding nb in retainedNamespaces) {
                        string suri = nb.getURI();
                        Saxon.Hej.om.NamespaceBinding ncode = sheet.getNamespaceAlias(suri);
                        if (((ncode != null) && !(ncode.getURI().Equals(suri)))) {
                            aliasedNamespaces = aliasedNamespaces.remove(nb.getPrefix());
                            if (!(ncode.getURI().Length==0)) {
                                aliasedNamespaces = aliasedNamespaces.put(ncode.getPrefix(), ncode.getURI());
                            }
                        }
                    }
                    retainedNamespaces = aliasedNamespaces;
                    Saxon.Hej.om.NamespaceBinding elementAlias = sheet.getNamespaceAlias(elementURI);
                    if (((elementAlias != null) && !(elementAlias.getURI().Equals(elementURI)))) {
                        resultNodeName = new Saxon.Hej.om.FingerprintedQName(elementAlias.getPrefix(), elementAlias.getURI(), getLocalPart());
                    }
                }
                string useAttSets = getAttributeValue(Saxon.Hej.lib.NamespaceConstant.XSLT, "use-attribute-sets");
                if ((useAttSets != null)) {
                    attributeSets = getUsedAttributeSets(useAttSets);
                }
                validation = getDefaultValidation();
                string type = getAttributeValue(Saxon.Hej.lib.NamespaceConstant.XSLT, "type");
                if ((type != null)) {
                    if (!(isSchemaAware())) {
                        compileError("The xsl:type attribute is available only with a schema-aware XSLT processor", "XTSE1660");
                    }
                    schemaType = getSchemaType(type);
                    validation = Saxon.Hej.lib.Validation.BY_TYPE;
                }
                string validate = getAttributeValue(Saxon.Hej.lib.NamespaceConstant.XSLT, "validation");
                if ((validate != null)) {
                    validation = validateValidationAttribute(validate);
                    if ((schemaType != null)) {
                        compileError("The attributes xsl:type and xsl:validation are mutually exclusive", "XTSE1505");
                    }
                }
                if ((numberOfAttributes>0)) {
                    bool changed = false;
                    for (int i = 0; (i<numberOfAttributes); i++) {
                        Saxon.Hej.om.NodeName anameCode = attributeNames[i];
                        Saxon.Hej.om.NodeName alias = anameCode;
                        string attURI = anameCode.getURI();
                        if (!(attURI.Length==0)) {
                            Saxon.Hej.om.NamespaceBinding newBinding = sheet.getNamespaceAlias(attURI);
                            if (((newBinding != null) && !(newBinding.getURI().Equals(attURI)))) {
                                alias = new Saxon.Hej.om.FingerprintedQName(newBinding.getPrefix(), newBinding.getURI(), anameCode.getLocalPart());
                                changed = true;
                            }
                        }
                        attributeNames[i] = alias;
                        attributeValues[i] = typeCheck(alias.getDisplayName(), attributeValues[i]);
                    }
                    if ((changed && (numberOfAttributes>1))) {
                        Saxon.Hej.z.IntSet names = new Saxon.Hej.z.IntHashSet(numberOfAttributes);
                        for (int i = 0; (i<numberOfAttributes); i++) {
                            int fp = attributeNames[i].obtainFingerprint(getNamePool());
                            bool absent = names.add(fp);
                            if (!(absent)) {
                                compileError("As a result of namespace aliasing, two attributes have the same expanded name", "XTSE0813");
                            }
                        }
                    }
                }
                Saxon.Hej.om.NamespaceMap afterExclusions = retainedNamespaces;
                foreach (Saxon.Hej.om.NamespaceBinding nb in retainedNamespaces) {
                    string uri = nb.getURI();
                    if ((isExcludedNamespace(uri) && !(sheet.isAliasResultNamespace(uri)))) {
                        afterExclusions = afterExclusions.remove(nb.getPrefix());
                    }
                }
                retainedNamespaces = afterExclusions;
            }
        }
        protected internal override void validateChildren(Saxon.Hej.style.ComponentDeclaration decl, bool excludeStylesheet) {
            if (!(toplevel)) {
                base.validateChildren(decl, excludeStylesheet);
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if (toplevel) {
                return null;
            }
            Saxon.Hej.expr.instruct.FixedElement inst = new Saxon.Hej.expr.instruct.FixedElement(resultNodeName, retainedNamespaces, inheritNamespaces, true, schemaType, validation);
            inst.setLocation(allocateLocation());
            Saxon.Hej.expr.Expression content = compileSequenceConstructor(exec, decl, true);
            if ((numberOfAttributes>0)) {
                for (int i = (attributeNames.Length-1); (i>=0); i--) {
                    Saxon.Hej.expr.instruct.FixedAttribute att = new Saxon.Hej.expr.instruct.FixedAttribute(attributeNames[i], Saxon.Hej.lib.Validation.STRIP, null);
                    att.setRetainedStaticContext(makeRetainedStaticContext());
                    att.setSelect(attributeValues[i]);
                    att.setLocation(allocateLocation());
                    Saxon.Hej.expr.Expression exp = att;
                    if ((content == null)) {
                        content = exp;
                    } else {
                        content = Saxon.Hej.expr.instruct.Block.makeBlock(exp, content);
                        content.setLocation(allocateLocation());
                    }
                }
            }
            if ((attributeSets != null)) {
                Saxon.Hej.expr.Expression use = Saxon.Hej.expr.instruct.UseAttributeSet.makeUseAttributeSets(attributeSets, this);
                if ((content == null)) {
                    content = use;
                } else {
                    content = Saxon.Hej.expr.instruct.Block.makeBlock(use, content);
                    content.setLocation(allocateLocation());
                }
            }
            if ((content == null)) {
                content = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            inst.setContentExpression(content);
            inst.setRetainedStaticContext(makeRetainedStaticContext());
            inst.setLocation(allocateLocation());
            return inst;
        }
        public Saxon.Hej.tree.linked.DocumentImpl makeStylesheet(bool topLevel) {
            Saxon.Hej.style.StyleNodeFactory nodeFactory = getCompilation().getStyleNodeFactory(topLevel);
            if (!(isInScopeNamespace(Saxon.Hej.lib.NamespaceConstant.XSLT))) {
                string message;
                if ((getLocalPart().Equals("stylesheet") || getLocalPart().Equals("transform"))) {
                    message = ("Namespace for stylesheet element should be "+Saxon.Hej.lib.NamespaceConstant.XSLT);
                } else {
                    message = "The supplied file does not appear to be a stylesheet";
                }
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
                err.setLocation(allocateLocation());
                err.setErrorCode("XTSE0150");
                err.setIsStaticError(true);
                compileError(err);
                throw err;
            }
            string version = getAttributeValue(Saxon.Hej.lib.NamespaceConstant.XSLT, "version");
            if ((version == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Simplified stylesheet: xsl:version attribute is missing");
                err.setErrorCode("XTSE0150");
                err.setIsStaticError(true);
                err.setLocation(allocateLocation());
                compileError(err);
                throw err;
            }
            try {
                Saxon.Hej.tree.linked.DocumentImpl oldRoot = (Saxon.Hej.tree.linked.DocumentImpl)(getRoot());
                Saxon.Hej.tree.linked.LinkedTreeBuilder builder = new Saxon.Hej.tree.linked.LinkedTreeBuilder(getConfiguration().makePipelineConfiguration());
                builder.setNodeFactory(nodeFactory);
                builder.setSystemId(this.getSystemId());
                builder.open();
                builder.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                Saxon.Hej.s9api.Location loc = Saxon.Hej.expr.parser.Loc.NONE;
                Saxon.Hej.om.NamespaceMap map = getAllNamespaces().put("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT);
                Saxon.Hej.om.AttributeMap atts = Saxon.Hej.om.EmptyAttributeMap.getInstance();
                atts = atts.put(new Saxon.Hej.om.AttributeInfo(new Saxon.Hej.om.NoNamespaceName("version"), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, version, loc, Saxon.Hej.@event.ReceiverOption.NONE));
                int st = Saxon.Hej.om.StandardNames.XSL_STYLESHEET;
                builder.startElement(new Saxon.Hej.om.CodedName(st, "xsl", getNamePool()), Saxon.Hej.type.Untyped.getInstance(), atts, map, loc, Saxon.Hej.@event.ReceiverOption.NONE);
                atts = Saxon.Hej.om.EmptyAttributeMap.getInstance();
                atts = atts.put(new Saxon.Hej.om.AttributeInfo(new Saxon.Hej.om.NoNamespaceName("match"), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, "/", loc, Saxon.Hej.@event.ReceiverOption.NONE));
                int te = Saxon.Hej.om.StandardNames.XSL_TEMPLATE;
                builder.startElement(new Saxon.Hej.om.CodedName(te, "xsl", getNamePool()), Saxon.Hej.type.Untyped.getInstance(), atts, map, loc, Saxon.Hej.@event.ReceiverOption.NONE);
                builder.graftElement(this);
                builder.endElement();
                builder.endElement();
                builder.endDocument();
                builder.close();
                Saxon.Hej.tree.linked.DocumentImpl newRoot = (Saxon.Hej.tree.linked.DocumentImpl)(builder.getCurrentRoot());
                newRoot.graftLocationMap(oldRoot);
                return newRoot;
            } catch (Saxon.Hej.trans.XPathException err) {
                err.setLocation(allocateLocation());
                throw;
            }
        }
        public override Saxon.Hej.om.StructuredQName getObjectName() {
            return new Saxon.Hej.om.StructuredQName(getPrefix(), getURI(), getLocalPart());
        }
    
    }
}