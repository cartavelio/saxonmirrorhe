using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class StylesheetPackage : Saxon.Hej.expr.PackageData {
        private const bool TRACING = false;
        private Saxon.Hej.style.PackageVersion packageVersion = null;
        private string packageName;
        private readonly System.Collections.Generic.IList<Saxon.Hej.style.StylesheetPackage> usedPackages = new System.Collections.Generic.List<Saxon.Hej.style.StylesheetPackage>();
        private Saxon.Hej.trans.rules.RuleManager ruleManager;
        private Saxon.Hej.serialize.CharacterMapIndex characterMapIndex;
        private bool createsSecondaryResultDocuments;
        private readonly System.Collections.Generic.IList<Saxon.Hej.om.Action> completionActions = new System.Collections.Generic.List<Saxon.Hej.om.Action>();
        protected internal Saxon.Hej.expr.instruct.GlobalContextRequirement globalContextRequirement = null;
        private bool containsGlobalContextItemDeclaration = false;
        protected internal Saxon.Hej.om.SpaceStrippingRule stripperRules;
        private bool stripsWhitespace = false;
        private bool stripsTypeAnnotations = false;
        protected internal Saxon.Ejava.util.Properties defaultOutputProperties;
        private Saxon.Hej.om.StructuredQName defaultMode;
        private bool declaredModes;
        protected internal System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Ejava.util.Properties> namedOutputProperties = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Ejava.util.Properties>(4);
        protected internal System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> schemaIndex = new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>(10);
        private Saxon.Hej.functions.FunctionLibraryList functionLibrary;
        private Saxon.Hej.query.XQueryFunctionLibrary queryFunctions;
        private Saxon.Hej.functions.ExecutableFunctionLibrary overriding;
        private Saxon.Hej.functions.ExecutableFunctionLibrary underriding;
        private int maxFunctionArity = -1;
        private bool retainUnusedFunctions = false;
        private bool implicitPackage;
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName, Saxon.Hej.expr.Component>(20);
        protected internal System.Collections.Generic.IList<Saxon.Hej.expr.Component> hiddenComponents = new System.Collections.Generic.List<Saxon.Hej.expr.Component>();
        protected internal System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> overriddenComponents = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName, Saxon.Hej.expr.Component>();
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> abstractComponents = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName, Saxon.Hej.expr.Component>();
        public StylesheetPackage(Saxon.Hej.Configuration config) : base (config) {
            setHostLanguage(Saxon.Api.HostLanguage.XSLT, 30);
            setAccumulatorRegistry(config.makeAccumulatorRegistry());
        }
        public System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> getComponentIndex() {
            return componentIndex;
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.style.StylesheetPackage> getUsedPackages() {
            return usedPackages;
        }
        public void addUsedPackage(Saxon.Hej.style.StylesheetPackage pack) {
            usedPackages.Add(pack);
        }
        public bool contains(Saxon.Hej.style.StylesheetPackage pack) {
            foreach (Saxon.Hej.style.StylesheetPackage p in usedPackages) {
                if (((p == pack) || p.contains(pack))) {
                    return true;
                }
            }
            return false;
        }
        public void setLanguageVersion(int version) {
            this.hostLanguageVersion = version;
        }
        public void setPackageName(string packageName) {
            this.packageName = packageName;
        }
        public Saxon.Hej.style.PackageVersion getPackageVersion() {
            return packageVersion;
        }
        public void setPackageVersion(Saxon.Hej.style.PackageVersion version) {
            packageVersion = version;
        }
        public string getPackageName() {
            return packageName;
        }
        public bool isImplicitPackage() {
            return implicitPackage;
        }
        public void setImplicitPackage(bool implicitPackage) {
            this.implicitPackage = implicitPackage;
        }
        public virtual bool isJustInTimeCompilation() {
            return false;
        }
        public virtual void setJustInTimeCompilation(bool justInTimeCompilation) {
        }
        public Saxon.Hej.trans.rules.RuleManager getRuleManager() {
            return ruleManager;
        }
        public void setRuleManager(Saxon.Hej.trans.rules.RuleManager ruleManager) {
            this.ruleManager = ruleManager;
        }
        public Saxon.Hej.om.StructuredQName getDefaultMode() {
            return defaultMode;
        }
        public void setDefaultMode(Saxon.Hej.om.StructuredQName defaultMode) {
            this.defaultMode = defaultMode;
        }
        public void setDeclaredModes(bool declared) {
            declaredModes = declared;
        }
        public bool isDeclaredModes() {
            return declaredModes;
        }
        public Saxon.Hej.om.SpaceStrippingRule getSpaceStrippingRule() {
            return stripperRules;
        }
        public Saxon.Hej.serialize.CharacterMapIndex getCharacterMapIndex() {
            return characterMapIndex;
        }
        public void setCharacterMapIndex(Saxon.Hej.serialize.CharacterMapIndex characterMapIndex) {
            this.characterMapIndex = characterMapIndex;
        }
        public bool isCreatesSecondaryResultDocuments() {
            return createsSecondaryResultDocuments;
        }
        public void setCreatesSecondaryResultDocuments(bool createsSecondaryResultDocuments) {
            this.createsSecondaryResultDocuments = createsSecondaryResultDocuments;
        }
        public bool isStripsTypeAnnotations() {
            return stripsTypeAnnotations;
        }
        public void setStripsTypeAnnotations(bool stripsTypeAnnotations) {
            this.stripsTypeAnnotations = stripsTypeAnnotations;
        }
        public Saxon.Hej.om.SpaceStrippingRule getStripperRules() {
            return stripperRules;
        }
        public void setStripperRules(Saxon.Hej.om.SpaceStrippingRule stripperRules) {
            this.stripperRules = stripperRules;
        }
        public void setDefaultOutputProperties(Saxon.Ejava.util.Properties props) {
            defaultOutputProperties = props;
        }
        public void setNamedOutputProperties(Saxon.Hej.om.StructuredQName name, Saxon.Ejava.util.Properties props) {
            Saxon.Impl.Helpers.MapUtils.Put(namedOutputProperties, name, props);
        }
        public Saxon.Ejava.util.Properties getNamedOutputProperties(Saxon.Hej.om.StructuredQName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(namedOutputProperties, name);
        }
        public System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> getSchemaNamespaces() {
            return schemaIndex;
        }
        public void setContextItemRequirements(Saxon.Hej.expr.instruct.GlobalContextRequirement requirement) {
            if (containsGlobalContextItemDeclaration) {
                if ((((!(requirement.isAbsentFocus()) && globalContextRequirement.isAbsentFocus())) || ((requirement.isMayBeOmitted() && !(globalContextRequirement.isMayBeOmitted()))))) {
                    throw new Saxon.Hej.trans.XPathException("The package contains two xsl:global-context-item declarations with conflicting @use attributes", "XTSE3087");
                }
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                if ((th.relationship(requirement.getRequiredItemType(), globalContextRequirement.getRequiredItemType()) != Saxon.Hej.type.Affinity.SAME_TYPE)) {
                    throw new Saxon.Hej.trans.XPathException("The package contains two xsl:global-context-item declarations with conflicting item types", "XTSE3087");
                }
            }
            containsGlobalContextItemDeclaration = true;
            globalContextRequirement = requirement;
        }
        public Saxon.Hej.expr.instruct.GlobalContextRequirement getContextItemRequirements() {
            return globalContextRequirement;
        }
        public void setStripsWhitespace(bool strips) {
            this.stripsWhitespace = strips;
        }
        public bool isStripsWhitespace() {
            return this.stripsWhitespace;
        }
        public void addCompletionAction(Saxon.Hej.om.Action action) {
            completionActions.Add(action);
        }
        protected internal void complete() {
            foreach (Saxon.Hej.om.Action a in completionActions) {
                /*Delegate.doAction()*/a();
            }
            allocateBinderySlots();
        }
        public void allocateBinderySlots() {
            Saxon.Hej.expr.instruct.SlotManager slotManager = getConfiguration().makeSlotManager();
            foreach (Saxon.Hej.expr.Component c in componentIndex.Values) {
                registerGlobalVariable(c, slotManager);
            }
            foreach (Saxon.Hej.expr.Component c in hiddenComponents) {
                registerGlobalVariable(c, slotManager);
            }
            setGlobalSlotManager(slotManager);
        }
        private void registerGlobalVariable(Saxon.Hej.expr.Component c, Saxon.Hej.expr.instruct.SlotManager slotManager) {
            if ((c.getActor() is Saxon.Hej.expr.instruct.GlobalVariable)) {
                Saxon.Hej.expr.instruct.GlobalVariable var = (Saxon.Hej.expr.instruct.GlobalVariable)(c.getActor());
                int slot = slotManager.allocateSlotNumber(var.getVariableQName());
                var.setPackageData(this);
                var.setBinderySlotNumber(slot);
            }
        }
        public void addComponent(Saxon.Hej.expr.Component component) {
            Saxon.Hej.trans.SymbolicName name = component.getActor().getSymbolicName();
            Saxon.Impl.Helpers.MapUtils.Put(componentIndex, name, component);
            if (((component.getVisibility() == Saxon.Hej.trans.Visibility.ABSTRACT) && (component.getContainingPackage() == this))) {
                Saxon.Impl.Helpers.MapUtils.Put(abstractComponents, component.getActor().getSymbolicName(), component);
            }
        }
        public override void addGlobalVariable(Saxon.Hej.expr.instruct.GlobalVariable variable) {
            base.addGlobalVariable(variable);
            Saxon.Hej.trans.SymbolicName name = variable.getSymbolicName();
            if ((Saxon.Impl.Helpers.MapUtils.Get(componentIndex, name) == null)) {
                Saxon.Hej.expr.Component comp = variable.getDeclaringComponent();
                if ((comp == null)) {
                    comp = variable.makeDeclaringComponent(/*explicit enum*/ Saxon.Hej.trans.Visibility.PRIVATE, this);
                }
                addComponent(comp);
            }
        }
        public int getMaxFunctionArity() {
            if ((maxFunctionArity == -1)) {
                foreach (Saxon.Hej.expr.Component c in componentIndex.Values) {
                    if ((c.getActor() is Saxon.Hej.expr.instruct.UserFunction)) {
                        if ((((Saxon.Hej.expr.instruct.UserFunction)(c.getActor())).getArity()>maxFunctionArity)) {
                            maxFunctionArity = ((Saxon.Hej.expr.instruct.UserFunction)(c.getActor())).getArity();
                        }
                    }
                }
            }
            return maxFunctionArity;
        }
        public Saxon.Hej.expr.Component getComponent(Saxon.Hej.trans.SymbolicName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(componentIndex, name);
        }
        public void addHiddenComponent(Saxon.Hej.expr.Component component) {
            hiddenComponents.Add(component);
        }
        public Saxon.Hej.expr.Component getOverriddenComponent(Saxon.Hej.trans.SymbolicName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(overriddenComponents, name);
        }
        public void addOverriddenComponent(Saxon.Hej.expr.Component comp) {
            Saxon.Impl.Helpers.MapUtils.Put(overriddenComponents, comp.getActor().getSymbolicName(), comp);
        }
        public void addComponentsFromUsedPackage(Saxon.Hej.style.StylesheetPackage usedPackage, System.Collections.Generic.IList<Saxon.Hej.style.XSLAccept> acceptors, System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> overrides) {
            usedPackages.Add(usedPackage);
            trace((((("=== Adding components from "+usedPackage.getPackageName())+" to ")+getPackageName())+" ==="));
            System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,Saxon.Hej.expr.Component> correspondence = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.Component, Saxon.Hej.expr.Component>();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> namedComponentEntry in usedPackage.componentIndex) {
                Saxon.Hej.trans.SymbolicName name = namedComponentEntry.Key;
                Saxon.Hej.expr.Component oldC = namedComponentEntry.Value;
                Saxon.Hej.trans.Visibility oldV = oldC.getVisibility();
                Saxon.Hej.trans.Visibility newV = Saxon.Hej.trans.Visibility.UNDEFINED;
                if ((overrides.Contains(name) && !(((oldC.getActor() is Saxon.Hej.trans.Mode))))) {
                    newV = Saxon.Hej.trans.Visibility.HIDDEN;
                } else {
                    Saxon.Hej.trans.Visibility acceptedVisibility = explicitAcceptedVisibility(name, acceptors);
                    if ((acceptedVisibility != Saxon.Hej.trans.Visibility.UNDEFINED)) {
                        if (!(Saxon.Hej.style.XSLAccept.isCompatible(oldV, acceptedVisibility))) {
                            throw new Saxon.Hej.trans.XPathException(((((((("Cannot accept a "+Saxon.Hej.trans.Err.describeVisibility(oldV))+" component (")+name)+") from package ")+usedPackage.getPackageName())+" with visibility ")+Saxon.Hej.trans.Err.describeVisibility(acceptedVisibility)), "XTSE3040");
                        }
                        newV = acceptedVisibility;
                    } else {
                        acceptedVisibility = wildcardAcceptedVisibility(name, acceptors);
                        if ((acceptedVisibility != Saxon.Hej.trans.Visibility.UNDEFINED)) {
                            if (Saxon.Hej.style.XSLAccept.isCompatible(oldV, acceptedVisibility)) {
                                newV = acceptedVisibility;
                            }
                        }
                    }
                    if ((newV == Saxon.Hej.trans.Visibility.UNDEFINED)) {
                        if (((oldV == Saxon.Hej.trans.Visibility.PUBLIC) || (oldV == Saxon.Hej.trans.Visibility.FINAL))) {
                            newV = Saxon.Hej.trans.Visibility.PRIVATE;
                        } else {
                            newV = Saxon.Hej.trans.Visibility.HIDDEN;
                        }
                    }
                }
                trace(((((oldC.getActor().getSymbolicName()+" (")+Saxon.Hej.trans.Err.describeVisibility(oldV))+") becomes ")+Saxon.Hej.trans.Err.describeVisibility(newV)));
                Saxon.Hej.expr.Component newC = Saxon.Hej.expr.Component.makeComponent(oldC.getActor(), newV, Saxon.Hej.trans.VisibilityProvenance.DERIVED, this, oldC.getDeclaringPackage());
                Saxon.Impl.Helpers.MapUtils.Put(correspondence, oldC, newC);
                newC.setBaseComponent(oldC);
                if (overrides.Contains(name)) {
                    Saxon.Impl.Helpers.MapUtils.Put(overriddenComponents, name, newC);
                    if ((newV != Saxon.Hej.trans.Visibility.ABSTRACT)) {
                        abstractComponents.Remove(name);
                    }
                }
                if ((newC.getVisibility() == Saxon.Hej.trans.Visibility.HIDDEN)) {
                    hiddenComponents.Add(newC);
                } else {
                    if ((Saxon.Impl.Helpers.MapUtils.Get(componentIndex, name) != null)) {
                        if (!(((oldC.getActor() is Saxon.Hej.trans.Mode)))) {
                            throw new Saxon.Hej.trans.XPathException(("Duplicate "+namedComponentEntry.Key), "XTSE3050", oldC.getActor());
                        }
                    } else {
                        Saxon.Impl.Helpers.MapUtils.Put(componentIndex, name, newC);
                        if (((oldC.getActor() is Saxon.Hej.trans.Mode) && (((oldV == Saxon.Hej.trans.Visibility.PUBLIC) || (oldV == Saxon.Hej.trans.Visibility.FINAL))))) {
                            Saxon.Hej.trans.Mode existing = getRuleManager().obtainMode(name.getComponentName(), false);
                            if ((existing != null)) {
                                throw new Saxon.Hej.trans.XPathException(("Duplicate "+namedComponentEntry.Key), "XTSE3050", oldC.getActor());
                            } else {
                            }
                        }
                    }
                }
                if (((newC.getActor() is Saxon.Hej.trans.Mode) && overrides.Contains(name))) {
                    addCompletionAction(() => {
                        trace(("Doing mode completion for "+newC.getActor().getSymbolicName()));
                        System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> oldBindings = newC.getBaseComponent().getComponentBindings();
                        System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> newBindings = newC.getComponentBindings();
                        for (int i = 0; (i<oldBindings.Count); i++) {
                            Saxon.Hej.trans.SymbolicName name12 = oldBindings[i].getSymbolicName();
                            Saxon.Hej.expr.Component target;
                            if (overrides.Contains(name12)) {
                                target = getComponent(name12);
                                if ((target == null)) {
                                    throw new Saxon.Ejava.lang.AssertionError((("We know there's an override for "+name12)+", but we can't find it"));
                                }
                            } else {
                                target = Saxon.Impl.Helpers.MapUtils.Get(correspondence, oldBindings[i].getTarget());
                                if ((target == null)) {
                                    throw new Saxon.Ejava.lang.AssertionError(("Saxon can't find the new component corresponding to "+name12));
                                }
                            }
                            Saxon.Hej.expr.ComponentBinding newBinding = new Saxon.Hej.expr.ComponentBinding(name12, target);
                            newBindings[i] = newBinding;
                        }
                    } );
                } else {
                    addCompletionAction(() => {
                        trace(("Doing normal completion for "+newC.getActor().getSymbolicName()));
                        System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> oldBindings = newC.getBaseComponent().getComponentBindings();
                        System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> newBindings = new System.Collections.Generic.List<Saxon.Hej.expr.ComponentBinding>(oldBindings.Count);
                        makeNewComponentBindings(overrides, correspondence, oldBindings, newBindings);
                        newC.setComponentBindings(newBindings);
                    } );
                }
            }
            foreach (Saxon.Hej.expr.Component oldC in usedPackage.hiddenComponents) {
                trace((((oldC.getActor().getSymbolicName()+" (HIDDEN, declared in ")+oldC.getDeclaringPackage().getPackageName())+") becomes HIDDEN"));
                Saxon.Hej.expr.Component newC = Saxon.Hej.expr.Component.makeComponent(oldC.getActor(), Saxon.Hej.trans.Visibility.HIDDEN, Saxon.Hej.trans.VisibilityProvenance.DERIVED, this, oldC.getDeclaringPackage());
                Saxon.Impl.Helpers.MapUtils.Put(correspondence, oldC, newC);
                newC.setBaseComponent(oldC);
                hiddenComponents.Add(newC);
                addCompletionAction(() => {
                    System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> oldBindings = newC.getBaseComponent().getComponentBindings();
                    System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> newBindings = new System.Collections.Generic.List<Saxon.Hej.expr.ComponentBinding>(oldBindings.Count);
                    makeNewComponentBindings(overrides, correspondence, oldBindings, newBindings);
                    newC.setComponentBindings(newBindings);
                } );
            }
            if (usedPackage.isCreatesSecondaryResultDocuments()) {
                setCreatesSecondaryResultDocuments(true);
            }
        }
        private void makeNewComponentBindings(System.Collections.Generic.ISet<Saxon.Hej.trans.SymbolicName> overrides, System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,Saxon.Hej.expr.Component> correspondence, System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> oldBindings, System.Collections.Generic.IList<Saxon.Hej.expr.ComponentBinding> newBindings) {
            foreach (Saxon.Hej.expr.ComponentBinding oldBinding in oldBindings) {
                Saxon.Hej.trans.SymbolicName name = oldBinding.getSymbolicName();
                Saxon.Hej.expr.Component target;
                if (overrides.Contains(name)) {
                    target = getComponent(name);
                    if ((target == null)) {
                        throw new Saxon.Ejava.lang.AssertionError((("We know there's an override for "+name)+", but we can't find it"));
                    }
                } else {
                    target = Saxon.Impl.Helpers.MapUtils.Get(correspondence, oldBinding.getTarget());
                    if ((target == null)) {
                        throw new Saxon.Ejava.lang.AssertionError(("Saxon can't find the new component corresponding to "+name));
                    }
                }
                Saxon.Hej.expr.ComponentBinding newBinding = new Saxon.Hej.expr.ComponentBinding(name, target);
                newBindings.Add(newBinding);
            }
        }
        private void trace(string message) {
            
        }
        private Saxon.Hej.trans.Visibility explicitAcceptedVisibility(Saxon.Hej.trans.SymbolicName name, System.Collections.Generic.IList<Saxon.Hej.style.XSLAccept> acceptors) {
            foreach (Saxon.Hej.style.XSLAccept acceptor in acceptors) {
                foreach (Saxon.Hej.trans.ComponentTest test in acceptor.getExplicitComponentTests()) {
                    if (test.matches(name)) {
                        return acceptor.getVisibility();
                    }
                }
            }
            return Saxon.Hej.trans.Visibility.UNDEFINED;
        }
        private Saxon.Hej.trans.Visibility wildcardAcceptedVisibility(Saxon.Hej.trans.SymbolicName name, System.Collections.Generic.IList<Saxon.Hej.style.XSLAccept> acceptors) {
            Saxon.Hej.trans.Visibility vis = Saxon.Hej.trans.Visibility.UNDEFINED;
            foreach (Saxon.Hej.style.XSLAccept acceptor in acceptors) {
                foreach (Saxon.Hej.trans.ComponentTest test in acceptor.getWildcardComponentTests()) {
                    if (((((Saxon.Hej.pattern.NodeTest)(test.getQNameTest())).getDefaultPriority() == -(0.25)) && test.matches(name))) {
                        vis = acceptor.getVisibility();
                    }
                }
            }
            if ((vis != Saxon.Hej.trans.Visibility.UNDEFINED)) {
                return vis;
            }
            foreach (Saxon.Hej.style.XSLAccept acceptor in acceptors) {
                foreach (Saxon.Hej.trans.ComponentTest test in acceptor.getWildcardComponentTests()) {
                    if (test.matches(name)) {
                        vis = acceptor.getVisibility();
                    }
                }
            }
            return vis;
        }
        public void createFunctionLibrary() {
            Saxon.Hej.functions.FunctionLibraryList functionLibrary = new Saxon.Hej.functions.FunctionLibraryList();
            functionLibrary.addFunctionLibrary(config.getXSLTFunctionSet(hostLanguageVersion));
            functionLibrary.addFunctionLibrary(new Saxon.Hej.style.StylesheetFunctionLibrary(this, true));
            functionLibrary.addFunctionLibrary(config.getBuiltInExtensionLibraryList(hostLanguageVersion));
            functionLibrary.addFunctionLibrary(new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(config));
            if ("JS".Equals(getTargetEdition())) {
                addIxslFunctionLibrary(functionLibrary);
            }
            queryFunctions = new Saxon.Hej.query.XQueryFunctionLibrary(config);
            functionLibrary.addFunctionLibrary(queryFunctions);
            functionLibrary.addFunctionLibrary(config.getIntegratedFunctionLibrary());
            config.addExtensionBinders(functionLibrary);
            functionLibrary.addFunctionLibrary(Saxon.Hej.style.XSLOriginalLibrary.getInstance());
            functionLibrary.addFunctionLibrary(new Saxon.Hej.style.StylesheetFunctionLibrary(this, false));
            this.functionLibrary = functionLibrary;
        }
        protected internal virtual void addIxslFunctionLibrary(Saxon.Hej.functions.FunctionLibraryList functionLibrary) {
        }
        public Saxon.Hej.functions.FunctionLibraryList getFunctionLibrary() {
            return functionLibrary;
        }
        public Saxon.Hej.functions.FunctionLibrary getPublicFunctions() {
            return new Saxon.Hej.style.PublicStylesheetFunctionLibrary(functionLibrary);
        }
        public Saxon.Hej.query.XQueryFunctionLibrary getXQueryFunctionLibrary() {
            return queryFunctions;
        }
        public void setFunctionLibraryDetails(Saxon.Hej.functions.FunctionLibraryList library, Saxon.Hej.functions.ExecutableFunctionLibrary overriding, Saxon.Hej.functions.ExecutableFunctionLibrary underriding) {
            if ((library != null)) {
                this.functionLibrary = library;
            }
            this.overriding = overriding;
            this.underriding = underriding;
        }
        public Saxon.Hej.expr.instruct.UserFunction getFunction(Saxon.Hej.trans.SymbolicName.F name) {
            if ((name.getArity() == -1)) {
                int maximumArity = 20;
                for (int a = 0; (a<maximumArity); a++) {
                    Saxon.Hej.trans.SymbolicName.F sn = new Saxon.Hej.trans.SymbolicName.F(name.getComponentName(), a);
                    Saxon.Hej.expr.instruct.UserFunction uf = getFunction(sn);
                    if ((uf != null)) {
                        uf.incrementReferenceCount();
                        return uf;
                    }
                }
                return null;
            } else {
                Saxon.Hej.expr.Component component = Saxon.Impl.Helpers.MapUtils.Get(getComponentIndex(), name);
                if ((component != null)) {
                    Saxon.Hej.expr.instruct.UserFunction uf = (Saxon.Hej.expr.instruct.UserFunction)(component.getActor());
                    uf.incrementReferenceCount();
                    return uf;
                } else {
                    return null;
                }
            }
        }
        public bool isRetainUnusedFunctions() {
            return retainUnusedFunctions;
        }
        public void setRetainUnusedFunctions() {
            this.retainUnusedFunctions = true;
        }
        public void updatePreparedStylesheet(Saxon.Hej.PreparedStylesheet pss) {
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> entry in componentIndex) {
                if ((entry.Value.getVisibility() == Saxon.Hej.trans.Visibility.ABSTRACT)) {
                    Saxon.Impl.Helpers.MapUtils.Put(abstractComponents, entry.Key, entry.Value);
                }
            }
            pss.setTopLevelPackage(this);
            if ((isSchemaAware() || !(schemaIndex.Count==0))) {
                pss.setSchemaAware(true);
            }
            pss.setHostLanguage(Saxon.Api.HostLanguage.XSLT);
            Saxon.Hej.functions.FunctionLibraryList libraryList = new Saxon.Hej.functions.FunctionLibraryList();
            foreach (Saxon.Hej.functions.FunctionLibrary lib in functionLibrary.getLibraryList()) {
                if ((lib is Saxon.Hej.style.StylesheetFunctionLibrary)) {
                    if (((Saxon.Hej.style.StylesheetFunctionLibrary)(lib)).isOverrideExtensionFunction()) {
                        libraryList.addFunctionLibrary(overriding);
                    } else {
                        libraryList.addFunctionLibrary(underriding);
                    }
                } else {
                    libraryList.addFunctionLibrary(lib);
                }
            }
            pss.setFunctionLibrary(libraryList);
            if (!(pss.createsSecondaryResult())) {
                pss.setCreatesSecondaryResult(mayCreateSecondaryResultDocuments());
            }
            pss.setDefaultOutputProperties(defaultOutputProperties);
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Ejava.util.Properties> entry in namedOutputProperties) {
                pss.setOutputProperties(entry.Key, entry.Value);
            }
            if ((characterMapIndex != null)) {
                foreach (Saxon.Hej.serialize.CharacterMap cm in characterMapIndex) {
                    pss.getCharacterMapIndex().putCharacterMap(cm.getName(), cm);
                }
            }
            pss.setRuleManager(ruleManager);
            foreach (Saxon.Hej.expr.Component comp in componentIndex.Values) {
                if ((comp.getActor() is Saxon.Hej.expr.instruct.NamedTemplate)) {
                    Saxon.Hej.expr.instruct.NamedTemplate t = (Saxon.Hej.expr.instruct.NamedTemplate)(comp.getActor());
                    pss.putNamedTemplate(t.getTemplateName(), t);
                }
            }
            pss.setComponentIndex(componentIndex);
            foreach (Saxon.Hej.expr.Component comp in componentIndex.Values) {
                if ((comp.getActor() is Saxon.Hej.expr.instruct.GlobalParam)) {
                    Saxon.Hej.expr.instruct.GlobalParam gv = (Saxon.Hej.expr.instruct.GlobalParam)(comp.getActor());
                    pss.registerGlobalParameter(gv);
                }
            }
            if ((globalContextRequirement != null)) {
                pss.setGlobalContextRequirement(globalContextRequirement);
            }
        }
        private bool mayCreateSecondaryResultDocuments() {
            if (createsSecondaryResultDocuments) {
                return true;
            }
            foreach (Saxon.Hej.style.StylesheetPackage p in usedPackages) {
                if (p.mayCreateSecondaryResultDocuments()) {
                    return true;
                }
            }
            return false;
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> getAbstractComponents() {
            return abstractComponents;
        }
        public virtual void markNonExportable(string message, string errorCode) {
        }
        public virtual void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            throw new Saxon.Hej.trans.XPathException("Exporting a stylesheet requires Saxon-EE");
        }
        public void checkForAbstractComponents() {
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> entry in componentIndex) {
                if (((entry.Value.getVisibility() == Saxon.Hej.trans.Visibility.ABSTRACT) && (entry.Value.getContainingPackage() == this))) {
                    Saxon.Impl.Helpers.MapUtils.Put(abstractComponents, entry.Key, entry.Value);
                }
            }
            if (!(abstractComponents.Count==0)) {
                Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(256);
                int count = 0;
                foreach (Saxon.Hej.trans.SymbolicName name in Saxon.Impl.Helpers.MapUtils.KeySet(abstractComponents)) {
                    if ((count++>0)) {
                        buff.append(", ");
                    }
                    buff.append(name.ToString());
                    if ((buff.length()>300)) {
                        buff.append(" ...");
                        break ;
                    }
                }
                throw new Saxon.Hej.trans.XPathException(("The package is not executable, because it contains abstract components: "+buff), "XTSE3080");
            }
        }
        public virtual bool isFallbackToNonStreaming() {
            return true;
        }
        public virtual void setFallbackToNonStreaming() {
        }
    
    }
}