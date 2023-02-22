using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class ElementDecl : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Eej.ee.schema.SerializableSchemaComponent, Saxon.Eej.ee.schema.ComponentWithValueConstraint, Saxon.Hej.type.SchemaDeclaration, Saxon.Eej.ee.schema.Term {
        private int block = 0;
        private Saxon.Eej.ee.schema.ModelGroupDefinition containingModelGroup = null;
        private Saxon.Eej.ee.schema.UserComplexType containingComplexType = null;
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.IdentityConstraintReference> constraintRefs = null;
        private int finalProhibitions = 0;
        private Saxon.Eej.ee.schema.ValueConstraint valueConstraint;
        private readonly bool global;
        private bool isAbstractDecl = false;
        private Saxon.Hej.om.StructuredQName elementName;
        private int fingerprint = -1;
        private Saxon.Hej.om.NamespaceResolver namespaceResolver;
        private bool nillable = false;
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.SimpleComponentReference> substitutionGroupOwners = null;
        private System.Collections.Generic.ICollection<Saxon.Eej.ee.schema.ElementDecl> substitutionGroupMembers = new System.Collections.Generic.List<Saxon.Eej.ee.schema.ElementDecl>();
        private Saxon.Hej.om.NamespaceUri targetNamespace;
        private Saxon.Eej.ee.schema.TypeReference typeReference = null;
        private System.Collections.Generic.ICollection<Saxon.Eej.ee.schema.UserComplexType> complexTypesUsingThisElement = new System.Collections.Generic.List<Saxon.Eej.ee.schema.UserComplexType>();
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.TypeAlternative> typeAlternatives = null;
        private string generatedId = null;
        public ElementDecl(Saxon.Eej.config.EnterpriseConfiguration config, bool global, Saxon.Ejavax.xml.transform.SourceLocator locator)  {
            setLocator(locator);
            this.global = global;
            setConfiguration(config);
            constraintRefs = new System.Collections.Generic.List<Saxon.Eej.ee.schema.IdentityConstraintReference>(3);
        }
        public override void setGeneratedId(string id) {
            this.generatedId = id;
        }
        public void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
            namespaceResolver = resolver;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return namespaceResolver;
        }
        public void addIdentityConstraint(Saxon.Eej.ee.schema.IdentityConstraintReference constraint) {
            if ((constraint == null)) {
                return ;
            }
            constraintRefs.Add(constraint);
        }
        public void setContainingComplexType(Saxon.Eej.ee.schema.UserComplexType type) {
            containingComplexType = type;
        }
        public void setContainingModelGroup(Saxon.Eej.ee.schema.ModelGroupDefinition type) {
            containingModelGroup = type;
        }
        public int getBlock() {
            return block;
        }
        public Saxon.Eej.ee.schema.ValueConstraint getValueConstraint() {
            return valueConstraint;
        }
        public int getSubstitutionGroupExclusions() {
            return finalProhibitions;
        }
        public bool allowsDerivation(int derivation) {
            return (((finalProhibitions&derivation)) == 0);
        }
        public Saxon.Eej.ee.schema.UserComplexType getContainingComplexType() {
            return containingComplexType;
        }
        public Saxon.Eej.ee.schema.ModelGroupDefinition getContainingModelGroup() {
            return containingModelGroup;
        }
        public Saxon.Hej.om.AtomicSequence getFixedValue() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getFixedValueConstraint();
            return ((vc == null) ? null : vc.getValue());
        }
        public Saxon.Hej.str.UnicodeString getFixedValueLexicalForm() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getFixedValueConstraint();
            return ((vc == null) ? null : vc.getLexicalForm());
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.IdentityConstraintReference> getIdentityConstraints() {
            return constraintRefs;
        }
        public string getDisplayName() {
            return elementName.getDisplayName();
        }
        public string getName() {
            return elementName.getLocalPart();
        }
        public Saxon.Hej.om.NamespaceUri getTargetNamespace() {
            return targetNamespace;
        }
        public int getTypeFingerprint() {
            if ((typeReference != null)) {
                return typeReference.getTargetFingerprint();
            } else {
                return -1;
            }
        }
        public Saxon.Hej.type.SchemaType getType() {
            Saxon.Hej.type.SchemaType schemaType;
            if ((typeReference == null)) {
                return Saxon.Hej.type.AnyType.getInstance();
            }
            schemaType = (Saxon.Hej.type.SchemaType)(typeReference.getTarget());
            if (((schemaType == null) || (schemaType.getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID))) {
                return Saxon.Hej.type.AnyType.getInstance();
            }
            if ((schemaType is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                schemaType = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(schemaType)).getWorkingType();
                typeReference.setTarget(schemaType);
            }
            return schemaType;
        }
        public System.Collections.Generic.ISet<Saxon.Eej.ee.schema.ElementDecl> getSubstitutionGroupOwners() {
            if ((substitutionGroupOwners != null)) {
                System.Collections.Generic.ISet<Saxon.Eej.ee.schema.ElementDecl> heads = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.ElementDecl>(substitutionGroupOwners.Count);
                foreach (Saxon.Eej.ee.schema.ComponentReference headRef in substitutionGroupOwners) {
                    heads.Add((Saxon.Eej.ee.schema.ElementDecl)(headRef.getTarget()));
                }
                return heads;
            } else {
                return new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.ElementDecl>();
            }
        }
        public System.Collections.Generic.ICollection<Saxon.Eej.ee.schema.ElementDecl> getSubstitutionGroupMembers() {
            return substitutionGroupMembers;
        }
        public void addSubstitutionGroupMember(Saxon.Eej.ee.schema.ElementDecl member, Saxon.Eej.ee.schema.SchemaCompiler compiler) { lock(this) {
            if (!(substitutionGroupMembers.Contains(member))) {
                Saxon.Eej.config.EnterpriseConfiguration config = getConfiguration();
                if ((member != this)) {
                    member.fixup(compiler);
                    string reason = member.isValidlySubstitutable(this);
                    if ((reason != null)) {
                        bool isError = false;
                        if (((compiler.getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD11) && reason.StartsWith("required type derivation method"))) {
                            isError = true;
                        }
                        string memberName = member.getDisplayName();
                        string msg = ((((("Element "+Saxon.Hej.trans.Err.wrap(memberName, Saxon.Hej.trans.Err.ELEMENT))+" cannot participate in the substitution group of element ")+Saxon.Hej.trans.Err.wrap(getName(), Saxon.Hej.trans.Err.ELEMENT))+": ")+reason);
                        try {
                            System.Collections.Generic.ISet<Saxon.Eej.ee.schema.ElementDecl> grandFathers = getSubstitutionGroupOwners();
                            foreach (Saxon.Eej.ee.schema.ElementDecl grandFather in grandFathers) {
                                if ((member.isValidlySubstitutable(grandFather) == null)) {
                                    msg += (((". However, element "+Saxon.Hej.trans.Err.wrap(memberName, Saxon.Hej.trans.Err.ELEMENT))+" can substitute for element ")+Saxon.Hej.trans.Err.wrap(grandFather.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT));
                                    break ;
                                }
                            }
                        } catch (Saxon.Hej.type.MissingComponentException err) {
                        }
                        if (isError) {
                            compiler.error(msg, member);
                            throw new Saxon.Hej.type.SchemaException("Disallowed substitution group membership");
                        } else {
                            compiler.warning(msg, Saxon.Hej.trans.SaxonErrorCode.SXSD1002, member);
                        }
                        return ;
                    }
                }
                if (((member != this) && config.isSealedNamespace(getTargetNamespace()))) {
                    string msg = ((((("It is not possible to add to the substitution group of element "+getName())+" in namespace '")+getTargetNamespace())+"', because the schema for that namespace has already been used for validating instance documents,")+" or for compiling queries or stylesheets");
                    Saxon.Hej.type.SchemaException err = new Saxon.Hej.type.SchemaException(msg);
                    err.setLocator(member);
                    throw err;
                }
                substitutionGroupMembers.Add(member);
                if ((substitutionGroupMembers.Count>10)) {
                    substitutionGroupMembers = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.ElementDecl>(substitutionGroupMembers);
                }
                if ((member != this)) {
                    foreach (Saxon.Eej.ee.schema.UserComplexType type in complexTypesUsingThisElement) {
                        type.recompile(compiler);
                    }
                }
            }
        }}
        public void registerComplexTypeUsingThisElement(Saxon.Eej.ee.schema.UserComplexType type) { lock(this) {
            if (!(complexTypesUsingThisElement.Contains(type))) {
                complexTypesUsingThisElement.Add(type);
                if ((complexTypesUsingThisElement.Count>10)) {
                    complexTypesUsingThisElement = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.UserComplexType>(complexTypesUsingThisElement);
                }
            }
        }}
        public bool isAbstract() {
            return isAbstractDecl;
        }
        public bool isNillable() {
            return nillable;
        }
        public bool isGlobal() {
            return global;
        }
        public void setAbstract(bool isAbstract) {
            this.isAbstractDecl = isAbstract;
        }
        public void setBlock(int block) {
            this.block = block;
        }
        public void setFinalProhibitions(int finalValue) {
            finalProhibitions = finalValue;
        }
        public void setValueConstraint(Saxon.Eej.ee.schema.ValueConstraint vc) {
            this.valueConstraint = vc;
        }
        public void setNillable(bool nillable) {
            this.nillable = nillable;
        }
        public void setElementName(Saxon.Hej.om.StructuredQName elementName, int fingerprint) {
            this.elementName = elementName;
            this.fingerprint = fingerprint;
            this.targetNamespace = elementName.getNamespaceUri();
            substitutionGroupMembers.Add(this);
        }
        public void setElementName(Saxon.Hej.om.StructuredQName elementName) {
            this.elementName = elementName;
            this.fingerprint = getConfiguration().getNamePool().allocateFingerprint(elementName.getNamespaceUri(), elementName.getLocalPart());
            this.targetNamespace = elementName.getNamespaceUri();
            substitutionGroupMembers.Add(this);
        }
        public Saxon.Hej.om.StructuredQName getComponentName() {
            return elementName;
        }
        public int getFingerprint() {
            return fingerprint;
        }
        public void addSubstitutionGroupHead(Saxon.Hej.om.StructuredQName qName) {
            if ((substitutionGroupOwners == null)) {
                substitutionGroupOwners = new System.Collections.Generic.List<Saxon.Eej.ee.schema.SimpleComponentReference>(2);
            }
            Saxon.Eej.ee.schema.SimpleComponentReference headRef = new Saxon.Eej.ee.schema.SimpleComponentReference(Saxon.Eej.ee.schema.SymbolSpace.ELEMENT, qName, getConfiguration());
            headRef.setLocator(this);
            substitutionGroupOwners.Add(headRef);
        }
        public void setTypeReference(Saxon.Eej.ee.schema.TypeReference type) {
            typeReference = type;
        }
        public void addTypeAlternative(Saxon.Eej.ee.schema.TypeAlternative alternative) {
            if ((typeAlternatives == null)) {
                typeAlternatives = new System.Collections.Generic.List<Saxon.Eej.ee.schema.TypeAlternative>(5);
            }
            typeAlternatives.Add(alternative);
        }
        public bool hasTypeAlternatives() {
            return (typeAlternatives != null);
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.TypeAlternative> getTypeAlternatives() {
            return typeAlternatives;
        }
        public override void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (System.Object reference in references) {
                Saxon.Eej.ee.schema.ElementDecl member = (Saxon.Eej.ee.schema.ElementDecl)(reference);
                if ((member == this)) {
                    compiler.error((("Element declaration "+getDisplayName())+" participates in a substitution group whose definition is circular"), this);
                    throw new Saxon.Hej.type.SchemaException("Circular definition found");
                }
                addSubstitutionGroupMember(member, compiler);
            }
            if ((substitutionGroupOwners != null)) {
                references.Push(this);
                foreach (Saxon.Eej.ee.schema.ComponentReference headRef in substitutionGroupOwners) {
                    headRef.tryToResolve(compiler, false);
                    if (headRef.isResolved()) {
                        Saxon.Eej.ee.schema.ElementDecl head = (Saxon.Eej.ee.schema.ElementDecl)(headRef.getTarget());
                        head.lookForCycles(references, compiler);
                    }
                }
                references.Pop();
            }
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result = true;
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
                if ((substitutionGroupOwners != null)) {
                    foreach (Saxon.Eej.ee.schema.ComponentReference headRef in substitutionGroupOwners) {
                        Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(headRef, compiler, false));
                        if ((decl == null)) {
                            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            substitutionGroupOwners = null;
                            return false;
                        }
                        result = decl.fixup(compiler);
                    }
                }
                if (((typeReference != null) && !(typeReference.isDangling()))) {
                    Saxon.Hej.type.SchemaType type;
                    try {
                        type = (Saxon.Hej.type.SchemaType)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(typeReference, compiler, false));
                    } catch (Saxon.Hej.type.MissingComponentException e) {
                        type = null;
                    }
                    if ((type == null)) {
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        typeReference = new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_ERROR, getConfiguration(), this);
                        return false;
                    }
                    if ((type is Saxon.Eej.ee.schema.UserSchemaComponent)) {
                        result &= ((Saxon.Eej.ee.schema.UserSchemaComponent)(type)).fixup(compiler);
                    }
                }
                if (hasTypeAlternatives()) {
                    foreach (Saxon.Eej.ee.schema.TypeAlternative ta in getTypeAlternatives()) {
                        result &= ta.fixup(compiler);
                    }
                }
                foreach (Saxon.Eej.ee.schema.IdentityConstraintReference icr in constraintRefs) {
                    icr.tryToResolve(compiler, false);
                }
            }
            setFixupStatus((result ? /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED : /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID));
            return result;
        }
        public Saxon.Hej.pattern.NodeTest makeSchemaNodeTest() {
            return new Saxon.Eej.ee.validate.SchemaElementTest(this);
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (!(isValidationNeeded())) {
                return true;
            }
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
            bool result = true;
            bool checkFirstSubstitutionGroup = true;
            if ((typeReference != null)) {
                typeReference.tryToResolve(compiler, false);
            } else {
                if ((substitutionGroupOwners != null)) {
                    Saxon.Eej.ee.schema.ElementDecl head = (Saxon.Eej.ee.schema.ElementDecl)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(substitutionGroupOwners[0], compiler, false));
                    if ((head != null)) {
                        Saxon.Hej.type.SchemaType headType = head.getType();
                        typeReference = new Saxon.Eej.ee.schema.TypeReference(headType.getFingerprint(), getConfiguration(), this);
                        typeReference.setTarget(headType);
                        checkFirstSubstitutionGroup = false;
                    } else {
                        typeReference = new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.type.AnyType.getInstance().getFingerprint(), getConfiguration(), this);
                        typeReference.setTarget(Saxon.Hej.type.AnyType.getInstance());
                        checkFirstSubstitutionGroup = false;
                    }
                }
            }
            if ((typeReference != null)) {
                /*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(typeReference, compiler, false);
                if (!(typeReference.isResolved())) {
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return true;
                }
            }
            Saxon.Hej.type.SchemaType type = getType();
            if ((type != null)) {
                if (type.isComplexType()) {
                    if ((type is Saxon.Eej.ee.schema.UserComplexType)) {
                        result = ((Saxon.Eej.ee.schema.UserComplexType)(type)).validate(compiler);
                    } else {
                    }
                } else {
                    if (type.isSimpleType()) {
                        Saxon.Hej.type.SimpleType simpleType = (Saxon.Hej.type.SimpleType)(type);
                        if ((simpleType is Saxon.Eej.ee.schema.UserSimpleType)) {
                            result = ((Saxon.Eej.ee.schema.UserSimpleType)(simpleType)).validate(compiler);
                        }
                        if ((((result && (compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) && (getValueConstraint() != null)) && (simpleType.getBuiltInBaseType().getFingerprint() == Saxon.Hej.om.StandardNames.XS_ID))) {
                            compiler.error("An element of type xs:ID must have no fixed or default value", this);
                            result = false;
                        }
                    }
                }
                if ((getFixedValue() != null)) {
                    result &= validateSimpleValue(type, "fixed", compiler);
                }
                if ((getDefaultValueLexicalForm() != null)) {
                    result &= validateSimpleValue(type, "default", compiler);
                }
                bool first = true;
                if (((substitutionGroupOwners != null) && checkFirstSubstitutionGroup)) {
                    foreach (Saxon.Eej.ee.schema.ComponentReference headRef in substitutionGroupOwners) {
                        if ((checkFirstSubstitutionGroup || !(first))) {
                            Saxon.Eej.ee.schema.ElementDecl head = (Saxon.Eej.ee.schema.ElementDecl)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(headRef, compiler, false));
                            if ((head != null)) {
                                Saxon.Hej.type.SchemaType headType = head.getType();
                                try {
                                    compiler.getConfiguration().checkTypeDerivationIsOK(type, headType, head.finalProhibitions);
                                } catch (Saxon.Hej.type.SchemaException err) {
                                    compiler.error(((((("Element "+getDisplayName())+" cannot be in the substitution group of ")+head.getDisplayName())+". ")+err.Message), this);
                                    result = false;
                                }
                            }
                        }
                        if (((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10) && !(first))) {
                            compiler.error("In XML Schema 1.0, an element can participate in one substitution group only", this);
                        }
                        first = false;
                    }
                }
                foreach (Saxon.Eej.ee.schema.IdentityConstraintReference constraintRef in constraintRefs) {
                    compiler.addPendingTypeCheck(constraintRef, type);
                }
            }
            if (hasTypeAlternatives()) {
                foreach (Saxon.Eej.ee.schema.TypeAlternative ta in getTypeAlternatives()) {
                    Saxon.Hej.type.SchemaType altType = ta.getSchemaType();
                    if ((altType is Saxon.Eej.ee.schema.UserDefinedType)) {
                        result &= ((Saxon.Eej.ee.schema.UserSchemaComponent)(altType)).validate(compiler);
                    }
                    if ((altType != Saxon.Hej.type.ErrorType.getInstance())) {
                        try {
                            compiler.getConfiguration().checkTypeDerivationIsOK(altType, type, finalProhibitions);
                        } catch (Saxon.Hej.type.SchemaException err) {
                            compiler.error(("Invalid alternative type. "+err.Message), ta);
                            result = false;
                        }
                    }
                }
            }
            foreach (Saxon.Eej.ee.schema.IdentityConstraintReference icr in constraintRefs) {
                icr.tryToResolve(compiler, false);
                if (!(icr.isResolved())) {
                    compiler.error(("Cannot resolve integrity constraint reference "+icr.getTargetComponentName().getDisplayName()), this);
                    return false;
                }
                Saxon.Eej.ee.schema.IdentityConstraint ic = ((/*1430*/Saxon.Eej.ee.schema.IdentityConstraint)icr.getTarget());
                switch (icr.getExpectedKindOfConstraint()) {
                  case Saxon.Hej.om.StandardNames.XS_UNIQUE:
                    if (!(((ic is Saxon.Eej.ee.schema.Unique)))) {
                        compiler.error("Target of xs:unique reference must be an xs:unique element", icr);
                        result = false;
                    }
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_KEY:
                    if (!(((ic is Saxon.Eej.ee.schema.Key)))) {
                        compiler.error("Target of xs:key reference must be an xs:key element", icr);
                        result = false;
                    }
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_KEYREF:
                    if (!(((ic is Saxon.Eej.ee.schema.KeyRef)))) {
                        compiler.error("Target of xs:keyref reference must be an xs:keyref element", icr);
                        result = false;
                    }
                    break ;
                }
            }
            setValidationStatus((result ? /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED : /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID));
            return result;
        }
        private string isValidlySubstitutable(Saxon.Eej.ee.schema.ElementDecl other) {
            int block = other.getBlock();
            if ((this == other)) {
                return null;
            }
            if ((((block&Saxon.Hej.type.Derivation.DERIVE_BY_SUBSTITUTION)) != 0)) {
                return "substitution has been blocked";
            }
            bool found = false;
            System.Collections.Generic.LinkedList<Saxon.Eej.ee.schema.ElementDecl> unsearched = new System.Collections.Generic.LinkedList<Saxon.Eej.ee.schema.ElementDecl>();
            unsearched.AddLast(this);
            while (!(unsearched.Count==0)) {
                if (unsearched.Contains(other)) {
                    found = true;
                    break ;
                }
                Saxon.Eej.ee.schema.ElementDecl first = Saxon.Impl.Helpers.LinkedListUtils.PollFirst(unsearched);
                Saxon.Impl.Helpers.LinkedListUtils.AddAll(unsearched, first.getSubstitutionGroupOwners());
            }
            if (!(found)) {
                return "element declaration is not a member of the substitution group";
            }
            int actualDerivations = 0;
            int prohibitedDerivations = block;
            Saxon.Hej.type.SchemaType t = getType();
            Saxon.Hej.type.SchemaType ot = other.getType();
            if ((ot == null)) {
                ot = Saxon.Hej.type.AnyType.getInstance();
            }
            while ((((t != null) && (t != Saxon.Hej.type.AnyType.getInstance())) && !(t.isSameType(ot)))) {
                actualDerivations |= t.getDerivationMethod();
                t = t.getBaseType();
                prohibitedDerivations |= t.getBlock();
            }
            if ((((actualDerivations&prohibitedDerivations)) != 0)) {
                return "required type derivation method (extension or restriction) has been blocked";
            }
            return null;
        }
        private bool validateSimpleValue(Saxon.Hej.type.SchemaType type, string property, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Hej.type.SimpleType simpleType;
            if (type.isSimpleType()) {
                simpleType = (Saxon.Hej.type.SimpleType)(type);
            } else {
                if (((Saxon.Hej.type.ComplexType)(type)).isSimpleContent()) {
                    simpleType = ((Saxon.Hej.type.ComplexType)(type)).getSimpleContentType();
                } else {
                    if (((Saxon.Hej.type.ComplexType)(type)).isMixedContent()) {
                        if (!(((Saxon.Hej.type.ComplexType)(type)).isEmptiable())) {
                            compiler.error((("An element with mixed content may have a "+property)+" value only if its type is emptiable"), this);
                            return false;
                        }
                        simpleType = Saxon.Hej.type.BuiltInAtomicType.STRING;
                    } else {
                        compiler.error((("An element with a "+property)+" value must have simple or mixed content"), this);
                        return false;
                    }
                }
            }
            System.Diagnostics.Debug.Assert((simpleType != null));
            if ((simpleType is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                bool result = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(simpleType)).validate(compiler);
                if (!(result)) {
                    return false;
                }
                simpleType = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(simpleType)).getWorkingType();
            }
            if ((valueConstraint != null)) {
                return valueConstraint.makeTypedValue(compiler, simpleType, getNamespaceResolver(), this);
            }
            return true;
        }
        protected internal void fixupSubstitutionGroup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((substitutionGroupOwners != null)) {
                foreach (Saxon.Eej.ee.schema.ComponentReference headRef in substitutionGroupOwners) {
                    Saxon.Eej.ee.schema.ElementDecl head = (Saxon.Eej.ee.schema.ElementDecl)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(headRef, compiler, false));
                    if ((head == null)) {
                        string err = (((("Element "+Saxon.Hej.trans.Err.wrap(headRef.getTargetComponentName().getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" is referenced as head of a substitution group for ")+Saxon.Hej.trans.Err.wrap(getDisplayName()))+", but is not declared in the schema");
                        compiler.warning(err, Saxon.Hej.trans.SaxonErrorCode.SXSD1003, this);
                    }
                }
                lookForCycles(new System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent>(), compiler);
            }
        }
        public bool isSameDeclaration(Saxon.Eej.ee.schema.ElementDecl other) {
            if ((this == other)) {
                return true;
            }
            if ((getFingerprint() != other.getFingerprint())) {
                return false;
            }
            if (!(generatedId.Equals(other.generatedId))) {
                return false;
            }
            if ((getBlock() != other.getBlock())) {
                return false;
            }
            if ((getSubstitutionGroupExclusions() != other.getSubstitutionGroupExclusions())) {
                return false;
            }
            if (((typeReference != null) && (other.typeReference != null))) {
                if (typeReference.isAnonymousType()) {
                    return other.typeReference.isAnonymousType();
                } else {
                    return (typeReference.getTargetFingerprint() == other.typeReference.getTargetFingerprint());
                }
            }
            return true;
        }
        public bool hasSameTypeTable(Saxon.Eej.ee.schema.ElementDecl other) {
            if (this.isSameDeclaration(other)) {
                return true;
            }
            if (((typeAlternatives == null) && (other.typeAlternatives == null))) {
                return true;
            }
            if (((typeAlternatives == null) || (other.typeAlternatives == null))) {
                return false;
            }
            if ((typeAlternatives.Count != other.typeAlternatives.Count)) {
                return false;
            }
            if (!(this.getType().isSameType(other.getType()))) {
                return false;
            }
            for (int i = 0; (i<typeAlternatives.Count); i++) {
                Saxon.Eej.ee.schema.TypeAlternative a0 = this.typeAlternatives[i];
                Saxon.Eej.ee.schema.TypeAlternative a1 = other.typeAlternatives[i];
                Saxon.Hej.sxpath.XPathExpression c0 = a0.getCondition();
                Saxon.Hej.sxpath.XPathExpression c1 = a1.getCondition();
                if ((((c0 == null)) != ((c1 == null)))) {
                    return false;
                }
                if (((c0 != null) && !(c0.getInternalExpression().isEqual(c1.getInternalExpression())))) {
                    return false;
                }
                if (!(a0.getDefaultElementNamespace().Equals(a1.getDefaultElementNamespace()))) {
                    return false;
                }
                if (!(a0.getBaseURI().Equals(a1.getBaseURI()))) {
                    return false;
                }
                if (!(a0.getSchemaType().isSameType(a1.getSchemaType()))) {
                    return false;
                }
                System.Collections.Generic.ISet<string> prefixes0 = new System.Collections.Generic.HashSet<string>();
                for (System.Collections.Generic.IEnumerator<string> it = a0.getNamespaceContext().iteratePrefixes(); it.MoveNext() /*UNSAFE CONVERSION*/; ) {
                    prefixes0.Add(it.Current);
                }
                for (System.Collections.Generic.IEnumerator<string> it = a1.getNamespaceContext().iteratePrefixes(); it.MoveNext() /*UNSAFE CONVERSION*/; ) {
                    string prefix = it.Current;
                    if (!(prefixes0.Contains(prefix))) {
                        return false;
                    }
                }
                foreach (string p in prefixes0) {
                    if (!(a0.getNamespaceContext().getURIForPrefix(p, false).Equals(a1.getNamespaceContext().getURIForPrefix(p, false)))) {
                        return false;
                    }
                }
            }
            return true;
        }
        public override string ToString() {
            return ("ElementDeclaration "+getName());
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            string id = serializer.getId(this, true);
            serializer.startElement("element");
            serializer.emitAttribute("id", id);
            if ((getName() != null)) {
                serializer.emitAttribute("name", getName());
                if (!(getTargetNamespace().isEmpty())) {
                    serializer.emitAttribute("targetNamespace", getTargetNamespace().ToString());
                }
            }
            serializer.emitAttribute("type", serializer.getTypeLink(getType()));
            serializer.emitAttribute("global", (isGlobal() ? "true" : "false"));
            if ((!(isGlobal()) && (getContainingComplexType() != null))) {
                serializer.emitAttribute("containingComplexType", serializer.getId(getContainingComplexType(), false));
            }
            if ((getDefaultValueLexicalForm() != null)) {
                serializer.emitAttribute("default", getDefaultValueLexicalForm().ToString());
            }
            serializer.emitAttribute("nillable", (isNillable() ? "true" : "false"));
            if ((finalProhibitions != 0)) {
                string ex = "";
                if ((((finalProhibitions&Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) != 0)) {
                    ex = "extension";
                }
                if ((((finalProhibitions&Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) != 0)) {
                    ex += (((ex.Length==0 ? "" : " "))+"restriction");
                }
                serializer.emitAttribute("final", ex);
            }
            if ((block != 0)) {
                string dis = "";
                if ((((block&Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) != 0)) {
                    dis = "extension";
                }
                if ((((block&Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) != 0)) {
                    dis += (((dis.Length==0 ? "" : " "))+"restriction");
                }
                if ((((block&Saxon.Hej.type.Derivation.DERIVE_BY_SUBSTITUTION)) != 0)) {
                    dis += (((dis.Length==0 ? "" : " "))+"substitution");
                }
                serializer.emitAttribute("block", dis);
            }
            serializer.emitAttribute("abstract", (isAbstractDecl ? "true" : "false"));
            if ((substitutionGroupOwners != null)) {
                foreach (Saxon.Eej.ee.schema.ComponentReference headRef in substitutionGroupOwners) {
                    serializer.startElement("substitutionGroupAffiliation");
                    serializer.emitAttribute("ref", serializer.getId(headRef.getTarget(), false));
                    serializer.endElement();
                }
            }
            if (hasTypeAlternatives()) {
                foreach (Saxon.Eej.ee.schema.TypeAlternative alt in getTypeAlternatives()) {
                    alt.serialize(serializer);
                }
            }
            foreach (Saxon.Eej.ee.schema.IdentityConstraintReference icr in getIdentityConstraints()) {
                serializer.startElement("identityConstraint");
                serializer.emitAttribute("ref", serializer.getId(((/*1430*/Saxon.Eej.ee.schema.IdentityConstraint)icr.getTarget()), false));
                serializer.endElement();
            }
            if ((getValueConstraint() != null)) {
                getValueConstraint().serialize(serializer);
            }
            serializer.endElement();
        }
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            Saxon.Eej.ee.schema.ElementDecl thisDecl = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Element Declaration");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.ElementDecl>(thisDecl);
                  case "name":
                    return new Saxon.Hej.value.StringValue(getName(), Saxon.Hej.type.BuiltInAtomicType.NCNAME);
                  case "target namespace":
                    return new Saxon.Hej.value.AnyURIValue(getTargetNamespace().toUnicodeString());
                  case "type definition":
                    return getType().getComponentAsFunction();
                  case "type table":
                    if (hasTypeAlternatives()) {
                        Saxon.Hej.expr.CallableDelegate typeTable = new Saxon.Hej.expr.CallableDelegate((context1, arguments1) => {
                            string key1 = arguments1[0].head().getStringValue();
                            switch (key1) {
                              case "class":
                                return Saxon.Hej.value.StringValue.bmp("Type Table");
                              case "alternatives":
                                System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                                foreach (Saxon.Eej.ee.schema.TypeAlternative ta in getTypeAlternatives()) {
                                    list.Add(ta.getComponentAsFunction());
                                }
                                return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                              case "default type definition":
                                return typeAlternatives[(typeAlternatives.Count-1)].getComponentAsFunction();
                              default:
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            }
                        } );
                        return new Saxon.Hej.functions.CallableFunction(1, typeTable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                  case "scope":
                    Saxon.Hej.expr.CallableDelegate scopeFunction = new Saxon.Hej.expr.CallableDelegate((context12, arguments12) => {
                        string key12 = ((Saxon.Hej.value.StringValue)(arguments12[0].head())).getStringValue();
                        switch (key12) {
                          case "class":
                            return Saxon.Hej.value.StringValue.bmp("Scope");
                          case "variety":
                            return Saxon.Hej.value.StringValue.bmp((isGlobal() ? "global" : "local"));
                          case "parent":
                            if (isGlobal()) {
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            } else {
                                if ((getContainingComplexType() != null)) {
                                    return getContainingComplexType().getComponentAsFunction();
                                } else {
                                    if ((getContainingModelGroup() != null)) {
                                        return getContainingModelGroup().getComponentAsFunction();
                                    } else {
                                        return Saxon.Hej.value.EmptySequence.getInstance();
                                    }
                                }
                            }
                          default:
                            return Saxon.Hej.value.EmptySequence.getInstance();
                        }
                    } );
                    return new Saxon.Hej.functions.CallableFunction(1, scopeFunction, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                  case "value constraint":
                    return getValueConstraintAsFunction(this);
                  case "nillable":
                    return Saxon.Hej.value.BooleanValue.get(isNillable());
                  case "identity-constraint definitions":
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> constraints = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                    foreach (Saxon.Eej.ee.schema.IdentityConstraintReference ic in getIdentityConstraints()) {
                        constraints.Add(((/*1430*/Saxon.Eej.ee.schema.IdentityConstraint)ic.getTarget()).getComponentAsFunction());
                    }
                    return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(constraints);
                  case "substitution group affiliations":
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> groups = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                    foreach (Saxon.Eej.ee.schema.ElementDecl decl in getSubstitutionGroupOwners()) {
                        groups.Add(decl.getComponentAsFunction());
                    }
                    return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(groups);
                  case "substitution group exclusions":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> exclusions = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                        if ((((getSubstitutionGroupExclusions()&Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) != 0)) {
                            exclusions.Add(Saxon.Hej.value.StringValue.bmp("extension"));
                        }
                        if ((((getSubstitutionGroupExclusions()&Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) != 0)) {
                            exclusions.Add(Saxon.Hej.value.StringValue.bmp("restriction"));
                        }
                        return new Saxon.Hej.om.AtomicArray(exclusions);
                    }
                  case "disallowed substitutions":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> exclusions = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                        if ((((getBlock()&Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) != 0)) {
                            exclusions.Add(Saxon.Hej.value.StringValue.bmp("extension"));
                        }
                        if ((((getBlock()&Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) != 0)) {
                            exclusions.Add(Saxon.Hej.value.StringValue.bmp("restriction"));
                        }
                        if ((((getBlock()&Saxon.Hej.type.Derivation.DERIVE_BY_SUBSTITUTION)) != 0)) {
                            exclusions.Add(Saxon.Hej.value.StringValue.bmp("substitution"));
                        }
                        return new Saxon.Hej.om.AtomicArray(exclusions);
                    }
                  case "abstract":
                    return Saxon.Hej.value.BooleanValue.get(isAbstract());
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
        public override bool Equals(System.Object other) {
            if (!(((other is Saxon.Eej.ee.schema.ElementDecl)))) {
                return false;
            }
            Saxon.Eej.ee.schema.ElementDecl e2 = (Saxon.Eej.ee.schema.ElementDecl)(other);
            return (((((getFingerprint() == e2.getFingerprint()) && (getTypeFingerprint() == e2.getTypeFingerprint())) && (getBlock() == e2.getBlock())) && (getSubstitutionGroupExclusions() == e2.getSubstitutionGroupExclusions())) && (((getFixedValueLexicalForm() == null) || getFixedValueLexicalForm().Equals(e2.getFixedValueLexicalForm()))));
        }
        public override int GetHashCode() {
            return (getFingerprint()^((getTypeFingerprint()<<10)));
        }
    
        /* Expanding default method getFixedValueConstraint from interface ComponentWithValueConstraint */
        public virtual Saxon.Eej.ee.schema.ValueConstraint getFixedValueConstraint() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getValueConstraint();
            if (((vc != null) && (vc.getVariety() == Saxon.Eej.ee.schema.ValueConstraint.Variety.FIXED))) {
                return vc;
            } else {
                return null;
            }
        }

        /* Expanding default method getDefaultValueConstraint from interface ComponentWithValueConstraint */
        public virtual Saxon.Eej.ee.schema.ValueConstraint getDefaultValueConstraint() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getValueConstraint();
            if (((vc != null) && (vc.getVariety() == Saxon.Eej.ee.schema.ValueConstraint.Variety.DEFAULT))) {
                return vc;
            } else {
                return null;
            }
        }

        /* Expanding default method getValueConstraintAsFunction from interface ComponentWithValueConstraint */
        public virtual Saxon.Hej.om.Sequence getValueConstraintAsFunction(Saxon.Eej.ee.schema.ComponentWithValueConstraint att) {
            if ((getValueConstraint() != null)) {
                return getValueConstraint().asFunction();
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }

        /* Expanding default method getDefaultValueLexicalForm from interface ComponentWithValueConstraint */
        public virtual Saxon.Hej.str.UnicodeString getDefaultValueLexicalForm() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getDefaultValueConstraint();
            return ((vc == null) ? null : vc.getLexicalForm());
        }

        /* Expanding default method getDefaultValue from interface ComponentWithValueConstraint */
        public virtual Saxon.Hej.om.AtomicSequence getDefaultValue() {
            Saxon.Eej.ee.schema.ValueConstraint vc = getDefaultValueConstraint();
            return ((vc == null) ? null : vc.getValue());
        }

    }
}