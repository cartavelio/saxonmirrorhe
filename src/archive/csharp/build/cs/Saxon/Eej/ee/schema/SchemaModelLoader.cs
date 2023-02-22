using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class SchemaModelLoader : Saxon.Hej.@event.ProxyReceiver {
        private readonly Saxon.Eej.config.EnterpriseConfiguration config;
        private readonly Saxon.Eej.ee.schema.PreparedSchema schema;
        private Saxon.Eej.ee.schema.SchemaCompiler compiler;
        private readonly System.Collections.Generic.Dictionary<string,Saxon.Hej.type.SchemaComponent> components = new System.Collections.Generic.Dictionary<string, Saxon.Hej.type.SchemaComponent>(100);
        private readonly System.Collections.Generic.HashSet<Saxon.Hej.type.SchemaComponent> anonComponents = new System.Collections.Generic.HashSet<Saxon.Hej.type.SchemaComponent>(100);
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.schema.SchemaModelLoader.Fixup> fixups = new System.Collections.Generic.List<Saxon.Eej.ee.schema.SchemaModelLoader.Fixup>();
        private readonly System.Collections.Generic.Stack<Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler> handlerStack = new System.Collections.Generic.Stack<Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler>();
        private Saxon.Hej.om.NamespaceResolver namespaceResolver;
        private Saxon.Hej.om.NodeName elementName;
        private readonly System.Collections.Generic.IList<Saxon.Eej.ee.schema.SchemaModelLoader.PendingEdge> pendingEdges = new System.Collections.Generic.List<Saxon.Eej.ee.schema.SchemaModelLoader.PendingEdge>();
        private string dmk = null;
        public SchemaModelLoader(Saxon.Hej.Configuration config) : base (new Saxon.Hej.@event.Sink(config.makePipelineConfiguration())) {
            getPipelineConfiguration().getParseOptions().setSchemaValidationMode(Saxon.Hej.lib.Validation.SKIP);
            this.config = (Saxon.Eej.config.EnterpriseConfiguration)(config);
            schema = new Saxon.Eej.ee.schema.PreparedSchema((Saxon.Eej.config.EnterpriseConfiguration)(config));
        }
        public Saxon.Eej.ee.schema.PreparedSchema load(Saxon.Ejavax.xml.transform.Source source) {
            Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.getParseOptions().setSchemaValidationMode(Saxon.Hej.lib.Validation.SKIP);
            pipe.setHostLanguage(Saxon.Api.HostLanguage.XML_SCHEMA);
            Saxon.Hej.@event.NamespaceReducer reducer = new Saxon.Hej.@event.NamespaceReducer(this);
            namespaceResolver = reducer;
            compiler = config.newSchemaCompiler();
            Saxon.Hej.@event.CheckSumFilter filter = new Saxon.Hej.@event.CheckSumFilter(reducer);
            filter.setCheckExistingChecksum(true);
            Saxon.Hej.@event.Sender.send(source, filter, null);
            if (!(filter.isChecksumCorrect())) {
                if ((filter.isChecksumFound() || (dmk != null))) {
                    throw new Saxon.Hej.trans.XPathException("Invalid checksum in compiled schema file");
                }
            }
            try {
                foreach (Saxon.Eej.ee.schema.SchemaModelLoader.Fixup fixup in fixups) {
                    fixup.apply(this);
                }
                foreach (Saxon.Eej.ee.schema.SchemaModelLoader.PendingEdge pe in pendingEdges) {
                    Saxon.Eej.ee.schema.fsa.DeterminizedState start = (Saxon.Eej.ee.schema.fsa.DeterminizedState)(pe.machine.getState(pe.fromState));
                    Saxon.Eej.ee.schema.fsa.DeterminizedState end = (Saxon.Eej.ee.schema.fsa.DeterminizedState)(pe.machine.getState(pe.toState));
                    if (pe.termId.Equals("##afterMax")) {
                        Saxon.Eej.ee.schema.Term term = Saxon.Eej.ee.schema.fsa.Edge.MaxOccursTerm.getInstance();
                        Saxon.Eej.ee.schema.fsa.Edge edge = new Saxon.Eej.ee.schema.fsa.Edge();
                        edge.setTerm(term);
                        edge.setTargetState(end);
                        start.setMaxTransition(edge);
                    } else {
                        Saxon.Eej.ee.schema.Term term = (Saxon.Eej.ee.schema.Term)(Saxon.Impl.Helpers.MapUtils.Get(components, pe.termId));
                        if ((term == null)) {
                            throw new Saxon.Hej.type.SchemaException(("Unresolved reference to term "+pe.termId));
                        }
                        Saxon.Eej.ee.schema.fsa.Edge edge = new Saxon.Eej.ee.schema.fsa.Edge();
                        edge.setTargetState(end);
                        edge.setTerm(term);
                        if ((term is Saxon.Eej.ee.schema.ElementDecl)) {
                            start.addSpecificTransition(edge, compiler);
                        } else {
                            start.addWildcardTransition(compiler, edge);
                        }
                    }
                }
                compiler.setPreparedSchema(schema);
                foreach (Saxon.Hej.type.SchemaComponent o in components.Values) {
                    if (((o is Saxon.Eej.ee.schema.ElementDecl) && ((Saxon.Eej.ee.schema.ElementDecl)(o)).isGlobal())) {
                        ((Saxon.Eej.ee.schema.ElementDecl)(o)).fixupSubstitutionGroup(compiler);
                    }
                }
                foreach (Saxon.Hej.type.SchemaComponent o in components.Values) {
                    if ((o is Saxon.Eej.ee.schema.UserSchemaComponent)) {
                        ((Saxon.Eej.ee.schema.UserSchemaComponent)(o)).elaborate(compiler);
                    }
                }
                foreach (Saxon.Hej.type.SchemaComponent o in anonComponents) {
                    if ((o is Saxon.Eej.ee.schema.UserSchemaComponent)) {
                        ((Saxon.Eej.ee.schema.UserSchemaComponent)(o)).elaborate(compiler);
                    }
                }
            } catch (Saxon.Hej.type.SchemaException err) {
                throw new Saxon.Hej.trans.XPathException(err);
            }
            return schema;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            elementName = elemName;
            Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handler;
            if ((handlerStack.Count==0)) {
                handler = new Saxon.Eej.ee.schema.SchemaModelLoader.SchemaHandler();
            } else {
                handler = handlerStack.Peek().handleChildElement(elementName.getLocalPart());
                if ((handler == null)) {
                    throw new Saxon.Hej.trans.XPathException(((("No handler defined for element "+elementName.getLocalPart())+" in ")+handlerStack.Peek().GetType().Name));
                }
            }
            handler.setSchemaModelLoader(this);
            handlerStack.Push(handler);
            try {
                handler.processElementStart(elementName.getLocalPart(), attributes);
            } catch (Saxon.Hej.type.SchemaException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (!(Saxon.Hej.value.Whitespace.isAllWhite(chars))) {
                throw new Saxon.Hej.trans.XPathException("Character content not allowed in SCM document");
            }
        }
        public override void endElement() {
            Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handler = handlerStack.Pop();
            handler.processElementEnd();
        }
        private abstract class SCMNodeHandler {
            protected internal Saxon.Eej.ee.schema.SchemaModelLoader loader;
            public virtual void setSchemaModelLoader(Saxon.Eej.ee.schema.SchemaModelLoader loader) {
                this.loader = loader;
            }
            public abstract void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes);
            public abstract Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName);
            public abstract void processElementEnd();
        
        }
        private class SchemaHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string xsdVersion = attributes.getValue("", "xsdVersion");
                if (((xsdVersion != null) && (System.String.Compare(xsdVersion, loader.config.getConfigurationProperty(Saxon.Hej.lib.Feature<string>.XSD_VERSION), System.StringComparison.Ordinal)>0))) {
                    throw new Saxon.Hej.type.SchemaException(("Imported schema component model requires XSD version "+xsdVersion));
                }
                loader.dmk = attributes.getValue("", "dmk");
                if ((loader.dmk != null)) {
                    loader.schema.setLocalLicenseId(loader.config.registerLocalLicense(loader.dmk));
                }
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                switch (localName) {
                  case "element":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ElementHandler();
                  case "attribute":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.AttributeHandler();
                  case "complexType":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ComplexTypeHandler();
                  case "simpleType":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.SimpleTypeHandler();
                  case "attributeGroup":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.AttributeGroupHandler();
                  case "modelGroupDefinition":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ModelGroupDefinitionHandler();
                  case "notation":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.NotationHandler();
                  case "wildcard":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.WildcardHandler();
                  case "unique":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.IdentityConstraintHandler();
                  case "key":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.IdentityConstraintHandler();
                  case "keyref":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.IdentityConstraintHandler();
                }
                return null;
            }
            public override void processElementEnd() {
            }
        
        }
        private class ElementHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private Saxon.Eej.ee.schema.ElementDecl elementDecl;
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                switch (localName) {
                  case "identityConstraint":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.IdentityConstraintReferenceHandler(elementDecl);
                  case "substitutionGroupAffiliation":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.SubstitutionGroupAffiliationHandler(elementDecl);
                  case "alternativeType":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.AlternativeTypeHandler(elementDecl);
                  case "fixed":
                  case "default":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ValueConstraintHandler(elementDecl, localName);
                  default:
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                Saxon.Eej.config.EnterpriseConfiguration config = loader.config;
                string id = attributes.getValue("", "id");
                string name = attributes.getValue("", "name");
                string @namespace = attributes.getValue("", "targetNamespace");
                bool isGlobal = toBoolean(attributes.getValue("", "global"));
                bool isNillable = toBoolean(attributes.getValue("", "nillable"));
                bool isAbstract = toBoolean(attributes.getValue("", "abstract"));
                string type = attributes.getValue("", "type");
                string finalAtt = attributes.getValue("", "final");
                string defaultValue = attributes.getValue("", "default");
                int finalSet = parseFinalOrBlock("final", finalAtt, ((Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)|Saxon.Hej.type.Derivation.DERIVE_BY_SUBSTITUTION));
                string blockAtt = attributes.getValue("", "block");
                int blockSet = parseFinalOrBlock("block", blockAtt, ((Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)|Saxon.Hej.type.Derivation.DERIVE_BY_SUBSTITUTION));
                elementDecl = new Saxon.Eej.ee.schema.ElementDecl(config, isGlobal, null);
                elementDecl.setGeneratedId(id);
                Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, elementDecl);
                elementDecl.setElementName(new Saxon.Hej.om.StructuredQName("", @namespace, name));
                elementDecl.setAbstract(isAbstract);
                elementDecl.setNillable(isNillable);
                elementDecl.setBlock(blockSet);
                elementDecl.setFinalProhibitions(finalSet);
                if ((defaultValue != null)) {
                    Saxon.Eej.ee.schema.ValueConstraint vc = new Saxon.Eej.ee.schema.ValueConstraint(Saxon.Eej.ee.schema.ValueConstraint.Variety.DEFAULT, Saxon.Hej.str.StringView.tidy(defaultValue));
                    elementDecl.setValueConstraint(vc);
                }
                if ((type != null)) {
                    if (type.StartsWith("#")) {
                        int fp = config.getNamePool().getFingerprint(Saxon.Hej.lib.NamespaceConstant.SCHEMA, type.Substring(1));
                        elementDecl.setTypeReference(new Saxon.Eej.ee.schema.TypeReference(fp, config, null));
                    } else {
                        loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(elementDecl, type, FIXUP_ELEMENT_TYPE));
                    }
                }
                if (isGlobal) {
                    loader.schema.addElementDecl(elementDecl);
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class SubstitutionGroupAffiliationHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.ElementDecl elementDecl;
            public SubstitutionGroupAffiliationHandler(Saxon.Eej.ee.schema.ElementDecl decl)  {
                elementDecl = decl;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string head = attributes.getValue("", "ref");
                if ((head != null)) {
                    loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(elementDecl, head, FIXUP_ELEMENT_SUBSTITUTION_GROUP));
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class AlternativeTypeHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.ElementDecl elementDecl;
            public AlternativeTypeHandler(Saxon.Eej.ee.schema.ElementDecl decl)  {
                elementDecl = decl;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                Saxon.Eej.config.EnterpriseConfiguration config = loader.config;
                string xpathText = attributes.getValue("", "test");
                string xmlBase = attributes.getValue(Saxon.Hej.lib.NamespaceConstant.XML, "base");
                string defaultNamespace = attributes.getValue("", "defaultNamespace");
                string type = attributes.getValue("", "type");
                Saxon.Hej.sxpath.XPathEvaluator evaluator = new Saxon.Hej.sxpath.XPathEvaluator(config);
                Saxon.Hej.sxpath.IndependentContext env = (Saxon.Hej.sxpath.IndependentContext)(evaluator.getStaticContext());
                env.setNamespaceResolver(loader.namespaceResolver);
                env.setDefaultElementNamespace(defaultNamespace);
                env.setBaseURI(xmlBase);
                Saxon.Hej.sxpath.XPathExpression condition;
                try {
                    condition = evaluator.createExpression(xpathText);
                } catch (Saxon.Hej.trans.XPathException err) {
                    throw new Saxon.Hej.type.SchemaException(err);
                }
                Saxon.Eej.ee.schema.TypeAlternative alt = new Saxon.Eej.ee.schema.TypeAlternative(loader.config, condition, xpathText);
                alt.setBaseURI(xmlBase);
                alt.setDefaultElementNamespace(defaultNamespace);
                alt.setNamespaceContext(Saxon.Hej.om.NamespaceMap.fromNamespaceResolver(loader.namespaceResolver));
                alt.setParameters(new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.sxpath.XPathVariable>());
                if (type.StartsWith("#")) {
                    int fp = config.getNamePool().getFingerprint(Saxon.Hej.lib.NamespaceConstant.SCHEMA, type.Substring(1));
                    alt.setTypeReference(new Saxon.Eej.ee.schema.TypeReference(fp, loader.config, null));
                } else {
                    loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(alt, type, FIXUP_ALTERNATIVE_TYPE));
                }
                elementDecl.addTypeAlternative(alt);
            }
            public override void processElementEnd() {
            }
        
        }
        private class IdentityConstraintReferenceHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.ElementDecl container;
            public IdentityConstraintReferenceHandler(Saxon.Eej.ee.schema.ElementDecl container)  {
                this.container = container;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string @ref = attributes.getValue("", "ref");
                Saxon.Eej.ee.schema.IdentityConstraintReference constraintRef = new Saxon.Eej.ee.schema.IdentityConstraintReference();
                container.addIdentityConstraint(constraintRef);
                loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(constraintRef, @ref, FIXUP_IDENTITY_CONSTRAINT_REF));
            }
            public override void processElementEnd() {
            }
        
        }
        private class IdentityConstraintHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private Saxon.Eej.ee.schema.IdentityConstraint identityConstraint;
            public IdentityConstraintHandler()  {
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                switch (localName) {
                  case "selector":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.SelectorHandler(identityConstraint);
                  case "field":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.SelectorHandler(identityConstraint);
                  default:
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string id = attributes.getValue("", "id");
                string name = attributes.getValue("", "name");
                string @namespace = attributes.getValue("", "targetNamespace");
                string key = attributes.getValue("", "key");
                Saxon.Hej.om.StructuredQName nameCode = new Saxon.Hej.om.StructuredQName("", @namespace, name);
                Saxon.Eej.ee.schema.IdentityConstraint ic;
                switch (localName) {
                  case "unique":
                    ic = new Saxon.Eej.ee.schema.Unique();
                    Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, ic);
                    break ;
                  case "key":
                    ic = new Saxon.Eej.ee.schema.Key();
                    Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, ic);
                    break ;
                  case "keyref":
                    ic = new Saxon.Eej.ee.schema.KeyRef();
                    Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, ic);
                    loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(ic, key, FIXUP_KEYREF_KEY));
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.AssertionError(("unknown IdentityConstraint "+localName));
                }
                ic.setConfiguration(loader.config);
                ic.setConstraintName(nameCode);
                identityConstraint = ic;
            }
            public override void processElementEnd() {
            }
        
        }
        private class SelectorHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.IdentityConstraint identityConstraint;
            public SelectorHandler(Saxon.Eej.ee.schema.IdentityConstraint identityConstraint)  {
                this.identityConstraint = identityConstraint;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string xpath = attributes.getValue("", "xpath");
                bool isField = localName.Equals("field");
                string defaultNamespace = attributes.getValue("", "defaultNamespace");
                Saxon.Hej.sxpath.IndependentContext env = new Saxon.Hej.sxpath.IndependentContext(loader.config);
                env.setNamespaceResolver(Saxon.Hej.om.NamespaceMap.fromNamespaceResolver(loader.namespaceResolver));
                env.setDefaultElementNamespace(defaultNamespace);
                Saxon.Hej.pattern.Pattern selector;
                try {
                    selector = new Saxon.Eej.ee.schema.sdoc.SelectionParser().parseSelector(xpath, env, isField);
                } catch (Saxon.Hej.trans.XPathException err) {
                    throw new Saxon.Hej.type.SchemaException(err);
                }
                if (isField) {
                    Saxon.Eej.ee.schema.IdentityField idf = new Saxon.Eej.ee.schema.IdentityField(xpath);
                    idf.setSelection(selector);
                    idf.setNamespaceContext(Saxon.Hej.om.NamespaceMap.fromNamespaceResolver(loader.namespaceResolver));
                    idf.setXPathDefaultNamespace(defaultNamespace);
                    identityConstraint.addField(idf);
                } else {
                    Saxon.Eej.ee.schema.IdentitySelector ids = new Saxon.Eej.ee.schema.IdentitySelector(xpath);
                    ids.setSelection(selector);
                    ids.setNamespaceContext(Saxon.Hej.om.NamespaceMap.fromNamespaceResolver(loader.namespaceResolver));
                    ids.setXPathDefaultNamespace(defaultNamespace);
                    identityConstraint.setSelector(ids);
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class AttributeHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private Saxon.Eej.ee.schema.AttributeDecl attributeDecl;
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                if ((localName.Equals("fixed") || localName.Equals("default"))) {
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ValueConstraintHandler(attributeDecl, localName);
                } else {
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string id = attributes.getValue("", "id");
                string name = attributes.getValue("", "name");
                string @namespace = attributes.getValue("", "targetNamespace");
                Saxon.Hej.om.StructuredQName attributeName = null;
                if ((name != null)) {
                    attributeName = new Saxon.Hej.om.StructuredQName("", @namespace, name);
                }
                bool isGlobal = toBoolean(attributes.getValue("", "global"));
                bool isInheritable = toBoolean(attributes.getValue("", "inheritable"));
                string type = attributes.getValue("", "type");
                string defaultValue = attributes.getValue("", "default");
                attributeDecl = new Saxon.Eej.ee.schema.AttributeDecl(loader.config, isGlobal);
                Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, attributeDecl);
                attributeDecl.setAttributeName(attributeName, loader.config.getNamePool().allocateFingerprint(@namespace, name));
                attributeDecl.setInheritable(isInheritable);
                if ((defaultValue != null)) {
                    Saxon.Eej.ee.schema.ValueConstraint vc = new Saxon.Eej.ee.schema.ValueConstraint(Saxon.Eej.ee.schema.ValueConstraint.Variety.DEFAULT, Saxon.Hej.str.StringView.tidy(defaultValue));
                    attributeDecl.setValueConstraint(vc);
                }
                if ((type != null)) {
                    if (type.StartsWith("#")) {
                        int fp = loader.config.getNamePool().getFingerprint(Saxon.Hej.lib.NamespaceConstant.SCHEMA, type.Substring(1));
                        attributeDecl.setTypeReference(new Saxon.Eej.ee.schema.TypeReference(fp, loader.config, null));
                    } else {
                        loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(attributeDecl, type, FIXUP_ATTRIBUTE_TYPE));
                    }
                }
                if (isGlobal) {
                    loader.schema.addAttributeDecl(attributeDecl);
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class ComplexTypeHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            internal Saxon.Eej.ee.schema.UserComplexType complexType;
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                switch (localName) {
                  case "openContent":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.OpenContentHandler(complexType);
                  case "attributeUse":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.AttributeUseHandler(complexType);
                  case "attributeWildcard":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.AttributeWildcardHandler(complexType);
                  case "modelGroup":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ParticleHandler(complexType);
                  case "elementParticle":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ParticleHandler(complexType);
                  case "elementWildcard":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ParticleHandler(complexType);
                  case "modelGroupParticle":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ParticleHandler(complexType);
                  case "finiteStateMachine":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.FiniteStateMachineHandler(complexType);
                  case "assertion":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.AssertionHandler(complexType);
                  default:
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                Saxon.Eej.config.EnterpriseConfiguration config = loader.config;
                string id = attributes.getValue("", "id");
                string name = attributes.getValue("", "name");
                string @namespace = attributes.getValue("", "targetNamespace");
                int fp;
                if ((name == null)) {
                    fp = loader.config.getNamePool().allocateFingerprint(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, id);
                    name = id;
                    @namespace = Saxon.Hej.lib.NamespaceConstant.ANONYMOUS;
                } else {
                    fp = loader.config.getNamePool().allocateFingerprint(@namespace, name);
                }
                string @base = attributes.getValue("", "base");
                string variety = attributes.getValue("", "variety");
                bool isAbstract = toBoolean(attributes.getValue("", "abstract"));
                string derivation = attributes.getValue("", "derivationMethod");
                string simpleType = attributes.getValue("", "simpleType");
                complexType = new Saxon.Eej.ee.schema.UserComplexType(config, null);
                complexType.setEmptyCombinedAttributeGroup();
                Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, complexType);
                complexType.setTypeName(new Saxon.Hej.om.StructuredQName("", @namespace, name), fp);
                if ((@base != null)) {
                    if (@base.StartsWith("#")) {
                        int basefp = config.getNamePool().getFingerprint(Saxon.Hej.lib.NamespaceConstant.SCHEMA, @base.Substring(1));
                        complexType.setBaseTypeReference(new Saxon.Eej.ee.schema.TypeReference(basefp, config, null));
                    } else {
                        loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(complexType, @base, FIXUP_COMPLEX_TYPE_BASE_TYPE));
                    }
                }
                Saxon.Hej.type.ComplexVariety varietyCode = Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY;
                switch (variety) {
                  case "mixed":
                    varietyCode = Saxon.Hej.type.ComplexVariety.MIXED;
                    break ;
                  case "empty":
                    varietyCode = Saxon.Hej.type.ComplexVariety.EMPTY;
                    break ;
                  case "element-only":
                    varietyCode = Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY;
                    break ;
                  case "simple":
                    varietyCode = Saxon.Hej.type.ComplexVariety.SIMPLE;
                    break ;
                }
                complexType.setVariety(varietyCode);
                complexType.setAbstract(isAbstract);
                complexType.setDerivationMethodName(derivation);
                if ((simpleType != null)) {
                    if (simpleType.StartsWith("#")) {
                        int typefp = config.getNamePool().getFingerprint(Saxon.Hej.lib.NamespaceConstant.SCHEMA, simpleType.Substring(1));
                        complexType.setSimpleContentTypeReference(new Saxon.Eej.ee.schema.TypeReference(typefp, config, null));
                    } else {
                        loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(complexType, simpleType, FIXUP_COMPLEX_TYPE_SIMPLE_TYPE));
                    }
                }
                if ((name == null)) {
                    loader.schema.addAnonymousType(complexType);
                } else {
                    loader.schema.addType(complexType);
                }
                string blockAtt = attributes.getValue("", "block");
                int blockSet = parseFinalOrBlock("block", blockAtt, (Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION));
                complexType.setBlock(blockSet);
                string finalAtt = attributes.getValue("", "final");
                int finalSet = parseFinalOrBlock("final", finalAtt, (Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION));
                complexType.setFinalProhibitions(finalSet);
            }
            public override void processElementEnd() {
            }
        
        }
        private class SimpleTypeHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private Saxon.Eej.ee.schema.UserSimpleType simpleType;
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                if ((/*134*/Saxon.Eej.ee.schema.Facet.isFacetName(localName, loader.config.getXsdVersion()) || localName.Equals("assert"))) {
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.FacetHandler(simpleType);
                } else {
                    if (localName.Equals("preprocess")) {
                        return new Saxon.Eej.ee.schema.SchemaModelLoader.PreprocessHandler(simpleType);
                    } else {
                        return null;
                    }
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                Saxon.Eej.config.EnterpriseConfiguration config = loader.config;
                string id = attributes.getValue("", "id");
                string name = attributes.getValue("", "name");
                string @namespace = attributes.getValue("", "targetNamespace");
                int fingerprint;
                if ((name == null)) {
                    fingerprint = config.getNamePool().allocateFingerprint(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, id);
                    name = id;
                    @namespace = Saxon.Hej.lib.NamespaceConstant.ANONYMOUS;
                } else {
                    fingerprint = loader.config.getNamePool().allocateFingerprint(@namespace, name);
                }
                string @base = attributes.getValue("", "base");
                string variety = attributes.getValue("", "variety");
                string itemType = attributes.getValue("", "itemType");
                string memberTypes = attributes.getValue("", "memberTypes");
                if ("atomic".Equals(variety)) {
                    simpleType = new Saxon.Eej.ee.schema.UserAtomicType(config);
                } else {
                    if ("list".Equals(variety)) {
                        simpleType = new Saxon.Eej.ee.schema.UserListType(config);
                    } else {
                        if ("union".Equals(variety)) {
                            simpleType = new Saxon.Eej.ee.schema.UserUnionType(config);
                        } else {
                            throw new Saxon.Hej.type.SchemaException(("Unknown simple type variety: "+variety));
                        }
                    }
                }
                Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, simpleType);
                simpleType.setTypeName(new Saxon.Hej.om.StructuredQName("", @namespace, name), fingerprint);
                if ((@base != null)) {
                    if (@base.StartsWith("#")) {
                        int fp = loader.config.getNamePool().getFingerprint(Saxon.Hej.lib.NamespaceConstant.SCHEMA, @base.Substring(1));
                        simpleType.setBaseTypeReference(new Saxon.Eej.ee.schema.TypeReference(fp, loader.config, null));
                    } else {
                        loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(simpleType, @base, FIXUP_SIMPLE_TYPE_BASE_TYPE));
                    }
                }
                if ((itemType != null)) {
                    if (itemType.StartsWith("#")) {
                        int fp = loader.config.getNamePool().getFingerprint(Saxon.Hej.lib.NamespaceConstant.SCHEMA, itemType.Substring(1));
                        ((Saxon.Eej.ee.schema.UserListType)(simpleType)).setItemTypeReference(new Saxon.Eej.ee.schema.TypeReference(fp, loader.config, null));
                    } else {
                        loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(simpleType, itemType, FIXUP_SIMPLE_TYPE_ITEM_TYPE));
                    }
                }
                if ((memberTypes != null)) {
                    Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(memberTypes, " \t\n\r");
                    while (st.hasMoreTokens()) {
                        string memberType = st.nextToken();
                        if (memberType.StartsWith("#")) {
                            int fp = loader.config.getNamePool().getFingerprint(Saxon.Hej.lib.NamespaceConstant.SCHEMA, memberType.Substring(1));
                            ((Saxon.Eej.ee.schema.UserUnionType)(simpleType)).addMemberTypeReference(new Saxon.Eej.ee.schema.TypeReference(fp, loader.config, null));
                        } else {
                            loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(simpleType, memberType, FIXUP_SIMPLE_TYPE_MEMBER_TYPE));
                        }
                    }
                }
                if ((name == null)) {
                    loader.schema.addAnonymousType(simpleType);
                } else {
                    loader.schema.addType(simpleType);
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class AttributeGroupHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private Saxon.Eej.ee.schema.AttributeGroupDecl group;
            public AttributeGroupHandler()  {
            }
            public override void setSchemaModelLoader(Saxon.Eej.ee.schema.SchemaModelLoader loader) {
                base.setSchemaModelLoader(loader);
                group = new Saxon.Eej.ee.schema.AttributeGroupDecl(loader.config);
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                switch (localName) {
                  case "attributeUse":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.AttributeUseHandler(group);
                  case "attributeWildcard":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.AttributeWildcardHandler(group);
                  default:
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string id = attributes.getValue("", "id");
                string name = attributes.getValue("", "name");
                string @namespace = attributes.getValue("", "targetNamespace");
                Saxon.Hej.om.StructuredQName nameCode = null;
                if ((name != null)) {
                    nameCode = new Saxon.Hej.om.StructuredQName("", @namespace, name);
                }
                Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, group);
                group.setAttributeGroupName(nameCode);
                loader.schema.addAttributeGroup(group);
            }
            public override void processElementEnd() {
            }
        
        }
        private class NotationHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                Saxon.Eej.ee.schema.Notation notation = new Saxon.Eej.ee.schema.Notation(loader.config, null);
                string id = attributes.getValue("", "id");
                string name = attributes.getValue("", "name");
                string @namespace = attributes.getValue("", "targetNamespace");
                Saxon.Hej.om.StructuredQName nameCode;
                if ((name == null)) {
                    nameCode = new Saxon.Hej.om.StructuredQName("anon", Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, id);
                } else {
                    nameCode = new Saxon.Hej.om.StructuredQName("", @namespace, name);
                }
                notation.setNotationName(nameCode);
                string systemId = attributes.getValue("", "systemId");
                notation.setNotationSystemId(systemId);
                string publicId = attributes.getValue("", "publicId");
                notation.setNotationPublicId(publicId);
                Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, notation);
                loader.schema.addNotation(notation);
            }
            public override void processElementEnd() {
            }
        
        }
        private class ModelGroupDefinitionHandler : Saxon.Eej.ee.schema.SchemaModelLoader.ModelGroupHandler {
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string id = attributes.getValue("", "id");
                string name = attributes.getValue("", "name");
                string @namespace = attributes.getValue("", "targetNamespace");
                Saxon.Hej.om.StructuredQName modelGroupName = new Saxon.Hej.om.StructuredQName("", @namespace, name);
                Saxon.Eej.ee.schema.ModelGroupDefinition mgd = new Saxon.Eej.ee.schema.ModelGroupDefinition(loader.config, null);
                mgd.setModelGroupName(modelGroupName);
                Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, mgd);
                loader.schema.addGroup(mgd);
                group = mgd;
            }
            public override void processElementEnd() {
            }
        
        }
        private class AttributeUseHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Hej.type.SchemaComponent container;
            private Saxon.Eej.ee.schema.AttributeUse attributeUse;
            public AttributeUseHandler(Saxon.Hej.type.SchemaComponent container)  {
                this.container = container;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                if ((localName.Equals("fixed") || localName.Equals("default"))) {
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ValueConstraintHandler(attributeUse, localName);
                } else {
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                bool required = toBoolean(attributes.getValue("", "required"));
                bool isInheritable = toBoolean(attributes.getValue("", "inheritable"));
                string @ref = attributes.getValue("", "ref");
                string defaultValue = attributes.getValue("", "default");
                attributeUse = new Saxon.Eej.ee.schema.AttributeUse(loader.config);
                attributeUse.setInheritable((isInheritable ? 1 : 0));
                if ((container is Saxon.Eej.ee.schema.UserComplexType)) {
                    ((Saxon.Eej.ee.schema.UserComplexType)(container)).addNonInheritedAttributeUse(attributeUse);
                } else {
                    ((Saxon.Eej.ee.schema.AttributeGroupDecl)(container)).addAttributeUse(attributeUse);
                }
                attributeUse.setUse((required ? Saxon.Eej.ee.schema.Optionality.REQUIRED : Saxon.Eej.ee.schema.Optionality.OPTIONAL));
                if ((defaultValue != null)) {
                    Saxon.Eej.ee.schema.ValueConstraint vc = new Saxon.Eej.ee.schema.ValueConstraint(Saxon.Eej.ee.schema.ValueConstraint.Variety.DEFAULT, Saxon.Hej.str.StringView.tidy(defaultValue));
                    attributeUse.setValueConstraint(vc);
                }
                loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(attributeUse, @ref, FIXUP_ATTRIBUTE_USE_DECLARATION));
            }
            public override void processElementEnd() {
            }
        
        }
        private class FacetHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.UserSimpleType simpleType;
            public FacetHandler(Saxon.Eej.ee.schema.UserSimpleType simpleType)  {
                this.simpleType = simpleType;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                if (localName.Equals("assertion")) {
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.AssertionHandler(simpleType);
                }
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                if (!(localName.Equals("assert"))) {
                    string value = attributes.getValue("", "value");
                    bool namespaceSensitive = toBoolean(attributes.getValue("", "namespaceSensitive"));
                    Saxon.Hej.om.NamespaceResolver resolver = null;
                    if (namespaceSensitive) {
                        resolver = Saxon.Hej.om.NamespaceMap.fromNamespaceResolver(loader.namespaceResolver);
                    }
                    Saxon.Eej.ee.schema.Facet facet = /*134*/Saxon.Eej.ee.schema.Facet.makeFacet(localName, value, loader.config, resolver);
                    string @fixed = attributes.getValue("", "fixed");
                    if ((@fixed != null)) {
                        facet.setFixed(toBoolean(@fixed));
                    }
                    if ((facet is Saxon.Eej.ee.schema.PatternFacet)) {
                        ((Saxon.Eej.ee.schema.PatternFacet)(facet)).setDoNotMerge(true);
                        string flags = attributes.getValue("", "flags");
                        if ((flags != null)) {
                            ((Saxon.Eej.ee.schema.PatternFacet)(facet)).setFlags(flags);
                        }
                        ((Saxon.Eej.ee.schema.PatternFacet)(facet)).compile(null);
                    }
                    simpleType.addFacet(facet);
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class PreprocessHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.UserSimpleType simpleType;
            private readonly Saxon.Eej.ee.schema.PreprocessFacet facet;
            public PreprocessHandler(Saxon.Eej.ee.schema.UserSimpleType simpleType)  {
                this.simpleType = simpleType;
                facet = new Saxon.Eej.ee.schema.PreprocessFacet();
                simpleType.addFacet(facet);
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                if (localName.Equals("assertion")) {
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.AssertionHandler(facet);
                }
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string @fixed = attributes.getValue("", "fixed");
                if ((@fixed != null)) {
                    facet.setFixed(toBoolean(@fixed));
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class ParticleHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Hej.type.SchemaComponent container;
            private Saxon.Eej.ee.schema.Particle particle;
            public ParticleHandler(Saxon.Hej.type.SchemaComponent container)  {
                this.container = container;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                switch (localName) {
                  case "sequence":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ModelGroupHandler((Saxon.Eej.ee.schema.ModelGroupParticle)(particle));
                  case "choice":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ModelGroupHandler((Saxon.Eej.ee.schema.ModelGroupParticle)(particle));
                  case "all":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ModelGroupHandler((Saxon.Eej.ee.schema.ModelGroupParticle)(particle));
                  default:
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                Saxon.Eej.config.EnterpriseConfiguration config = loader.config;
                switch (localName) {
                  case "elementParticle":
                    particle = new Saxon.Eej.ee.schema.ElementParticle(config, null);
                    break ;
                  case "elementWildcard":
                    particle = new Saxon.Eej.ee.schema.ElementWildcard(config);
                    break ;
                  case "modelGroupParticle":
                    particle = new Saxon.Eej.ee.schema.ModelGroupParticle(config, null);
                    break ;
                  default:
                    throw new Saxon.Hej.type.SchemaException(("Unknown kind of particle "+localName));
                }
                string minOccursAtt = attributes.getValue("", "minOccurs");
                int minOccurs = 1;
                if ((minOccursAtt != null)) {
                    minOccurs = Saxon.Impl.Helpers.Utils.ParseInt(minOccursAtt.Trim());
                }
                particle.setMinOccurs(minOccurs);
                string maxOccursAtt = attributes.getValue("", "maxOccurs");
                int maxOccurs = -1;
                if ((maxOccursAtt != null)) {
                    maxOccursAtt = maxOccursAtt.Trim();
                    if (maxOccursAtt.Equals("unbounded")) {
                        maxOccurs = -1;
                    } else {
                        maxOccurs = Saxon.Impl.Helpers.Utils.ParseInt(maxOccursAtt);
                    }
                }
                particle.setMaxOccurs(maxOccurs);
                if ((particle is Saxon.Eej.ee.schema.ElementParticle)) {
                    string @ref = attributes.getValue("", "ref");
                    loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(particle, @ref, FIXUP_ELEMENT_PARTICLE_REF));
                } else {
                    if ((particle is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                        string @ref = attributes.getValue("", "ref");
                        if ((@ref != null)) {
                            loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(particle, @ref, FIXUP_MODEL_GROUP_PARTICLE_REF));
                        }
                    } else {
                        if ((particle is Saxon.Eej.ee.schema.ElementWildcard)) {
                            string @ref = attributes.getValue("", "ref");
                            loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(particle, @ref, FIXUP_ELEMENT_WILDCARD_REF));
                        } else {
                            throw new Saxon.Ejava.lang.IllegalStateException(("Unknown particle "+particle.GetType().Name));
                        }
                    }
                }
                if ((container is Saxon.Eej.ee.schema.ModelGroup)) {
                    ((Saxon.Eej.ee.schema.ModelGroup)(container)).addParticle(particle);
                } else {
                    if ((container is Saxon.Eej.ee.schema.UserComplexType)) {
                        ((Saxon.Eej.ee.schema.UserComplexType)(container)).setParticle(particle);
                    } else {
                        throw new Saxon.Ejava.lang.IllegalArgumentException(container.GetType().Name);
                    }
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class ModelGroupHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.ModelGroupParticle referrer;
            protected internal Saxon.Eej.ee.schema.ModelGroup group;
            public ModelGroupHandler()  {
                referrer = null;
            }
            public ModelGroupHandler(Saxon.Eej.ee.schema.ModelGroupParticle referrer)  {
                this.referrer = referrer;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                switch (localName) {
                  case "elementParticle":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ParticleHandler(group);
                  case "elementWildcard":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ParticleHandler(group);
                  case "modelGroupParticle":
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ParticleHandler(group);
                  default:
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                switch (localName) {
                  case "sequence":
                    group = new Saxon.Eej.ee.schema.SequenceModelGroup();
                    break ;
                  case "choice":
                    group = new Saxon.Eej.ee.schema.ChoiceModelGroup();
                    break ;
                  case "all":
                    group = new Saxon.Eej.ee.schema.AllModelGroup();
                    break ;
                  default:
                    throw new Saxon.Hej.type.SchemaException(("Unknown kind of model group "+localName));
                }
                if ((referrer != null)) {
                    referrer.setTarget(group);
                }
                loader.anonComponents.Add(group);
            }
            public override void processElementEnd() {
            }
        
        }
        private class AssertionHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly System.Object container;
            public AssertionHandler(System.Object container)  {
                this.container = container;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string xpathText = attributes.getValue("", "test");
                string xmlBase = attributes.getValue(Saxon.Hej.lib.NamespaceConstant.XML, "base");
                string defaultNamespace = attributes.getValue("", "defaultNamespace");
                string sweep = attributes.getValue("", "sweep");
                Saxon.Hej.sxpath.XPathEvaluator evaluator;
                if ((container is Saxon.Eej.ee.schema.UserComplexType)) {
                    evaluator = new Saxon.Eej.ee.schema.sdoc.XSDAssert.CustomizedXPath(loader.config);
                } else {
                    evaluator = new Saxon.Hej.sxpath.XPathEvaluator(loader.config);
                }
                Saxon.Hej.sxpath.IndependentContext env = (Saxon.Hej.sxpath.IndependentContext)(evaluator.getStaticContext());
                env.setNamespaceResolver(Saxon.Hej.om.NamespaceMap.fromNamespaceResolver(loader.namespaceResolver));
                env.setBaseURI(xmlBase);
                env.setDefaultElementNamespace(defaultNamespace);
                env.getPackageData().setLocalLicenseId(loader.schema.getLocalLicenseId());
                env.setSchemaAware(true);
                bool isSimple = (((container is Saxon.Eej.ee.schema.UserSimpleType)) || (((container is Saxon.Eej.ee.schema.UserComplexType) && ((Saxon.Eej.ee.schema.UserComplexType)(container)).isSimpleContent())));
                Saxon.Hej.sxpath.XPathVariable valueVariable = null;
                if (((isSimple || (container is Saxon.Eej.ee.schema.PreprocessFacet)) || xpathText.Contains("$value"))) {
                    valueVariable = env.declareVariable("", "value");
                }
                Saxon.Hej.sxpath.XPathExpression condition;
                try {
                    condition = evaluator.createExpression(xpathText);
                } catch (Saxon.Hej.trans.XPathException err) {
                    throw new Saxon.Hej.type.SchemaException(err);
                }
                Saxon.Eej.ee.schema.Assertion assertion = new Saxon.Eej.ee.schema.Assertion(loader.config, condition, valueVariable, new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.sxpath.XPathVariable>(), xpathText);
                assertion.setStaticContext(env);
                if ((sweep != null)) {
                    assertion.setSweep((Saxon.Eej.ee.stream.Sweep)Enum.Parse(typeof(Saxon.Eej.ee.stream.Sweep), sweep));
                }
                assertion.setReturnsFailingNodes(((container is Saxon.Eej.ee.schema.UserComplexType) && (sweep == null)));
                if ((container is Saxon.Eej.ee.schema.UserComplexType)) {
                    ((Saxon.Eej.ee.schema.UserComplexType)(container)).addAssertion(assertion);
                } else {
                    if ((container is Saxon.Eej.ee.schema.UserSimpleType)) {
                        Saxon.Eej.ee.schema.UserSimpleType ust = (Saxon.Eej.ee.schema.UserSimpleType)(container);
                        Saxon.Eej.ee.schema.AssertionFacet facet = new Saxon.Eej.ee.schema.AssertionFacet(assertion);
                        ust.addFacet(facet);
                    } else {
                        if ((container is Saxon.Eej.ee.schema.PreprocessFacet)) {
                            Saxon.Eej.ee.schema.PreprocessFacet ppf = (Saxon.Eej.ee.schema.PreprocessFacet)(container);
                            if ((ppf.getAction() == null)) {
                                ppf.setAction(assertion);
                            } else {
                                ppf.setReverseAction(assertion);
                            }
                        }
                    }
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class OpenContentHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.UserComplexType container;
            public OpenContentHandler(Saxon.Eej.ee.schema.UserComplexType container)  {
                this.container = container;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string mode = attributes.getValue("", "mode");
                string wildcardRef = attributes.getValue("", "wildcard");
                if (mode.Equals("suffix")) {
                    loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(container, wildcardRef, FIXUP_COMPLEX_TYPE_OPEN_CONTENT_SUFFIX));
                } else {
                    loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(container, wildcardRef, FIXUP_COMPLEX_TYPE_OPEN_CONTENT_INTERLEAVE));
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class AttributeWildcardHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Hej.type.SchemaComponent container;
            public AttributeWildcardHandler(Saxon.Hej.type.SchemaComponent container)  {
                this.container = container;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                Saxon.Eej.ee.schema.AttributeWildcard awc = new Saxon.Eej.ee.schema.AttributeWildcard((Saxon.Eej.config.EnterpriseConfiguration)(loader.getConfiguration()));
                if ((container is Saxon.Eej.ee.schema.AttributeGroupDecl)) {
                    ((Saxon.Eej.ee.schema.AttributeGroupDecl)(container)).setAnyAttribute(awc);
                } else {
                    if ((container is Saxon.Eej.ee.schema.UserComplexType)) {
                        ((Saxon.Eej.ee.schema.UserComplexType)(container)).setNonInheritedAnyAttribute(awc);
                    } else {
                        throw new Saxon.Ejava.lang.IllegalArgumentException(container.GetType().Name);
                    }
                }
                string @ref = attributes.getValue("", "ref");
                loader.fixups.Add(new Saxon.Eej.ee.schema.SchemaModelLoader.Fixup(awc, @ref, FIXUP_ATTRIBUTE_WILDCARD_REF));
            }
            public override void processElementEnd() {
            }
        
        }
        private class ValueConstraintHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Hej.type.SchemaComponent container;
            private readonly System.Collections.Generic.List<Saxon.Hej.value.AtomicValue> atomicValues = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>(4);
            private string lexicalForm;
            private readonly string variety;
            public ValueConstraintHandler(Saxon.Hej.type.SchemaComponent container, string variety)  {
                this.container = container;
                this.variety = variety;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                if (localName.Equals("item")) {
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.ItemHandler(this);
                } else {
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                lexicalForm = attributes.getValue("", "lexicalForm");
            }
            public override void processElementEnd() {
                Saxon.Hej.om.AtomicSequence v;
                if ((atomicValues.Count == 1)) {
                    v = atomicValues[0];
                } else {
                    v = new Saxon.Hej.om.AtomicArray(atomicValues);
                }
                if ((container is Saxon.Eej.ee.schema.ComponentWithValueConstraint)) {
                    Saxon.Eej.ee.schema.ValueConstraint.Variety var = (variety.Equals("fixed") ? Saxon.Eej.ee.schema.ValueConstraint.Variety.FIXED : Saxon.Eej.ee.schema.ValueConstraint.Variety.DEFAULT);
                    Saxon.Eej.ee.schema.ValueConstraint vc = new Saxon.Eej.ee.schema.ValueConstraint(var, ((lexicalForm == null) ? null : Saxon.Hej.str.StringView.tidy(lexicalForm)));
                    vc.setValue(v);
                    ((Saxon.Eej.ee.schema.ComponentWithValueConstraint)(container)).setValueConstraint(vc);
                }
            }
            public void appendAtomicValue(Saxon.Hej.value.AtomicValue item) {
                atomicValues.Add(item);
            }
        
        }
        private class ItemHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.SchemaModelLoader.ValueConstraintHandler container;
            public ItemHandler(Saxon.Eej.ee.schema.SchemaModelLoader.ValueConstraintHandler container)  {
                this.container = container;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string type = attributes.getValue("", "type");
                Saxon.Hej.type.BuiltInAtomicType t = (Saxon.Hej.type.BuiltInAtomicType)(loader.config.getSchemaType(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.lib.NamespaceConstant.SCHEMA, type.Substring(1))));
                string value = attributes.getValue("", "value");
                string @namespace = attributes.getValue("", "namespace");
                Saxon.Hej.value.AtomicValue typedValue;
                try {
                    if ((@namespace == null)) {
                        typedValue = ((/*1430*/Saxon.Hej.value.AtomicValue)t.getTypedValue(Saxon.Hej.str.StringView.tidy(value), null, loader.config.getConversionRules()).head());
                    } else {
                        string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(value);
                        if (t.Equals(Saxon.Hej.type.BuiltInAtomicType.QNAME)) {
                            typedValue = new Saxon.Hej.value.QNameValue(parts[0], @namespace, parts[1]);
                        } else {
                            if (t.Equals(Saxon.Hej.type.BuiltInAtomicType.NOTATION)) {
                                typedValue = new Saxon.Hej.value.NotationValue(parts[0], @namespace, parts[1]);
                            } else {
                                throw new Saxon.Ejava.lang.AssertionError(("Unknown namespace-sensitive type "+type));
                            }
                        }
                    }
                } catch (Exception e) when (e is Saxon.Hej.om.QNameException || e is Saxon.Hej.trans.XPathException) {
                    throw new Saxon.Hej.type.SchemaException(e);
                }
                container.appendAtomicValue(typedValue);
            }
            public override void processElementEnd() {
            }
        
        }
        private class WildcardHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            public WildcardHandler()  {
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                Saxon.Eej.ee.schema.Wildcard wildcard = new Saxon.Eej.ee.schema.Wildcard();
                wildcard.setConfiguration((Saxon.Eej.config.EnterpriseConfiguration)(loader.getConfiguration()));
                string id = attributes.getValue("", "id");
                if ((id != null)) {
                    Saxon.Impl.Helpers.MapUtils.Put(loader.components, id, wildcard);
                }
                string processContents = attributes.getValue("", "processContents");
                wildcard.setProcessContents(processContents);
                string constraint = attributes.getValue("", "constraint");
                switch (constraint) {
                  case "any":
                    break ;
                  case "not":
                    {
                        string namespaces = attributes.getValue("", "namespaces");
                        Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(namespaces, " \t\n\r");
                        while (st.hasMoreTokens()) {
                            string ns = st.nextToken();
                            wildcard.addDisallowedNamespace((ns.Equals("##local") ? "" : ns));
                        }
                        break ;
                    }
                  case "enumeration":
                    {
                        wildcard.setNoNamespacesAllowed();
                        string namespaces = attributes.getValue("", "namespaces");
                        Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(namespaces, " \t\n\r");
                        while (st.hasMoreTokens()) {
                            string ns = st.nextToken();
                            wildcard.addAllowedNamespace((ns.Equals("##local") ? "" : ns));
                        }
                        break ;
                    }
                  default:
                    throw new Saxon.Hej.type.SchemaException(("Unknown wildcard constraint "+constraint));
                }
                string disallowedQNames = attributes.getValue("", "notQName");
                if ((disallowedQNames != null)) {
                    Saxon.Hej.om.NamePool pool = loader.getNamePool();
                    Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(disallowedQNames, " \r\t\n");
                    while (st.hasMoreTokens()) {
                        string token = st.nextToken();
                        if ("##defined".Equals(token)) {
                            wildcard.setDisallowDefinedNames(true);
                        } else {
                            if ("##definedSibling".Equals(token)) {
                                wildcard.setDisallowDefinedSiblings(true);
                            } else {
                                int nc = pool.allocateClarkName(token);
                                wildcard.addDisallowedQName((nc&Saxon.Hej.om.NamePool.FP_MASK));
                            }
                        }
                    }
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class FiniteStateMachineHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.fsa.FiniteStateMachine fsm;
            public FiniteStateMachineHandler(Saxon.Eej.ee.schema.UserComplexType complexType)  {
                fsm = new Saxon.Eej.ee.schema.fsa.FiniteStateMachine(complexType, true);
                complexType.setFiniteStateMachine(fsm);
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                if (localName.Equals("state")) {
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.FiniteStateMachineStateHandler(this);
                } else {
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
            }
            public Saxon.Eej.ee.schema.fsa.FiniteStateMachine getFiniteStateMachine() {
                return fsm;
            }
            public override void processElementEnd() {
            }
        
        }
        private class FiniteStateMachineStateHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.SchemaModelLoader.FiniteStateMachineHandler fsmHandler;
            private Saxon.Eej.ee.schema.fsa.DeterminizedState state;
            public FiniteStateMachineStateHandler(Saxon.Eej.ee.schema.SchemaModelLoader.FiniteStateMachineHandler fsmHandler)  {
                this.fsmHandler = fsmHandler;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                if (localName.Equals("edge")) {
                    return new Saxon.Eej.ee.schema.SchemaModelLoader.FiniteStateMachineEdgeHandler(fsmHandler, state);
                } else {
                    return null;
                }
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string stateNumber = attributes.getValue("", "nr");
                Saxon.Eej.ee.schema.fsa.FiniteStateMachine fsm = fsmHandler.getFiniteStateMachine();
                if (stateNumber.Equals("0")) {
                    state = (Saxon.Eej.ee.schema.fsa.DeterminizedState)(fsm.getInitialState());
                } else {
                    state = new Saxon.Eej.ee.schema.fsa.DeterminizedState(fsm);
                }
                string isFinal = attributes.getValue("", "final");
                state.setFinalState(toBoolean(isFinal));
                string min = attributes.getValue("", "minOccurs");
                string max = attributes.getValue("", "maxOccurs");
                if (((min != null) && (max != null))) {
                    state.setLimits(Saxon.Impl.Helpers.Utils.ParseInt(min), (max.Equals("unbounded") ? -1 : Saxon.Impl.Helpers.Utils.ParseInt(max)));
                }
                string afterMax = attributes.getValue("", "afterMax");
                if ((afterMax != null)) {
                    Saxon.Eej.ee.schema.SchemaModelLoader.PendingEdge leavingEdge = new Saxon.Eej.ee.schema.SchemaModelLoader.PendingEdge();
                    leavingEdge.machine = fsm;
                    leavingEdge.termId = "##afterMax";
                    leavingEdge.fromState = Saxon.Impl.Helpers.Utils.ParseInt(stateNumber);
                    leavingEdge.toState = Saxon.Impl.Helpers.Utils.ParseInt(afterMax);
                    loader.pendingEdges.Add(leavingEdge);
                }
            }
            public override void processElementEnd() {
            }
        
        }
        private class FiniteStateMachineEdgeHandler : Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler {
            private readonly Saxon.Eej.ee.schema.SchemaModelLoader.FiniteStateMachineHandler fsm;
            private readonly Saxon.Eej.ee.schema.fsa.DeterminizedState state;
            public FiniteStateMachineEdgeHandler(Saxon.Eej.ee.schema.SchemaModelLoader.FiniteStateMachineHandler fsm, Saxon.Eej.ee.schema.fsa.DeterminizedState state)  {
                this.fsm = fsm;
                this.state = state;
            }
            public override Saxon.Eej.ee.schema.SchemaModelLoader.SCMNodeHandler handleChildElement(string localName) {
                return null;
            }
            public override void processElementStart(string localName, Saxon.Hej.om.AttributeMap attributes) {
                string term = attributes.getValue("", "term");
                int targetState = Saxon.Impl.Helpers.Utils.ParseInt(attributes.getValue("", "to"));
                Saxon.Eej.ee.schema.SchemaModelLoader.PendingEdge pe = new Saxon.Eej.ee.schema.SchemaModelLoader.PendingEdge();
                pe.machine = fsm.getFiniteStateMachine();
                pe.fromState = state.getStateNumber();
                pe.toState = targetState;
                pe.termId = term;
                loader.pendingEdges.Add(pe);
            }
            public override void processElementEnd() {
            }
        
        }
        private class PendingEdge {
            public Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine;
            public int fromState;
            public int toState;
            public string termId;
        
        }
        private static bool toBoolean(string s) {
            if ((s == null)) {
                return false;
            }
            s = s.Trim();
            return (s.Equals("1") || s.Equals("true"));
        }
        public static int parseFinalOrBlock(string attrName, string finalValue, int allowed) {
            if ((finalValue == null)) {
                return 0;
            }
            if (Saxon.Hej.value.Whitespace.trim(finalValue).Equals("#all")) {
                return allowed;
            } else {
                int result = 0;
                Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(finalValue, " \t\n\r", false);
                while (tokenizer.hasMoreTokens()) {
                    string token = tokenizer.nextToken();
                    int v;
                    switch (token) {
                      case "union":
                        v = Saxon.Hej.type.Derivation.DERIVATION_UNION;
                        break ;
                      case "list":
                        v = Saxon.Hej.type.Derivation.DERIVATION_LIST;
                        break ;
                      case "restriction":
                        v = Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION;
                        break ;
                      case "extension":
                        v = Saxon.Hej.type.Derivation.DERIVATION_EXTENSION;
                        break ;
                      case "substitution":
                        v = Saxon.Hej.type.Derivation.DERIVE_BY_SUBSTITUTION;
                        break ;
                      default:
                        string err = ((("Invalid "+Saxon.Hej.trans.Err.wrap(attrName, Saxon.Hej.trans.Err.ATTRIBUTE))+" value ")+Saxon.Hej.trans.Err.wrap(token, Saxon.Hej.trans.Err.VALUE));
                        throw new Saxon.Hej.type.SchemaException(err);
                    }
                    if ((((v&~(allowed))) != 0)) {
                        string err = (((("The "+Saxon.Hej.trans.Err.wrap(attrName, Saxon.Hej.trans.Err.ATTRIBUTE))+" value ")+Saxon.Hej.trans.Err.wrap(token, Saxon.Hej.trans.Err.VALUE))+" is not allowed here");
                        throw new Saxon.Hej.type.SchemaException(err);
                    }
                    result |= v;
                }
                return result;
            }
        }
        private const int FIXUP_ELEMENT_TYPE = 1;
        private const int FIXUP_ELEMENT_SUBSTITUTION_GROUP = 2;
        private const int FIXUP_ATTRIBUTE_TYPE = 11;
        private const int FIXUP_ALTERNATIVE_TYPE = 12;
        private const int FIXUP_COMPLEX_TYPE_BASE_TYPE = 21;
        private const int FIXUP_COMPLEX_TYPE_SIMPLE_TYPE = 22;
        private const int FIXUP_COMPLEX_TYPE_OPEN_CONTENT_SUFFIX = 23;
        private const int FIXUP_COMPLEX_TYPE_OPEN_CONTENT_INTERLEAVE = 24;
        private const int FIXUP_SIMPLE_TYPE_ITEM_TYPE = 31;
        private const int FIXUP_SIMPLE_TYPE_MEMBER_TYPE = 32;
        private const int FIXUP_SIMPLE_TYPE_BASE_TYPE = 33;
        private const int FIXUP_ATTRIBUTE_USE_DECLARATION = 41;
        private const int FIXUP_ATTRIBUTE_WILDCARD_REF = 42;
        private const int FIXUP_ELEMENT_PARTICLE_REF = 51;
        private const int FIXUP_ELEMENT_WILDCARD_REF = 52;
        private const int FIXUP_MODEL_GROUP_PARTICLE_REF = 61;
        private const int FIXUP_KEYREF_KEY = 62;
        private const int FIXUP_IDENTITY_CONSTRAINT_REF = 63;
        private class Fixup {
            private readonly System.Object source;
            private readonly string targetId;
            private readonly int property;
            public Fixup(System.Object source, string targetId, int property)  {
                this.source = source;
                this.targetId = targetId;
                this.property = property;
            }
            public void apply(Saxon.Eej.ee.schema.SchemaModelLoader loader) {
                switch (property) {
                  case FIXUP_ELEMENT_TYPE:
                    {
                        ((Saxon.Eej.ee.schema.ElementDecl)(source)).setTypeReference(loader.makeTypeReference(targetId));
                        break ;
                    }
                  case FIXUP_ELEMENT_SUBSTITUTION_GROUP:
                    {
                        Saxon.Eej.ee.schema.ElementDecl head = (Saxon.Eej.ee.schema.ElementDecl)(Saxon.Impl.Helpers.MapUtils.Get(loader.components, targetId));
                        ((Saxon.Eej.ee.schema.ElementDecl)(source)).addSubstitutionGroupHead(head.getComponentName());
                        break ;
                    }
                  case FIXUP_ALTERNATIVE_TYPE:
                    {
                        ((Saxon.Eej.ee.schema.TypeAlternative)(source)).setTypeReference(loader.makeTypeReference(targetId));
                        break ;
                    }
                  case FIXUP_ATTRIBUTE_TYPE:
                    {
                        ((Saxon.Eej.ee.schema.AttributeDecl)(source)).setTypeReference(loader.makeTypeReference(targetId));
                        break ;
                    }
                  case FIXUP_COMPLEX_TYPE_BASE_TYPE:
                    {
                        ((Saxon.Eej.ee.schema.UserComplexType)(source)).setBaseTypeReference(loader.makeTypeReference(targetId));
                        break ;
                    }
                  case FIXUP_COMPLEX_TYPE_SIMPLE_TYPE:
                    {
                        ((Saxon.Eej.ee.schema.UserComplexType)(source)).setSimpleContentTypeReference(loader.makeTypeReference(targetId));
                        break ;
                    }
                  case FIXUP_COMPLEX_TYPE_OPEN_CONTENT_SUFFIX:
                    {
                        Saxon.Eej.ee.schema.Wildcard wildcard = (Saxon.Eej.ee.schema.Wildcard)(Saxon.Impl.Helpers.MapUtils.Get(loader.components, targetId));
                        ((Saxon.Eej.ee.schema.UserComplexType)(source)).setOpenContentWildcard(wildcard, "suffix", true);
                        Saxon.Eej.ee.schema.fsa.FiniteStateMachine fsm = ((Saxon.Eej.ee.schema.UserComplexType)(source)).getFiniteStateMachine();
                        if ((fsm != null)) {
                            fsm.setOpenContentWildcard(wildcard, false);
                        }
                        break ;
                    }
                  case FIXUP_COMPLEX_TYPE_OPEN_CONTENT_INTERLEAVE:
                    {
                        Saxon.Eej.ee.schema.Wildcard wildcard = (Saxon.Eej.ee.schema.Wildcard)(Saxon.Impl.Helpers.MapUtils.Get(loader.components, targetId));
                        ((Saxon.Eej.ee.schema.UserComplexType)(source)).setOpenContentWildcard(wildcard, "interleave", true);
                        Saxon.Eej.ee.schema.fsa.FiniteStateMachine fsm = ((Saxon.Eej.ee.schema.UserComplexType)(source)).getFiniteStateMachine();
                        if ((fsm != null)) {
                            fsm.setOpenContentWildcard(wildcard, true);
                        }
                        break ;
                    }
                  case FIXUP_SIMPLE_TYPE_ITEM_TYPE:
                    {
                        ((Saxon.Eej.ee.schema.UserListType)(source)).setItemTypeReference(loader.makeTypeReference(targetId));
                        break ;
                    }
                  case FIXUP_SIMPLE_TYPE_MEMBER_TYPE:
                    {
                        ((Saxon.Eej.ee.schema.UserUnionType)(source)).addMemberTypeReference(loader.makeTypeReference(targetId));
                        break ;
                    }
                  case FIXUP_SIMPLE_TYPE_BASE_TYPE:
                    {
                        ((Saxon.Eej.ee.schema.UserSimpleType)(source)).setBaseTypeReference(loader.makeTypeReference(targetId));
                        break ;
                    }
                  case FIXUP_ATTRIBUTE_USE_DECLARATION:
                    {
                        Saxon.Hej.type.SchemaComponent target = Saxon.Impl.Helpers.MapUtils.Get(loader.components, targetId);
                        ((Saxon.Eej.ee.schema.AttributeUse)(source)).setTarget(target);
                        ((Saxon.Eej.ee.schema.AttributeUse)(source)).setTargetComponentName(((Saxon.Eej.ee.schema.AttributeDecl)(target)).getComponentName());
                        break ;
                    }
                  case FIXUP_ATTRIBUTE_WILDCARD_REF:
                    {
                        Saxon.Hej.type.SchemaComponent target = Saxon.Impl.Helpers.MapUtils.Get(loader.components, targetId);
                        ((Saxon.Eej.ee.schema.AttributeWildcard)(source)).setWildcard((Saxon.Eej.ee.schema.Wildcard)(target));
                        break ;
                    }
                  case FIXUP_ELEMENT_PARTICLE_REF:
                    {
                        Saxon.Hej.type.SchemaComponent target = Saxon.Impl.Helpers.MapUtils.Get(loader.components, targetId);
                        ((Saxon.Eej.ee.schema.ElementParticle)(source)).setTarget(target);
                        ((Saxon.Eej.ee.schema.ElementParticle)(source)).setTargetComponentName(((Saxon.Eej.ee.schema.ElementDecl)(target)).getComponentName());
                        break ;
                    }
                  case FIXUP_ELEMENT_WILDCARD_REF:
                    {
                        Saxon.Hej.type.SchemaComponent target = Saxon.Impl.Helpers.MapUtils.Get(loader.components, targetId);
                        ((Saxon.Eej.ee.schema.ElementWildcard)(source)).setWildcard((Saxon.Eej.ee.schema.Wildcard)(target));
                        break ;
                    }
                  case FIXUP_MODEL_GROUP_PARTICLE_REF:
                    {
                        Saxon.Hej.type.SchemaComponent target = Saxon.Impl.Helpers.MapUtils.Get(loader.components, targetId);
                        ((Saxon.Eej.ee.schema.ModelGroupParticle)(source)).setTarget(target);
                        ((Saxon.Eej.ee.schema.ModelGroupParticle)(source)).setTargetComponentName(((Saxon.Eej.ee.schema.ModelGroupDefinition)(target)).getModelGroupName());
                        break ;
                    }
                  case FIXUP_KEYREF_KEY:
                    {
                        Saxon.Eej.ee.schema.IdentityConstraint target = (Saxon.Eej.ee.schema.IdentityConstraint)(Saxon.Impl.Helpers.MapUtils.Get(loader.components, targetId));
                        ((Saxon.Eej.ee.schema.KeyRef)(source)).setTargetKey(target);
                        ((Saxon.Eej.ee.schema.KeyRef)(source)).setTargetComponentName(target.getConstraintName());
                        break ;
                    }
                  case FIXUP_IDENTITY_CONSTRAINT_REF:
                    {
                        Saxon.Eej.ee.schema.IdentityConstraint target = (Saxon.Eej.ee.schema.IdentityConstraint)(Saxon.Impl.Helpers.MapUtils.Get(loader.components, targetId));
                        ((Saxon.Eej.ee.schema.IdentityConstraintReference)(source)).setTarget(target);
                        break ;
                    }
                }
            }
        
        }
        private Saxon.Eej.ee.schema.TypeReference makeTypeReference(string targetId) {
            if (targetId.StartsWith("#")) {
                Saxon.Hej.type.SchemaType builtInType = Saxon.Hej.type.BuiltInType.getSchemaTypeByLocalName(targetId.Substring(1));
                Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(builtInType.getFingerprint(), config, null);
                @ref.setTarget(builtInType);
                return @ref;
            } else {
                Saxon.Hej.type.SchemaType userType = (Saxon.Hej.type.SchemaType)(Saxon.Impl.Helpers.MapUtils.Get(components, targetId));
                int fp = userType.getFingerprint();
                Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(fp, config, null);
                @ref.setTarget(userType);
                return @ref;
            }
        }
    
    }
}