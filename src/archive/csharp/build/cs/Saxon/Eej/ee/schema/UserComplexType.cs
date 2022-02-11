using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class UserComplexType : Saxon.Eej.ee.schema.UserDefinedType, Saxon.Hej.type.ComplexType, Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Eej.ee.schema.SerializableSchemaComponent {
        private bool debug = false;
        private Saxon.Hej.type.ComplexVariety variety;
        private bool _abstract = false;
        private bool allContent = false;
        private Saxon.Eej.ee.schema.AttributeGroupDecl attributes = null;
        private Saxon.Eej.ee.schema.AttributeGroupDecl extendedAttributes = null;
        private int block = 0;
        private Saxon.Eej.ee.schema.TypeReference simpleContentTypeRef = null;
        private Saxon.Eej.ee.schema.Particle particle;
        private Saxon.Eej.ee.schema.Particle extendedParticle = null;
        private bool restricted = false;
        private Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine;
        private bool compiled = false;
        private System.Collections.Generic.ISet<Saxon.Eej.ee.schema.Assertion> assertions;
        private Saxon.Eej.ee.schema.Wildcard openContentWildcard;
        private Saxon.Hej.type.OpenContentVariety openContentMode = Saxon.Hej.type.OpenContentVariety.ABSENT;
        private bool openContentAppliesToEmpty;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.type.SchemaType> contextDeterminedTypesForElements;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.type.SimpleType> contextDeterminedTypesForAttributes;
        private int xsdVersion = Saxon.Hej.Configuration.XSD10;
        public UserComplexType(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Ejavax.xml.transform.SourceLocator locator)  {
            setConfiguration(config);
            setLocator(locator);
            attributes = new Saxon.Eej.ee.schema.AttributeGroupDecl(config);
            attributes.setLocator(locator);
            machine = new Saxon.Eej.ee.schema.fsa.FiniteStateMachine(this, false);
        }
        public Saxon.Hej.type.ComplexVariety getVariety() {
            return variety;
        }
        public override string getEQName() {
            return ((("Q{"+getTargetNamespace())+"}")+getName());
        }
        public void setSaxonDebug(bool debug) {
            this.debug = debug;
        }
        public int getLanguageVersion() {
            return xsdVersion;
        }
        public void setFiniteStateMachine(Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine) {
            this.machine = machine;
        }
        public void addAttributeUse(Saxon.Eej.ee.schema.AttributeUse use) {
            attributes.addAttributeUse(use);
        }
        public void setEmptyCombinedAttributeGroup() {
            extendedAttributes = new Saxon.Eej.ee.schema.AttributeGroupDecl(getConfiguration());
        }
        public void addNonInheritedAttributeUse(Saxon.Eej.ee.schema.AttributeUse use) {
            attributes.addAttributeUse(use);
            extendedAttributes.addAttributeUse(use);
        }
        public void addAttributeGroupReference(Saxon.Eej.ee.schema.AttributeGroupReference attrGroupRef) {
            attributes.addAttributeGroupReference(attrGroupRef);
        }
        public Saxon.Eej.ee.schema.AttributeGroupDecl getAttributeGroup() {
            return attributes;
        }
        public Saxon.Eej.ee.schema.AttributeGroupDecl getCombinedAttributeGroup() {
            if ((extendedAttributes == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException((("Extended attribute group for complex type "+getDescription())+" has not been computed"));
            }
            return extendedAttributes;
        }
        private bool makeCombinedAttributeGroup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((extendedAttributes != null)) {
                return true;
            } else {
                if ((getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) {
                    bool result;
                    if ((compiler != null)) {
                        result = attributes.validate(compiler);
                        if (!(result)) {
                            return false;
                        }
                    }
                    Saxon.Hej.type.SchemaType @base = getBaseType();
                    if (@base.isSimpleType()) {
                        extendedAttributes = attributes;
                        return true;
                    }
                    if ((@base is Saxon.Eej.ee.schema.UserComplexType)) {
                        result = ((Saxon.Eej.ee.schema.UserComplexType)(@base)).makeCombinedAttributeGroup(compiler);
                        if (!(result)) {
                            return false;
                        }
                        extendedAttributes = new Saxon.Eej.ee.schema.AttributeGroupDecl(getConfiguration());
                        extendedAttributes.setLocator(this);
                        foreach (Saxon.Eej.ee.schema.AttributeUse att in getAttributeGroup().getAttributeUses()) {
                            extendedAttributes.addAttributeUse(att);
                        }
                        Saxon.Eej.ee.schema.AttributeGroupDecl baseAtts = ((Saxon.Eej.ee.schema.UserComplexType)(@base)).getCombinedAttributeGroup();
                        foreach (Saxon.Eej.ee.schema.AttributeUse att in baseAtts.getAttributeUses()) {
                            extendedAttributes.addAttributeUse(att);
                        }
                        Saxon.Eej.ee.schema.AttributeWildcard baseWild = baseAtts.getAttributeWildcard(compiler);
                        Saxon.Eej.ee.schema.AttributeWildcard thisWild = attributes.getAttributeWildcard(compiler);
                        if ((baseWild != null)) {
                            if ((thisWild != null)) {
                                Saxon.Eej.ee.schema.Wildcard union = /*134*/Saxon.Eej.ee.schema.Wildcard.makeUnion(thisWild.getWildcard(), baseWild.getWildcard(), getNamePool());
                                Saxon.Eej.ee.schema.AttributeWildcard newawc = (union.isInexpressible() ? thisWild : new Saxon.Eej.ee.schema.AttributeWildcard(union));
                                extendedAttributes.setAnyAttribute(newawc);
                            } else {
                                extendedAttributes.setAnyAttribute(baseWild);
                            }
                        } else {
                            extendedAttributes.setAnyAttribute(thisWild);
                        }
                    } else {
                        extendedAttributes = attributes;
                    }
                    return true;
                } else {
                    bool result;
                    if ((compiler != null)) {
                        result = attributes.validate(compiler);
                        if (!(result)) {
                            return false;
                        }
                    }
                    Saxon.Hej.type.SchemaType @base = getBaseType();
                    extendedAttributes = new Saxon.Eej.ee.schema.AttributeGroupDecl(getConfiguration());
                    extendedAttributes.setLocator(this);
                    Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.AttributeUse> map = new Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.AttributeUse>(20);
                    if ((@base is Saxon.Eej.ee.schema.UserComplexType)) {
                        ((Saxon.Eej.ee.schema.UserComplexType)(@base)).makeCombinedAttributeGroup(compiler);
                        Saxon.Eej.ee.schema.AttributeGroupDecl baseAtts = ((Saxon.Eej.ee.schema.UserComplexType)(@base)).getCombinedAttributeGroup();
                        foreach (Saxon.Eej.ee.schema.AttributeUse att in baseAtts.getAttributeUses()) {
                            if (!(att.isProhibited())) {
                                map.put(att.getAttributeDeclaration().getFingerprint(), att);
                            }
                        }
                        foreach (Saxon.Eej.ee.schema.AttributeUse att in getAttributeGroup().getAttributeUses()) {
                            int key = att.getAttributeDeclaration().getFingerprint();
                            Saxon.Eej.ee.schema.AttributeUse old = map.get(key);
                            if ((old != null)) {
                                if (att.isProhibited()) {
                                    map.remove(key);
                                } else {
                                    map.put(key, att);
                                }
                            } else {
                                map.put(key, att);
                            }
                        }
                        System.Collections.Generic.IEnumerator<Saxon.Eej.ee.schema.AttributeUse> iter2 = map.valueIterator();
                        while (iter2.MoveNext() /*UNSAFE CONVERSION*/) {
                            extendedAttributes.addAttributeUse(iter2.Current);
                        }
                        Saxon.Eej.ee.schema.AttributeWildcard wat = getAttributeGroup().getAttributeWildcard(compiler);
                        if ((wat != null)) {
                            extendedAttributes.setAnyAttribute(wat);
                        }
                    } else {
                        extendedAttributes = attributes;
                    }
                    return true;
                }
            }
        }
        public override int getBlock() {
            return block;
        }
        public bool isAbstract() {
            return _abstract;
        }
        public bool isComplexContent() {
            return (variety != Saxon.Hej.type.ComplexVariety.SIMPLE);
        }
        public bool isSimpleContent() {
            return (variety == Saxon.Hej.type.ComplexVariety.SIMPLE);
        }
        public bool isAllContent() {
            return allContent;
        }
        public bool computeIsAllContent() {
            if (isSimpleContent()) {
                return false;
            }
            if (((particle is Saxon.Eej.ee.schema.ModelGroupParticle) && ((Saxon.Eej.ee.schema.ModelGroupParticle)(particle)).getGroup().containsAll(true))) {
                return allContent = true;
            }
            Saxon.Hej.type.SchemaType @base;
            if (((((particle == null) && (getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) && ((@base = getBaseType()) is Saxon.Hej.type.ComplexType)) && ((Saxon.Hej.type.ComplexType)(@base)).isAllContent())) {
                return allContent = true;
            }
            return allContent = false;
        }
        public Saxon.Eej.ee.schema.ModelGroupParticle getAllCompositor() {
            Saxon.Hej.type.SchemaType @base;
            if ((extendedParticle is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                Saxon.Eej.ee.schema.ModelGroup group = ((Saxon.Eej.ee.schema.ModelGroupParticle)(extendedParticle)).getGroup();
                if ((group is Saxon.Eej.ee.schema.AllModelGroup)) {
                    return (Saxon.Eej.ee.schema.ModelGroupParticle)(extendedParticle);
                }
                if (group.containsAll(true)) {
                    Saxon.Eej.ee.schema.ModelGroupParticle gpref = (Saxon.Eej.ee.schema.ModelGroupParticle)(extendedParticle);
                    while (true) {
                        Saxon.Eej.ee.schema.Particle part;
                        System.Collections.Generic.IEnumerator<Saxon.Eej.ee.schema.Particle> iter = group.getParticles().GetEnumerator();
                        if (iter.MoveNext() /*UNSAFE CONVERSION*/) {
                            part = iter.Current;
                        } else {
                            throw new Saxon.Ejava.lang.IllegalStateException("Empty group");
                        }
                        if ((part is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                            if ((part == gpref)) {
                                throw new Saxon.Ejava.lang.IllegalStateException("Circular group");
                            }
                            gpref = (Saxon.Eej.ee.schema.ModelGroupParticle)(part);
                            group = gpref.getGroup();
                            if ((group is Saxon.Eej.ee.schema.AllModelGroup)) {
                                return gpref;
                            }
                        } else {
                            throw new Saxon.Ejava.lang.IllegalStateException("Failure finding xs:all model group");
                        }
                    }
                }
                return null;
            } else {
                if (((((extendedParticle == null) && (getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) && ((@base = getBaseType()) is Saxon.Hej.type.ComplexType)) && ((Saxon.Hej.type.ComplexType)(@base)).isAllContent())) {
                    return ((Saxon.Eej.ee.schema.UserComplexType)(@base)).getAllCompositor();
                } else {
                    return null;
                }
            }
        }
        public void setSimpleContentTypeReference(Saxon.Eej.ee.schema.TypeReference @ref) {
            simpleContentTypeRef = @ref;
        }
        public Saxon.Eej.ee.schema.TypeReference getSimpleContentTypeReference() {
            if ((simpleContentTypeRef == null)) {
                if (isSimpleContent()) {
                    Saxon.Hej.type.SchemaType @base = getBaseType();
                    if (@base.isSimpleType()) {
                        simpleContentTypeRef = getBaseTypeReference();
                    } else {
                        if ((@base is Saxon.Eej.ee.schema.UserComplexType)) {
                            simpleContentTypeRef = ((Saxon.Eej.ee.schema.UserComplexType)(@base)).getSimpleContentTypeReference();
                        }
                    }
                }
            }
            return simpleContentTypeRef;
        }
        public Saxon.Hej.type.SimpleType getSimpleContentType() {
            Saxon.Hej.type.SimpleType simpleContentType;
            Saxon.Eej.ee.schema.TypeReference @ref = getSimpleContentTypeReference();
            if ((@ref == null)) {
                return null;
            }
            simpleContentType = (Saxon.Hej.type.SimpleType)(@ref.getTarget());
            if ((simpleContentType is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                simpleContentType = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(simpleContentType)).getWorkingType();
            }
            return simpleContentType;
        }
        public bool isRestricted() {
            return restricted;
        }
        public bool isEmptyContent() {
            return (variety == Saxon.Hej.type.ComplexVariety.EMPTY);
        }
        public bool isMixedContent() {
            return (variety == Saxon.Hej.type.ComplexVariety.MIXED);
        }
        public override bool isSimpleType() {
            return false;
        }
        public override bool isAtomicType() {
            return false;
        }
        public override bool isIdType() {
            return (isSimpleContent() && getSimpleContentType().isIdType());
        }
        public override bool isIdRefType() {
            return (isSimpleContent() && getSimpleContentType().isIdRefType());
        }
        public Saxon.Eej.ee.schema.Wildcard getOpenContentWildcard() {
            return openContentWildcard;
        }
        public Saxon.Hej.type.OpenContentVariety getOpenContentMode() {
            return openContentMode;
        }
        public string getOpenContentModeString() {
            switch (openContentMode) {
              case Saxon.Hej.type.OpenContentVariety.ABSENT:
                return "absent";
              case Saxon.Hej.type.OpenContentVariety.NONE:
                return "none";
              case Saxon.Hej.type.OpenContentVariety.INTERLEAVE:
                return "interleave";
              case Saxon.Hej.type.OpenContentVariety.SUFFIX:
                return "suffix";
            }
            return null;
        }
        public void setAbstract(bool isAbstract) {
            this._abstract = isAbstract;
        }
        public void setAnyAttribute(Saxon.Eej.ee.schema.AttributeWildcard wildcard) {
            attributes.setAnyAttribute(wildcard);
        }
        public void setNonInheritedAnyAttribute(Saxon.Eej.ee.schema.AttributeWildcard wildcard) {
            attributes.setAnyAttribute(wildcard);
            extendedAttributes.setAnyAttribute(wildcard);
        }
        public void setBlock(int block) {
            this.block = block;
        }
        public void setVariety(Saxon.Hej.type.ComplexVariety variety) {
            this.variety = variety;
        }
        public void setRestriction(bool restricted) {
            this.restricted = restricted;
        }
        public void setParticle(Saxon.Eej.ee.schema.Particle particle) {
            this.particle = particle;
        }
        public void setOpenContentWildcard(Saxon.Eej.ee.schema.Wildcard wildcard, string mode, bool appliesToEmpty) {
            openContentWildcard = wildcard;
            if ("interleave".Equals(mode)) {
                openContentMode = Saxon.Hej.type.OpenContentVariety.INTERLEAVE;
            } else {
                if ("suffix".Equals(mode)) {
                    openContentMode = Saxon.Hej.type.OpenContentVariety.SUFFIX;
                } else {
                    if ("none".Equals(mode)) {
                        openContentMode = Saxon.Hej.type.OpenContentVariety.NONE;
                    } else {
                        throw new Saxon.Ejava.lang.IllegalArgumentException(("Invalid value of open content mode - "+mode));
                    }
                }
            }
            openContentAppliesToEmpty = appliesToEmpty;
        }
        public void addAssertion(Saxon.Eej.ee.schema.Assertion assertion) {
            if ((assertions == null)) {
                assertions = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.Assertion>(4);
            }
            assertions.Add(assertion);
        }
        public bool hasAssertions() {
            return ((assertions != null) && !(assertions.Count==0));
        }
        public System.Collections.Generic.ISet<Saxon.Eej.ee.schema.Assertion> getAssertions() {
            if ((assertions == null)) {
                return new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.Assertion>();
            } else {
                return assertions;
            }
        }
        public System.Collections.Generic.ISet<Saxon.Eej.ee.schema.Assertion> getAssertions(Saxon.Eej.ee.stream.Sweep sweep) {
            if ((assertions == null)) {
                return new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.Assertion>();
            } else {
                System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.Assertion> filtered = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.Assertion>(assertions.Count);
                foreach (Saxon.Eej.ee.schema.Assertion a in assertions) {
                    if ((a.getSweep() == sweep)) {
                        filtered.Add(a);
                    }
                }
                return filtered;
            }
        }
        public override void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (System.Object reference in references) {
                if (((Saxon.Hej.type.SchemaType)(reference)).isSameType(this)) {
                    compiler.error((("The definition of the complex type "+getDescription())+" is circular"), this);
                    throw new Saxon.Hej.type.SchemaException("Circular definition found");
                }
            }
            references.Push(this);
            getBaseTypeReference().tryToResolve(compiler, false);
            if (getBaseTypeReference().isResolved()) {
                Saxon.Hej.type.SchemaType @base = (Saxon.Hej.type.SchemaType)(getBaseTypeReference().getTarget());
                if ((@base is Saxon.Eej.ee.schema.UserSchemaComponent)) {
                    ((Saxon.Eej.ee.schema.UserSchemaComponent)(@base)).lookForCycles(references, compiler);
                }
            }
            references.Pop();
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
                if ((attributes != null)) {
                    bool ok = attributes.fixup(compiler);
                    if (!(ok)) {
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    }
                }
                if ((simpleContentTypeRef != null)) {
                    Saxon.Hej.type.SchemaType type = (Saxon.Hej.type.SchemaType)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(simpleContentTypeRef, compiler, false));
                    if ((type == null)) {
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    }
                    if ((type is Saxon.Eej.ee.schema.UserSchemaComponent)) {
                        bool ok = ((Saxon.Eej.ee.schema.UserSchemaComponent)(type)).fixup(compiler);
                        if (!(ok)) {
                            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    }
                }
                if ((getBaseTypeReference() != null)) {
                    Saxon.Hej.type.SchemaType type = (Saxon.Hej.type.SchemaType)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(getBaseTypeReference(), compiler, true));
                    if ((type == null)) {
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    }
                    if ((type is Saxon.Eej.ee.schema.UserSchemaComponent)) {
                        ((Saxon.Eej.ee.schema.UserSchemaComponent)(type)).fixup(compiler);
                    }
                }
                if ((particle != null)) {
                    particle.fixup(compiler);
                }
            }
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result = true;
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID)) {
                return false;
            }
            if (!(isValidationNeeded())) {
                return true;
            }
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
            xsdVersion = compiler.getLanguageVersion();
            if ((particle != null)) {
                result = particle.validate(compiler);
                if (!(result)) {
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                }
            }
            if ((getBaseTypeReference() != null)) {
                /*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(getBaseTypeReference(), compiler, true);
                if (!(getBaseTypeReference().isResolved())) {
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                }
            }
            Saxon.Hej.type.SchemaType @base = getBaseType();
            if ((@base is Saxon.Eej.ee.schema.UserComplexType)) {
                lookForCycles(new System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent>(), compiler);
                result = ((Saxon.Eej.ee.schema.UserComplexType)(@base)).validate(compiler);
                foreach (Saxon.Eej.ee.schema.Assertion a in ((Saxon.Eej.ee.schema.UserComplexType)(@base)).getAssertions()) {
                    addAssertion(a);
                }
            }
            if ((result&isSimpleContent())) {
                result &= validateSimpleContent(compiler, @base);
            }
            if (result) {
                result = makeCombinedAttributeGroup(compiler);
            }
            if (result) {
                result = extendedAttributes.validate(compiler);
            }
            if ((getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) {
                result &= validateExtension(compiler, @base);
            }
            if ((getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) {
                result &= validateRestriction(compiler, @base);
            }
            allContent = computeIsAllContent();
            if (result) {
                compile(compiler);
            }
            setValidationStatus((result ? /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED : /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID));
            return result;
        }
        private bool validateSimpleContent(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.type.SchemaType @base) {
            if ((simpleContentTypeRef == null)) {
                if ((getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID)) {
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                } else {
                    if (@base.isSimpleType()) {
                        simpleContentTypeRef = getBaseTypeReference();
                    } else {
                        if ((@base.isComplexType() && ((Saxon.Hej.type.ComplexType)(@base)).isSimpleContent())) {
                            simpleContentTypeRef = ((Saxon.Eej.ee.schema.UserComplexType)(@base)).getSimpleContentTypeReference();
                        } else {
                            string err = ("The complex type "+getDescription());
                            err += " has simple content but its base type has complex content";
                            compiler.error(err, this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    }
                }
            }
            Saxon.Hej.type.SimpleType content = (Saxon.Hej.type.SimpleType)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(getSimpleContentTypeReference(), compiler, false));
            if ((content is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                Saxon.Hej.type.SchemaType type = compiler.getPreparedSchema().getSchemaType(content.getStructuredQName());
                if ((type != null)) {
                    if ((type.getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID)) {
                        setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    } else {
                        if (type.isSimpleType()) {
                            if ((type is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                                type = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(type)).getWorkingType();
                            }
                            getSimpleContentTypeReference().setTarget(type);
                        } else {
                            string err = ("The complex type "+getDescription());
                            err += " has simple content that references a complex type";
                            compiler.error(err, this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    }
                } else {
                }
                if ((getSimpleContentType() is Saxon.Eej.ee.schema.UserSimpleType)) {
                    return ((Saxon.Eej.ee.schema.UserSimpleType)(getSimpleContentType())).validate(compiler);
                }
            }
            return true;
        }
        private bool validateExtension(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.type.SchemaType @base) {
            if ((@base is Saxon.Eej.ee.schema.UserDefinedType)) {
                System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType> existingExtensions = ((Saxon.Eej.ee.schema.UserDefinedType)(@base)).getExtensionTypes();
                foreach (Saxon.Eej.ee.schema.UserComplexType ex in existingExtensions) {
                    if ((ex.getTypeName().Equals(getTypeName()) && ex.isSameType(this))) {
                        return true;
                    }
                }
            }
            Saxon.Eej.config.EnterpriseConfiguration config = getConfiguration();
            if (config.isSealedNamespace(@base.getTargetNamespace())) {
                string err = ((("It is not possible to extend type "+@base.getDescription())+" because that type has already been used for validating instance documents")+" or for compiling queries or stylesheets");
                compiler.error(err, this);
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            if (!(@base.allowsDerivation(Saxon.Hej.type.Derivation.DERIVATION_EXTENSION))) {
                string err = (("The complex type "+getDescription())+" is derived by extension, but the base type prohibits extension");
                compiler.error(err, this);
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            if ((((variety == Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY) && isLocallyEmpty()) && isEmptyContent(@base))) {
                variety = Saxon.Hej.type.ComplexVariety.EMPTY;
                particle = null;
            }
            if ((isComplexContent() && @base.isSimpleType())) {
                string err = (((("The type "+getDescription())+" has complex content so it ")+"cannot be based on the simple type ")+@base.getDescription());
                compiler.error(err, this);
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                if ((isAllParticle(particle) && !(isEmptyContent(@base)))) {
                    string err = ((("The type "+getDescription())+" is defined with xs:all so (in XSD 1.0) it ")+"cannot be derived by extension from a non-empty type");
                    compiler.error(err, this);
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                }
                if (((!(isLocallyEmpty()) && (@base is Saxon.Eej.ee.schema.UserComplexType)) && isAllParticle(((Saxon.Eej.ee.schema.UserComplexType)(@base)).particle))) {
                    string err = ("In XSD 1.0, a type cannot be derived by extension from a "+"type defined using xs:all unless the extension is empty");
                    compiler.error(err, this);
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                }
            } else {
                if ((isAllParticle(particle) && (@base is Saxon.Eej.ee.schema.UserComplexType))) {
                    if (isAllParticle(((Saxon.Eej.ee.schema.UserComplexType)(@base)).particle)) {
                        Saxon.Eej.ee.schema.AllModelGroup amg = (Saxon.Eej.ee.schema.AllModelGroup)(((Saxon.Eej.ee.schema.ModelGroupParticle)(((Saxon.Eej.ee.schema.UserComplexType)(@base)).particle)).getGroup());
                        if ((amg.getSimplifiedContentModel().Count==0 && ((Saxon.Eej.ee.schema.UserComplexType)(@base)).isMixedContent())) {
                            string err = ((((("The type "+getDescription())+" defined with xs:all cannot ")+"be derived by extension from a base type that is defined using xs:all with mixed ")+"content but no element content. This is because the base type ")+"is treated as if the compositor were xs:sequence rather than xs:all");
                            compiler.error(err, this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                        if ((particle.getMinOccurs() != ((Saxon.Eej.ee.schema.UserComplexType)(@base)).particle.getMinOccurs())) {
                            string err = (((("The type "+getDescription())+" defined with xs:all cannot ")+"be derived by extension from a base type that is defined using xs:all because ")+"the two xs:all particles have different minOccurs values");
                            compiler.error(err, this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                        Saxon.Eej.ee.schema.Particle p = makeExtendedParticle(compiler);
                        bool validParticle = p.validate(compiler);
                        if (!(validParticle)) {
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    } else {
                        if (((Saxon.Eej.ee.schema.UserComplexType)(@base)).isEmptyContent()) {
                        } else {
                            string err = ((("The type "+getDescription())+" is defined with xs:all so it ")+"cannot be derived by extension unless the base type is empty or is itself defined with xs:all");
                            compiler.error(err, this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    }
                }
                if ((((!(isLocallyEmpty()) && !(isAllParticle(particle))) && (@base is Saxon.Eej.ee.schema.UserComplexType)) && isAllParticle(((Saxon.Eej.ee.schema.UserComplexType)(@base)).particle))) {
                    string err = (("A type cannot be derived by extension from a "+"type defined using xs:all unless the extension is empty ")+"or the extension is itself defined using xs:all");
                    compiler.error(err, this);
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                }
            }
            if (((isComplexContent() && (@base is Saxon.Eej.ee.schema.UserComplexType)) && ((Saxon.Eej.ee.schema.UserComplexType)(@base)).isSimpleContent())) {
                if ((isLocallyEmpty() && (compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10))) {
                    setVariety(Saxon.Hej.type.ComplexVariety.SIMPLE);
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED);
                    return validate(compiler);
                }
                string err = ((((("The type "+getDescription())+" has complex content so it ")+"cannot be derived by extension from the complex type ")+@base.getDescription())+" which has simple content");
                compiler.error(err, this);
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            if (isSimpleContent()) {
                if (@base.isSimpleType()) {
                } else {
                    if ((@base is Saxon.Hej.type.AnyType)) {
                        string err = ((("The type "+getDescription())+" has simple content so it ")+"cannot be derived by extension from xs:anyType");
                        compiler.error(err, this);
                        setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    } else {
                        if (!(((Saxon.Hej.type.ComplexType)(@base)).isSimpleContent())) {
                            string err = ((((("The type "+getDescription())+" has simple content so it ")+"cannot be derived by extension from the type ")+@base.getDescription())+" which has complex content");
                            compiler.error(err, this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    }
                }
            }
            if (@base.isComplexType()) {
                Saxon.Hej.type.ComplexType cbase = (Saxon.Hej.type.ComplexType)(@base);
                if (isMixedContent()) {
                    if ((((@base is Saxon.Hej.type.AnyType) || cbase.isMixedContent()) || cbase.isEmptyContent())) {
                    } else {
                        string err = ((("The type "+getDescription())+" has mixed content, ")+"so its base type must have mixed or empty content");
                        compiler.error(err, this);
                        setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    }
                } else {
                    if ((particle != null)) {
                        if (((@base is Saxon.Hej.type.ComplexType) && cbase.isMixedContent())) {
                            string err = ((("The type "+getDescription())+" has element-only content, ")+"so it cannot be derived by extension from a type with mixed content");
                            compiler.error(err, this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    } else {
                        if (isLocallyEmpty()) {
                            if (((@base is Saxon.Hej.type.ComplexType) && cbase.isMixedContent())) {
                                setVariety(Saxon.Hej.type.ComplexVariety.MIXED);
                            }
                        }
                    }
                }
                if ((@base is Saxon.Eej.ee.schema.UserComplexType)) {
                    if ((openContentMode == Saxon.Hej.type.OpenContentVariety.NONE)) {
                    }
                    if ((openContentWildcard == null)) {
                        openContentWildcard = ((Saxon.Eej.ee.schema.UserComplexType)(@base)).getOpenContentWildcard();
                        openContentMode = ((Saxon.Eej.ee.schema.UserComplexType)(@base)).getOpenContentMode();
                    } else {
                        Saxon.Eej.ee.schema.Wildcard baseOpenWildcard = ((Saxon.Eej.ee.schema.UserComplexType)(@base)).getOpenContentWildcard();
                        bool baseOpenWildcardInterleaved = (((Saxon.Eej.ee.schema.UserComplexType)(@base)).getOpenContentMode() == Saxon.Hej.type.OpenContentVariety.INTERLEAVE);
                        if ((baseOpenWildcard != null)) {
                            if ((((openContentWildcard != null) && baseOpenWildcardInterleaved) && (openContentMode != Saxon.Hej.type.OpenContentVariety.INTERLEAVE))) {
                                string err = ((("Invalid extension. The type "+getDescription())+" has interleaved open content,")+" but its base type has suffix open content");
                                compiler.error(err, this);
                                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                return false;
                            } else {
                                if ((openContentWildcard == null)) {
                                    string err = (("Invalid extension. The base type of type "+getDescription())+" allows open content, but this type does not");
                                    compiler.error(err, this);
                                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                    return false;
                                }
                            }
                            if ((openContentWildcard != null)) {
                                openContentWildcard = /*134*/Saxon.Eej.ee.schema.Wildcard.makeUnion(openContentWildcard, baseOpenWildcard, getNamePool());
                            }
                        }
                    }
                }
                if (isComplexContent()) {
                    Saxon.Eej.ee.schema.Particle p = makeExtendedParticle(compiler);
                    bool ok = ((p == null) || p.validate(compiler));
                    if (!(ok)) {
                        setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    }
                }
                foreach (Saxon.Eej.ee.schema.AttributeUse au in attributes.getAttributeUses()) {
                    Saxon.Hej.om.StructuredQName fp = au.getTargetComponentName();
                    if ((@base is Saxon.Eej.ee.schema.UserComplexType)) {
                        Saxon.Hej.type.SchemaType baseComplexType = @base;
                        while ((baseComplexType is Saxon.Eej.ee.schema.UserComplexType)) {
                            int derivation = baseComplexType.getDerivationMethod();
                            Saxon.Hej.type.SchemaType g = baseComplexType.getBaseType();
                            if (((g is Saxon.Eej.ee.schema.UserComplexType) && (derivation == Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION))) {
                                Saxon.Eej.ee.schema.AttributeUse baseAttributeUse = ((Saxon.Eej.ee.schema.UserComplexType)(g)).getCombinedAttributeGroup().getAttributeUse(fp);
                                if ((baseAttributeUse != null)) {
                                    Saxon.Hej.type.SchemaType original = ((/*1430*/Saxon.Hej.type.SimpleType)baseAttributeUse.getAttributeDeclaration().getType());
                                    if ((original != null)) {
                                        Saxon.Hej.type.SchemaType derived = ((/*1430*/Saxon.Hej.type.SimpleType)au.getAttributeDeclaration().getType());
                                        try {
                                            derived.checkTypeDerivationIsOK(original, 0);
                                        } catch (Saxon.Hej.type.SchemaException se) {
                                            string err = (((((((("The type "+getDescription())+" is derived by extension, and reintroduces")+" attribute ")+fp.getStructuredQName().getEQName())+" which was defined in the content model of the ancestor type ")+g.getDescription())+" with an incompatible type. ")+se.Message);
                                            compiler.error(err, this);
                                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                            return false;
                                        }
                                    }
                                }
                            }
                            baseComplexType = g;
                        }
                    }
                }
                if ((particle != null)) {
                    Saxon.Hej.z.IntHashSet extensionElements = new Saxon.Hej.z.IntHashSet(10);
                    particle.gatherAllPermittedElements(extensionElements, true);
                    Saxon.Hej.z.IntIterator extensionElementIter = extensionElements.iterator();
                    while (extensionElementIter.hasNext()) {
                        int fp = extensionElementIter.next();
                        if ((@base is Saxon.Eej.ee.schema.UserComplexType)) {
                            Saxon.Hej.type.SchemaType particleBaseType = @base;
                            while ((particleBaseType is Saxon.Eej.ee.schema.UserComplexType)) {
                                int derivation = particleBaseType.getDerivationMethod();
                                Saxon.Hej.type.SchemaType g = particleBaseType.getBaseType();
                                if (((g is Saxon.Eej.ee.schema.UserComplexType) && (derivation == Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION))) {
                                    Saxon.Hej.type.SchemaType original = ((Saxon.Eej.ee.schema.UserComplexType)(g)).getElementParticleType(fp, false);
                                    if ((original != null)) {
                                        Saxon.Hej.type.SchemaType derived = particle.getElementParticleType(fp);
                                        try {
                                            derived.checkTypeDerivationIsOK(original, 0);
                                        } catch (Saxon.Hej.type.SchemaException se) {
                                            string err = (((((((("The type "+getDescription())+" is derived by extension, and reintroduces")+" element ")+getNamePool().getClarkName(fp))+" which was defined in the content model of the ancestor type ")+g.getDescription())+" with an incompatible type. ")+se.Message);
                                            compiler.error(err, this);
                                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                            return false;
                                        }
                                    }
                                }
                                particleBaseType = g;
                            }
                        }
                    }
                }
            }
            Saxon.Hej.type.SchemaType baseType = @base;
            while (true) {
                if ((baseType is Saxon.Eej.ee.schema.UserDefinedType)) {
                    if ((baseType is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                        baseType = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(baseType)).getWorkingType();
                    }
                    ((Saxon.Eej.ee.schema.UserDefinedType)(baseType)).registerExtensionType(this);
                    baseType = baseType.getBaseType();
                } else {
                    if (((baseType is Saxon.Hej.type.SimpleType) && ((Saxon.Hej.type.SimpleType)(baseType)).isBuiltInType())) {
                        compiler.getPreparedSchema().registerExtensionOfBuiltInType(baseType.getFingerprint(), this);
                        baseType = baseType.getBaseType();
                    } else {
                        break ;
                    }
                }
            }
            return true;
        }
        private bool validateRestriction(Saxon.Eej.ee.schema.SchemaCompiler compiler, Saxon.Hej.type.SchemaType @base) {
            if (!(@base.allowsDerivation(Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION))) {
                string err = (("The complex type "+getDescription())+" is derived by restriction, but the base type prohibits restriction");
                compiler.error(err, this);
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            if ((((variety == Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY) && (((openContentWildcard == null) || !(openContentAppliesToEmpty)))) && isLocallyEmpty())) {
                variety = Saxon.Hej.type.ComplexVariety.EMPTY;
                particle = null;
            }
            if (@base.isSimpleType()) {
                string err = ("The complex type "+getDescription());
                err += " is defined as a restriction of the simple type ";
                err += @base.getDescription();
                compiler.error(err, this);
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            } else {
                if (@base.isComplexType()) {
                    Saxon.Hej.type.ComplexType cbase = (Saxon.Hej.type.ComplexType)(@base);
                    bool OK = false;
                    if (isSimpleContent()) {
                        if (cbase.isSimpleContent()) {
                            Saxon.Hej.type.SimpleType thisContent = getSimpleContentType();
                            Saxon.Hej.type.SimpleType baseContent = cbase.getSimpleContentType();
                            string reason;
                            if (((thisContent == null) || (baseContent == null))) {
                                reason = "Cannot establish the simple content type";
                            } else {
                                reason = /*134*/Saxon.Eej.ee.schema.UserSimpleType.isTypeDerivationOK(thisContent, baseContent, 0);
                            }
                            if ((reason == null)) {
                                OK = true;
                            } else {
                                string err = (((("The complex type "+getDescription())+" has simple content of a type that is not validly derived from that ")+"of the base type. ")+reason);
                                compiler.error(err, this);
                                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                return false;
                            }
                        }
                        if (!(OK)) {
                            if (!(cbase.isMixedContent())) {
                                string err = ("The complex type "+getDescription());
                                err += " has simple content so its base type, if complex, must have mixed content";
                                compiler.error(err, this);
                                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                return false;
                            }
                            if (!(((Saxon.Hej.type.ComplexType)(@base)).isEmptiable())) {
                                string err = (("The complex type "+getDescription())+" has simple content so its base type, if complex, must allow empty content");
                                compiler.error(err, this);
                                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                return false;
                            }
                            Saxon.Hej.type.SchemaType simpleContentBase = getSimpleContentType().getBaseType();
                            if (((simpleContentBase == null) || (simpleContentBase is Saxon.Hej.type.AnySimpleType))) {
                                string err = ((("The complex type "+getDescription())+" has simple content and its base type is a mixed complex type,")+" so it must specify a simpleType");
                                compiler.error(err, this);
                                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                return false;
                            }
                        }
                    } else {
                        if (cbase.isSimpleContent()) {
                            string err = (("The complex type "+getDescription())+" has complex content, but is derived by restriction from a base type that has simple content");
                            compiler.error(err, this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    }
                    if ((isMixedContent() && !(((Saxon.Hej.type.ComplexType)(@base)).isMixedContent()))) {
                        string err = (("The complex type "+getDescription())+" has mixed content so its base type must also have mixed content");
                        compiler.error(err, this);
                        setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    }
                    if ((particle != null)) {
                        if (particle.isPointless(null)) {
                            Saxon.Eej.ee.schema.ModelGroup group = ((Saxon.Eej.ee.schema.ModelGroupParticle)(particle)).getGroup();
                            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> contentModel = group.getSimplifiedContentModel();
                            if (((((variety == Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY) && contentModel.Count==0) && !(((group is Saxon.Eej.ee.schema.ChoiceModelGroup)))) && (((openContentWildcard == null) || !(openContentAppliesToEmpty))))) {
                                particle = null;
                                variety = Saxon.Hej.type.ComplexVariety.EMPTY;
                            } else {
                                if (((contentModel.Count == 1) && !(((group is Saxon.Eej.ee.schema.AllModelGroup))))) {
                                    particle = contentModel[0];
                                } else {
                                }
                            }
                        } else {
                            if ((((particle is Saxon.Eej.ee.schema.ModelGroupParticle) && (particle.getMinOccurs() == 1)) && (particle.getMaxOccurs() == 1))) {
                                ((Saxon.Eej.ee.schema.ModelGroupParticle)(particle)).getGroup().getSimplifiedContentModel();
                            }
                        }
                    }
                    return isValidRestriction(@base, compiler);
                }
            }
            return true;
        }
        private static bool isAllParticle(Saxon.Eej.ee.schema.Particle particle) {
            return ((particle is Saxon.Eej.ee.schema.ModelGroupParticle) && (((Saxon.Eej.ee.schema.ModelGroupParticle)(particle)).getGroup() is Saxon.Eej.ee.schema.AllModelGroup));
        }
        private bool isLocallyEmpty() {
            if (isSimpleContent()) {
                return false;
            }
            if (((particle == null) || (particle.getMaxOccurs() == 0))) {
                return true;
            }
            if ((particle is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                Saxon.Eej.ee.schema.ModelGroup group = ((Saxon.Eej.ee.schema.ModelGroupParticle)(particle)).getGroup();
                return (group.isEmpty() && ((((group is Saxon.Eej.ee.schema.AllModelGroup) || (group is Saxon.Eej.ee.schema.SequenceModelGroup)) || (((group is Saxon.Eej.ee.schema.ChoiceModelGroup) && (particle.getMinOccurs() == 0))))));
            }
            return false;
        }
        private bool isEmptyContent(Saxon.Hej.type.SchemaType type) {
            return (((type is Saxon.Hej.type.ComplexType)) && ((Saxon.Hej.type.ComplexType)(type)).isEmptyContent());
        }
        public bool isValidRestriction(Saxon.Hej.type.SchemaType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((@base is Saxon.Hej.type.AnyType)) {
                return true;
            }
            if ((this == @base)) {
                return true;
            }
            if (((getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID) || (@base.getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID))) {
                return false;
            }
            Saxon.Hej.type.ComplexType cbase = (Saxon.Hej.type.ComplexType)(@base);
            makeCombinedAttributeGroup(compiler);
            if ((cbase is Saxon.Eej.ee.schema.UserComplexType)) {
                Saxon.Eej.ee.schema.UserComplexType ucbase = (Saxon.Eej.ee.schema.UserComplexType)(cbase);
                ucbase.makeCombinedAttributeGroup(compiler);
                if (!(ucbase.compiled)) {
                    ucbase.compile(compiler);
                }
            }
            if ((cbase is Saxon.Eej.ee.schema.UserComplexType)) {
                if (!(getCombinedAttributeGroup().isValidRestriction(((Saxon.Eej.ee.schema.UserComplexType)(cbase)).getCombinedAttributeGroup(), compiler))) {
                    compiler.error(((("The attributes of complex type "+getDescription())+" are not a restricted subset of the attributes of the type ")+@base.getDescription()), this);
                    return false;
                }
            }
            if (!(compiled)) {
                compile(compiler);
            }
            string subsumeError = isSubsumedBy(cbase, compiler);
            if ((subsumeError != null)) {
                if (subsumeError.StartsWith("?")) {
                    compiler.error((((((("Saxon is unable to determine whether or not the"+" content model of the complex type ")+getDescription())+" is a valid restriction of the content model of the type ")+@base.getDescription())+". ")+subsumeError.Substring(1)), this);
                } else {
                    compiler.error(((((("The content model of the complex type "+getDescription())+" is not a valid restriction of the content model of the type ")+@base.getDescription())+". ")+subsumeError), this);
                }
                return false;
            }
            return true;
        }
        public void compile(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((compiled || (getValidationStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID))) {
                return ;
            }
            if (debug) {
                Saxon.Hej.lib.Logger err = compiler.getConfiguration().getLogger();
                err.info(("==== Compiling complex type "+getDescription()));
                err.info(("  simple content: "+isSimpleContent()));
                err.info(("  complex content: "+isComplexContent()));
                err.info(("  all content: "+isAllContent()));
                err.info(("  empty content: "+isEmptyContent()));
                err.info(("  emptiable: "+isEmptiable()));
                err.info(("  mixed content: "+isMixedContent()));
                err.info(("  base type: "+getBaseType().getDescription()));
            }
            compiled = true;
            if ((isSimpleContent() || isEmptyContent())) {
                machine.getInitialState().setFinalState(true);
            } else {
                if (isAllContent()) {
                    makeExtendedParticle(compiler);
                } else {
                    Saxon.Eej.ee.schema.Particle extended = makeExtendedParticle(compiler);
                    if ((extended == null)) {
                        machine.getInitialState().setFinalState(true);
                        machine.setOpenContentWildcard(openContentWildcard, (openContentMode == Saxon.Hej.type.OpenContentVariety.INTERLEAVE));
                    } else {
                        compiler.scheduleAutomaton(this);
                        if ((particle != null)) {
                            particle.compile(compiler);
                        }
                    }
                }
            }
            makeCombinedAttributeGroup(compiler);
            makeContextDeterminedTypeMapForAttributes();
            makeContextDeterminedTypeMapForElements();
        }
        protected internal void makeAutomaton(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Eej.ee.schema.Particle extended = extendedParticle;
            Saxon.Eej.ee.schema.fsa.NonDeterminizedState finalState = new Saxon.Eej.ee.schema.fsa.NonDeterminizedState(machine);
            finalState.setFinalState(true);
            try {
                Saxon.Eej.ee.schema.fsa.AutomatonState state = /*134*/Saxon.Eej.ee.schema.fsa.FiniteStateMachine.compileParticle(compiler, extended, finalState, this, machine);
                machine.setInitialState(state);
                if (debug) {
                    Saxon.Hej.lib.Logger err = compiler.getConfiguration().getLogger();
                    err.info(("==== NFSA for complex type "+getDescription()));
                    machine.display(err);
                }
                machine = /*134*/Saxon.Eej.ee.schema.fsa.FiniteStateMachine.determinize(compiler, machine);
                if (debug) {
                    Saxon.Hej.lib.Logger err = compiler.getConfiguration().getLogger();
                    err.info(("==== DFSA for complex type "+getDescription()));
                    machine.display(err);
                }
                machine.setOpenContentWildcard(openContentWildcard, (openContentMode == Saxon.Hej.type.OpenContentVariety.INTERLEAVE));
            } catch (Saxon.Hej.type.SchemaException err) {
                compiler.error(((("Error in complex type "+getDescription())+": ")+err.Message), this);
                throw;
            }
        }
        public void recompile(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (debug) {
                getConfiguration().getLogger().info(("Recompiling type "+getDescription()));
            }
            compiled = false;
            compile(compiler);
        }
        public void cancelValidation() {
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED);
            compiled = false;
        }
        public Saxon.Eej.ee.schema.fsa.FiniteStateMachine getFiniteStateMachine() {
            return machine;
        }
        public Saxon.Eej.ee.schema.fsa.State getInitialState() {
            Saxon.Eej.ee.schema.fsa.AutomatonState @as = machine.getInitialState();
            if (@as.requiresCounter()) {
                return new Saxon.Eej.ee.schema.fsa.CountingState(@as, 0);
            } else {
                return @as;
            }
        }
        public Saxon.Eej.ee.schema.Particle makeExtendedParticle(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((extendedParticle != null)) {
                return extendedParticle;
            }
            if ((compiler != null)) {
                lookForCycles(new System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent>(), compiler);
            }
            if ((getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) {
                Saxon.Hej.type.ComplexType cbase = (Saxon.Hej.type.ComplexType)(getBaseType());
                if ((isAllContent() || cbase.isAllContent())) {
                    if (cbase.isEmptyContent()) {
                        extendedParticle = particle;
                    } else {
                        if (isLocallyEmpty()) {
                            extendedParticle = ((Saxon.Eej.ee.schema.UserComplexType)(cbase)).getParticle();
                        } else {
                            Saxon.Eej.ee.schema.ModelGroup all = new Saxon.Eej.ee.schema.AllModelGroup();
                            all.setLocator(this);
                            Saxon.Eej.ee.schema.ModelGroupParticle @base = (Saxon.Eej.ee.schema.ModelGroupParticle)(((Saxon.Eej.ee.schema.UserComplexType)(cbase)).makeExtendedParticle(compiler));
                            foreach (Saxon.Eej.ee.schema.Particle p in @base.getGroup().getParticles()) {
                                all.addParticle(p);
                            }
                            foreach (Saxon.Eej.ee.schema.Particle p in ((Saxon.Eej.ee.schema.ModelGroupParticle)(particle)).getGroup().getParticles()) {
                                all.addParticle(p);
                            }
                            all.elaborate(compiler);
                            Saxon.Eej.ee.schema.ModelGroupParticle mgp = new Saxon.Eej.ee.schema.ModelGroupParticle(getConfiguration(), this);
                            mgp.setTarget(all);
                            mgp.setLocator(this);
                            extendedParticle = mgp;
                            extendedParticle.setMinOccurs(particle.getMinOccurs());
                            variety = ((variety == Saxon.Hej.type.ComplexVariety.MIXED) ? Saxon.Hej.type.ComplexVariety.MIXED : Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY);
                        }
                    }
                    return extendedParticle;
                } else {
                    if ((cbase is Saxon.Eej.ee.schema.UserComplexType)) {
                        Saxon.Eej.ee.schema.ModelGroup seq = new Saxon.Eej.ee.schema.SequenceModelGroup();
                        Saxon.Eej.ee.schema.Particle @base = ((Saxon.Eej.ee.schema.UserComplexType)(cbase)).makeExtendedParticle(compiler);
                        if ((@base != null)) {
                            seq.addParticle(@base);
                        }
                        if ((particle != null)) {
                            seq.addParticle(particle);
                        }
                        if (seq.isPointless(null)) {
                            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> contentModel = seq.getSimplifiedContentModel();
                            if (contentModel.Count==0) {
                                extendedParticle = null;
                                if ((variety == Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY)) {
                                    variety = Saxon.Hej.type.ComplexVariety.EMPTY;
                                }
                            } else {
                                if ((contentModel.Count == 1)) {
                                    extendedParticle = (Saxon.Eej.ee.schema.Particle)(contentModel[0]);
                                } else {
                                    Saxon.Eej.ee.schema.ModelGroupParticle mgp = new Saxon.Eej.ee.schema.ModelGroupParticle(getConfiguration(), this);
                                    mgp.setTarget(seq);
                                    extendedParticle = mgp;
                                }
                            }
                        } else {
                            Saxon.Eej.ee.schema.ModelGroupParticle mgp = new Saxon.Eej.ee.schema.ModelGroupParticle(getConfiguration(), this);
                            mgp.setTarget(seq);
                            extendedParticle = mgp;
                        }
                        if ((extendedParticle != null)) {
                            extendedParticle.markVulnerableSubParticles();
                        }
                        return extendedParticle;
                    } else {
                        if ((cbase is Saxon.Hej.type.AnyType)) {
                            if (isLocallyEmpty()) {
                                Saxon.Eej.ee.schema.ElementWildcard any = new Saxon.Eej.ee.schema.ElementWildcard(getConfiguration());
                                any.setMinOccurs(0);
                                any.setMaxOccurs(-1);
                                any.getWildcard().setProcessContents("lax");
                                extendedParticle = any;
                                return extendedParticle;
                            } else {
                                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                string err = "xs:anyType cannot be extended, because any non-empty extension creates an ambiguity";
                                if ((compiler == null)) {
                                    throw new Saxon.Ejava.lang.IllegalStateException(err);
                                } else {
                                    compiler.error(err, this);
                                }
                                extendedParticle = particle;
                                return particle;
                            }
                        } else {
                            throw new Saxon.Ejava.lang.IllegalStateException(("Unknown implementation of ComplexType: "+(((cbase == null) ? "null" : cbase.GetType()))));
                        }
                    }
                }
            } else {
                if ((particle == null)) {
                    extendedParticle = null;
                    return extendedParticle;
                } else {
                    Saxon.Eej.ee.schema.Particle p = particle;
                    if ((((p is Saxon.Eej.ee.schema.ModelGroupParticle) && (p.getMinOccurs() == 1)) && (p.getMaxOccurs() == 1))) {
                        Saxon.Eej.ee.schema.ModelGroup group = ((Saxon.Eej.ee.schema.ModelGroupParticle)(p)).getGroup();
                        if (((group.getSimplifiedContentModel().Count == 1) && !(((group is Saxon.Eej.ee.schema.AllModelGroup))))) {
                            p = group.getSimplifiedContentModel()[0];
                        }
                    }
                    if ((p.isPointless(null) && ((p is Saxon.Eej.ee.schema.ModelGroupParticle)))) {
                        System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> contentModel = ((Saxon.Eej.ee.schema.ModelGroupParticle)(p)).getGroup().getSimplifiedContentModel();
                        if (((((variety == Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY) && contentModel.Count==0) && !(((((Saxon.Eej.ee.schema.ModelGroupParticle)(p)).getGroup() is Saxon.Eej.ee.schema.ChoiceModelGroup)))) && (((openContentWildcard == null) || !(openContentAppliesToEmpty))))) {
                            extendedParticle = null;
                            variety = Saxon.Hej.type.ComplexVariety.EMPTY;
                        } else {
                            if ((contentModel.Count == 1)) {
                                extendedParticle = contentModel[0];
                            } else {
                                extendedParticle = p;
                            }
                        }
                    } else {
                        extendedParticle = p;
                    }
                    if ((extendedParticle != null)) {
                        extendedParticle.markVulnerableSubParticles();
                    }
                    return extendedParticle;
                }
            }
        }
        public Saxon.Eej.ee.schema.Particle getParticle() {
            return extendedParticle;
        }
        public bool isEmptiable() {
            Saxon.Eej.ee.schema.Particle p = getParticle();
            return ((p == null) || p.isEmptiable());
        }
        public Saxon.Hej.type.SchemaType getElementParticleType(int elementName, bool considerExtensions) {
            Saxon.Eej.ee.schema.Particle p = getParticle();
            Saxon.Hej.type.SchemaType child = ((p == null) ? null : p.getElementParticleType(elementName));
            if ((child != null)) {
                return child;
            } else {
                if (considerExtensions) {
                    if (((openContentWildcard != null) && openContentWildcard.matches(elementName, true, getConfiguration(), this))) {
                        return Saxon.Hej.type.AnyType.getInstance();
                    }
                    foreach (Saxon.Eej.ee.schema.UserComplexType ext in getExtensionTypes()) {
                        Saxon.Hej.type.SchemaType st = ext.getElementParticleType(elementName, false);
                        if ((st != null)) {
                            if ((child == null)) {
                                child = st;
                            } else {
                                if ((child == st)) {
                                } else {
                                    return Saxon.Hej.type.AnyType.getInstance();
                                }
                            }
                        }
                    }
                    return child;
                } else {
                    return null;
                }
            }
        }
        public int getElementParticleCardinality(int elementName, bool considerExtensions) {
            int baseCardinality;
            Saxon.Eej.ee.schema.Particle p = getParticle();
            if ((p == null)) {
                baseCardinality = Saxon.Hej.expr.StaticProperty.EMPTY;
            } else {
                baseCardinality = p.getElementParticleCardinality(elementName);
            }
            if (((openContentWildcard != null) && openContentWildcard.matches(elementName, true, getConfiguration(), this))) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
            }
            if (considerExtensions) {
                foreach (Saxon.Eej.ee.schema.UserComplexType ext in getExtensionTypes()) {
                    if ((baseCardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE)) {
                        return baseCardinality;
                    }
                    int extCardinality = ext.getElementParticleCardinality(elementName, false);
                    baseCardinality = Saxon.Hej.value.Cardinality.union(baseCardinality, extCardinality);
                }
            }
            return baseCardinality;
        }
        public Saxon.Hej.type.SimpleType getAttributeUseType(Saxon.Hej.om.StructuredQName attributeName) {
            Saxon.Eej.ee.schema.AttributeGroupDecl ag = getCombinedAttributeGroup();
            Saxon.Eej.ee.schema.AttributeUse use = ag.getAttributeUse(attributeName);
            if (((use != null) && !(use.isProhibited()))) {
                return ((/*1430*/Saxon.Hej.type.SimpleType)((Saxon.Eej.ee.schema.AttributeDecl)(use.getTarget())).getType());
            }
            Saxon.Hej.type.SimpleType attType = null;
            foreach (Saxon.Eej.ee.schema.UserComplexType ext in getExtensionTypes()) {
                Saxon.Hej.type.SimpleType st = ext.getAttributeUseType(attributeName);
                if ((st != null)) {
                    if ((attType == null)) {
                        attType = st;
                    } else {
                        if ((attType == st)) {
                        } else {
                            return Saxon.Hej.type.AnySimpleType.getInstance();
                        }
                    }
                }
            }
            if ((attType != null)) {
                return attType;
            }
            Saxon.Eej.ee.schema.AttributeWildcard awild = ag.getAttributeWildcard(null);
            if ((awild == null)) {
                return null;
            }
            Saxon.Eej.ee.schema.Wildcard wild = awild.getWildcard();
            if (wild.matches(attributeName, false, getConfiguration(), null)) {
                switch (wild.getProcessContents()) {
                  case "skip":
                    return Saxon.Hej.type.AnySimpleType.getInstance();
                  case "strict":
                    {
                        Saxon.Eej.ee.schema.AttributeDecl decl = getConfiguration().getSuperSchema().getAttributeDecl(attributeName);
                        if ((decl != null)) {
                            return ((/*1430*/Saxon.Hej.type.SimpleType)decl.getType());
                        } else {
                            return null;
                        }
                    }
                  default:
                    {
                        Saxon.Eej.ee.schema.AttributeDecl decl = getConfiguration().getSuperSchema().getAttributeDecl(attributeName);
                        if ((decl != null)) {
                            return ((/*1430*/Saxon.Hej.type.SimpleType)decl.getType());
                        } else {
                            return Saxon.Hej.type.AnySimpleType.getInstance();
                        }
                    }
                }
            }
            return null;
        }
        public int getAttributeUseCardinality(Saxon.Hej.om.StructuredQName attributeName) {
            Saxon.Eej.ee.schema.AttributeGroupDecl ag = getCombinedAttributeGroup();
            Saxon.Eej.ee.schema.AttributeUse use = ag.getAttributeUse(attributeName);
            if ((use != null)) {
                return use.getCardinality();
            }
            int card = 0;
            foreach (Saxon.Eej.ee.schema.UserComplexType ext in getExtensionTypes()) {
                int c = ext.getAttributeUseCardinality(attributeName);
                card |= c;
            }
            if ((card != 0)) {
                return card;
            }
            Saxon.Eej.ee.schema.AttributeWildcard awild = ag.getAttributeWildcard(null);
            if ((awild == null)) {
                return Saxon.Hej.expr.StaticProperty.EMPTY;
            }
            Saxon.Eej.ee.schema.Wildcard wild = awild.getWildcard();
            if (wild.matches(attributeName, false, getConfiguration(), null)) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            }
            return Saxon.Hej.expr.StaticProperty.EMPTY;
        }
        public bool allowsAttributes() {
            Saxon.Eej.ee.schema.AttributeGroupDecl ag = getCombinedAttributeGroup();
            if (!(ag.getAttributeUses().Count==0)) {
                return true;
            }
            if ((ag.getAttributeWildcard(null) != null)) {
                return true;
            }
            foreach (Saxon.Eej.ee.schema.UserComplexType ext in getExtensionTypes()) {
                if (ext.allowsAttributes()) {
                    return true;
                }
            }
            return false;
        }
        public bool containsElementWildcard() {
            if (((extendedParticle != null) && extendedParticle.containsElementWildcard())) {
                return true;
            }
            foreach (Saxon.Eej.ee.schema.UserComplexType ext in getExtensionTypes()) {
                if (ext.containsElementWildcard()) {
                    return true;
                }
            }
            return false;
        }
        public void makeContextDeterminedTypeMapForElements() {
            if ((contextDeterminedTypesForElements != null)) {
                return ;
            }
            contextDeterminedTypesForElements = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.type.SchemaType>();
            gatherContextDeterminedTypesForElements(contextDeterminedTypesForElements);
        }
        private void gatherContextDeterminedTypesForElements(System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.type.SchemaType> map) {
            Saxon.Hej.type.SchemaType @base = getBaseType();
            if ((@base is Saxon.Eej.ee.schema.UserComplexType)) {
                ((Saxon.Eej.ee.schema.UserComplexType)(@base)).gatherContextDeterminedTypesForElements(map);
            }
            Saxon.Eej.ee.schema.Particle p = getParticle();
            if ((p != null)) {
                Saxon.Hej.z.IntHashSet children = new Saxon.Hej.z.IntHashSet();
                p.gatherAllPermittedElements(children, true);
                Saxon.Hej.z.IntIterator kids = children.iterator();
                while (kids.hasNext()) {
                    int fp = kids.next();
                    Saxon.Hej.type.SchemaType type = getElementParticleType(fp, false);
                    Saxon.Impl.Helpers.MapUtils.Put(map, getNamePool().getStructuredQName(fp), type);
                }
            }
        }
        public Saxon.Hej.type.SchemaType getContextDeterminedTypeForElement(Saxon.Hej.om.StructuredQName fp) {
            if ((contextDeterminedTypesForElements == null)) {
                return null;
            }
            return Saxon.Impl.Helpers.MapUtils.Get(contextDeterminedTypesForElements, fp);
        }
        public void makeContextDeterminedTypeMapForAttributes() {
            if ((contextDeterminedTypesForAttributes != null)) {
                return ;
            }
            contextDeterminedTypesForAttributes = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.type.SimpleType>();
            gatherContextDeterminedTypesForAttributes(contextDeterminedTypesForAttributes);
        }
        private void gatherContextDeterminedTypesForAttributes(System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.type.SimpleType> map) {
            Saxon.Hej.type.SchemaType @base = getBaseType();
            if ((@base is Saxon.Eej.ee.schema.UserComplexType)) {
                ((Saxon.Eej.ee.schema.UserComplexType)(@base)).gatherContextDeterminedTypesForAttributes(map);
            }
            foreach (Saxon.Eej.ee.schema.AttributeUse use in getAttributeGroup().getAttributeUses()) {
                if (!(use.isProhibited())) {
                    Saxon.Hej.om.StructuredQName fp = use.getTargetComponentName();
                    Saxon.Hej.type.SimpleType type = getAttributeUseType(fp);
                    Saxon.Impl.Helpers.MapUtils.Put(map, fp, type);
                }
            }
        }
        public Saxon.Hej.type.SimpleType getContextDeterminedTypeForAttribute(Saxon.Hej.om.StructuredQName fp) {
            if ((contextDeterminedTypesForAttributes == null)) {
                return null;
            }
            return Saxon.Impl.Helpers.MapUtils.Get(contextDeterminedTypesForAttributes, fp);
        }
        public void gatherAllPermittedChildren(Saxon.Hej.z.IntHashSet children, bool ignoreWildcards) {
            Saxon.Eej.ee.schema.Particle p = getParticle();
            if ((p == null)) {
                return ;
            }
            p.gatherAllPermittedElements(children, ignoreWildcards);
            if ((extendedTypes != null)) {
                foreach (System.Object extendedType in extendedTypes) {
                    Saxon.Eej.ee.schema.UserComplexType uct = (Saxon.Eej.ee.schema.UserComplexType)(extendedType);
                    uct.gatherAllPermittedChildren(children, ignoreWildcards);
                }
            }
        }
        public void gatherAllPermittedDescendants(Saxon.Hej.z.IntHashSet descendants) {
            Saxon.Eej.ee.schema.Particle p = getParticle();
            if ((p == null)) {
                return ;
            }
            Saxon.Hej.z.IntHashSet permittedChildren = new Saxon.Hej.z.IntHashSet(16);
            gatherAllPermittedChildren(permittedChildren, false);
            Saxon.Hej.om.StructuredQName wildcardDummy = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
            if (permittedChildren.contains(-1)) {
                descendants.add(-1);
                return ;
            }
            Saxon.Hej.z.IntIterator kids = permittedChildren.iterator();
            while (kids.hasNext()) {
                int c = kids.next();
                if (descendants.add(c)) {
                    Saxon.Hej.type.SchemaType childType = getElementParticleType(c, true);
                    if ((childType == null)) {
                        throw new Saxon.Ejava.lang.AssertionError((("Child particle "+c)+" not found"));
                    }
                    if ((childType is Saxon.Hej.type.ComplexType)) {
                        if ((childType is Saxon.Eej.ee.schema.UserComplexType)) {
                            ((Saxon.Eej.ee.schema.UserComplexType)(childType)).gatherAllPermittedDescendants(descendants);
                        } else {
                            descendants.add(-1);
                            return ;
                        }
                    }
                }
            }
        }
        public Saxon.Hej.type.SchemaType getDescendantElementType(int elementName) {
            System.Collections.Generic.ISet<Saxon.Hej.type.SchemaType> parentsConsidered = new System.Collections.Generic.HashSet<Saxon.Hej.type.SchemaType>();
            System.Collections.Generic.ISet<Saxon.Hej.type.SchemaType> foundTypes = new System.Collections.Generic.HashSet<Saxon.Hej.type.SchemaType>();
            getDescendantElementTypes(elementName, parentsConsidered, foundTypes);
            if (foundTypes.Count==0) {
                return null;
            }
            if ((foundTypes.Count == 1)) {
                System.Collections.Generic.IEnumerator<Saxon.Hej.type.SchemaType> iter = foundTypes.GetEnumerator();
                if (iter.MoveNext() /*UNSAFE CONVERSION*/) {
                    return iter.Current;
                }
            }
            return Saxon.Hej.type.AnyType.getInstance();
        }
        private void getDescendantElementTypes(int elementName, System.Collections.Generic.ISet<Saxon.Hej.type.SchemaType> parentsConsidered, System.Collections.Generic.ISet<Saxon.Hej.type.SchemaType> foundTypes) {
            Saxon.Eej.ee.schema.Particle p = getParticle();
            if ((p == null)) {
                return ;
            }
            Saxon.Hej.type.SchemaType targetType = getElementParticleType(elementName, true);
            if ((targetType != null)) {
                foundTypes.Add(targetType);
                if ((targetType == Saxon.Hej.type.AnyType.getInstance())) {
                    return ;
                }
            }
            Saxon.Hej.z.IntHashSet permittedChildren = new Saxon.Hej.z.IntHashSet(16);
            gatherAllPermittedChildren(permittedChildren, false);
            if (permittedChildren.contains(-1)) {
                foundTypes.Add(Saxon.Hej.type.AnyType.getInstance());
                return ;
            }
            Saxon.Hej.z.IntIterator kids = permittedChildren.iterator();
            while (kids.hasNext()) {
                int c = kids.next();
                Saxon.Hej.type.SchemaType childType = getElementParticleType(c, true);
                if ((childType == null)) {
                    throw new Saxon.Ejava.lang.AssertionError((("Child particle "+c)+" not found"));
                }
                if (!(parentsConsidered.Contains(childType))) {
                    parentsConsidered.Add(childType);
                    if ((childType is Saxon.Hej.type.ComplexType)) {
                        if ((childType is Saxon.Eej.ee.schema.UserComplexType)) {
                            ((Saxon.Eej.ee.schema.UserComplexType)(childType)).getDescendantElementTypes(elementName, parentsConsidered, foundTypes);
                        } else {
                            foundTypes.Add(childType);
                        }
                    }
                }
            }
        }
        public int getDescendantElementCardinality(int elementFingerprint) {
            int cardinality = Saxon.Hej.expr.StaticProperty.EMPTY;
            Saxon.Hej.z.IntHashSet children = new Saxon.Hej.z.IntHashSet();
            gatherAllPermittedChildren(children, false);
            if (children.contains(-1)) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
            }
            Saxon.Hej.z.IntIterator kids = children.iterator();
            while (kids.hasNext()) {
                int child = kids.next();
                int childCard = getElementParticleCardinality(child, true);
                if ((child == elementFingerprint)) {
                    cardinality = Saxon.Hej.value.Cardinality.sum(cardinality, childCard);
                    if (Saxon.Hej.value.Cardinality.allowsMany(cardinality)) {
                        return cardinality;
                    }
                }
                Saxon.Hej.type.SchemaType childType = getElementParticleType(child, true);
                if (((childType != null) && childType.isComplexType())) {
                    Saxon.Hej.z.IntHashSet childDescendants = new Saxon.Hej.z.IntHashSet();
                    ((Saxon.Hej.type.ComplexType)(childType)).gatherAllPermittedDescendants(childDescendants);
                    if (childDescendants.contains(-1)) {
                        return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
                    }
                    if (childDescendants.contains(elementFingerprint)) {
                        cardinality = Saxon.Hej.value.Cardinality.sum(cardinality, Saxon.Hej.value.Cardinality.multiply(childCard, ((Saxon.Hej.type.ComplexType)(childType)).getDescendantElementCardinality(elementFingerprint)));
                        if ((cardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE)) {
                            return cardinality;
                        }
                    }
                }
            }
            return cardinality;
        }
        public string isSubsumedBy(Saxon.Hej.type.ComplexType @base, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((@base is Saxon.Hej.type.AnyType)) {
                return null;
            } else {
                if ((@base is Saxon.Hej.type.Untyped)) {
                    return "Untyped does not subsume any other type";
                } else {
                    Saxon.Eej.ee.schema.UserComplexType cbase = (Saxon.Eej.ee.schema.UserComplexType)(@base);
                    Saxon.Eej.ee.schema.Wildcard bw = cbase.getOpenContentWildcard();
                    Saxon.Eej.ee.schema.Wildcard rw = this.getOpenContentWildcard();
                    if ((bw == null)) {
                        if ((rw != null)) {
                            Saxon.Eej.ee.schema.Particle bp = cbase.getParticle();
                            if ((((((bp is Saxon.Eej.ee.schema.ElementWildcard) && (bp.getMinOccurs() == 0)) && (bp.getMaxOccurs() == -1)) && ((this.isEmptyContent() || (((Saxon.Eej.ee.schema.ModelGroup)(this.getParticle().getTarget())).getContentModelSize() == 0)))) && rw.isSubset(((Saxon.Eej.ee.schema.ElementWildcard)(bp)).getWildcard(), getNamePool()))) {
                                return null;
                            }
                            return "Restricted type has open content; base type does not";
                        }
                    } else {
                        if ((rw != null)) {
                            if (!(rw.isSubset(bw, compiler.getNamePool()))) {
                                return ("Restricted type has an open content wildcard that matches names which are "+"not matched by the open content of the base type");
                            }
                            if ((bw.compareStrength(rw)>0)) {
                                return ("The processContents on the open content wildcard of the restricted type"+" is weaker than that on the open content wildcard of the base type");
                            }
                            if (!(isLocallyEmpty())) {
                                bool bInterleaved = (cbase.getOpenContentMode() == Saxon.Hej.type.OpenContentVariety.INTERLEAVE);
                                bool rInterleaved = (this.getOpenContentMode() == Saxon.Hej.type.OpenContentVariety.INTERLEAVE);
                                if ((rInterleaved && !(bInterleaved))) {
                                    return ("Restricted type allows interleaved open content, but "+"base type only allows suffixed open content");
                                }
                            }
                        }
                    }
                    return cbase.subsumes(this, compiler);
                }
            }
        }
        private string subsumes(Saxon.Eej.ee.schema.UserComplexType sub, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            computeIsAllContent();
            if (!((getAssertions()).All(sub.getAssertions().Contains))) {
                return ((("The assertions defined on complex type "+getDescription())+" are not a subset of the assertions defined on the type ")+sub.getDescription());
            }
            sub.computeIsAllContent();
            if (isAllContent()) {
                return /*134*/Saxon.Eej.ee.schema.AllModelGroup.allSubsumes(this, sub, compiler);
            } else {
                if (sub.isAllContent()) {
                    return subsumesAll(sub, compiler);
                } else {
                    if (sub.isEmptyContent()) {
                        if (isEmptiable()) {
                            return null;
                        } else {
                            return "Derived type has empty content model, but base type does not allow empty content";
                        }
                    } else {
                        compiler.scheduleAutomatonCheck(this, sub);
                        return null;
                    }
                }
            }
        }
        protected internal static string elementParticleMatches(Saxon.Eej.ee.schema.ElementDecl @base, Saxon.Eej.ee.schema.ElementDecl sub, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Hej.type.SchemaType baseType = @base.getType();
            Saxon.Hej.type.SchemaType subType = sub.getType();
            if ((sub.isNillable() && !(@base.isNillable()))) {
                return (((("Element "+eName(sub))+" in derived type is nillable but element ")+eName(@base))+" in base type is not");
            }
            if ((@base.getFixedValue() != null)) {
                if ((sub.getFixedValue() == null)) {
                    return (((("Element "+eName(@base))+" in base type has a fixed value, but element ")+eName(sub))+" in derived type does not");
                }
                if (!(Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().equal(sub.getFixedValue(), @base.getFixedValue()))) {
                    return (((("Element "+eName(sub))+" in derived type has a different fixed value from element ")+eName(@base))+" in base type");
                }
            }
            bool elementTypeOK = true;
            string reason = null;
            if (subType.isSimpleType()) {
                reason = /*134*/Saxon.Eej.ee.schema.UserSimpleType.isTypeDerivationOK((Saxon.Hej.type.SimpleType)(subType), baseType, 0);
                elementTypeOK = (reason == null);
            } else {
                while (!(baseType.isSameType(subType))) {
                    if ((subType.getDerivationMethod() != Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) {
                        elementTypeOK = false;
                        break ;
                    }
                    subType = subType.getBaseType();
                    if ((subType == null)) {
                        elementTypeOK = false;
                        break ;
                    }
                }
            }
            if (!(elementTypeOK)) {
                return (((((("The type of element "+eName(sub))+" in the restricted content model is not validly derived from the ")+"type of the corresponding element ")+eName(sub))+" in the base content model")+(((reason == null) ? "" : (". "+reason))));
            }
            if (((@base.getFixedValue() != null) && !(Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().equal(@base.getFixedValue(), sub.getFixedValue())))) {
                return ((((("The fixed value of element "+eName(sub))+" in the base content model is not reflected in the corresponding ")+"element ")+eName(sub))+" of the restricted content model");
            }
            System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint> baseUniqueSet = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint>(4);
            System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint> baseKeySet = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint>(4);
            System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint> baseKeyRefSet = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint>(4);
            foreach (Saxon.Eej.ee.schema.IdentityConstraintReference icr in @base.getIdentityConstraints()) {
                Saxon.Eej.ee.schema.IdentityConstraint ic = ((/*1430*/Saxon.Eej.ee.schema.IdentityConstraint)icr.getTarget());
                if ((ic is Saxon.Eej.ee.schema.Unique)) {
                    baseUniqueSet.Add(ic);
                } else {
                    if ((ic is Saxon.Eej.ee.schema.Key)) {
                        baseKeySet.Add(ic);
                    } else {
                        if ((ic is Saxon.Eej.ee.schema.KeyRef)) {
                            baseKeyRefSet.Add(ic);
                        }
                    }
                }
            }
            System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint> subUniqueSet = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint>(4);
            System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint> subKeySet = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint>(4);
            System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint> subKeyRefSet = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.IdentityConstraint>(4);
            foreach (Saxon.Eej.ee.schema.IdentityConstraintReference icr in sub.getIdentityConstraints()) {
                Saxon.Eej.ee.schema.IdentityConstraint ic = ((/*1430*/Saxon.Eej.ee.schema.IdentityConstraint)icr.getTarget());
                if ((ic is Saxon.Eej.ee.schema.Unique)) {
                    subUniqueSet.Add(ic);
                } else {
                    if ((ic is Saxon.Eej.ee.schema.Key)) {
                        subKeySet.Add(ic);
                    } else {
                        if ((ic is Saxon.Eej.ee.schema.KeyRef)) {
                            subKeyRefSet.Add(ic);
                        }
                    }
                }
            }
            if ((baseUniqueSet.Count>subUniqueSet.Count)) {
                return ((("Element "+eName(@base))+"in the base type has an xs:unique constraint that ")+"is not present in the restricted type");
            }
            if ((baseKeySet.Count>subKeySet.Count)) {
                return ((("Element "+eName(sub))+" in the base type has an xs:key constraint that is not present ")+"in the restricted type");
            }
            if ((baseKeyRefSet.Count>subKeyRefSet.Count)) {
                return ((("Element "+eName(sub))+" in the base type has an xs:keyref constraint that is not present ")+"in the restricted type");
            }
            if ((((baseUniqueSet.Count+baseKeySet.Count)+baseKeyRefSet.Count)>0)) {
                compiler.warning((("Saxon is not able to verify that the identity constraints in element "+eName(sub))+" are compatible with those in the the base type"), sub);
            }
            if ((((@base.getBlock()&~(sub.getBlock()))) != 0)) {
                return ((("The disallowed substitutions of element "+eName(sub))+"in the restricted content model (@block) must be ")+"a superset of those in the base content model");
            }
            return null;
        }
        private static string eName(Saxon.Eej.ee.schema.ElementDecl e) {
            return Saxon.Hej.trans.Err.wrap(e.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT);
        }
        public string subsumesAll(Saxon.Eej.ee.schema.UserComplexType sub, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Eej.ee.schema.AllModelGroup subAll = (Saxon.Eej.ee.schema.AllModelGroup)(sub.getAllCompositor().getGroup());
            if ((subAll.getNumberOfElementParticles() == 1)) {
                Saxon.Eej.ee.schema.ModelGroup seq = new Saxon.Eej.ee.schema.SequenceModelGroup();
                seq.addParticle(subAll.getSimplifiedContentModel()[0]);
                Saxon.Eej.ee.schema.Particle temp = sub.getParticle();
                sub.setParticle(subAll.getSimplifiedContentModel()[0]);
                string message = subsumes(sub, compiler);
                sub.setParticle(temp);
                return message;
            }
            Saxon.Eej.ee.schema.Particle @base = makeExtendedParticle(compiler);
            Saxon.Eej.config.EnterpriseConfiguration config = getConfiguration();
            if ((@base is Saxon.Eej.ee.schema.ElementWildcard)) {
                int min = 0;
                int max = 0;
                foreach (Saxon.Eej.ee.schema.Particle p in subAll.getParticles()) {
                    min += p.getMinOccurs();
                    max += p.getMaxOccurs();
                    if (!(((Saxon.Eej.ee.schema.ElementWildcard)(@base)).getWildcard().matches(p.getTargetComponentName(), true, config, this))) {
                        string uri = p.getTargetComponentName().getURI();
                        return ("The base type does not allow elements in "+(("".Equals(uri) ? "no namespace" : ("namespace "+uri))));
                    }
                }
                if ((min<@base.getMinOccurs())) {
                    return "The derived type allows fewer elements to appear than the base type requires in its @minOccurs";
                }
                if (((@base.getMaxOccurs()>=0) && (max>@base.getMaxOccurs()))) {
                    return "The derived type allows more elements to appear than the base type permits in its @maxOccurs";
                }
                return null;
            } else {
                return "A type using xs:all cannot be validly derived from this base type";
            }
        }
        public override void analyzeContentExpression(Saxon.Hej.expr.Expression expression, int kind) {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            foreach (Saxon.Eej.ee.schema.AttributeUse use in getAttributeGroup().getAttributeUses()) {
                if (use.isRequired()) {
                    if ((th.relationship(expression.getItemType(), Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE) == Saxon.Hej.type.Affinity.DISJOINT)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Type "+getDescription())+" defines a mandatory attribute ")+use.getTargetComponentName().getDisplayName())+", but there is no instruction to create such an attribute"));
                        err.setLocation(expression.getLocation());
                        err.setIsStaticError(true);
                        throw err;
                    }
                    if ((expression is Saxon.Hej.expr.instruct.Block)) {
                        Saxon.Hej.expr.Operand[] children = ((Saxon.Hej.expr.instruct.Block)(expression)).getOperanda();
                        bool foundPossible = false;
                        if ((children != null)) {
                            foreach (Saxon.Hej.expr.Operand o in children) {
                                Saxon.Hej.expr.Expression child = o.getChildExpression();
                                if ((child is Saxon.Hej.expr.instruct.FixedAttribute)) {
                                    if ((((Saxon.Hej.expr.instruct.FixedAttribute)(child)).getAttributeFingerprint() == use.getAttributeDeclaration().getFingerprint())) {
                                        foundPossible = true;
                                        break ;
                                    }
                                } else {
                                    if ((th.relationship(child.getItemType(), Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE) != Saxon.Hej.type.Affinity.DISJOINT)) {
                                        foundPossible = true;
                                        break ;
                                    }
                                }
                            }
                        }
                        if (!(foundPossible)) {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("Type "+getDescription())+" defines a mandatory attribute ")+use.getTargetComponentName().getDisplayName())+", but there is no instruction to create such an attribute"));
                            err.setLocation(expression.getLocation());
                            err.setIsStaticError(true);
                            throw err;
                        }
                    }
                }
            }
            if ((expression is Saxon.Hej.expr.instruct.Block)) {
                Saxon.Hej.expr.Operand[] children = ((Saxon.Hej.expr.instruct.Block)(expression)).getOperanda();
                if ((children == null)) {
                    return ;
                }
                foreach (Saxon.Hej.expr.Operand o in children) {
                    o.getChildExpression().checkPermittedContents(this, false);
                }
                if (((getFiniteStateMachine() != null) && !(isAllContent()))) {
                    Saxon.Eej.ee.schema.fsa.State state = getInitialState();
                    foreach (Saxon.Hej.expr.Operand o in children) {
                        Saxon.Hej.expr.Expression child = o.getChildExpression();
                        if ((child is Saxon.Hej.expr.instruct.FixedElement)) {
                            Saxon.Hej.om.NodeName elementName = ((Saxon.Hej.expr.instruct.FixedElement)(child)).getFixedElementName();
                            int fingerprint = ((Saxon.Hej.expr.instruct.FixedElement)(child)).getFixedElementName().getFingerprint();
                            Saxon.Eej.ee.schema.fsa.Edge edge = state.getTransition(fingerprint, this);
                            if ((edge == null)) {
                                Saxon.Eej.ee.schema.fsa.FiniteStateMachine machine = getFiniteStateMachine();
                                Saxon.Eej.ee.schema.Wildcard openContentWildcard = machine.getOpenContentWildcard();
                                if (((openContentWildcard != null) && openContentWildcard.matches(elementName, true, getConfiguration(), this))) {
                                    if (machine.isOpenContentInterleaved()) {
                                        continue ;
                                    } else {
                                        if (state.isFinalState()) {
                                            state = /*134*/Saxon.Eej.ee.schema.fsa.SuffixState.getInstance();
                                            continue ;
                                        }
                                    }
                                }
                                if ((state.isFinalState() && !(getExtensionTypes().Count==0))) {
                                    break ;
                                }
                                string sb = state.listAllowedElements();
                                string message;
                                if ((state is Saxon.Eej.ee.schema.fsa.SuffixState)) {
                                    message = (("The content model does not allow element "+Saxon.Hej.trans.Err.wrap(elementName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" to appear after an element that matches the suffix open content wildcard. ");
                                } else {
                                    message = ((((("The content model does not allow element "+Saxon.Hej.trans.Err.wrap(elementName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" to appear here")+(((openContentWildcard == null) ? "" : " (it does not match the open content wildcard)")))+". ")+sb);
                                }
                                Saxon.Hej.trans.XPathException ve = new Saxon.Hej.trans.XPathException(message);
                                ve.setErrorCode("XPTY0004");
                                ve.setLocation(child.getLocation());
                                throw ve;
                            }
                            try {
                                state = edge.makeTransition(state);
                            } catch (Saxon.Hej.type.ValidationException e) {
                                Saxon.Hej.trans.XPathException ve = new Saxon.Hej.trans.XPathException(e.Message);
                                ve.setErrorCode("XPTY0004");
                                ve.setLocation(expression.getLocation());
                                throw ve;
                            }
                        } else {
                            Saxon.Hej.type.ItemType t = child.getItemType();
                            if ((th.relationship(t, Saxon.Hej.pattern.NodeKindTest.ELEMENT) != Saxon.Hej.type.Affinity.DISJOINT)) {
                                return ;
                            }
                        }
                    }
                    if (!(state.isFinalState())) {
                        int len = children.Length;
                        if ((len == 0)) {
                            Saxon.Hej.trans.XPathException ve = new Saxon.Hej.trans.XPathException("The generated content is empty: the schema requires at least one child element");
                            ve.setErrorCode("XPTY0004");
                            ve.setLocation(expression.getLocation());
                            throw ve;
                        } else {
                            Saxon.Hej.trans.XPathException ve = new Saxon.Hej.trans.XPathException("The generated content is incomplete: the schema requires further child elements");
                            ve.setErrorCode("XPTY0004");
                            ve.setLocation(children[(len-1)].getChildExpression().getLocation());
                            throw ve;
                        }
                    }
                }
            } else {
                expression.checkPermittedContents(this, true);
            }
        }
        public override Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node) {
            if (isSimpleContent()) {
                if (node.isNilled()) {
                    return Saxon.Hej.om.EmptyAtomicSequence.getInstance();
                }
                try {
                    return getSimpleContentType().atomize(node);
                } catch (Saxon.Hej.type.MissingComponentException e) {
                    throw new Saxon.Hej.trans.XPathException("Can't get typed value; node is annotated with an incomplete type definition");
                }
            } else {
                if (isMixedContent()) {
                    if (node.isNilled()) {
                        return Saxon.Hej.om.EmptyAtomicSequence.getInstance();
                    }
                    return Saxon.Hej.value.StringValue.makeUntypedAtomic(node.getUnicodeStringValue());
                } else {
                    if (isEmptyContent()) {
                        return Saxon.Hej.om.EmptyAtomicSequence.getInstance();
                    } else {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Cannot get the typed value of an element ("+node.getDisplayName())+") with element-only content"));
                        err.setErrorCode("FOTY0012");
                        err.setIsTypeError(true);
                        throw err;
                    }
                }
            }
        }
        public static void checkTypeDerivation(Saxon.Hej.type.ComplexType derived, Saxon.Hej.type.SchemaType @base, int block) {
            bool sameType = derived.isSameType(@base);
            if ((!(sameType) && ((((derived.getDerivationMethod()&block)) != 0)))) {
                throw new Saxon.Hej.type.SchemaException((((("Derivation of the requested type "+derived.getDescription())+" is blocked either by the base type ")+derived.getDescription())+" or by the element declaration"));
            }
            if (sameType) {
                return ;
            }
            Saxon.Hej.type.SchemaType baseTypeOfD = derived.getBaseType();
            if ((baseTypeOfD == null)) {
                baseTypeOfD = Saxon.Hej.type.AnyType.getInstance();
            }
            if (@base.isSameType(baseTypeOfD)) {
                return ;
            }
            if ((baseTypeOfD == Saxon.Hej.type.AnyType.getInstance())) {
                throw new Saxon.Hej.type.SchemaException(((("The requested type "+derived.getDescription())+" is not derived from the declared type ")+@base.getDescription()));
            }
            if (baseTypeOfD.isComplexType()) {
                try {
                    checkTypeDerivation((Saxon.Hej.type.ComplexType)(baseTypeOfD), @base, block);
                } catch (Saxon.Hej.type.SchemaException err) {
                    throw new Saxon.Hej.type.SchemaException(((("The requested type "+derived.getDescription())+" is not validly derived from the declared type ")+@base.getDescription()));
                }
            } else {
                string err = /*134*/Saxon.Eej.ee.schema.UserSimpleType.isTypeDerivationOK((Saxon.Hej.type.SimpleType)(baseTypeOfD), @base, block);
                if ((err != null)) {
                    throw new Saxon.Hej.type.SchemaException(((((("The requested type "+derived.getDescription())+" is not validly derived from the declared type ")+@base.getDescription())+". ")+err));
                }
            }
        }
        public override void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getValidationStatus() != /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED)) {
                Saxon.Hej.type.SchemaType @base = getBaseType();
                if ((@base is Saxon.Eej.ee.schema.UserComplexType)) {
                    ((Saxon.Eej.ee.schema.UserComplexType)(@base)).elaborate(compiler);
                }
                xsdVersion = compiler.getLanguageVersion();
                extendedParticle = particle;
                if ((particle != null)) {
                    particle.elaborate(compiler);
                }
                makeCombinedAttributeGroup(null);
                computeIsAllContent();
                extendedAttributes.buildCounterMap();
                foreach (Saxon.Eej.ee.schema.AttributeUse att in extendedAttributes.getAttributeUses()) {
                    att.elaborate(compiler);
                }
                makeContextDeterminedTypeMapForAttributes();
                makeContextDeterminedTypeMapForElements();
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            }
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            string id = serializer.getId(this, true);
            serializer.startElement("complexType");
            serializer.emitAttribute("id", id);
            if (!(isAnonymousType())) {
                serializer.emitAttribute("name", getName());
                if (!(getTargetNamespace().Length==0)) {
                    serializer.emitAttribute("targetNamespace", getTargetNamespace());
                }
            }
            serializer.emitAttribute("base", serializer.getTypeLink(getBaseType()));
            if ((finalProhibitions != 0)) {
                string ex = "";
                if ((((finalProhibitions&Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) != 0)) {
                    ex = (((ex.Length==0 ? "" : " "))+"extension");
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
                serializer.emitAttribute("block", dis);
            }
            serializer.emitAttribute("derivationMethod", ((getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_EXTENSION) ? "extension" : "restriction"));
            serializer.emitAttribute("abstract", (isAbstract() ? "true" : "false"));
            string varietyName;
            switch (variety) {
              case Saxon.Hej.type.ComplexVariety.EMPTY:
                varietyName = "empty";
                break ;
              case Saxon.Hej.type.ComplexVariety.MIXED:
                varietyName = "mixed";
                break ;
              case Saxon.Hej.type.ComplexVariety.SIMPLE:
                varietyName = "simple";
                break ;
              case Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY:
              default:
                varietyName = "element-only";
                break ;
            }
            serializer.emitAttribute("variety", varietyName);
            if (isSimpleContent()) {
                Saxon.Hej.type.SimpleType st = getSimpleContentType();
                serializer.emitAttribute("simpleType", serializer.getTypeLink(st));
            }
            if ((((openContentWildcard != null) && (openContentMode != Saxon.Hej.type.OpenContentVariety.ABSENT)) && (openContentMode != Saxon.Hej.type.OpenContentVariety.NONE))) {
                string modeName;
                switch (openContentMode) {
                  default:
                    throw new Saxon.Ejava.lang.IllegalStateException("openContent mode must be suffix or interleave");
                  case /*implicit enum*/ Saxon.Hej.type.OpenContentVariety.INTERLEAVE:
                    modeName = "interleave";
                    break ;
                  case /*implicit enum*/ Saxon.Hej.type.OpenContentVariety.SUFFIX:
                    modeName = "suffix";
                    break ;
                }
                string openContentId = serializer.getId(openContentWildcard, false);
                serializer.startElement("openContent");
                serializer.emitAttribute("mode", modeName);
                serializer.emitAttribute("wildcard", openContentId);
                serializer.endElement();
            }
            getCombinedAttributeGroup().serializeContents(serializer);
            if (isComplexContent()) {
                Saxon.Eej.ee.schema.Particle particle = getParticle();
                if ((particle != null)) {
                    particle.serializeParticle(serializer);
                }
                if ((machine.getInitialState() != null)) {
                    machine.serialize(serializer);
                }
            }
            if (hasAssertions()) {
                foreach (Saxon.Eej.ee.schema.Assertion assertion in getAssertions()) {
                    assertion.serialize(serializer);
                }
            }
            serializer.endElement();
        }
        public override Saxon.Hej.om.Function getComponentAsFunction() {
            return getComponentAsFunction(this);
        }
        public static Saxon.Hej.om.Function getComponentAsFunction(Saxon.Hej.type.ComplexType type) {
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Complex Type Definition");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Hej.type.ComplexType>(type);
                  case "name":
                    if (type.isAnonymousType()) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return new Saxon.Hej.value.StringValue(type.getName(), Saxon.Hej.type.BuiltInAtomicType.NCNAME);
                    }
                  case "target namespace":
                    if (type.isAnonymousType()) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return new Saxon.Hej.value.AnyURIValue((type.getTargetNamespace()));
                    }
                  case "base type definition":
                    return type.getBaseType().getComponentAsFunction();
                  case "final":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> list = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                        int f = type.getFinalProhibitions();
                        if ((((f&Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) != 0)) {
                            list.Add(Saxon.Hej.value.StringValue.bmp("restriction"));
                        }
                        if ((((f&Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) != 0)) {
                            list.Add(Saxon.Hej.value.StringValue.bmp("extension"));
                        }
                        return new Saxon.Hej.om.AtomicArray(list);
                    }
                  case "context":
                    if (((type is Saxon.Eej.ee.schema.UserComplexType) && !(type.isAnonymousType()))) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                  case "derivation method":
                    return new Saxon.Hej.value.StringValue(((type.getDerivationMethod() == Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION) ? "restriction" : "extension"));
                  case "abstract":
                    return Saxon.Hej.value.BooleanValue.get(type.isAbstract());
                  case "attribute uses":
                    if ((type is Saxon.Eej.ee.schema.UserComplexType)) {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        foreach (Saxon.Eej.ee.schema.AttributeUse use in ((Saxon.Eej.ee.schema.UserComplexType)(type)).getAttributeGroup().getLocalAttributes()) {
                            list.Add(use.getComponentAsFunction());
                        }
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                  case "attribute wildcard":
                    if ((type is Saxon.Eej.ee.schema.UserComplexType)) {
                        Saxon.Eej.ee.schema.AttributeWildcard wild = ((Saxon.Eej.ee.schema.UserComplexType)(type)).getAttributeGroup().getLocalAttributeWildcard();
                        return wild.getComponentAsFunction();
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                  case "content type":
                    Saxon.Hej.expr.CallableDelegate content = new Saxon.Hej.expr.CallableDelegate((context2, arguments2) => {
                        string key2 = arguments2[0].head().getStringValue();
                        switch (key2) {
                          case "class":
                            return Saxon.Hej.value.StringValue.bmp("Content Type");
                          case "variety":
                            string variety;
                            switch (type.getVariety()) {
                              case Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY:
                                variety = "element-only";
                                break ;
                              case Saxon.Hej.type.ComplexVariety.EMPTY:
                                variety = "empty";
                                break ;
                              case Saxon.Hej.type.ComplexVariety.MIXED:
                                variety = "mixed";
                                break ;
                              case Saxon.Hej.type.ComplexVariety.SIMPLE:
                              default:
                                variety = "simple";
                                break ;
                            }
                            return Saxon.Hej.value.StringValue.bmp(variety);
                          case "particle":
                            if (((type.getVariety() == Saxon.Hej.type.ComplexVariety.MIXED) || (type.getVariety() == Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY))) {
                                return ((Saxon.Eej.ee.schema.UserComplexType)(type)).getParticle().getComponentAsFunction();
                            } else {
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            }
                          case "open content":
                            if ((type is Saxon.Eej.ee.schema.UserComplexType)) {
                                Saxon.Eej.ee.schema.UserComplexType ctype = (Saxon.Eej.ee.schema.UserComplexType)(type);
                                string mode = ctype.getOpenContentModeString();
                                if (("interleave".Equals(mode) || "suffix".Equals(mode))) {
                                    Saxon.Hej.expr.CallableDelegate open = new Saxon.Hej.expr.CallableDelegate((context3, arguments3) => {
                                        string key3 = arguments[0].head().getStringValue();
                                        switch (key3) {
                                          case "class":
                                            return Saxon.Hej.value.StringValue.bmp("Open Content");
                                          case "mode":
                                            return new Saxon.Hej.value.StringValue(mode);
                                          case "wildcard":
                                            return ctype.getOpenContentWildcard().getComponentAsFunction();
                                          default:
                                            return Saxon.Hej.value.EmptySequence.getInstance();
                                        }
                                    } );
                                    return new Saxon.Hej.functions.CallableFunction(1, open, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                                } else {
                                    return Saxon.Hej.value.EmptySequence.getInstance();
                                }
                            } else {
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            }
                          case "simple type definition":
                            if (type.isSimpleContent()) {
                                return type.getSimpleContentType().getComponentAsFunction();
                            } else {
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            }
                          default:
                            return Saxon.Hej.value.EmptySequence.getInstance();
                        }
                    } );
                    return new Saxon.Hej.functions.CallableFunction(1, content, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                  case "prohibited substitutions":
                    {
                        System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> list = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                        int s = type.getBlock();
                        if ((((s&Saxon.Hej.type.Derivation.DERIVATION_EXTENSION)) != 0)) {
                            list.Add(Saxon.Hej.value.StringValue.bmp("extension"));
                        }
                        if ((((s&Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) != 0)) {
                            list.Add(Saxon.Hej.value.StringValue.bmp("restriction"));
                        }
                        return new Saxon.Hej.om.AtomicArray(list);
                    }
                  case "assertions":
                    if ((type is Saxon.Eej.ee.schema.UserComplexType)) {
                        System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                        foreach (Saxon.Eej.ee.schema.Assertion assertion in ((Saxon.Eej.ee.schema.UserComplexType)(type)).getAssertions()) {
                            list.Add(assertion.getComponentAsFunction());
                        }
                        return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(list);
                    } else {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    }
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}