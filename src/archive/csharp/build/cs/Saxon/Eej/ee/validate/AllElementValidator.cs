using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class AllElementValidator : Saxon.Eej.ee.validate.AttributeValidator {
        private readonly Saxon.Eej.ee.schema.UserComplexType type;
        private readonly Saxon.Eej.ee.schema.AllModelGroup allGroup;
        private readonly bool mixed;
        private bool empty = true;
        private readonly bool emptiable;
        private readonly int[] counters;
        private bool ignoreIgnorable;
        private bool inOpenSuffix;
        internal AllElementValidator(Saxon.Eej.ee.schema.UserComplexType type, Saxon.Hej.@event.Receiver next) : base (next) {
            this.type = type;
            mixed = type.isMixedContent();
            setAnnotation(type);
            setAttributeGroup(type.getCombinedAttributeGroup());
            Saxon.Eej.ee.schema.ModelGroupParticle allParticle = type.getAllCompositor();
            if ((allParticle == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("Calling xs:all validator, cannot find xs:all compositor");
            }
            allGroup = (Saxon.Eej.ee.schema.AllModelGroup)(allParticle.getGroup());
            emptiable = allParticle.isEmptiable();
            counters = new int[allGroup.getMinima().Length];
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            base.setPipelineConfiguration(pipe);
            ignoreIgnorable = (getConfiguration().getParseOptions().getSpaceStrippingRule() != Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance());
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            return type;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType suppliedType, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.om.NamePool pool = config.getNamePool();
            Saxon.Hej.type.SchemaType annotation = null;
            int key = elemName.obtainFingerprint(pool);
            empty = false;
            Saxon.Hej.z.IntToIntMap counterMap = allGroup.getCounterMap();
            int counter = counterMap.get(key);
            bool foundElementParticle = (counter != counterMap.getDefaultValue());
            int count;
            int max;
            bool exhausted = !(foundElementParticle);
            if (foundElementParticle) {
                count = ++counters[counter];
                max = allGroup.getMaxima()[counter];
                exhausted = ((max != -1) && (count>max));
            }
            if (exhausted) {
                System.Collections.Generic.IList<Saxon.Eej.ee.schema.ElementWildcard> wildcards = allGroup.getWildcards();
                int w = 0;
                foreach (Saxon.Eej.ee.schema.ElementWildcard wildcard in wildcards) {
                    if (wildcard.getWildcard().matches(elemName, true, config, type)) {
                        if (inOpenSuffix) {
                            reportNotAllowedInSuffix(elemName, location);
                        }
                        int found = ++counters[w];
                        int wmax = wildcard.getMaxOccurs();
                        if (((wmax != -1) && (found>wmax))) {
                            reportExcess(elemName, location, wmax);
                            counters[w] = System.Int32.MinValue;
                        }
                        annotation = processWildcardTerm(wildcard.getWildcard(), elemName, location);
                        break ;
                    }
                    w++;
                }
                if ((annotation == null)) {
                    Saxon.Eej.ee.schema.Wildcard openWildcard = type.getOpenContentWildcard();
                    if ((openWildcard != null)) {
                        if (openWildcard.matches(elemName, true, config, type)) {
                            annotation = processWildcardTerm(openWildcard, elemName, location);
                            inOpenSuffix = (type.getOpenContentMode() == Saxon.Hej.type.OpenContentVariety.SUFFIX);
                        }
                    }
                }
                if ((annotation == null)) {
                    if (foundElementParticle) {
                        reportExcess(elemName, location, allGroup.getMaxima()[counter]);
                        counters[counter] = System.Int32.MinValue;
                    } else {
                        string reason = "";
                        if ((wildcards.Count == 1)) {
                            System.Collections.Generic.IEnumerator<Saxon.Eej.ee.schema.ElementWildcard> it = wildcards.GetEnumerator();
                            Saxon.Eej.ee.schema.ElementWildcard wildcard = it.Current;
                            reason = (". "+wildcard.getWildcard().reasonForNonMatch(elemName, true, config, type));
                        }
                        reportNotAllowed(elemName, location, reason);
                    }
                    childValidator = new Saxon.Eej.ee.validate.AnyTypeValidator(getNextReceiver());
                    annotation = Saxon.Hej.type.AnyType.getInstance();
                }
            } else {
                if (inOpenSuffix) {
                    reportNotAllowedInSuffix(elemName, location);
                }
                Saxon.Eej.ee.schema.ElementDecl contextDeterminedDeclaration = (Saxon.Eej.ee.schema.ElementDecl)(allGroup.getDeclarationMap().get(key));
                makeChildValidator(contextDeterminedDeclaration, elemName.getStructuredQName(), location, Saxon.Hej.lib.Validation.DEFAULT);
                annotation = childValidator.getAnnotation();
            }
            base.startElement(elemName, annotation, attributes, namespaces, location, properties);
        }
        private void reportExcess(Saxon.Hej.om.NodeName elemName, Saxon.Hej.s9api.Location locationId, int maxAllowed) {
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("Element "+Saxon.Hej.trans.Err.wrap(elemName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" exceeds limits defined in xs:all group: maxOccurs is ")+maxAllowed));
            ve.setConstraintReference(1, "cvc-model-group", "1");
            ve.setSchemaType(type);
            reportValidationError(ve, true, locationId);
        }
        private void reportNotAllowed(Saxon.Hej.om.NodeName elemName, Saxon.Hej.s9api.Location locationId, string reason) {
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("Element "+Saxon.Hej.trans.Err.wrap(elemName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" is not allowed in the content of the xs:all group")+reason));
            ve.setConstraintReference(1, "cvc-complex-content", "1");
            ve.setSchemaType(type);
            reportValidationError(ve, true, locationId);
        }
        private void reportNotAllowedInSuffix(Saxon.Hej.om.NodeName elemName, Saxon.Hej.s9api.Location locationId) {
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure((("Element "+Saxon.Hej.trans.Err.wrap(elemName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" cannot appear in the content of the xs:all group after an element that matches open content with mode=suffix"));
            ve.setConstraintReference(1, "cvc-complex-content", "2");
            ve.setSchemaType(type);
            reportValidationError(ve, true, locationId);
        }
        protected internal override Saxon.Eej.ee.validate.ContentValidator getChildValidator() {
            return childValidator;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            checkNoCharactersWhenNil(locationId);
            if (mixed) {
                nextReceiver.characters(chars, locationId, properties);
                empty = false;
            } else {
                if (Saxon.Hej.value.Whitespace.isAllWhite(chars)) {
                    if (!(ignoreIgnorable)) {
                        nextReceiver.characters(chars, locationId, properties);
                    }
                } else {
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure((("The content model for the type "+type.getDescription())+" does not allow character content"));
                    ve.setConstraintReference(1, "cvc-complex-type", "2.3");
                    ve.setSchemaType(type);
                    reportValidationError(ve, true, locationId);
                }
            }
        }
        public override void endElement() {
            if ((((empty && emptiable)) || isNilled())) {
            } else {
                System.Collections.Generic.IList<Saxon.Eej.ee.schema.ElementWildcard> wildcards = allGroup.getWildcards();
                int[] minima = allGroup.getMinima();
                for (int i = wildcards.Count; (i<counters.Length); i++) {
                    int found = counters[i];
                    if (((found<minima[i]) && (found>=0))) {
                        string name = allGroup.getNameCodes()[i].getDisplayName();
                        string container;
                        if ((getContainingElement() == null)) {
                            if (type.isAnonymousType()) {
                                Saxon.Hej.om.StructuredQName nc = type.getContainingDeclarationName();
                                container = ("element "+nc.getDisplayName());
                            } else {
                                container = ("element of type "+type.getDescription());
                            }
                        } else {
                            container = getContainingElementName();
                        }
                        string msg = ((((("In content of "+container)+": ")+"element ")+Saxon.Hej.trans.Err.wrap(name, Saxon.Hej.trans.Err.ELEMENT))+(((found == 0) ? " is not present" : (((((" has "+found)+" occurrence")+(((found == 1) ? "" : "s")))+" but minOccurs is ")+allGroup.getMinima()[i]))));
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(msg);
                        ve.setConstraintReference(1, "cvc-model-group", "3");
                        ve.setSchemaType(type);
                        reportValidationError(ve, true, getContainingElementLocationId());
                    }
                }
                int index = 0;
                foreach (Saxon.Eej.ee.schema.ElementWildcard wildcard in wildcards) {
                    int found = counters[index++];
                    int min = wildcard.getMinOccurs();
                    if (((found<min) && (found>=0))) {
                        string msg = ((((((("In content of "+Saxon.Hej.trans.Err.wrap(getContainingElement().getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+": ")+(((found == 0) ? "No" : ("Only "+found))))+" child element")+(((found == 1) ? "" : "s")))+" found to match element wildcard with minOccurs=")+min);
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(msg);
                        ve.setConstraintReference(1, "cvc-model-group", "3");
                        ve.setSchemaType(type);
                        reportValidationError(ve, true, getContainingElementLocationId());
                    }
                }
            }
            nextReceiver.endElement();
        }
    
    }
}