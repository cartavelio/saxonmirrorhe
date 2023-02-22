using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class ComplexContentValidator : Saxon.Eej.ee.validate.AttributeValidator {
        private Saxon.Eej.ee.schema.UserComplexType type;
        private bool mixed;
        private Saxon.Hej.str.UnicodeString fixedValue = null;
        private Saxon.Hej.str.UnicodeString defaultValue = null;
        private Saxon.Eej.ee.schema.fsa.State state;
        private int previousElement = -1;
        private bool ignoreIgnorable;
        private Saxon.Hej.str.UnicodeBuilder buffer;
        private bool foundChildren = false;
        public ComplexContentValidator(Saxon.Eej.ee.schema.ElementDecl declaration, Saxon.Eej.ee.schema.UserComplexType type, Saxon.Hej.@event.Receiver next) : base (next) {
            setSchemaType(type);
            if ((declaration != null)) {
                defaultValue = declaration.getDefaultValueLexicalForm();
                Saxon.Hej.om.AtomicSequence @fixed = declaration.getFixedValue();
                if ((@fixed != null)) {
                    fixedValue = @fixed.getUnicodeStringValue();
                }
            }
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            base.setPipelineConfiguration(pipe);
            ignoreIgnorable = (getConfiguration().getParseOptions().getSpaceStrippingRule() != Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance());
        }
        public void setSchemaType(Saxon.Eej.ee.schema.UserComplexType type) {
            this.type = type;
            mixed = type.isMixedContent();
            state = type.getInitialState();
            setAnnotation(type);
            setAttributeGroup(type.getCombinedAttributeGroup());
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            return type;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType elemType, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (locallyInvalid) {
                recover(elemName, attributes, namespaces, location, properties);
                return ;
            }
            Saxon.Hej.type.SchemaType annotation;
            if ((fixedValue != null)) {
                string message = (((("Element "+Saxon.Hej.trans.Err.wrap(elemName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" cannot appear here, because the containing ")+getContainingElementName())+" element has fixed content");
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-elt", "5.2.2.1");
                ve.setSchemaType(type);
                reportValidationError(ve, true, location);
                locallyInvalid = true;
                recover(elemName, attributes, namespaces, location, properties);
                return ;
            }
            foundChildren = true;
            int fingerprint = elemName.obtainFingerprint(getNamePool());
            Saxon.Eej.ee.schema.fsa.Edge edge = state.getTransition(fingerprint, type);
            if ((edge == null)) {
                Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine = type.getFiniteStateMachine();
                Saxon.Eej.ee.schema.Wildcard openContentWildcard = machine.getOpenContentWildcard();
                if (((openContentWildcard != null) && openContentWildcard.matches(elemName, true, getConfiguration(), type))) {
                    if (machine.isOpenContentInterleaved()) {
                        childValidator = new Saxon.Eej.ee.validate.LaxValidator(getNextReceiver());
                        annotation = processWildcardTerm(openContentWildcard, elemName, location);
                        base.startElement(elemName, annotation, attributes, namespaces, location, properties);
                        return ;
                    } else {
                        if (state.isFinalState()) {
                            state = /*134*/Saxon.Eej.ee.schema.fsa.SuffixState.getInstance();
                            childValidator = new Saxon.Eej.ee.validate.LaxValidator(getNextReceiver());
                            annotation = processWildcardTerm(openContentWildcard, elemName, location);
                            base.startElement(elemName, annotation, attributes, namespaces, location, properties);
                            return ;
                        }
                    }
                }
                string message = supplyReasonForNonMatch(elemName);
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-complex-type", "2.4");
                ve.setSchemaType(type);
                reportValidationError(ve, true, location);
                locallyInvalid = true;
                recover(elemName, attributes, namespaces, location, properties);
                return ;
            }
            try {
                previousElement = fingerprint;
                state = edge.makeTransition(state);
            } catch (Saxon.Hej.type.ValidationException e) {
                string message = e.Message;
                if ((getContainingElement() != null)) {
                    message = ((("In content of "+getContainingElementName())+": ")+message);
                }
                Saxon.Hej.type.ValidationFailure ve = e.getValidationFailure();
                ve.setSchemaType(type);
                reportValidationError(ve, true, location);
                locallyInvalid = true;
                recover(elemName, attributes, namespaces, location, properties);
                return ;
            }
            Saxon.Eej.ee.schema.Term term = edge.getTerm();
            if ((term is Saxon.Eej.ee.schema.ElementDecl)) {
                Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(term);
                makeChildValidator(decl, elemName.getStructuredQName(), location, Saxon.Hej.lib.Validation.DEFAULT);
                childValidator.setContainingElement(decl.getComponentName(), location);
                annotation = childValidator.getAnnotation();
            } else {
                Saxon.Eej.ee.schema.Wildcard card = (Saxon.Eej.ee.schema.Wildcard)(term);
                annotation = processWildcardTerm(card, elemName, location);
                if ((childValidator == null)) {
                    childValidator = new Saxon.Eej.ee.validate.LaxValidator(getNextReceiver());
                }
            }
            base.startElement(elemName, annotation, attributes, namespaces, location, properties);
        }
        private string supplyReasonForNonMatch(Saxon.Hej.om.NodeName nodeName) {
            int fingerprint = ((nodeName == null) ? -1 : nodeName.obtainFingerprint(getNamePool()));
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(64);
            if ((getContainingElement() != null)) {
                fsb.append("In content of ").append(getContainingElementName()).append(": ");
            }
            bool explained = false;
            if ((nodeName == null)) {
                if ((previousElement == -1)) {
                    fsb.append("Empty content is not allowed. ");
                } else {
                    fsb.append("The content is incomplete. ");
                    try {
                        Saxon.Hej.om.StructuredQName omission;
                        Saxon.Hej.om.StructuredQName firstOmission = null;
                        System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName> possibleOmissions = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>();
                        foreach (Saxon.Eej.ee.schema.fsa.Edge e in state.getEdges()) {
                            if ((e.getParticle() is Saxon.Eej.ee.schema.ElementParticle)) {
                                omission = e.getParticle().getTargetComponentName();
                                int omittedfp = ((Saxon.Eej.ee.schema.ElementParticle)(e.getParticle())).getDeclaration().getFingerprint();
                                Saxon.Eej.ee.schema.fsa.AutomatonState s = state.getTransition(omittedfp, type).getTargetState();
                                if (s.isFinalState()) {
                                    possibleOmissions.Add(omission);
                                    if ((firstOmission == null)) {
                                        firstOmission = omission;
                                    }
                                }
                            }
                        }
                        if (!(possibleOmissions.Count==0)) {
                            if ((possibleOmissions.Count == 1)) {
                                fsb.append("It would be valid if followed by ").append(Saxon.Hej.trans.Err.wrap(firstOmission.getEQName(), Saxon.Hej.trans.Err.ELEMENT)).append(". ");
                            } else {
                                if ((possibleOmissions.Count>5)) {
                                    fsb.append("It would be valid if followed by another element, for example ").append(Saxon.Hej.trans.Err.wrap(firstOmission.getEQName(), Saxon.Hej.trans.Err.ELEMENT)).append(". ");
                                } else {
                                    fsb.append("It would be valid if followed by one of: ");
                                    bool first = true;
                                    foreach (Saxon.Hej.om.StructuredQName poss in possibleOmissions) {
                                        if (!(first)) {
                                            fsb.append(", ");
                                        }
                                        fsb.append(Saxon.Hej.trans.Err.wrap(poss.getEQName(), Saxon.Hej.trans.Err.ELEMENT));
                                        first = false;
                                    }
                                    fsb.append(". ");
                                }
                            }
                            explained = true;
                        }
                    } catch (Saxon.Hej.type.MissingComponentException e) {
                    }
                }
            } else {
                if ((state is Saxon.Eej.ee.schema.fsa.SuffixState)) {
                    fsb.append("The content model does not allow element ").append(Saxon.Hej.trans.Err.wrap(nodeName.getStructuredQName().getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)).append(" to appear after an element that matches the suffix open content wildcard. ");
                    explained = true;
                } else {
                    fsb.append("The content model does not allow element ");
                    fsb.append(Saxon.Hej.trans.Err.wrap(nodeName.getStructuredQName().getDisplayName(), Saxon.Hej.trans.Err.ELEMENT));
                    if ((previousElement == -1)) {
                        fsb.append(" to appear as the first child. ");
                    } else {
                        if ((previousElement == fingerprint)) {
                            try {
                                if (Saxon.Hej.value.Cardinality.allowsMany(type.getElementParticleCardinality(fingerprint, false))) {
                                    fsb.append(" to appear this many times. ");
                                } else {
                                    fsb.append(" to appear more than once. ");
                                }
                            } catch (Saxon.Hej.type.MissingComponentException e) {
                                fsb.append(" to appear more than once. ");
                            }
                            explained = true;
                        } else {
                            fsb.append(" to appear immediately after element ");
                            fsb.append(Saxon.Hej.trans.Err.wrap(getNamePool().getDisplayName(previousElement), Saxon.Hej.trans.Err.ELEMENT));
                            fsb.append(". ");
                        }
                    }
                    try {
                        if ((!(explained) && (type.getElementParticleType(fingerprint, false) != null))) {
                            Saxon.Hej.om.StructuredQName omission;
                            Saxon.Hej.om.StructuredQName firstOmission = null;
                            System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName> possibleOmissions = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>();
                            foreach (Saxon.Eej.ee.schema.fsa.Edge e in state.getEdges()) {
                                if ((e.getParticle() is Saxon.Eej.ee.schema.ElementParticle)) {
                                    omission = e.getParticle().getTargetComponentName();
                                    int omittedfp = ((Saxon.Eej.ee.schema.ElementParticle)(e.getParticle())).getDeclaration().getFingerprint();
                                    Saxon.Eej.ee.schema.fsa.AutomatonState s = state.getTransition(omittedfp, type).getTargetState();
                                    if ((s.getTransition(fingerprint, type) != null)) {
                                        possibleOmissions.Add(omission);
                                        if ((firstOmission == null)) {
                                            firstOmission = omission;
                                        }
                                    }
                                }
                            }
                            if (!(possibleOmissions.Count==0)) {
                                if ((possibleOmissions.Count == 1)) {
                                    fsb.append("It must be preceded by ").append(Saxon.Hej.trans.Err.wrap(firstOmission.getEQName(), Saxon.Hej.trans.Err.ELEMENT)).append(". ");
                                } else {
                                    if ((possibleOmissions.Count>5)) {
                                        fsb.append("It must be preceded by another element, for example ").append(Saxon.Hej.trans.Err.wrap(firstOmission.getEQName(), Saxon.Hej.trans.Err.ELEMENT)).append(". ");
                                    } else {
                                        fsb.append("It must be preceded by one of: ");
                                        bool first = true;
                                        foreach (Saxon.Hej.om.StructuredQName poss in possibleOmissions) {
                                            if (!(first)) {
                                                fsb.append(", ");
                                            }
                                            if (poss.hasURI(nodeName.getNamespaceUri())) {
                                                fsb.append(Saxon.Hej.trans.Err.wrap(poss.getLocalPart(), Saxon.Hej.trans.Err.ELEMENT));
                                            } else {
                                                fsb.append(Saxon.Hej.trans.Err.wrap(poss.getEQName(), Saxon.Hej.trans.Err.ELEMENT));
                                            }
                                            first = false;
                                        }
                                        fsb.append(". ");
                                    }
                                }
                                explained = true;
                            }
                        }
                    } catch (Saxon.Hej.type.MissingComponentException e) {
                    }
                    if (!(explained)) {
                        string foundLocal = nodeName.getLocalPart();
                        Saxon.Hej.om.NamespaceUri foundURI = nodeName.getNamespaceUri();
                        foreach (Saxon.Eej.ee.schema.fsa.Edge e in state.getEdges()) {
                            if ((e.getParticle() is Saxon.Eej.ee.schema.ElementParticle)) {
                                string local = ((Saxon.Eej.ee.schema.ElementParticle)(e.getParticle())).getName();
                                if (local.Equals(foundLocal)) {
                                    Saxon.Hej.om.NamespaceUri uri = ((Saxon.Eej.ee.schema.ElementParticle)(e.getParticle())).getNamespaceURI();
                                    fsb.append("The element is in ").append((foundURI.isEmpty() ? "no namespace" : ("namespace "+foundURI))).append(" but it should be in ").append((uri.isEmpty() ? "no namespace" : ("namespace "+uri))).append(". ");
                                    explained = true;
                                    break ;
                                }
                            }
                        }
                    }
                    if (!(explained)) {
                        Saxon.Eej.ee.schema.Wildcard openContentWildcard = type.getOpenContentWildcard();
                        if ((openContentWildcard != null)) {
                            string s = openContentWildcard.reasonForNonMatch(nodeName, true, getConfiguration(), type);
                            if ((s != null)) {
                                fsb.append("It does not match the open content wildcard: ");
                                fsb.append(s);
                            }
                        }
                    }
                }
            }
            if (!(explained)) {
                System.Collections.Generic.ISet<string> specificTransitions = new System.Collections.Generic.HashSet<string>();
                Saxon.Hej.om.StructuredQName expected = null;
                System.Collections.Generic.ISet<Saxon.Hej.om.NamespaceUri> specificURIs = new System.Collections.Generic.HashSet<Saxon.Hej.om.NamespaceUri>();
                Saxon.Hej.om.NamespaceUri @namespace = null;
                foreach (Saxon.Eej.ee.schema.fsa.Edge e in state.getEdges()) {
                    if ((e.getParticle() is Saxon.Eej.ee.schema.ElementParticle)) {
                        expected = e.getParticle().getTargetComponentName();
                        specificTransitions.Add(((Saxon.Eej.ee.schema.ElementParticle)(e.getParticle())).getDisplayName());
                        @namespace = ((Saxon.Eej.ee.schema.ElementParticle)(e.getParticle())).getNamespaceURI();
                        specificURIs.Add(@namespace);
                    }
                }
                System.Collections.Generic.ISet<Saxon.Eej.ee.schema.Wildcard> wildcards = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.Wildcard>();
                foreach (Saxon.Eej.ee.schema.fsa.Edge e in state.getEdges()) {
                    if ((e.getParticle() is Saxon.Eej.ee.schema.ElementWildcard)) {
                        wildcards.Add(((Saxon.Eej.ee.schema.ElementWildcard)(e.getParticle())).getWildcard());
                    }
                }
                if ((specificTransitions.Count==0 && wildcards.Count==0)) {
                    if ((previousElement == -1)) {
                        fsb.append("In fact, nothing is allowed: the type has no valid instances. ");
                    } else {
                        fsb.append("No further elements are allowed at this point. ");
                    }
                } else {
                    if (!(specificTransitions.Count==0)) {
                        if ((specificTransitions.Count == 1)) {
                            fsb.append("Expected ");
                            fsb.append(Saxon.Hej.trans.Err.wrap(expected.getEQName(), Saxon.Hej.trans.Err.ELEMENT));
                            if (state.isFinalState()) {
                                fsb.append(" or nothing");
                            }
                            fsb.append(". ");
                        } else {
                            bool uniform = (specificURIs.Count == 1);
                            fsb.append("The following elements would be valid here");
                            if (uniform) {
                                fsb.append(", all in ");
                                fsb.append((@namespace.isEmpty() ? "no namespace" : ("namespace "+@namespace)));
                            }
                            fsb.append(": ");
                            int i = 0;
                            foreach (string s in specificTransitions) {
                                if ((i>=10)) {
                                    fsb.append(", and ").append((specificTransitions.Count-10)).append(" others");
                                    break ;
                                }
                                if ((i++>0)) {
                                    fsb.append(", ");
                                }
                                if (uniform) {
                                    int c = s.IndexOf(':');
                                    if ((c<0)) {
                                        fsb.append(s);
                                    } else {
                                        fsb.append(s.Substring((c+1)));
                                    }
                                } else {
                                    fsb.append(s);
                                }
                            }
                            if (state.isFinalState()) {
                                fsb.append(" (or nothing)");
                            }
                            fsb.append(". ");
                        }
                    }
                    if ((nodeName == null)) {
                        if (!(wildcards.Count==0)) {
                            if (!(specificTransitions.Count==0)) {
                                fsb.append("Alternatively, an element that matches ");
                            } else {
                                fsb.append("Expected an element that matches ");
                            }
                            if ((wildcards.Count == 1)) {
                                fsb.append("the");
                            } else {
                                fsb.append("an");
                            }
                            fsb.append(" <xs:any> wildcard. ");
                        }
                    } else {
                        if ((wildcards.Count == 1)) {
                            foreach (Saxon.Eej.ee.schema.Wildcard w in wildcards) {
                                fsb.append("There is an <xs:any> wildcard, but it does not match: ");
                                fsb.append(w.reasonForNonMatch(nodeName, true, getConfiguration(), type));
                            }
                        } else {
                            if ((wildcards.Count>1)) {
                                fsb.append("There are several <xs:any> wildcards, but none of them match. ");
                            }
                        }
                    }
                }
            }
            return fsb.ToString();
        }
        private void recover(Saxon.Hej.om.NodeName elemName, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location locationId, int properties) {
            childValidator = null;
            Saxon.Hej.type.SchemaType childType = type.getContextDeterminedTypeForElement(elemName.getStructuredQName());
            if (((childType != null) && !(((childType.isComplexType() && ((Saxon.Hej.type.ComplexType)(childType)).isAbstract()))))) {
                System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> failures = new System.Collections.Generic.List<Saxon.Hej.type.ValidationFailure>(1);
                childValidator = makeValidator(null, elemName.getStructuredQName(), locationId, getValidationContext(), childType, Saxon.Hej.lib.Validation.BY_TYPE, getPipelineConfiguration(), nextReceiver, failures);
                foreach (Saxon.Hej.type.ValidationFailure failure in failures) {
                    reportValidationError(failure, false, locationId);
                }
            }
            if ((childValidator == null)) {
                childType = Saxon.Hej.type.AnyType.getInstance();
                childValidator = new Saxon.Eej.ee.validate.AnyTypeValidator(nextReceiver);
            }
            nextReceiver.startElement(elemName, childType, attributes, namespaces, locationId, properties);
        }
        protected internal override Saxon.Eej.ee.validate.ContentValidator getChildValidator() {
            return childValidator;
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            checkNoCharactersWhenNil(locationId);
            if (mixed) {
                if (((fixedValue != null) || (defaultValue != null))) {
                    if ((buffer == null)) {
                        buffer = new Saxon.Hej.str.UnicodeBuilder();
                    }
                    buffer.accept(chars);
                }
                nextReceiver.characters(chars, locationId, properties);
            } else {
                if (Saxon.Hej.value.Whitespace.isAllWhite(chars)) {
                    if (ignoreIgnorable) {
                    } else {
                        nextReceiver.characters(chars, locationId, properties);
                    }
                } else {
                    string message = (("The content model for "+getContainingElementName())+" does not allow character content");
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                    ve.setSchemaType(type);
                    ve.setConstraintReference(1, "cvc-complex-type", "2.3");
                    reportValidationError(ve, true, locationId);
                }
            }
        }
        public override void endElement() {
            if ((fixedValue != null)) {
                if (isNilled()) {
                    string message = (("The "+getContainingElementName())+" has a fixed value so it must not be nil");
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                    ve.setConstraintReference(1, "cvc-elt", "3.2.2");
                    ve.setSchemaType(type);
                    reportValidationError(ve, true, getContainingElementLocationId());
                    nextReceiver.endElement();
                    return ;
                }
                if (((buffer == null) || buffer.isEmpty())) {
                    characters(fixedValue, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                } else {
                    if (!(buffer.toUnicodeString().Equals(fixedValue))) {
                        string message = ((((("The content of "+getContainingElementName())+" differs from the fixed value defined in the schema. Fixed value is ")+Saxon.Hej.trans.Err.wrap(fixedValue, Saxon.Hej.trans.Err.VALUE))+", actual value is ")+Saxon.Hej.trans.Err.wrap(buffer.ToString(), Saxon.Hej.trans.Err.VALUE));
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                        ve.setSchemaType(type);
                        ve.setConstraintReference(1, "cvc-elt", "5.2.2.2");
                        reportValidationError(ve, true, getContainingElementLocationId());
                    }
                }
            }
            if (((defaultValue != null) && !(foundChildren))) {
                if (((buffer == null) || buffer.isEmpty())) {
                    characters(defaultValue, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.NONE);
                }
            }
            if (((!(state.isFinalState()) && !(isNilled())) && !(locallyInvalid))) {
                string message = supplyReasonForNonMatch(null);
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                ve.setSchemaType(type);
                ve.setConstraintReference(1, "cvc-complex-type", "2.4");
                reportValidationError(ve, true, getContainingElementLocationId());
            }
            nextReceiver.endElement();
        }
    
    }
}