using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLMode : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.trans.SimpleMode mode;
        private System.Collections.Generic.ISet<Saxon.Hej.expr.accum.Accumulator> accumulators;
        private bool prepared = false;
        private bool streamable = false;
        private bool failOnMultipleMatch = false;
        private bool warningOnNoMatch = false;
        private bool warningOnMultipleMatch = true;
        private bool traceMatching = false;
        private Saxon.Hej.trans.rules.BuiltInRuleSet defaultRules = Saxon.Hej.trans.rules.TextOnlyCopyRuleSet.getInstance();
        public override bool isDeclaration() {
            return true;
        }
        public override bool isInstruction() {
            return false;
        }
        public override Saxon.Hej.om.StructuredQName getObjectName() {
            Saxon.Hej.om.StructuredQName qn = base.getObjectName();
            if ((qn == null)) {
                string nameAtt = Saxon.Hej.value.Whitespace.trim(getAttributeValue("", "name"));
                if ((nameAtt == null)) {
                    return Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME;
                }
                qn = makeQName(nameAtt, null, "name");
                setObjectName(qn);
            }
            return qn;
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            Saxon.Hej.om.StructuredQName name = getObjectName();
            Saxon.Hej.trans.SymbolicName sName = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_MODE, name);
            System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex = top.getStylesheetPackage().getComponentIndex();
            if (!(name.Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME))) {
                Saxon.Hej.expr.Component other = Saxon.Impl.Helpers.MapUtils.Get(componentIndex, sName);
                if (((other != null) && (other.getDeclaringPackage() != top.getStylesheetPackage()))) {
                    compileError(((("Mode "+name.getDisplayName())+" conflicts with a public named mode in package ")+other.getDeclaringPackage().getPackageName()), "XTSE3050");
                }
            }
            mode = (Saxon.Hej.trans.SimpleMode)(top.getRuleManager().obtainMode(name, true));
            if (name.Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME)) {
                top.getRuleManager().setUnnamedModeExplicit(true);
            } else {
                if ((mode.getDeclaringComponent().getDeclaringPackage() != getContainingPackage())) {
                    compileError("Mode name conflicts with a mode in a used package", "XTSE3050");
                } else {
                    top.indexMode(decl);
                    Saxon.Hej.trans.Visibility declaredVisibility = getDeclaredVisibility();
                    Saxon.Hej.trans.Visibility actualVisibility = ((declaredVisibility == Saxon.Hej.trans.Visibility.UNDEFINED) ? Saxon.Hej.trans.Visibility.PRIVATE : declaredVisibility);
                    Saxon.Hej.trans.VisibilityProvenance provenance = ((declaredVisibility == Saxon.Hej.trans.Visibility.UNDEFINED) ? Saxon.Hej.trans.VisibilityProvenance.DEFAULTED : Saxon.Hej.trans.VisibilityProvenance.EXPLICIT);
                    mode.getDeclaringComponent().setVisibility(actualVisibility, provenance);
                    top.indexMode(decl);
                }
            }
        }
        protected internal override void prepareAttributes() {
            string nameAtt = null;
            string visibilityAtt = null;
            string extraAsAtt = null;
            if (prepared) {
                return ;
            }
            prepared = true;
            Saxon.Hej.trans.Visibility visibility = Saxon.Hej.trans.Visibility.PRIVATE;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "streamable":
                    streamable = processStreamableAtt(value);
                    break ;
                  case "name":
                    nameAtt = Saxon.Hej.value.Whitespace.trim(value);
                    if (!(nameAtt.Equals("#default"))) {
                        setObjectName(makeQName(nameAtt, null, "name"));
                    }
                    break ;
                  case "use-accumulators":
                    accumulators = getPrincipalStylesheetModule().getStylesheetPackage().getAccumulatorRegistry().getUsedAccumulators(value, this);
                    break ;
                  case "on-multiple-match":
                    {
                        switch (Saxon.Hej.value.Whitespace.trim(value)) {
                          case "fail":
                            failOnMultipleMatch = true;
                            break ;
                          case "use-last":
                            failOnMultipleMatch = false;
                            break ;
                          default:
                            invalidAttribute(f, "fail|use-last");
                            break ;
                        }
                        break ;
                    }
                  case "on-no-match":
                    switch (Saxon.Hej.value.Whitespace.trim(value)) {
                      case "text-only-copy":
                        break ;
                      case "shallow-copy":
                        defaultRules = Saxon.Hej.trans.rules.ShallowCopyRuleSet.getInstance();
                        break ;
                      case "deep-copy":
                        defaultRules = Saxon.Hej.trans.rules.DeepCopyRuleSet.getInstance();
                        break ;
                      case "shallow-skip":
                        defaultRules = Saxon.Hej.trans.rules.ShallowSkipRuleSet.getInstance();
                        break ;
                      case "deep-skip":
                        defaultRules = Saxon.Hej.trans.rules.DeepSkipRuleSet.getInstance();
                        break ;
                      case "fail":
                        defaultRules = Saxon.Hej.trans.rules.FailRuleSet.getInstance();
                        break ;
                      default:
                        invalidAttribute(f, "text-only-copy|shallow-copy|deep-copy|shallow-skip|deep-skip|fail");
                        break ;
                    }
                    break ;
                  case "warning-on-multiple-match":
                    {
                        warningOnMultipleMatch = processBooleanAttribute("warning-on-multiple-match", value);
                        break ;
                    }
                  case "warning-on-no-match":
                    {
                        warningOnNoMatch = processBooleanAttribute("warning-on-no-match", value);
                        break ;
                    }
                  case "typed":
                    {
                        checkAttributeValue("typed", Saxon.Hej.value.Whitespace.trim(value), false, new string[]{"0", "1", "false", "lax", "no", "strict", "true", "unspecified", "yes"});
                        break ;
                    }
                  case "visibility":
                    visibilityAtt = Saxon.Hej.value.Whitespace.trim(value);
                    visibility = interpretVisibilityValue(visibilityAtt, "");
                    if ((visibility == Saxon.Hej.trans.Visibility.ABSTRACT)) {
                        invalidAttribute(f, "public|private|final");
                    }
                    mode.setDeclaredVisibility(visibility);
                    break ;
                  default:
                    if (attName.hasURI(Saxon.Hej.lib.NamespaceConstant.SAXON)) {
                        isExtensionAttributeAllowed(attName.getDisplayName());
                        if (attName.getLocalPart().Equals("trace")) {
                            traceMatching = processBooleanAttribute("saxon:trace", value);
                        } else {
                            if (attName.getLocalPart().Equals("as")) {
                                extraAsAtt = value;
                            }
                        }
                    } else {
                        checkUnknownAttribute(attName);
                    }
                    break ;
                }
            }
            if ((((nameAtt == null) && (visibilityAtt != null)) && (mode.getDeclaredVisibility() != Saxon.Hej.trans.Visibility.PRIVATE))) {
                compileError("The unnamed mode must be private", "XTSE0020");
            }
            Saxon.Hej.trans.rules.RuleManager manager = getCompilation().getPrincipalStylesheetModule().getRuleManager();
            if ((getObjectName() == null)) {
                mode = manager.getUnnamedMode();
            } else {
                Saxon.Hej.trans.Mode m = manager.obtainMode(getObjectName(), true);
                if ((m is Saxon.Hej.trans.SimpleMode)) {
                    mode = (Saxon.Hej.trans.SimpleMode)(m);
                } else {
                    compileError("Mode name refers to an overridden mode");
                    mode = manager.getUnnamedMode();
                }
            }
            mode.setStreamable(streamable);
            if (streamable) {
                Saxon.Hej.trans.Mode omniMode = manager.obtainMode(Saxon.Hej.trans.Mode.OMNI_MODE, true);
                omniMode.setStreamable(true);
            }
            if (warningOnNoMatch) {
                defaultRules = new Saxon.Hej.trans.rules.RuleSetWithWarnings(defaultRules);
            }
            mode.setBuiltInRuleSet(defaultRules);
            Saxon.Hej.trans.RecoveryPolicy recoveryPolicy;
            if (failOnMultipleMatch) {
                recoveryPolicy = Saxon.Hej.trans.RecoveryPolicy.DO_NOT_RECOVER;
            } else {
                if (warningOnMultipleMatch) {
                    recoveryPolicy = Saxon.Hej.trans.RecoveryPolicy.RECOVER_WITH_WARNINGS;
                } else {
                    recoveryPolicy = Saxon.Hej.trans.RecoveryPolicy.RECOVER_SILENTLY;
                }
            }
            mode.setRecoveryPolicy(recoveryPolicy);
            mode.obtainDeclaringComponent(this);
            mode.setModeTracing(traceMatching);
            if ((extraAsAtt != null)) {
                Saxon.Hej.value.SequenceType extraResultType = null;
                try {
                    extraResultType = makeExtendedSequenceType(extraAsAtt);
                } catch (Saxon.Hej.trans.XPathException e) {
                    compileErrorInAttribute(e.Message, e.getErrorCodeLocalPart(), "saxon:as");
                    extraResultType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                }
                mode.setDefaultResultType(extraResultType);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkTopLevel("XTSE0010", false);
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string attValue = att.getValue();
                if ((((((f.Equals("streamable") || f.Equals("on-multiple-match")) || f.Equals("on-no-match")) || f.Equals("warning-on-multiple-match")) || f.Equals("warning-on-no-match")) || f.Equals("typed"))) {
                    string trimmed = Saxon.Hej.value.Whitespace.trim(attValue);
                    string normalizedAtt;
                    if (("true".Equals(trimmed) || "1".Equals(trimmed))) {
                        normalizedAtt = "yes";
                    } else {
                        if (("false".Equals(trimmed) || "0".Equals(trimmed))) {
                            normalizedAtt = "no";
                        } else {
                            normalizedAtt = trimmed;
                        }
                    }
                    mode.getActivePart().setExplicitProperty(f, normalizedAtt, decl.getPrecedence());
                    if ((mode.isMustBeTyped() && System.Text.RegularExpressions.Regex.IsMatch(getContainingPackage().getTargetEdition(), "JS\\d?"))) {
                        compileWarning("In Saxon-JS, all data is untyped", "XTTE3110");
                    }
                } else {
                    if ((f.Equals("use-accumulators") && (accumulators != null))) {
                        string[] names = new string[accumulators.Count];
                        int i = 0;
                        foreach (Saxon.Hej.expr.accum.Accumulator acc in accumulators) {
                            names[i++] = acc.getAccumulatorName().getEQName();
                        }
                        Saxon.Ejava.util.Arrays.sort(names);
                        string allNames = Saxon.Ejava.util.Arrays.ToString(names);
                        mode.getActivePart().setExplicitProperty(f, allNames, decl.getPrecedence());
                    }
                }
            }
            checkEmpty();
            checkTopLevel("XTSE0010", false);
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.style.StylesheetPackage pack = getPrincipalStylesheetModule().getStylesheetPackage();
            Saxon.Hej.expr.Component c = pack.getComponent(mode.getSymbolicName());
            if ((c == null)) {
                throw new Saxon.Ejava.lang.AssertionError();
            }
            if ((accumulators != null)) {
                mode.setAccumulators(accumulators);
            }
        }
    
    }
}