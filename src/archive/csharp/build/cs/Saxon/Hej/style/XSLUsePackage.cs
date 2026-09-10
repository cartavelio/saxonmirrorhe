using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLUsePackage : Saxon.Hej.style.StyleElement {
        private string nameAtt = null;
        private Saxon.Hej.style.PackageVersionRanges versionRanges = null;
        private Saxon.Hej.style.StylesheetPackage usedPackage;
        private System.Collections.Generic.IList<Saxon.Hej.style.XSLAccept> acceptors = null;
        internal void findUsedPackage(Saxon.Hej.trans.CompilerInfo info) {
            if ((usedPackage == null)) {
                if ((nameAtt == null)) {
                    nameAtt = Saxon.Hej.value.Whitespace.trim(getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "name"));
                }
                if ((nameAtt == null)) {
                    reportAbsence("name");
                    nameAtt = "unnamed-package";
                }
                Saxon.Hej.style.PackageVersionRanges ranges = getPackageVersionRanges();
                Saxon.Hej.trans.packages.PackageDetails pack = ((ranges == null) ? null : info.getPackageLibrary().findPackage(nameAtt, ranges));
                usedPackage = ((pack == null) ? null : pack.loadedPackage);
                if ((usedPackage == null)) {
                    compileErrorInAttribute((("Package "+nameAtt)+" could not be found"), "XTSE3000", "name");
                    usedPackage = getConfiguration().makeStylesheetPackage();
                    usedPackage.setJustInTimeCompilation(info.isJustInTimeCompilation());
                }
                Saxon.Hej.expr.instruct.GlobalContextRequirement gcr = usedPackage.getContextItemRequirements();
                if (((gcr != null) && !(gcr.isMayBeOmitted()))) {
                    compileError((("Package "+getAttributeValue("name"))+" requires a global context item, so it cannot be used as a library package"), "XTTE0590");
                }
            }
        }
        public override Saxon.Hej.style.StylesheetPackage getUsedPackage() {
            return usedPackage;
        }
        private Saxon.Hej.style.PackageVersionRanges getPackageVersionRanges() {
            if ((versionRanges == null)) {
                prepareAttributes();
            }
            return versionRanges;
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string ranges = "*";
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("name")) {
                    nameAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                } else {
                    if (f.Equals("package-version")) {
                        ranges = Saxon.Hej.value.Whitespace.trim(att.getValue()).Replace("\\", "");
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            try {
                versionRanges = new Saxon.Hej.style.PackageVersionRanges(ranges);
            } catch (Saxon.Hej.trans.XPathException e) {
                compileError(e);
            }
        }
        public override bool isDeclaration() {
            return true;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                    compileError("Character content is not allowed as a child of xsl:use-package");
                } else {
                    if (((child is Saxon.Hej.style.XSLAccept) || (child is Saxon.Hej.style.XSLOverride))) {
                    } else {
                        compileError((("Child element "+Saxon.Hej.trans.Err.wrap(child.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" is not allowed as a child of xsl:use-package"), "XTSE0010");
                    }
                }
            }
        }
        private System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> getExplicitAcceptedComponentNames() {
            System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> explicitAccepts = new System.Collections.Generic.HashSet<Saxon.Hej.trans.SymbolicName>();
            foreach (Saxon.Hej.om.NodeInfo child in children(x => (x is Saxon.Hej.style.XSLAccept))) {
                System.Collections.Generic.ISet<Saxon.Hej.trans.ComponentTest> explicitComponentTests = ((Saxon.Hej.style.XSLAccept)(child)).getExplicitComponentTests();
                foreach (Saxon.Hej.trans.ComponentTest test in explicitComponentTests) {
                    Saxon.Hej.trans.SymbolicName name = test.getSymbolicNameIfExplicit();
                    explicitAccepts.Add(name);
                }
            }
            return explicitAccepts;
        }
        public override void postValidate() {
            foreach (Saxon.Hej.om.NodeInfo curr in children()) {
                if (((curr is Saxon.Hej.style.XSLOverride) || (curr is Saxon.Hej.style.XSLAccept))) {
                    ((Saxon.Hej.style.StyleElement)(curr)).postValidate();
                }
            }
            System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> accepts = getExplicitAcceptedComponentNames();
            System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> overrides = getNamedOverrides();
            if (!(accepts.Count==0)) {
                foreach (Saxon.Hej.trans.SymbolicName o in overrides) {
                    if (accepts.Contains(o)) {
                        compileError((("Cannot accept and override the same component ("+o)+")"), "XTSE3051");
                    }
                    if ((o.getComponentKind() == Saxon.Hej.om.StandardNames.XSL_FUNCTION)) {
                        Saxon.Hej.trans.SymbolicName n = new Saxon.Hej.trans.SymbolicName.F(o.getComponentName(), -1);
                        if (accepts.Contains(n)) {
                            compileError((("Cannot accept and override the same function ("+o)+")"), "XTSE3051");
                        }
                    }
                }
            }
        }
        internal System.Collections.Generic.IList<Saxon.Hej.style.XSLAccept> getAcceptors() {
            if ((this.acceptors == null)) {
                acceptors = new System.Collections.Generic.List<Saxon.Hej.style.XSLAccept>();
                foreach (Saxon.Hej.om.NodeInfo decl in children(x => (x is Saxon.Hej.style.XSLAccept))) {
                    acceptors.Add((Saxon.Hej.style.XSLAccept)(decl));
                }
            }
            return acceptors;
        }
        internal void gatherNamedOverrides(Saxon.Hej.style.PrincipalStylesheetModule module, System.Collections.Generic.IList<Saxon.Hej.style.ComponentDeclaration> topLevel, System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> overrides) {
            if ((usedPackage == null)) {
                return ;
            }
            foreach (Saxon.Hej.om.NodeInfo @override in children(x => (x is Saxon.Hej.style.XSLOverride))) {
                foreach (Saxon.Hej.om.NodeInfo overridingDeclaration in @override.children(Saxon.Hej.pattern.NodeSelector.of(x => (x is Saxon.Hej.style.StylesheetComponent)))) {
                    Saxon.Hej.style.ComponentDeclaration decl = new Saxon.Hej.style.ComponentDeclaration(module, (Saxon.Hej.style.StyleElement)(overridingDeclaration));
                    topLevel.Add(decl);
                    Saxon.Hej.trans.SymbolicName name = ((Saxon.Hej.style.StylesheetComponent)(overridingDeclaration)).getSymbolicName();
                    if ((name != null)) {
                        overrides.Add(name);
                    } else {
                        if (((overridingDeclaration is Saxon.Hej.style.XSLTemplate) && (overridingDeclaration.getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "match") != null))) {
                            Saxon.Hej.om.StructuredQName[] modeNames = ((Saxon.Hej.style.XSLTemplate)(overridingDeclaration)).getModeNames();
                            foreach (Saxon.Hej.om.StructuredQName m in modeNames) {
                                overrides.Add(new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_MODE, m));
                            }
                        }
                    }
                }
            }
        }
        private System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> getNamedOverrides() {
            System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> overrides = new System.Collections.Generic.HashSet<Saxon.Hej.trans.SymbolicName>();
            Saxon.Hej.tree.iter.AxisIterator kids = iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            Saxon.Hej.om.NodeInfo @override;
            while (((@override = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next())) != null)) {
                if ((@override is Saxon.Hej.style.XSLOverride)) {
                    Saxon.Hej.tree.iter.AxisIterator overridings = @override.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                    Saxon.Hej.om.NodeInfo overridingDeclaration;
                    while (((overridingDeclaration = ((/*1430*/Saxon.Hej.om.NodeInfo)overridings.next())) != null)) {
                        if ((overridingDeclaration is Saxon.Hej.style.StylesheetComponent)) {
                            Saxon.Hej.trans.SymbolicName name = ((Saxon.Hej.style.StylesheetComponent)(overridingDeclaration)).getSymbolicName();
                            if ((name != null)) {
                                overrides.Add(name);
                            }
                        }
                    }
                }
            }
            return overrides;
        }
        internal void gatherRuleOverrides(Saxon.Hej.style.PrincipalStylesheetModule module, System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> overrides) {
            Saxon.Hej.style.StylesheetPackage thisPackage = module.getStylesheetPackage();
            Saxon.Hej.trans.rules.RuleManager ruleManager = module.getRuleManager();
            Saxon.Hej.tree.iter.AxisIterator kids = iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> overriddenModes = new System.Collections.Generic.HashSet<Saxon.Hej.trans.SymbolicName>();
            Saxon.Hej.om.NodeInfo @override;
            while (((@override = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next())) != null)) {
                if ((@override is Saxon.Hej.style.XSLOverride)) {
                    Saxon.Hej.tree.iter.AxisIterator overridings = @override.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                    Saxon.Hej.om.NodeInfo overridingDeclaration;
                    while (((overridingDeclaration = ((/*1430*/Saxon.Hej.om.NodeInfo)overridings.next())) != null)) {
                        if (((overridingDeclaration is Saxon.Hej.style.XSLTemplate) && (overridingDeclaration.getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "match") != null))) {
                            Saxon.Hej.om.StructuredQName[] modeNames = ((Saxon.Hej.style.XSLTemplate)(overridingDeclaration)).getModeNames();
                            foreach (Saxon.Hej.om.StructuredQName modeName in modeNames) {
                                if (modeName.Equals(Saxon.Hej.trans.Mode.OMNI_MODE)) {
                                    ((Saxon.Hej.style.StyleElement)(overridingDeclaration)).compileError("The mode name #all must not appear in an overriding template rule", "XTSE3440");
                                    continue ;
                                }
                                Saxon.Hej.trans.SymbolicName symbolicName = new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_MODE, modeName);
                                overrides.Add(symbolicName);
                                overriddenModes.Add(symbolicName);
                                Saxon.Hej.expr.Component.M derivedComponent = (Saxon.Hej.expr.Component.M)(thisPackage.getComponent(symbolicName));
                                if ((derivedComponent == null)) {
                                    ((Saxon.Hej.style.StyleElement)(overridingDeclaration)).compileError((("Mode "+modeName.getDisplayName())+" is not defined in the used package"), "XTSE3060");
                                    continue ;
                                }
                                if ((derivedComponent.getBaseComponent() == null)) {
                                    ((Saxon.Hej.style.StyleElement)(overridingDeclaration)).compileError((("Mode "+modeName.getDisplayName())+" cannot be overridden because it is local to this package"), "XTSE3440");
                                    continue ;
                                }
                                Saxon.Hej.expr.Component.M usedComponent = (Saxon.Hej.expr.Component.M)(derivedComponent.getBaseComponent());
                                if ((usedComponent.getVisibility() == Saxon.Hej.trans.Visibility.FINAL)) {
                                    ((Saxon.Hej.style.StyleElement)(overridingDeclaration)).compileError((("Cannot define overriding template rules in mode "+modeName.getDisplayName())+" because it has visibility=final"), "XTSE3060");
                                    continue ;
                                }
                                Saxon.Hej.trans.Mode usedMode = usedComponent.getActor();
                                if ((usedComponent.getVisibility() != Saxon.Hej.trans.Visibility.PUBLIC)) {
                                    ((Saxon.Hej.style.StyleElement)(overridingDeclaration)).compileError((("Cannot override template rules in mode "+modeName.getDisplayName())+", because the mode is not public"), "XTSE3060");
                                    continue ;
                                }
                                if ((derivedComponent.getActor() == usedMode)) {
                                    Saxon.Hej.trans.SimpleMode overridingMode = new Saxon.Hej.trans.SimpleMode(modeName);
                                    Saxon.Hej.trans.CompoundMode newCompoundMode = new Saxon.Hej.trans.CompoundMode(usedMode, overridingMode);
                                    newCompoundMode.setDeclaringComponent(derivedComponent);
                                    ruleManager.registerMode(newCompoundMode);
                                    derivedComponent.setActor(newCompoundMode);
                                }
                            }
                        }
                    }
                }
            }
            Saxon.Hej.trans.rules.RuleManager usedPackageRuleManager = usedPackage.getRuleManager();
            if ((usedPackageRuleManager != null)) {
                foreach (Saxon.Hej.trans.Mode m in usedPackageRuleManager.getAllNamedModes()) {
                    Saxon.Hej.trans.SymbolicName sn = m.getSymbolicName();
                    if (!(overriddenModes.Contains(sn))) {
                        Saxon.Hej.expr.Component c = thisPackage.getComponent(sn);
                        if (((c != null) && (c.getVisibility() != Saxon.Hej.trans.Visibility.PRIVATE))) {
                            ruleManager.registerMode((Saxon.Hej.trans.Mode)(c.getActor()));
                        }
                    }
                }
            }
        }
    
    }
}