using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLKey : Saxon.Hej.style.StyleElement, Saxon.Hej.style.StylesheetComponent {
        private Saxon.Hej.pattern.Pattern match;
        private Saxon.Hej.expr.Expression use;
        private string collationName;
        private Saxon.Hej.om.StructuredQName keyName;
        private Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        private bool rangeKey;
        private bool composite = false;
        private Saxon.Hej.trans.KeyDefinition keyDefinition;
        public Saxon.Hej.expr.instruct.Actor getActor() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return null;
        }
        public void checkCompatibility(Saxon.Hej.expr.Component component) {
        }
        public override bool isDeclaration() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        public Saxon.Hej.expr.instruct.SlotManager getSlotManager() {
            return stackFrameMap;
        }
        protected internal override void prepareAttributes() {
            string nameAtt = null;
            string matchAtt = null;
            string useAtt;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string uri = attName.getURI();
                string local = attName.getLocalPart();
                if ("".Equals(uri)) {
                    switch (local) {
                      case "name":
                        nameAtt = Saxon.Hej.value.Whitespace.trim(value);
                        break ;
                      case "use":
                        useAtt = value;
                        use = makeExpression(useAtt, att);
                        break ;
                      case "match":
                        matchAtt = value;
                        break ;
                      case "collation":
                        collationName = Saxon.Hej.value.Whitespace.trim(value);
                        break ;
                      case "composite":
                        composite = processBooleanAttribute("composite", value);
                        break ;
                      default:
                        checkUnknownAttribute(attName);
                        break ;
                    }
                } else {
                    if ((local.Equals("range-key") && uri.Equals(Saxon.Hej.lib.NamespaceConstant.SAXON))) {
                        if (Saxon.Hej.Version.platform.isDotNet()) {
                            compileError("saxon:range-key is not supported in Saxon-CS");
                        }
                        rangeKey = processBooleanAttribute("range-key", value);
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((nameAtt == null)) {
                reportAbsence("name");
                nameAtt = "_dummy_key_name";
            }
            keyName = makeQName(nameAtt, null, "name");
            setObjectName(keyName);
            if ((matchAtt == null)) {
                reportAbsence("match");
                matchAtt = "*";
            }
            match = makePattern(matchAtt, "match");
            if ((match == null)) {
                match = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance());
            }
        }
        public Saxon.Hej.om.StructuredQName getKeyName() {
            if ((getObjectName() == null)) {
                string nameAtt = getAttributeValue("", "name");
                if ((nameAtt != null)) {
                    setObjectName(makeQName(nameAtt, null, "name"));
                }
            }
            return getObjectName();
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.Configuration config = getConfiguration();
            stackFrameMap = config.makeSlotManager();
            checkTopLevel("XTSE0010", false);
            if ((use != null)) {
                if (hasChildNodes()) {
                    compileError("An xsl:key element with a @use attribute must be empty", "XTSE1205");
                }
                try {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:key/use", 0);
                    use = config.getTypeChecker(false).staticTypeCheck(use, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE, role, makeExpressionVisitor());
                } catch (Saxon.Hej.trans.XPathException err) {
                    compileError(err);
                }
            } else {
                if (!(hasChildNodes())) {
                    compileError("An xsl:key element must either have a @use attribute or have content", "XTSE1205");
                }
            }
            use = typeCheck("use", use);
            match = typeCheck("match", match);
            if ((use != null)) {
                use = use.typeCheck(makeExpressionVisitor(), config.makeContextItemStaticInfo(match.getItemType(), false));
            }
            if ((collationName != null)) {
                System.Uri collationURI;
                try {
                    collationURI = new System.Uri(collationName, System.UriKind.RelativeOrAbsolute);
                    if (!(collationURI.IsAbsoluteUri)) {
                        System.Uri @base = new System.Uri(getBaseURI(), System.UriKind.RelativeOrAbsolute);
                        collationURI = new System.Uri(@base, collationURI);
                        collationName = collationURI.ToString();
                    }
                } catch (System.UriFormatException err) {
                    compileError((("Collation name '"+collationName)+"' is not a valid URI"));
                }
            } else {
                collationName = getDefaultCollationName();
            }
        }
        private static readonly System.Predicate<Saxon.Hej.expr.Expression> containsGlobalVariable = e => (((((e is Saxon.Hej.expr.GlobalVariableReference) || (e is Saxon.Hej.expr.UserFunctionCall)) || (e is Saxon.Hej.expr.instruct.CallTemplate)) || (e is Saxon.Hej.expr.instruct.ApplyTemplates)));
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            Saxon.Hej.om.StructuredQName keyName = getKeyName();
            if ((keyName != null)) {
                top.getKeyManager().preRegisterKeyDefinition(keyName);
            }
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.StaticContext env = getStaticContext();
            Saxon.Hej.Configuration config = env.getConfiguration();
            Saxon.Hej.lib.StringCollator collator = null;
            if ((collationName != null)) {
                collator = findCollation(collationName, getBaseURI());
                if ((collator == null)) {
                    compileError((("The collation name "+Saxon.Hej.trans.Err.wrap(collationName, Saxon.Hej.trans.Err.URI))+" is not recognized"), "XTSE1210");
                    collator = Saxon.Hej.expr.sort.CodepointCollator.getInstance();
                }
                if ((collator is Saxon.Hej.expr.sort.CodepointCollator)) {
                    collator = null;
                    collationName = null;
                } else {
                    if (!(Saxon.Hej.Version.platform.canReturnCollationKeys(collator))) {
                        compileError("The collation used for xsl:key must be capable of generating collation keys", "XTSE1210");
                    }
                }
            }
            if ((use == null)) {
                Saxon.Hej.expr.Expression body = compileSequenceConstructor(compilation, decl, true);
                try {
                    use = Saxon.Hej.expr.Atomizer.makeAtomizer(body, null);
                    use = use.simplify();
                } catch (Saxon.Hej.trans.XPathException e) {
                    compileError(e);
                }
                try {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:key/use", 0);
                    use = config.getTypeChecker(false).staticTypeCheck(use, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE, role, makeExpressionVisitor());
                    System.Diagnostics.Debug.Assert((match != null));
                    use = use.typeCheck(makeExpressionVisitor(), config.makeContextItemStaticInfo(match.getItemType(), false));
                } catch (Saxon.Hej.trans.XPathException err) {
                    compileError(err);
                }
            }
            Saxon.Hej.type.ItemType useItemType = use.getItemType();
            if ((useItemType == Saxon.Hej.type.ErrorType.getInstance())) {
                useItemType = Saxon.Hej.type.BuiltInAtomicType.STRING;
            }
            Saxon.Hej.type.BuiltInAtomicType useType = (Saxon.Hej.type.BuiltInAtomicType)(useItemType.getPrimitiveItemType());
            if (xPath10ModeIsEnabled()) {
                if ((!(useType.Equals(Saxon.Hej.type.BuiltInAtomicType.STRING)) && !(useType.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)))) {
                    use = new Saxon.Hej.expr.AtomicSequenceConverter(use, Saxon.Hej.type.BuiltInAtomicType.STRING);
                    ((Saxon.Hej.expr.AtomicSequenceConverter)(use)).allocateConverter(config, false);
                    useType = Saxon.Hej.type.BuiltInAtomicType.STRING;
                }
            }
            int nextFree = 0;
            if ((((match.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CURRENT_ITEM)) != 0)) {
                nextFree = 1;
            }
            match.allocateSlots(stackFrameMap, nextFree);
            bool sensitive = (Saxon.Hej.expr.parser.ExpressionTool.contains(use, false, containsGlobalVariable) || Saxon.Hej.expr.parser.ExpressionTool.contains(match, false, containsGlobalVariable));
            Saxon.Hej.trans.KeyManager km = getCompilation().getPrincipalStylesheetModule().getKeyManager();
            Saxon.Hej.trans.SymbolicName symbolicName = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_KEY, keyName);
            Saxon.Hej.trans.KeyDefinition keydef = new Saxon.Hej.trans.KeyDefinition(symbolicName, match, use, collationName, collator);
            keydef.setPackageData(getCompilation().getPackageData());
            keydef.setRangeKey(rangeKey);
            keydef.setIndexedItemType(useType);
            keydef.setStackFrameMap(stackFrameMap);
            keydef.setLocation(this);
            keydef.setBackwardsCompatible(xPath10ModeIsEnabled());
            keydef.setComposite(composite);
            keydef.obtainDeclaringComponent(this);
            try {
                km.addKeyDefinition(keyName, keydef, !(sensitive), compilation.getConfiguration());
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err);
            }
            keyDefinition = keydef;
        }
        public void optimize(Saxon.Hej.style.ComponentDeclaration declaration) {
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = makeExpressionVisitor();
            Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType = getConfiguration().makeContextItemStaticInfo(match.getItemType(), false);
            Saxon.Hej.expr.Expression useExp = keyDefinition.getUse();
            useExp = useExp.optimize(visitor, contextItemType);
            allocateLocalSlots(useExp);
            keyDefinition.setBody(useExp);
        }
        public void generateByteCode(Saxon.Hej.expr.parser.Optimizer opt) {
        }
    
    }
}