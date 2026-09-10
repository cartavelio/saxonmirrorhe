using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLApplyTemplates : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        private Saxon.Hej.expr.Expression separator;
        private Saxon.Hej.om.StructuredQName modeName;
        private bool useCurrentMode = false;
        private bool useTailRecursion = false;
        private bool defaultedSelectExpression = true;
        private Saxon.Hej.trans.Mode mode;
        private string modeAttribute;
        public override bool isInstruction() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string selectAtt;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "mode":
                    modeAttribute = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "select":
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    defaultedSelectExpression = false;
                    break ;
                  case "separator":
                    requireXslt40("separator");
                    separator = makeAttributeValueTemplate(value, att);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((modeAttribute != null)) {
                switch (modeAttribute) {
                  case "#current":
                    useCurrentMode = true;
                    break ;
                  case "#unnamed":
                    modeName = Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME;
                    break ;
                  case "#default":
                    break ;
                  default:
                    modeName = makeQName(modeAttribute, null, "mode");
                    break ;
                }
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (useCurrentMode) {
                if ((((/*1430*/Saxon.Hej.om.NodeInfo)iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, Saxon.Hej.om.StandardNames.XSL_TEMPLATE, getNamePool())).next()) == null)) {
                    issueWarning("Specifying mode=\"#current\" when not inside an xsl:template serves no useful purpose", Saxon.Hej.trans.SaxonErrorCode.SXWN9023);
                }
            } else {
                Saxon.Hej.style.PrincipalStylesheetModule psm = getPrincipalStylesheetModule();
                if ((modeName == null)) {
                    modeName = getDefaultMode();
                    if ((((((modeName == null) || modeName.Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME))) && psm.isDeclaredModes()) && !(psm.getRuleManager().isUnnamedModeExplicit()))) {
                        compileError("The unnamed mode must be explicitly declared in an xsl:mode declaration", "XTSE3085");
                    }
                } else {
                    if (((modeName.Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME) && psm.isDeclaredModes()) && !(psm.getRuleManager().isUnnamedModeExplicit()))) {
                        compileError("The #unnamed mode must be explicitly declared in an xsl:mode declaration", "XTSE3085");
                    }
                }
                Saxon.Hej.trans.SymbolicName sName = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_MODE, modeName);
                Saxon.Hej.style.StylesheetPackage containingPackage = decl.getSourceElement().getContainingPackage();
                System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex = containingPackage.getComponentIndex();
                Saxon.Hej.expr.Component existing = Saxon.Impl.Helpers.MapUtils.Get(componentIndex, sName);
                if ((existing != null)) {
                    mode = (Saxon.Hej.trans.Mode)(existing.getActor());
                }
                if ((mode == null)) {
                    if (psm.isDeclaredModes()) {
                        compileError((("Mode name "+modeName.getDisplayName())+" must be explicitly declared in an xsl:mode declaration"), "XTSE3085");
                    }
                    mode = psm.getRuleManager().obtainMode(modeName, true);
                }
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                    if (!(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                        compileError("No character data is allowed within xsl:apply-templates", "XTSE0010");
                    }
                } else {
                    if (!((((child is Saxon.Hej.style.XSLSort) || (child is Saxon.Hej.style.XSLWithParam))))) {
                        compileError((("Invalid element "+Saxon.Hej.trans.Err.wrap(child.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" within xsl:apply-templates"), "XTSE0010");
                    }
                }
            }
            if ((select == null)) {
                Saxon.Hej.expr.Expression here = new Saxon.Hej.expr.ContextItemExpression();
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.CONTEXT_ITEM, "", 0, "XTTE0510");
                here = new Saxon.Hej.expr.ItemChecker(here, Saxon.Hej.pattern.AnyNodeTest.getInstance(), role);
                select = new Saxon.Hej.expr.SimpleStepExpression(here, new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, null));
                select.setLocation(allocateLocation());
                select.setRetainedStaticContext(makeRetainedStaticContext());
            }
            select = typeCheck("select", select);
            if ((separator != null)) {
                separator = typeCheck("separator", separator);
            }
        }
        protected internal override bool markTailCalls() {
            useTailRecursion = true;
            return true;
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys = makeSortKeys(compilation, decl);
            if ((sortKeys != null)) {
                useTailRecursion = false;
            }
            System.Diagnostics.Debug.Assert((select != null));
            Saxon.Hej.expr.Expression sortedSequence = select;
            if ((sortKeys != null)) {
                sortedSequence = new Saxon.Hej.expr.sort.SortExpression(select, sortKeys);
            }
            compileSequenceConstructor(compilation, decl, true);
            Saxon.Hej.trans.rules.RuleManager rm = compilation.getPrincipalStylesheetModule().getRuleManager();
            Saxon.Hej.expr.instruct.ApplyTemplates app = new Saxon.Hej.expr.instruct.ApplyTemplates(sortedSequence, useCurrentMode, useTailRecursion, defaultedSelectExpression, isWithinDeclaredStreamableConstruct(), mode, rm);
            app.setActualParams(getWithParamInstructions(app, compilation, decl, false));
            app.setTunnelParams(getWithParamInstructions(app, compilation, decl, true));
            if ((separator != null)) {
                app.setSeparatorExpression(separator);
            }
            return app;
        }
    
    }
}