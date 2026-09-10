using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public abstract class AttributeValidator : Saxon.Eej.ee.validate.ContentValidator {
        private Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.AttributeUse> declaredAttributes;
        private Saxon.Hej.z.IntToIntMap attributeCounterMap;
        private bool[] found;
        private Saxon.Eej.ee.schema.AttributeWildcard attributeWildcard = null;
        private int idAttributes = 0;
        internal Saxon.Hej.type.SimpleType latestAttributeType = null;
        private Saxon.Hej.om.AttributeMap previousAttributeMap;
        private Saxon.Hej.om.AttributeMap previousValidatedAttributes;
        private System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> previousNamespaceBindings;
        public AttributeValidator(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public void setAttributeGroup(Saxon.Eej.ee.schema.AttributeGroupDecl attributeGroup) {
            if ((attributeGroup != null)) {
                attributeWildcard = attributeGroup.getAttributeWildcard(null);
                declaredAttributes = attributeGroup.getDeclaredAttributes();
                attributeCounterMap = attributeGroup.getCounterMap();
                found = new bool[attributeCounterMap.size()];
            }
        }
        public override Saxon.Hej.om.AttributeMap validateAttributes(Saxon.Hej.om.AttributeMap @in, System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> additionalNamespaces) {
            if ((@in == previousAttributeMap)) {
                Saxon.Impl.Helpers.ListUtils.AddAll(additionalNamespaces, previousNamespaceBindings);
                return previousValidatedAttributes;
            }
            Saxon.Hej.@event.PipelineConfiguration pipe = getPipelineConfiguration();
            Saxon.Hej.om.AttributeMap result = Saxon.Hej.om.EmptyAttributeMap.getInstance();
            foreach (Saxon.Hej.om.AttributeInfo att in @in) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                Saxon.Hej.s9api.Location location = att.getLocation();
                Saxon.Hej.type.SimpleType type = att.getType();
                int properties = att.getProperties();
                int fingerprint = att.getNodeName().obtainFingerprint(getNamePool());
                if ((fingerprint == Saxon.Hej.om.StandardNames.XSI_NIL)) {
                    if ((getNillability() == Saxon.Eej.ee.validate.ContentValidator.Nillability.DISALLOWED)) {
                        reportNotNillable(attName, location);
                    }
                    string b = Saxon.Hej.value.Whitespace.trim(value);
                    if (((getNillability() != Saxon.Eej.ee.validate.ContentValidator.Nillability.IGNORED) && ((b.Equals("true") || b.Equals("1"))))) {
                        nilled = true;
                    }
                }
                Saxon.Hej.type.SimpleType annotation = type;
                Saxon.Eej.ee.schema.AttributeUse use = ((declaredAttributes == null) ? null : declaredAttributes.get(fingerprint));
                Saxon.Eej.ee.schema.AttributeDecl decl = null;
                if (((use != null) && !(use.isProhibited()))) {
                    decl = (Saxon.Eej.ee.schema.AttributeDecl)(use.getTarget());
                    int counter = attributeCounterMap.get(fingerprint);
                    if ((counter != attributeCounterMap.getDefaultValue())) {
                        found[counter] = true;
                    }
                } else {
                    bool implicitlyAllowed = false;
                    Saxon.Hej.om.NamespaceUri uri = attName.getNamespaceUri();
                    if ((uri.Equals(Saxon.Hej.om.NamespaceUri.SCHEMA_INSTANCE) && (((((fingerprint == Saxon.Hej.om.StandardNames.XSI_TYPE) || (fingerprint == Saxon.Hej.om.StandardNames.XSI_NIL)) || (fingerprint == Saxon.Hej.om.StandardNames.XSI_SCHEMA_LOCATION)) || (fingerprint == Saxon.Hej.om.StandardNames.XSI_NO_NAMESPACE_SCHEMA_LOCATION))))) {
                        implicitlyAllowed = true;
                    }
                    if (!(implicitlyAllowed)) {
                        if ((attributeWildcard != null)) {
                            if (!(attributeWildcard.getWildcard().matches(attName, false, getConfiguration(), null))) {
                                attributeDoesNotMatchWildcard(attName, location);
                            }
                            string process = attributeWildcard.getWildcard().getProcessContents();
                            if (process.Equals("skip")) {
                            } else {
                                decl = (Saxon.Eej.ee.schema.AttributeDecl)(getConfiguration().getAttributeDeclaration(attName.getStructuredQName()));
                                if (((decl == null) && process.Equals("strict"))) {
                                    noAttributeDeclarationAvailable(attName, location);
                                }
                            }
                        } else {
                            attributeIsNotAllowed(attName, location);
                        }
                    }
                }
                if ((decl != null)) {
                    Saxon.Hej.type.SimpleType declaredType = decl.getSimpleType();
                    latestAttributeType = declaredType;
                    if ((declaredType.getBuiltInBaseType().getFingerprint() == Saxon.Hej.om.StandardNames.XS_ID)) {
                        idAttributes++;
                        if (((idAttributes == 2) && (getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10))) {
                            multipleIdAttributes(attName, location);
                        }
                    }
                    if ((declaredType.getFingerprint() == Saxon.Hej.om.StandardNames.XS_NOTATION)) {
                        notationIsAbstract(location);
                    }
                    Saxon.Hej.type.ValidationFailure err = declaredType.validateContent(Saxon.Hej.str.StringView.of(value).tidy(), getNamespaceResolver(), getConfiguration().getConversionRules());
                    if ((err != null)) {
                        err.setConstraintReference(2, "cvc-complex-type", "3");
                        Saxon.Hej.s9api.Location attLocation = makeAttributeLocation(attName, location);
                        reportValidationError(err, false, attLocation);
                    } else {
                        if ((declaredType is Saxon.Eej.ee.schema.UserAtomicType)) {
                            value = ((Saxon.Eej.ee.schema.UserAtomicType)(declaredType)).getSharedInstance(value);
                        }
                        annotation = declaredType;
                        bool matchesFixed = true;
                        if (((use != null) && (use.getFixedValueConstraint() != null))) {
                            matchesFixed = use.getFixedValueConstraint().testFixedValue(getConfiguration(), Saxon.Hej.str.StringView.of(value).tidy(), declaredType, getNamespaceResolver());
                        }
                        if ((decl.getFixedValueConstraint() != null)) {
                            matchesFixed = (matchesFixed && decl.getFixedValueConstraint().testFixedValue(getConfiguration(), Saxon.Hej.str.StringView.of(value).tidy(), declaredType, getNamespaceResolver()));
                        }
                        if (!(matchesFixed)) {
                            mismatchWithFixedValue(attName, value, location, use, decl);
                        }
                    }
                }
                if (((((use != null) && use.isInheritable())) || (((use == null) && (decl != null)) && decl.isInheritable()))) {
                    getAttributeInheritor().notifyInheritableAttribute(attName, value, location, properties);
                }
                result = result.put(new Saxon.Hej.om.AttributeInfo(attName, annotation, value, location, properties));
            }
            if ((declaredAttributes != null)) {
                Saxon.Hej.z.IntIterator iter2 = declaredAttributes.keyIterator();
                while (iter2.hasNext()) {
                    int key = iter2.next();
                    int counter = attributeCounterMap.get(key);
                    if (!(found[counter])) {
                        Saxon.Eej.ee.schema.AttributeUse use = declaredAttributes.get(key);
                        if (use.isRequired()) {
                            string name = use.getTargetComponentName().getEQName();
                            string message = ((("Required attribute "+Saxon.Hej.trans.Err.wrap(name, Saxon.Hej.trans.Err.ATTRIBUTE))+" is missing on ")+getContainingElementName());
                            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                            ve.setConstraintReference(1, "cvc-complex-type", "4");
                            ve.setSchemaType(getSchemaType());
                            reportValidationError(ve, true, getContainingElementLocationId());
                        } else {
                            if (pipe.getParseOptions().isExpandAttributeDefaults()) {
                                Saxon.Hej.str.UnicodeString value = use.getUnderlyingDefaultValue();
                                if ((value == null)) {
                                    Saxon.Hej.om.AtomicSequence @fixed = use.getUnderlyingFixedValue();
                                    value = ((@fixed == null) ? null : @fixed.getUnicodeStringValue());
                                }
                                if ((value != null)) {
                                    if (use.getAttributeDeclaration().getSimpleType().isNamespaceSensitive()) {
                                        checkNamespaceSensitiveDefaultValue(use, value);
                                    }
                                    Saxon.Hej.om.NamePool pool = getNamePool();
                                    Saxon.Hej.om.NamespaceUri uri = use.getTargetComponentName().getNamespaceUri();
                                    string prefix = null;
                                    if (uri.isEmpty()) {
                                        prefix = "";
                                    } else {
                                        Saxon.Hej.om.NamespaceResolver resolver = getNamespaceResolver();
                                        System.Collections.Generic.IEnumerator<string> iter = resolver.iteratePrefixes();
                                        while (iter.MoveNext() /*UNSAFE CONVERSION*/) {
                                            string p = iter.Current;
                                            if ((!(p.Length==0) && uri.Equals(resolver.getURIForPrefix(p, false)))) {
                                                prefix = p;
                                                break ;
                                            }
                                        }
                                        if ((prefix == null)) {
                                            prefix = use.getTargetComponentName().getPrefix();
                                            if (prefix.Length==0) {
                                                prefix = pool.suggestPrefixForURI(uri);
                                            }
                                            if ((prefix == null)) {
                                                prefix = "ns";
                                            }
                                            while (true) {
                                                Saxon.Hej.om.NamespaceUri u = getNamespaceResolver().getURIForPrefix(prefix, false);
                                                if (((u == null) || u.Equals(uri))) {
                                                    break ;
                                                }
                                                prefix = (prefix+"1");
                                            }
                                            additionalNamespaces.Add(new Saxon.Hej.om.NamespaceBinding(prefix, uri));
                                        }
                                    }
                                    result = result.put(new Saxon.Hej.om.AttributeInfo(new Saxon.Hej.om.CodedName(key, prefix, getNamePool()), use.getAttributeDeclaration().getSimpleType(), value.ToString(), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.DEFAULTED_VALUE));
                                }
                            }
                        }
                    }
                }
            }
            previousAttributeMap = @in;
            previousValidatedAttributes = result;
            previousNamespaceBindings = additionalNamespaces;
            return result;
        }
        private void checkNamespaceSensitiveDefaultValue(Saxon.Eej.ee.schema.AttributeUse use, Saxon.Hej.str.UnicodeString value) {
            Saxon.Hej.type.ValidationFailure err = use.getAttributeDeclaration().getSimpleType().validateContent(value, getNamespaceResolver(), getConfiguration().getConversionRules());
            if ((err != null)) {
                string message = ("The namespace-sensitive default value is invalid in the "+"namespace context where it is being used");
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                ve.setSchemaType(getSchemaType());
                reportValidationError(ve, true, getContainingElementLocationId());
            }
        }
        private void mismatchWithFixedValue(Saxon.Hej.om.NodeName attName, string value, Saxon.Hej.s9api.Location location, Saxon.Eej.ee.schema.AttributeUse use, Saxon.Eej.ee.schema.AttributeDecl decl) {
            Saxon.Hej.om.AtomicSequence @fixed;
            if ((use != null)) {
                @fixed = use.getUnderlyingFixedValue();
            } else {
                @fixed = decl.getFixedValue();
            }
            string message = ((((("The actual value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" of attribute ")+Saxon.Hej.trans.Err.wrap(attName.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" does not match the required fixed value ")+(((@fixed is Saxon.Hej.value.AtomicValue) ? Saxon.Hej.trans.Err.wrap(@fixed.getUnicodeStringValue(), Saxon.Hej.trans.Err.VALUE) : "")));
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-au", "0");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, false, makeAttributeLocation(attName, location));
        }
        private void notationIsAbstract(Saxon.Hej.s9api.Location location) {
            string message = "Cannot validate against type xs:NOTATION (only against a subtype defining an enumeration of values)";
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(1, "enumeration-required-notation", "0");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, false, location);
        }
        private void multipleIdAttributes(Saxon.Hej.om.NodeName attName, Saxon.Hej.s9api.Location location) {
            string message = "Element has more than one ID attribute";
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-complex-type", "5");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, false, makeAttributeLocation(attName, location));
        }
        private void attributeIsNotAllowed(Saxon.Hej.om.NodeName attName, Saxon.Hej.s9api.Location location) {
            string possibleNamespaceMessage = "";
            if ((declaredAttributes != null)) {
                for (System.Collections.Generic.IEnumerator<Saxon.Eej.ee.schema.AttributeUse> ii = declaredAttributes.valueIterator(); ii.MoveNext() /*UNSAFE CONVERSION*/; ) {
                    Saxon.Eej.ee.schema.AttributeUse au = ii.Current;
                    if (au.getTargetComponentName().getLocalPart().Equals(attName.getLocalPart())) {
                        Saxon.Hej.om.NamespaceUri ns = au.getTargetComponentName().getNamespaceUri();
                        if (ns.isEmpty()) {
                            possibleNamespaceMessage = " (it would be allowed in no namespace)";
                        } else {
                            possibleNamespaceMessage = ((" (it would be allowed in namespace "+ns)+")");
                        }
                        break ;
                    }
                }
            }
            string message = (((("Attribute "+Saxon.Hej.trans.Err.wrap(attName.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" is not allowed on ")+getContainingElementName())+possibleNamespaceMessage);
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-complex-type", "3");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, true, makeAttributeLocation(attName, location));
        }
        private void reportNotNillable(Saxon.Hej.om.NodeName attName, Saxon.Hej.s9api.Location location) {
            string message = "The element is not nillable, so xsi:nil must not be present";
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-elt", "3.1");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, true, makeAttributeLocation(attName, location));
        }
        private void noAttributeDeclarationAvailable(Saxon.Hej.om.NodeName attName, Saxon.Hej.s9api.Location location) {
            string message = (("The <xs:anyAttribute> that matches attribute "+Saxon.Hej.trans.Err.wrap(attName.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" requires strict validation, but no attribute declaration is available");
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-wildcard", "2");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, false, makeAttributeLocation(attName, location));
        }
        private void attributeDoesNotMatchWildcard(Saxon.Hej.om.NodeName attName, Saxon.Hej.s9api.Location location) {
            string reason = attributeWildcard.getWildcard().reasonForNonMatch(attName, false, getConfiguration(), null);
            string message = ((((("Attribute "+Saxon.Hej.trans.Err.wrap(attName.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" is not allowed on ")+getContainingElementName())+". ")+reason);
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-complex-type", "3.2.1");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, false, makeAttributeLocation(attName, location));
        }
        private Saxon.Hej.tree.AttributeLocation makeAttributeLocation(Saxon.Hej.om.NodeName attName, Saxon.Hej.s9api.Location location) {
            return new Saxon.Hej.tree.AttributeLocation(getContainingElement(), attName.getStructuredQName(), location);
        }
        public Saxon.Hej.type.SimpleType getMostRecentAttributeType() {
            return latestAttributeType;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            idAttributes = 0;
            checkNoChildrenWhenNil(location);
            base.startElement(elemName, type, attributes, namespaces, location, properties);
            if ((locallyInvalid && (pendingMessages != null))) {
                foreach (string pendingMessage in pendingMessages) {
                    nextReceiver.comment(Saxon.Hej.str.StringView.of(pendingMessage), Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                }
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            checkNoCharactersWhenNil(locationId);
            nextReceiver.characters(chars, locationId, properties);
        }
        protected internal Saxon.Hej.type.SchemaType processWildcardTerm(Saxon.Eej.ee.schema.Wildcard card, Saxon.Hej.om.NodeName elementName, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.type.SchemaType annotation;
            Saxon.Hej.om.StructuredQName elementQName = elementName.getStructuredQName();
            int fingerprint = elementName.getFingerprint();
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.@event.PipelineConfiguration pipe = getPipelineConfiguration();
            string mode = card.getProcessContents();
            Saxon.Hej.type.SchemaType xsiType = getValidationContext().getXSIType();
            switch (mode) {
              case "strict":
                {
                    Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(getConfiguration().getElementDeclaration(fingerprint));
                    if ((decl == null)) {
                        if ((xsiType == null)) {
                            string message = ("No element declaration found for element "+Saxon.Hej.trans.Err.wrap(elementName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT));
                            if ((getContainingElement() != null)) {
                                message = ((("In content of "+getContainingElementName())+": ")+message);
                            }
                            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                            ve.setConstraintReference(1, "cvc-wildcard", "2");
                            ve.setSchemaType(getSchemaType());
                            reportValidationError(ve, true, locationId);
                            annotation = Saxon.Hej.type.AnyType.getInstance();
                            return annotation;
                        } else {
                            makeChildValidator(null, elementQName, locationId, Saxon.Hej.lib.Validation.DEFAULT);
                        }
                    } else {
                        makeChildValidator(decl, elementQName, locationId, Saxon.Hej.lib.Validation.DEFAULT);
                    }
                    childValidator.setContainingElement(elementName.getStructuredQName(), locationId);
                    annotation = childValidator.getAnnotation();
                    break ;
                }
              case "lax":
                {
                    Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(getConfiguration().getElementDeclaration(fingerprint));
                    if ((decl == null)) {
                        if ((xsiType != null)) {
                            System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> failures = new System.Collections.Generic.List<Saxon.Hej.type.ValidationFailure>(1);
                            childValidator = makeValidator(null, elementQName, locationId, getValidationContext(), xsiType, Saxon.Hej.lib.Validation.DEFAULT, pipe, getNextReceiver(), failures);
                            if (!(failures.Count==0)) {
                                reportValidationError(failures[0], true, locationId);
                                childValidator = new Saxon.Eej.ee.validate.SkipValidator(getNextReceiver());
                                annotation = Saxon.Hej.type.AnyType.getInstance();
                            } else {
                                annotation = childValidator.getAnnotation();
                            }
                        } else {
                            childValidator = new Saxon.Eej.ee.validate.LaxValidator(getNextReceiver());
                            childValidator.setContainingElement(elementQName, locationId);
                            annotation = Saxon.Hej.type.AnyType.getInstance();
                        }
                    } else {
                        makeChildValidator(decl, elementQName, locationId, Saxon.Hej.lib.Validation.DEFAULT);
                        childValidator.setContainingElement(elementQName, locationId);
                        annotation = childValidator.getAnnotation();
                    }
                    break ;
                }
              default:
                annotation = Saxon.Hej.type.AnyType.getInstance();
                childValidator = new Saxon.Eej.ee.validate.SkipValidator(getNextReceiver());
                childValidator.setValidationContext(getValidationContext());
                break ;
            }
            if (!(((childValidator is Saxon.Eej.ee.validate.SkipValidator)))) {
                Saxon.Eej.ee.schema.UserComplexType containingType = (Saxon.Eej.ee.schema.UserComplexType)(getSchemaType());
                if ((containingType.getLanguageVersion() != Saxon.Hej.Configuration.XSD10)) {
                    Saxon.Hej.type.SchemaType contextDeterminedType = containingType.getContextDeterminedTypeForElement(elementQName);
                    Saxon.Hej.type.SchemaType governingType = childValidator.getSchemaType();
                    if (((contextDeterminedType != null) && (governingType != null))) {
                        try {
                            config.checkTypeDerivationIsOK(governingType, contextDeterminedType, 0);
                        } catch (Saxon.Hej.type.SchemaException err) {
                            string message = (((((((((("The governing type ("+governingType.getDescription())+") of the ")+Saxon.Hej.trans.Err.wrap(elementName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" element which matches against a wildcard is not substitutable for the ")+"context-determined type of the element particle named ")+elementName.getDisplayName())+" within the content model of type ")+containingType.getDescription())+". ")+err.Message);
                            Saxon.Hej.type.ValidationFailure verr = new Saxon.Hej.type.ValidationFailure(message);
                            verr.setConstraintReference(1, "cvc-complex-type", "5");
                            verr.setSchemaType(getSchemaType());
                            reportValidationError(verr, true, locationId);
                        }
                    }
                }
            }
            return annotation;
        }
    
    }
}