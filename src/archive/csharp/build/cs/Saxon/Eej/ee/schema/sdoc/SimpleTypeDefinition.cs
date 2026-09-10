using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class SimpleTypeDefinition : Saxon.Eej.ee.schema.UserSimpleType, Saxon.Eej.ee.schema.UserSchemaComponent {
        private bool isSimpleContent = false;
        private Saxon.Eej.ee.schema.TypeReference itemTypeReference = null;
        private Saxon.Hej.regex.RegularExpression tokenSeparator = null;
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.TypeReference> members = null;
        private Saxon.Eej.ee.schema.UserSimpleType workingType = null;
        public SimpleTypeDefinition(Saxon.Eej.config.EnterpriseConfiguration config)  {
            setConfiguration(config);
        }
        public void setItemTypeReference(Saxon.Eej.ee.schema.TypeReference itemType) {
            itemTypeReference = itemType;
        }
        public override bool isAtomicType() {
            return !(((isListType() || isUnionType())));
        }
        public override bool isListType() {
            return ((itemTypeReference != null) || (((getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION) && (getBaseTypeReference() != null)) && ((Saxon.Hej.type.SimpleType)(getBaseType())).isListType()));
        }
        public override bool isUnionType() {
            return ((members != null) || (((getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION) && (getBaseTypeReference() != null)) && ((Saxon.Hej.type.SimpleType)(getBaseType())).isUnionType()));
        }
        public void setIsSimpleContent(bool b) {
            isSimpleContent = b;
        }
        public void addMemberType(Saxon.Eej.ee.schema.TypeReference memberType) {
            if ((members == null)) {
                members = new System.Collections.Generic.List<Saxon.Eej.ee.schema.TypeReference>(5);
            }
            members.Add(memberType);
        }
        public void setTokenSeparator(Saxon.Hej.regex.RegularExpression separator) {
            tokenSeparator = separator;
        }
        public Saxon.Eej.ee.schema.UserSimpleType getWorkingType() {
            return workingType;
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
            } else {
                return true;
            }
            if ((getBaseTypeReference() == null)) {
                Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE, compiler.getConfiguration(), this);
                @ref.setTarget(Saxon.Hej.type.AnySimpleType.getInstance());
                setBaseTypeReference(@ref);
            }
            Saxon.Eej.ee.schema.TypeReference baseRef = getBaseTypeReference();
            if (((baseRef != null) && !(baseRef.isResolved()))) {
                Saxon.Hej.type.SchemaType typeDef = (Saxon.Hej.type.SchemaType)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(baseRef, compiler, true));
                if ((typeDef == null)) {
                    setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                } else {
                    if ((typeDef is Saxon.Eej.ee.schema.UserSchemaComponent)) {
                        bool b = ((Saxon.Eej.ee.schema.UserSchemaComponent)(typeDef)).fixup(compiler);
                        if (!(b)) {
                            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                        if ((typeDef is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                            Saxon.Hej.type.SchemaType s = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(typeDef)).getWorkingType();
                            if ((s != null)) {
                                baseRef.setTarget(s);
                            }
                        } else {
                            if (typeDef.isComplexType()) {
                                if (isSimpleContent) {
                                    if (((Saxon.Hej.type.ComplexType)(typeDef)).isSimpleContent()) {
                                        if ((typeDef is Saxon.Eej.ee.schema.UserComplexType)) {
                                            Saxon.Eej.ee.schema.TypeReference @ref = ((Saxon.Eej.ee.schema.UserComplexType)(typeDef)).getSimpleContentTypeReference();
                                            if ((@ref != null)) {
                                                /*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(@ref, compiler, false);
                                                if (!(@ref.isResolved())) {
                                                    setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                                    return false;
                                                }
                                            }
                                            setBaseTypeReference(@ref);
                                        }
                                    } else {
                                        compiler.error((("A complex type with simple content "+Saxon.Hej.trans.Err.wrap(typeDef.getDescription()))+" cannot have a type with complex content as its base type"), this);
                                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                        setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                        return false;
                                    }
                                } else {
                                    compiler.error((("A simple type cannot have a complex type "+Saxon.Hej.trans.Err.wrap(typeDef.getDescription()))+" as its base type"), this);
                                    setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                    return false;
                                }
                            } else {
                            }
                        }
                    }
                }
            }
            if ((itemTypeReference != null)) {
                bool badItemType = false;
                Saxon.Hej.type.SchemaType type = (Saxon.Hej.type.SchemaType)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(itemTypeReference, compiler, false));
                if ((type == null)) {
                    badItemType = true;
                } else {
                    if ((type is Saxon.Eej.ee.schema.UserSchemaComponent)) {
                        bool r = ((Saxon.Eej.ee.schema.UserSchemaComponent)(type)).fixup(compiler);
                        if (!(r)) {
                            badItemType = true;
                        } else {
                            if ((type is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                                Saxon.Hej.type.SchemaType s = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(type)).getWorkingType();
                                if ((s != null)) {
                                    itemTypeReference.setTarget(s);
                                }
                            } else {
                                if (type.isComplexType()) {
                                    badItemType = true;
                                    compiler.error((("A list type cannot have a complex type "+Saxon.Hej.trans.Err.wrap(type.getDescription()))+" as its item type"), this);
                                }
                            }
                        }
                    }
                }
                if (badItemType) {
                    Saxon.Eej.ee.schema.UserListType list = makeListType();
                    list.setItemTypeReference(new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_ERROR, getConfiguration(), this));
                    workingType = list;
                    setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                }
            }
            if ((members != null)) {
                foreach (Saxon.Eej.ee.schema.TypeReference o in members) {
                    Saxon.Hej.type.SchemaType typeDef = (Saxon.Hej.type.SchemaType)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(o, compiler, false));
                    if ((typeDef == null)) {
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    } else {
                        if ((typeDef is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                            bool b = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(typeDef)).fixup(compiler);
                            if (!(b)) {
                                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                return false;
                            }
                            Saxon.Hej.type.SchemaType s = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(typeDef)).getWorkingType();
                            if ((s != null)) {
                                o.setTarget(s);
                            }
                        } else {
                            if (typeDef.isComplexType()) {
                                compiler.error(("A member of a union type cannot be a complex type "+typeDef.getDescription()), this);
                                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                return false;
                            }
                        }
                    }
                }
            }
            bool result = makeWorkingType(compiler);
            if (result) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
                return true;
            } else {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
        }
        private bool makeWorkingType(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_LIST)) {
                bool badItemType = false;
                bool b = checkFacetsForListType(compiler);
                if (b) {
                    Saxon.Hej.type.SimpleType itemType = (Saxon.Hej.type.SimpleType)(itemTypeReference.getTarget());
                    if (!(itemType.allowsDerivation(Saxon.Hej.type.Derivation.DERIVATION_LIST))) {
                        badItemType = true;
                        compiler.error((("The item type of the list type "+getDescription())+" disallows derivation by list"), this);
                    }
                    if ((itemType is Saxon.Hej.type.ListType)) {
                        badItemType = true;
                        compiler.error((("The item type of the list type "+getDescription())+" is itself a list type"), this);
                    }
                    if (((itemType == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || (itemType == Saxon.Hej.type.AnySimpleType.getInstance()))) {
                        badItemType = true;
                        compiler.error((("The item type of the list type "+getDescription())+" must not be xs:anySimpleType or xs:anyAtomicType"), this);
                    }
                } else {
                    badItemType = true;
                }
                Saxon.Eej.ee.schema.UserListType list = makeListType();
                workingType = list;
                if (badItemType) {
                    list.setItemTypeReference(new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_ERROR, getConfiguration(), this));
                    return false;
                } else {
                    list.setItemTypeReference(itemTypeReference);
                }
            } else {
                if ((getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_UNION)) {
                    if ((members == null)) {
                        members = new System.Collections.Generic.List<Saxon.Eej.ee.schema.TypeReference>();
                    }
                    if (members.Count==0) {
                        compiler.error((("The union type "+getDescription())+" has no members"), this);
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        members.Add(new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_ERROR, getConfiguration(), this));
                    }
                    foreach (Saxon.Eej.ee.schema.TypeReference member in members) {
                        Saxon.Hej.type.SimpleType mem = (Saxon.Hej.type.SimpleType)(member.getTarget());
                        if (!(mem.allowsDerivation(Saxon.Hej.type.Derivation.DERIVATION_UNION))) {
                            compiler.error((((("The member type "+mem.getDescription())+" of the type ")+getDescription())+" disallows derivation by union"), this);
                            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            member.setTarget(Saxon.Hej.type.ErrorType.getInstance());
                        }
                    }
                    bool b = checkFacetsForUnionType(compiler);
                    if (!(b)) {
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    }
                    Saxon.Eej.ee.schema.UserUnionType union = makeUnionType();
                    union.setMemberTypeReferences(members);
                    workingType = union;
                    return (getFixupStatus() != /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                } else {
                    Saxon.Hej.type.SchemaType @base = getBaseType();
                    if (!(@base.allowsDerivation(Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION))) {
                        compiler.error((("The base type of the type "+getDescription())+" disallows derivation by restriction"), this);
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    }
                    if ((@base is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                        if ((((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(@base)).workingType != null)) {
                            @base = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(@base)).workingType;
                        } else {
                            if (((@base.getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID) || (((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(@base)).getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID))) {
                                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            } else {
                                ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(@base)).makeWorkingType(compiler);
                                @base = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(@base)).workingType;
                            }
                        }
                    }
                    if ((((@base != null) && (@base.getFingerprint() == Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE)) && !(getTypeName().hasURI(Saxon.Hej.om.NamespaceUri.SCHEMA)))) {
                        compiler.error("A simple type cannot be derived by restricting xs:anySimpleType", this);
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        setBaseTypeReference(new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.type.BuiltInAtomicType.STRING.getFingerprint(), getConfiguration(), this));
                    }
                    if ((((@base != null) && (@base.getFingerprint() == Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE)) && !(getTypeName().hasURI(Saxon.Hej.om.NamespaceUri.SCHEMA)))) {
                        compiler.error("A simple type cannot be derived by restricting xs:anyAtomicType", this);
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        setBaseTypeReference(new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.type.BuiltInAtomicType.STRING.getFingerprint(), getConfiguration(), this));
                    }
                    if ((@base is Saxon.Hej.type.SimpleType)) {
                        try {
                            checkFacetsForRestrictedType((Saxon.Hej.type.SimpleType)(@base), compiler);
                        } catch (Exception err) when (err is Saxon.Hej.type.SchemaException || err is Saxon.Hej.type.ValidationException) {
                            compiler.error(err.Message, this);
                            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            setFacetList(new System.Collections.Generic.List<Saxon.Eej.ee.schema.Facet>());
                        }
                        if (((@base is Saxon.Hej.type.ListType) || ((Saxon.Hej.type.SimpleType)(@base)).isListType())) {
                            bool b = checkFacetsForListType(compiler);
                            if (!(b)) {
                                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            }
                            Saxon.Eej.ee.schema.UserListType list = makeListType();
                            if ((@base is Saxon.Eej.ee.schema.UserListType)) {
                                list.setItemTypeReference(((Saxon.Eej.ee.schema.UserListType)(@base)).getItemTypeReference());
                                list.inheritSeparator((Saxon.Eej.ee.schema.UserListType)(@base));
                            } else {
                                System.Diagnostics.Debug.Assert((@base is Saxon.Hej.type.ListType));
                                list.setItemTypeReference(new Saxon.Eej.ee.schema.TypeReference(((Saxon.Hej.type.ListType)(@base)).getItemType().getFingerprint(), compiler.getConfiguration(), this));
                            }
                            workingType = list;
                        } else {
                            if (((@base is Saxon.Eej.ee.schema.UserUnionType) || ((Saxon.Hej.type.SimpleType)(@base)).isUnionType())) {
                                bool b = checkFacetsForUnionType(compiler);
                                if (!(b)) {
                                    setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                }
                                Saxon.Eej.ee.schema.UserUnionType union = makeUnionType();
                                if ((@base is Saxon.Eej.ee.schema.UserUnionType)) {
                                    union.setMemberTypeReferences(((Saxon.Eej.ee.schema.UserUnionType)(@base)).getMemberTypeReferences());
                                } else {
                                    if ((@base is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                                        union.setMemberTypeReferences(((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(@base)).members);
                                    }
                                }
                                workingType = union;
                            }
                        }
                    }
                    if ((workingType == null)) {
                        Saxon.Eej.ee.schema.UserAtomicType atomic = new Saxon.Eej.ee.schema.UserAtomicType(getConfiguration());
                        atomic.setBaseTypeReference(getBaseTypeReference());
                        atomic.setDerivationMethod(getDerivationMethod());
                        atomic.setTypeName(getTypeName(), getFingerprint());
                        atomic.setFacetList(getLocalFacetList());
                        atomic.convertFacetValues();
                        atomic.setFinalProhibitions(finalProhibitions);
                        atomic.setSystemId(getSystemId());
                        atomic.setLineNumber(getLineNumber());
                        atomic.setGeneratedId(getGeneratedId());
                        atomic.setContainingDeclaration(getContainingDeclarationName(), containingDeclarationIsElement());
                        atomic.setExtendedTypes(extendedTypes);
                        atomic.setRedefinitionLevel(getRedefinitionLevel());
                        atomic.setSchemaDocumentURI(getSchemaDocumentURI());
                        workingType = atomic;
                    }
                }
            }
            workingType.mergeEnumerationFacets(compiler);
            workingType.getExtendedFacetList();
            workingType.setContextComponent(getContextComponent());
            if (((workingType.getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID) || (getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID))) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            } else {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            }
            if ((getName() == null)) {
                getConfiguration().getSuperSchema().addAnonymousType(workingType);
            }
            return true;
        }
        private Saxon.Eej.ee.schema.UserUnionType makeUnionType() {
            Saxon.Eej.ee.schema.UserUnionType union = new Saxon.Eej.ee.schema.UserUnionType(getConfiguration());
            union.setBaseTypeReference(getBaseTypeReference());
            union.setDerivationMethod(getDerivationMethod());
            union.setTypeName(getTypeName(), getFingerprint());
            union.setFacetList(getLocalFacetList());
            union.convertFacetValues();
            union.setFinalProhibitions(finalProhibitions);
            union.setSystemId(getSystemId());
            union.setLineNumber(getLineNumber());
            union.setGeneratedId(getGeneratedId());
            union.setContainingDeclaration(getContainingDeclarationName(), containingDeclarationIsElement());
            union.setExtendedTypes(extendedTypes);
            union.setRedefinitionLevel(getRedefinitionLevel());
            union.setSchemaDocumentURI(getSchemaDocumentURI());
            return union;
        }
        private Saxon.Eej.ee.schema.UserListType makeListType() {
            Saxon.Eej.ee.schema.UserListType list = new Saxon.Eej.ee.schema.UserListType(getConfiguration());
            list.setBaseTypeReference(getBaseTypeReference());
            list.setDerivationMethod(getDerivationMethod());
            list.setTypeName(getTypeName(), getFingerprint());
            list.setFacetList(getLocalFacetList());
            list.convertFacetValues();
            list.setFinalProhibitions(finalProhibitions);
            list.setSystemId(getSystemId());
            list.setLineNumber(getLineNumber());
            list.setGeneratedId(getGeneratedId());
            list.setContainingDeclaration(getContainingDeclarationName(), containingDeclarationIsElement());
            list.setExtendedTypes(extendedTypes);
            list.setRedefinitionLevel(getRedefinitionLevel());
            list.setSchemaDocumentURI(getSchemaDocumentURI());
            list.setTokenSeparator(tokenSeparator);
            return list;
        }
        private bool checkFacetsForListType(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> facets = getLocalFacetList();
            foreach (Saxon.Eej.ee.schema.Facet facet in facets) {
                System.Type name = facet.GetType();
                if ((((((name.Equals(typeof(Saxon.Eej.ee.schema.MaxExclusiveFacet)) || name.Equals(typeof(Saxon.Eej.ee.schema.MaxInclusiveFacet))) || name.Equals(typeof(Saxon.Eej.ee.schema.MinExclusiveFacet))) || name.Equals(typeof(Saxon.Eej.ee.schema.MinInclusiveFacet))) || name.Equals(typeof(Saxon.Eej.ee.schema.TotalDigitsFacet))) || name.Equals(typeof(Saxon.Eej.ee.schema.FractionDigitsFacet)))) {
                    compiler.error(((("The "+name)+" facet is not allowed on the list type ")+getDescription()), this);
                    return false;
                }
            }
            return true;
        }
        private bool checkFacetsForUnionType(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> facets = getLocalFacetList();
            foreach (Saxon.Eej.ee.schema.Facet facet in facets) {
                System.Type facetClass = facet.GetType();
                if (((facetClass.Equals(typeof(Saxon.Eej.ee.schema.PatternFacet)) || facetClass.Equals(typeof(Saxon.Eej.ee.schema.EnumerationFacet))) || facetClass.Equals(typeof(Saxon.Eej.ee.schema.AssertionFacet)))) {
                } else {
                    compiler.error(((("The "+facet.getName())+" facet is not allowed on the union type ")+getDescription()), this);
                    return false;
                }
            }
            return true;
        }
        private void checkFacetsForRestrictedType(Saxon.Hej.type.SimpleType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Facet> facets = getLocalFacetList();
            foreach (Saxon.Eej.ee.schema.Facet facet in facets) {
                facet.checkFacetRestriction(this, @base, compiler);
            }
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((workingType != null)) {
                return workingType.validate(compiler);
            }
            return false;
        }
        public override Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            if ((workingType != null)) {
                return workingType.getTypedValue(value, resolver, rules);
            } else {
                throw new Saxon.Ejava.lang.UnsupportedOperationException(getUnresolvedMessage());
            }
        }
        public override bool isBuiltInType() {
            return false;
        }
        public override bool isNamespaceSensitive() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException(getUnresolvedMessage());
        }
        public override Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            if ((workingType != null)) {
                return workingType.validateContent(value, nsResolver, rules);
            } else {
                if ((getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID)) {
                    return new Saxon.Hej.type.ValidationFailure("Cannot validate value, because simple type definition is invalid");
                } else {
                    throw new Saxon.Ejava.lang.UnsupportedOperationException(getUnresolvedMessage());
                }
            }
        }
        public override void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            if ((workingType == null)) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException(getUnresolvedMessage());
            } else {
                workingType.serialize(serializer);
            }
        }
        protected internal override void serializeVariety(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException(getUnresolvedMessage());
        }
        private string getUnresolvedMessage() {
            return ((((((("SimpleTypeDefinition "+this)+" should have been resolved by now (")+getDescription())+") #")+getFingerprint())+": ")+getNamePool().getClarkName(getFingerprint()));
        }
    
    }
}