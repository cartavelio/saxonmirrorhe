using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class PreparedSchema : Saxon.Hej.om.NotationSet {
        public const int NAMESPACE_UNKNOWN = 1;
        public const int NAMESPACE_UNDER_CONSTRUCTION = 2;
        public const int NAMESPACE_KNOWN = 3;
        public const int NAMESPACE_SEALED = 4;
        private Saxon.Eej.config.EnterpriseConfiguration config;
        private readonly System.Collections.Generic.ISet<string> targetNamespaces = new System.Collections.Generic.HashSet<string>();
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Eej.ee.schema.AttributeDecl> attributesByName = null;
        private Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.AttributeDecl> attributesByFingerprint = null;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Eej.ee.schema.AttributeGroupDecl> attributeGroups = null;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Eej.ee.schema.ModelGroupDefinition> groups = null;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.type.SchemaType> typesByName = null;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.type.SchemaType> anonymousTypesByName = null;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Eej.ee.schema.ElementDecl> elementsByName = null;
        private Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl> elementsByFingerprint = null;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Eej.ee.schema.IdentityConstraint> identityConstraints;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Eej.ee.schema.Notation> notations;
        private Saxon.Hej.om.NamePool namePool;
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.UserSchemaComponent> components;
        private Saxon.Hej.z.IntHashMap<System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType>> extensionsOfBuiltInTypes;
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.GlobalParam> @params;
        private int localLicenseId = -1;
        protected internal PreparedSchema()  {
        }
        public PreparedSchema(Saxon.Eej.config.EnterpriseConfiguration config)  {
            namePool = config.getNamePool();
            this.config = config;
            attributesByName = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Eej.ee.schema.AttributeDecl>(10);
            attributesByFingerprint = new Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.AttributeDecl>(10);
            attributeGroups = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Eej.ee.schema.AttributeGroupDecl>(20);
            groups = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Eej.ee.schema.ModelGroupDefinition>(20);
            typesByName = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.type.SchemaType>(100);
            elementsByName = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Eej.ee.schema.ElementDecl>(100);
            elementsByFingerprint = new Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl>(100);
            anonymousTypesByName = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.type.SchemaType>(100);
            @params = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.instruct.GlobalParam>(10);
        }
        public void setConfiguration(Saxon.Eej.config.EnterpriseConfiguration config) {
            this.config = config;
        }
        public Saxon.Eej.config.EnterpriseConfiguration getConfiguration() {
            return config;
        }
        public void setLocalLicenseId(int id) {
            localLicenseId = id;
        }
        public int getLocalLicenseId() {
            return localLicenseId;
        }
        public void addComponent(Saxon.Eej.ee.schema.UserSchemaComponent obj) {
            if ((components == null)) {
                components = new System.Collections.Generic.List<Saxon.Eej.ee.schema.UserSchemaComponent>(20);
            }
            components.Add(obj);
        }
        private void fixupComponentReferences(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            int errors = 0;
            if ((components != null)) {
                foreach (Saxon.Eej.ee.schema.UserSchemaComponent component in components) {
                    try {
                        bool b = component.fixup(compiler);
                        if ((!(b) && !(compiler.isAllowDanglingReferences()))) {
                            errors++;
                        }
                    } catch (Saxon.Hej.type.SchemaException err) {
                        errors++;
                        compiler.getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(err));
                    }
                }
                if ((errors>0)) {
                    throw new Saxon.Hej.type.SchemaException("Errors were found in the schema");
                }
            }
        }
        private bool isBuiltInNamespace(string @namespace) {
            return ((@namespace.Equals(Saxon.Hej.lib.NamespaceConstant.XML) || @namespace.Equals(Saxon.Hej.lib.NamespaceConstant.FN)) || @namespace.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE));
        }
        public Saxon.Eej.ee.schema.AttributeDecl getAttributeDecl(Saxon.Hej.om.StructuredQName name) {
            Saxon.Eej.ee.schema.AttributeDecl decl = Saxon.Impl.Helpers.MapUtils.Get(attributesByName, name);
            if (((decl == null) && isBuiltInNamespace(name.getURI()))) {
                config.addSchemaForBuiltInNamespace(name.getURI());
                decl = Saxon.Impl.Helpers.MapUtils.Get(attributesByName, name);
            }
            return decl;
        }
        public Saxon.Eej.ee.schema.AttributeDecl getAttributeDecl(int fingerprint) {
            return attributesByFingerprint.get(fingerprint);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.AttributeDecl> getAttributeDeclarations() {
            return attributesByName.Values;
        }
        public void addAttributeDecl(Saxon.Eej.ee.schema.AttributeDecl attribute) {
            Saxon.Eej.ee.schema.AttributeDecl oldDecl = Saxon.Impl.Helpers.MapUtils.Put(attributesByName, attribute.getComponentName(), attribute);
            if ((oldDecl != attribute)) {
                attributesByFingerprint.put(attribute.getFingerprint(), attribute);
                addComponent(attribute);
                targetNamespaces.Add(attribute.getTargetNamespace());
            }
        }
        public Saxon.Eej.ee.schema.AttributeGroupDecl getAttributeGroup(Saxon.Hej.om.StructuredQName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(attributeGroups, name);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.AttributeGroupDecl> getAllAttributeGroups() {
            return attributeGroups.Values;
        }
        public void addAttributeGroup(Saxon.Eej.ee.schema.AttributeGroupDecl attributeGroup) {
            Saxon.Eej.ee.schema.AttributeGroupDecl existing = Saxon.Impl.Helpers.MapUtils.Get(attributeGroups, attributeGroup.getAttributeGroupName());
            if (((existing == null) || (existing.getRedefinitionLevel()<=attributeGroup.getRedefinitionLevel()))) {
                Saxon.Impl.Helpers.MapUtils.Put(attributeGroups, attributeGroup.getAttributeGroupName(), attributeGroup);
                if ((existing != attributeGroup)) {
                    addComponent(attributeGroup);
                    targetNamespaces.Add(attributeGroup.getTargetNamespace());
                }
            }
        }
        public Saxon.Eej.ee.schema.ModelGroupDefinition getGroup(Saxon.Hej.om.StructuredQName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(groups, name);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.ModelGroupDefinition> getAllModelGroups() {
            return groups.Values;
        }
        public void addGroup(Saxon.Eej.ee.schema.ModelGroupDefinition group) {
            Saxon.Eej.ee.schema.ModelGroupDefinition existing = Saxon.Impl.Helpers.MapUtils.Get(groups, group.getModelGroupName());
            if (((existing == null) || (existing.getRedefinitionLevel()<=group.getRedefinitionLevel()))) {
                Saxon.Impl.Helpers.MapUtils.Put(groups, group.getModelGroupName(), group);
                if ((existing != group)) {
                    addComponent(group);
                    targetNamespaces.Add(group.getTargetNamespace());
                }
            }
        }
        public Saxon.Hej.type.SchemaType getSchemaType(Saxon.Hej.om.StructuredQName name) {
            if (name.hasURI(Saxon.Hej.lib.NamespaceConstant.SCHEMA)) {
                Saxon.Hej.type.SchemaType type = Saxon.Hej.type.BuiltInType.getSchemaTypeByLocalName(name.getLocalPart());
                if ((type != null)) {
                    return type;
                }
            }
            return Saxon.Impl.Helpers.MapUtils.Get(typesByName, name);
        }
        public Saxon.Hej.type.SimpleType getSimpleType(Saxon.Hej.om.StructuredQName name) {
            Saxon.Hej.type.SchemaType t = getSchemaType(name);
            if ((t is Saxon.Hej.type.SimpleType)) {
                return (Saxon.Hej.type.SimpleType)(t);
            }
            return null;
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.type.SchemaType> getAllTypes() {
            return typesByName.Values;
        }
        public void addType(Saxon.Hej.type.SchemaType type) {
            Saxon.Hej.type.SchemaType existing = Saxon.Impl.Helpers.MapUtils.Get(typesByName, type.getStructuredQName());
            if (((existing == null) || (existing.getRedefinitionLevel()<type.getRedefinitionLevel()))) {
                Saxon.Impl.Helpers.MapUtils.Put(typesByName, type.getStructuredQName(), type);
                if (((existing != type) && (type is Saxon.Eej.ee.schema.UserSchemaComponent))) {
                    addComponent((Saxon.Eej.ee.schema.UserSchemaComponent)(type));
                    targetNamespaces.Add(type.getTargetNamespace());
                }
            }
        }
        public void addAnonymousType(Saxon.Hej.type.SchemaType type) {
            Saxon.Impl.Helpers.MapUtils.Put(anonymousTypesByName, type.getStructuredQName(), type);
        }
        public Saxon.Hej.type.SchemaType getType(Saxon.Hej.om.StructuredQName name) {
            Saxon.Hej.type.SchemaType type = getSchemaType(name);
            if ((type == null)) {
                type = Saxon.Impl.Helpers.MapUtils.Get(anonymousTypesByName, name);
            }
            return type;
        }
        public void registerExtensionOfBuiltInType(int fp, Saxon.Eej.ee.schema.UserComplexType extension) {
            if ((extensionsOfBuiltInTypes == null)) {
                extensionsOfBuiltInTypes = new Saxon.Hej.z.IntHashMap<System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType>>(20);
            }
            System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType> set = extensionsOfBuiltInTypes.get(fp);
            if ((set == null)) {
                set = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.UserComplexType>(4);
                extensionsOfBuiltInTypes.put(fp, set);
            }
            set.Add(extension);
        }
        public System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType> getExtensionsOfBuiltInType(int fp) {
            if ((extensionsOfBuiltInTypes == null)) {
                return new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.UserComplexType>();
            }
            System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType> set = extensionsOfBuiltInTypes.get(fp);
            if ((set == null)) {
                return new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.UserComplexType>();
            } else {
                return set;
            }
        }
        public void addElementDecl(Saxon.Eej.ee.schema.ElementDecl elementDecl) {
            Saxon.Eej.ee.schema.ElementDecl existing = Saxon.Impl.Helpers.MapUtils.Get(elementsByName, elementDecl.getComponentName());
            if (((existing == null) || (existing.getRedefinitionLevel()<elementDecl.getRedefinitionLevel()))) {
                Saxon.Impl.Helpers.MapUtils.Put(elementsByName, elementDecl.getComponentName(), elementDecl);
                elementsByFingerprint.put(elementDecl.getFingerprint(), elementDecl);
                if ((existing != elementDecl)) {
                    addComponent(elementDecl);
                    targetNamespaces.Add(elementDecl.getTargetNamespace());
                }
            }
        }
        public Saxon.Eej.ee.schema.ElementDecl getElementDecl(Saxon.Hej.om.StructuredQName name) {
            Saxon.Eej.ee.schema.ElementDecl decl = Saxon.Impl.Helpers.MapUtils.Get(elementsByName, name);
            if (((decl == null) && isBuiltInNamespace(name.getURI()))) {
                config.addSchemaForBuiltInNamespace(name.getURI());
                decl = Saxon.Impl.Helpers.MapUtils.Get(elementsByName, name);
            }
            return decl;
        }
        public Saxon.Eej.ee.schema.ElementDecl getElementDecl(int fingerprint) {
            return elementsByFingerprint.get(fingerprint);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.ElementDecl> getElementDeclarations() {
            return elementsByName.Values;
        }
        public void addNotation(Saxon.Eej.ee.schema.Notation notation) {
            if ((notations == null)) {
                notations = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Eej.ee.schema.Notation>(20);
            }
            System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Eej.ee.schema.Notation> nn = notations;
            Saxon.Eej.ee.schema.Notation oldNotation = Saxon.Impl.Helpers.MapUtils.Put(nn, notation.getNotationName(), notation);
            if ((oldNotation != notation)) {
                addComponent(notation);
                targetNamespaces.Add(notation.getTargetNamespace());
            }
        }
        public Saxon.Eej.ee.schema.Notation getNotation(Saxon.Hej.om.StructuredQName notationName) {
            if ((notations == null)) {
                return null;
            }
            return Saxon.Impl.Helpers.MapUtils.Get(notations, notationName);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.Notation> getAllNotations() {
            if ((notations == null)) {
                return new System.Collections.Generic.List<Saxon.Eej.ee.schema.Notation>();
            }
            return notations.Values;
        }
        public void addIdentityConstraint(Saxon.Eej.ee.schema.IdentityConstraint constraint) {
            if ((identityConstraints == null)) {
                identityConstraints = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Eej.ee.schema.IdentityConstraint>(20);
            }
            Saxon.Eej.ee.schema.IdentityConstraint oldConstraint = Saxon.Impl.Helpers.MapUtils.Put(identityConstraints, constraint.getConstraintName(), constraint);
            if ((oldConstraint != constraint)) {
                addComponent(constraint);
                targetNamespaces.Add(constraint.getTargetNamespace());
            }
        }
        public Saxon.Eej.ee.schema.IdentityConstraint getIdentityConstraint(Saxon.Hej.om.StructuredQName name) {
            if ((identityConstraints == null)) {
                return null;
            }
            return Saxon.Impl.Helpers.MapUtils.Get(identityConstraints, name);
        }
        public System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.IdentityConstraint> getIdentityConstraints() {
            if ((identityConstraints == null)) {
                return new System.Collections.Generic.List<Saxon.Eej.ee.schema.IdentityConstraint>();
            }
            return identityConstraints.Values;
        }
        public System.Collections.Generic.ISet<string> getTargetNamespaces() {
            return targetNamespaces;
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return namePool;
        }
        public void addGlobalParam(Saxon.Hej.expr.instruct.GlobalParam param) {
            Saxon.Hej.expr.instruct.GlobalParam prev = Saxon.Impl.Helpers.MapUtils.Put(@params, param.getVariableQName(), param);
            if (((prev != null) && (prev != param))) {
                throw new Saxon.Hej.type.SchemaException(("Duplicate declaration of parameter "+param.getVariableQName().getEQName()));
            }
        }
        public Saxon.Hej.expr.instruct.GlobalParam getGlobalParam(Saxon.Hej.om.StructuredQName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(@params, name);
        }
        public System.Collections.Generic.ICollection<Saxon.Hej.expr.instruct.GlobalParam> getGlobalParams() {
            return @params.Values;
        }
        public void copyTo(Saxon.Eej.ee.schema.PreparedSchema destination, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            System.Collections.Generic.IList<Saxon.Hej.om.Action> fixupActions = new System.Collections.Generic.List<Saxon.Hej.om.Action>();
            checkConsistency(destination, compiler, fixupActions);
            copyWithoutChecking(destination);
            foreach (Saxon.Hej.om.Action action in fixupActions) {
                try {
                    /*Delegate.doAction()*/action();
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.type.SchemaException(e);
                }
            }
        }
        private void copyWithoutChecking(Saxon.Eej.ee.schema.PreparedSchema destination) {
            foreach (Saxon.Hej.om.StructuredQName fp in Saxon.Impl.Helpers.MapUtils.KeySet(typesByName)) {
                Saxon.Hej.type.SchemaType type = Saxon.Impl.Helpers.MapUtils.Get(typesByName, fp);
                destination.addType(type);
            }
            foreach (Saxon.Hej.type.SchemaType schemaType in anonymousTypesByName.Values) {
                Saxon.Eej.ee.schema.UserDefinedType type = (Saxon.Eej.ee.schema.UserDefinedType)(schemaType);
                destination.addAnonymousType(type);
            }
            foreach (Saxon.Eej.ee.schema.ElementDecl el in elementsByName.Values) {
                destination.addElementDecl(el);
            }
            foreach (Saxon.Eej.ee.schema.AttributeDecl at in attributesByName.Values) {
                destination.addAttributeDecl(at);
            }
            foreach (Saxon.Eej.ee.schema.ModelGroupDefinition mg in groups.Values) {
                destination.addGroup(mg);
            }
            foreach (Saxon.Eej.ee.schema.AttributeGroupDecl ag in attributeGroups.Values) {
                destination.addAttributeGroup(ag);
            }
            if ((notations != null)) {
                foreach (Saxon.Eej.ee.schema.Notation n in notations.Values) {
                    destination.addNotation(n);
                }
            }
            if ((identityConstraints != null)) {
                foreach (Saxon.Eej.ee.schema.IdentityConstraint ic in identityConstraints.Values) {
                    destination.addIdentityConstraint(ic);
                }
            }
            System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.UserSchemaComponent> destinationComponents = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.UserSchemaComponent>();
            if ((destination.components != null)) {
                Saxon.Impl.Helpers.SetUtils.AddAll(destinationComponents, destination.components);
            }
            if ((components != null)) {
                foreach (Saxon.Eej.ee.schema.UserSchemaComponent comp in components) {
                    if (!(destinationComponents.Contains(comp))) {
                        destination.addComponent(comp);
                    }
                }
            }
            if ((extensionsOfBuiltInTypes != null)) {
                Saxon.Hej.z.IntIterator ii = extensionsOfBuiltInTypes.keyIterator();
                while (ii.hasNext()) {
                    int fp = ii.next();
                    System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType> types = getExtensionsOfBuiltInType(fp);
                    foreach (Saxon.Eej.ee.schema.UserComplexType type in types) {
                        destination.registerExtensionOfBuiltInType(fp, type);
                    }
                }
            }
            foreach (Saxon.Hej.expr.instruct.GlobalParam param in @params.Values) {
                try {
                    destination.addGlobalParam(param);
                } catch (Saxon.Hej.type.SchemaException err) {
                    throw new Saxon.Ejava.lang.IllegalStateException(err);
                }
            }
        }
        private void checkConsistency(Saxon.Eej.ee.schema.PreparedSchema destination, Saxon.Eej.ee.schema.SchemaCompiler compiler, System.Collections.Generic.IList<Saxon.Hej.om.Action> fixupActions) {
            bool conflicts = false;
            foreach (Saxon.Hej.om.StructuredQName fp in Saxon.Impl.Helpers.MapUtils.KeySet(typesByName)) {
                Saxon.Hej.type.SchemaType type = Saxon.Impl.Helpers.MapUtils.Get(typesByName, fp);
                Saxon.Hej.type.SchemaType existing = destination.getSchemaType(fp);
                if ((((existing != null) && (type.getRedefinitionLevel() == existing.getRedefinitionLevel())) && !(type.isSameType(existing)))) {
                    string loc = "";
                    if ((existing is Saxon.Ejavax.xml.transform.SourceLocator)) {
                        loc = existingLocation((Saxon.Ejavax.xml.transform.SourceLocator)(existing));
                    }
                    compiler.error((("Duplicate type "+Saxon.Hej.trans.Err.wrap(type.getDisplayName()))+loc), ((type is Saxon.Hej.s9api.Location) ? (Saxon.Hej.s9api.Location)(type) : null));
                    conflicts = true;
                }
            }
            foreach (Saxon.Hej.type.SchemaType schemaType in anonymousTypesByName.Values) {
                Saxon.Eej.ee.schema.UserDefinedType type = (Saxon.Eej.ee.schema.UserDefinedType)(schemaType);
                Saxon.Hej.type.SchemaType existing = destination.getSchemaType(type.getTypeName());
                if (((existing != null) && !(type.isSameType(existing)))) {
                    string loc = "";
                    if ((existing is Saxon.Ejavax.xml.transform.SourceLocator)) {
                        loc = existingLocation((Saxon.Ejavax.xml.transform.SourceLocator)(existing));
                    }
                    compiler.error((("Duplicate type "+Saxon.Hej.trans.Err.wrap(type.getName()))+loc), type);
                    conflicts = true;
                }
            }
            foreach (Saxon.Eej.ee.schema.ElementDecl decl in elementsByName.Values) {
                Saxon.Eej.ee.schema.ElementDecl existing = destination.getElementDecl(decl.getComponentName());
                if (((existing != null) && !(existing.isSameDeclaration(decl)))) {
                    string loc = existingLocation(existing);
                    compiler.error((("Duplicate element declaration "+Saxon.Hej.trans.Err.wrap(decl.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+loc), decl);
                    conflicts = true;
                }
            }
            foreach (Saxon.Eej.ee.schema.AttributeDecl decl in attributesByName.Values) {
                Saxon.Eej.ee.schema.AttributeDecl existing = destination.getAttributeDecl(decl.getComponentName());
                if (((existing != null) && !(existing.isSameDeclaration(decl)))) {
                    string loc = existingLocation(existing);
                    compiler.error((("Duplicate attribute declaration "+Saxon.Hej.trans.Err.wrap(decl.getDisplayName()))+loc), decl);
                    conflicts = true;
                }
            }
            foreach (Saxon.Eej.ee.schema.ModelGroupDefinition decl in groups.Values) {
                Saxon.Eej.ee.schema.ModelGroupDefinition existing = destination.getGroup(decl.getModelGroupName());
                if ((((existing != null) && (existing.getRedefinitionLevel() == decl.getRedefinitionLevel())) && !(existing.isSameGroup(decl)))) {
                    string loc = existingLocation(existing);
                    compiler.error((("Duplicate group declaration "+Saxon.Hej.trans.Err.wrap(decl.getName()))+loc), decl);
                    conflicts = true;
                }
            }
            foreach (Saxon.Eej.ee.schema.AttributeGroupDecl decl in attributeGroups.Values) {
                Saxon.Eej.ee.schema.AttributeGroupDecl existing = destination.getAttributeGroup(decl.getAttributeGroupName());
                if ((((existing != null) && (existing.getRedefinitionLevel() == decl.getRedefinitionLevel())) && !(existing.isSameDeclaration(decl)))) {
                    string loc = existingLocation(existing);
                    compiler.error((("Duplicate attribute group declaration "+Saxon.Hej.trans.Err.wrap(decl.getDisplayName()))+loc), decl);
                    conflicts = true;
                }
            }
            if ((notations != null)) {
                foreach (Saxon.Eej.ee.schema.Notation decl in notations.Values) {
                    Saxon.Eej.ee.schema.Notation existing = destination.getNotation(decl.getNotationName());
                    if ((((existing != null) && (existing != decl)) && !(decl.isSameDeclaration(existing)))) {
                        string loc = existingLocation(existing);
                        compiler.error((("Duplicate notation declaration "+Saxon.Hej.trans.Err.wrap(decl.getName()))+loc), decl);
                        conflicts = true;
                    }
                }
            }
            if ((identityConstraints != null)) {
                foreach (Saxon.Eej.ee.schema.IdentityConstraint decl in identityConstraints.Values) {
                    Saxon.Eej.ee.schema.IdentityConstraint existing = destination.getIdentityConstraint(decl.getConstraintName());
                    if (((existing != null) && !(existing.isSameDeclaration(decl)))) {
                        string loc = existingLocation(existing);
                        compiler.error((("Duplicate identity constraint name "+Saxon.Hej.trans.Err.wrap(decl.getName()))+loc), decl);
                        conflicts = true;
                    }
                }
            }
            foreach (Saxon.Hej.expr.instruct.GlobalParam param in @params.Values) {
                if ((destination.getGlobalParam(param.getVariableQName()) != null)) {
                    compiler.error(("Duplicate saxon:param declaration "+param.getVariableQName().getEQName()), param);
                    conflicts = true;
                }
            }
            if (conflicts) {
                throw new Saxon.Hej.type.SchemaException("Duplicated components found in schema");
            }
        }
        private string existingLocation(Saxon.Ejavax.xml.transform.SourceLocator existing) {
            if ((existing.getLineNumber()<1)) {
                return " - previously defined in imported component";
            } else {
                return (((" - previously defined on line "+existing.getLineNumber())+" of ")+existing.getSystemId());
            }
        }
        public bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result = true;
            fixupComponentReferences(compiler);
            foreach (Saxon.Eej.ee.schema.ElementDecl elementDecl in elementsByName.Values) {
                elementDecl.fixupSubstitutionGroup(compiler);
            }
            if ((components != null)) {
                foreach (Saxon.Eej.ee.schema.UserSchemaComponent s in components) {
                    result &= s.validate(compiler);
                    if ((result && ((s is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)))) {
                        Saxon.Eej.ee.schema.UserSimpleType st = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(s)).getWorkingType();
                        Saxon.Impl.Helpers.MapUtils.Put(typesByName, st.getTypeName(), st);
                    }
                }
            }
            if (result) {
                compiler.typeCheckIdentityConstraints();
            }
            components = null;
            if (result) {
                config.addSchema(this);
            }
            return result;
        }
        public static Saxon.Hej.type.SchemaComponent validateReference(Saxon.Eej.ee.schema.ComponentReference @ref, Saxon.Eej.ee.schema.SchemaCompiler compiler, bool fatal) {
            if (@ref.isDangling()) {
                return @ref.getTarget();
            }
            @ref.tryToResolve(compiler, fatal);
            if (@ref.isResolved()) {
                return @ref.getTarget();
            } else {
                string componentType = /*134*/Saxon.Eej.ee.schema.SymbolSpace.getSymbolSpaceName(@ref.getSymbolSpace());
                string reason = "has not been declared";
                if (@ref.getTargetComponentName().hasURI(Saxon.Hej.lib.NamespaceConstant.SCHEMA)) {
                    reason = "is not defined in the XML Schema namespace";
                }
                string message = ((((("The "+componentType)+' ')+Saxon.Hej.trans.Err.wrap(@ref.getTargetComponentName().getDisplayName()))+" is referenced, but ")+reason);
                if (compiler.reportMissingComponent(componentType, @ref.getTargetComponentName())) {
                    if ((fatal || !(compiler.isAllowDanglingReferences()))) {
                        compiler.error(message, @ref);
                    } else {
                        compiler.warning(message, @ref);
                    }
                }
                return null;
            }
        }
        public static Saxon.Eej.ee.schema.PreparedSchema merge(Saxon.Eej.ee.schema.PreparedSchema ps1, Saxon.Eej.ee.schema.PreparedSchema ps2) {
            if ((ps1.config != ps2.config)) {
                throw new Saxon.Hej.type.SchemaException("The two schemas to be merged must use the same EnterpriseConfiguration");
            }
            Saxon.Hej.om.NamePool pool = ps1.config.getNamePool();
            Saxon.Eej.ee.schema.PreparedSchema result = new Saxon.Eej.ee.schema.PreparedSchema(ps1.config);
            result.attributesByName = combine(ps1.attributesByName, ps2.attributesByName, "Attribute declaration");
            result.attributesByFingerprint = combine(ps1.attributesByFingerprint, ps2.attributesByFingerprint, pool, "Attribute declaration");
            result.attributeGroups = combine(ps1.attributeGroups, ps2.attributeGroups, "Attribute group");
            result.groups = combine(ps1.groups, ps2.groups, "Model group");
            result.typesByName = combine(ps1.typesByName, ps2.typesByName, "Type");
            result.anonymousTypesByName = combine(ps1.anonymousTypesByName, ps2.anonymousTypesByName, "Anonymous type");
            result.elementsByName = combine(ps1.elementsByName, ps2.elementsByName, "Element declaration");
            result.elementsByFingerprint = combine(ps1.elementsByFingerprint, ps2.elementsByFingerprint, pool, "Element declaration");
            result.notations = combine(ps1.notations, ps2.notations, "Notation");
            result.identityConstraints = combine(ps1.identityConstraints, ps2.identityConstraints, "Identity constraint");
            if (((ps1.components != null) || (ps2.components != null))) {
                result.components = new System.Collections.Generic.List<Saxon.Eej.ee.schema.UserSchemaComponent>(((((ps1.components == null) ? 0 : ps1.components.Count))+(((ps2.components == null) ? 0 : ps2.components.Count))));
                if ((ps1.components != null)) {
                    Saxon.Impl.Helpers.ListUtils.AddAll(result.components, ps1.components);
                }
                if ((ps2.components != null)) {
                    Saxon.Impl.Helpers.ListUtils.AddAll(result.components, ps2.components);
                }
            }
            Saxon.Impl.Helpers.SetUtils.AddAll(result.targetNamespaces, ps1.targetNamespaces);
            Saxon.Impl.Helpers.SetUtils.AddAll(result.targetNamespaces, ps2.targetNamespaces);
            return result;
        }
        private static System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,T> combine<T> (System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,T> m1, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,T> m2, string kind) where T : Saxon.Hej.type.SchemaComponent {
            if ((m1 == null)) {
                return m2;
            }
            if ((m2 == null)) {
                return m1;
            }
            System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,T> result = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, T>((m1.Count+m2.Count));
            foreach (Saxon.Hej.om.StructuredQName k1 in Saxon.Impl.Helpers.MapUtils.KeySet(m1)) {
                T v1 = Saxon.Impl.Helpers.MapUtils.Get(m1, k1);
                T v2 = Saxon.Impl.Helpers.MapUtils.Get(m2, k1);
                if (((v2 != null) && ((Saxon.Hej.type.SchemaComponent)(v2) != (Saxon.Hej.type.SchemaComponent)(v1)))) {
                    string loc1 = identify(v1);
                    string loc2 = identify(v2);
                    if (!(loc1.Equals(loc2))) {
                        throw new Saxon.Hej.type.SchemaException((((((((kind+" ")+k1.getDisplayName())+" is defined in more than one schema document (")+loc1)+", ")+loc2)+")"));
                    }
                }
                Saxon.Impl.Helpers.MapUtils.Put(result, k1, v1);
            }
            foreach (Saxon.Hej.om.StructuredQName k2 in Saxon.Impl.Helpers.MapUtils.KeySet(m2)) {
                Saxon.Impl.Helpers.MapUtils.Put(result, k2, Saxon.Impl.Helpers.MapUtils.Get(m2, k2));
            }
            return result;
        }
        private static string identify(Saxon.Hej.type.SchemaComponent component) {
            if ((component is Saxon.Eej.ee.schema.SchemaStructure)) {
                Saxon.Eej.ee.schema.SchemaStructure ss = (Saxon.Eej.ee.schema.SchemaStructure)(component);
                return ((("line "+ss.getLineNumber())+" of ")+ss.getSystemId());
            } else {
                if ((component is Saxon.Hej.type.SchemaType)) {
                    return ((Saxon.Hej.type.SchemaType)(component)).getEQName();
                } else {
                    return component.GetType().Name;
                }
            }
        }
        private static Saxon.Hej.z.IntHashMap<T> combine<T> (Saxon.Hej.z.IntHashMap<T> m1, Saxon.Hej.z.IntHashMap<T> m2, Saxon.Hej.om.NamePool pool, string kind) where T : Saxon.Eej.ee.schema.SchemaStructure {
            if ((m1 == null)) {
                return m2;
            }
            if ((m2 == null)) {
                return m1;
            }
            Saxon.Hej.z.IntHashMap<T> result = new Saxon.Hej.z.IntHashMap<T>((m1.size()+m2.size()));
            Saxon.Hej.z.IntIterator i1 = m1.keyIterator();
            while (i1.hasNext()) {
                int k1 = i1.next();
                T v1 = m1.get(k1);
                T v2 = m2.get(k1);
                if (((v2 != null) && (v2 != v1))) {
                    string loc1 = identify(v1);
                    string loc2 = identify(v2);
                    if (!(loc1.Equals(loc2))) {
                        throw new Saxon.Hej.type.SchemaException((((((((kind+" ")+pool.getEQName(k1))+" is defined in more than one schema document (")+loc1)+", ")+loc2)+")"));
                    }
                }
                result.put(k1, v1);
            }
            Saxon.Hej.z.IntIterator i2 = m2.keyIterator();
            while (i2.hasNext()) {
                int k2 = i2.next();
                result.put(k2, m2.get(k2));
            }
            return result;
        }
        public bool isDeclaredNotation(string uri, string local) {
            return ((notations != null) && (Saxon.Impl.Helpers.MapUtils.Get(notations, new Saxon.Hej.om.StructuredQName("", uri, local)) != null));
        }
    
    }
}