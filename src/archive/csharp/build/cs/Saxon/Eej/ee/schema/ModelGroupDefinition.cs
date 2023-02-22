using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class ModelGroupDefinition : Saxon.Eej.ee.schema.ModelGroup, Saxon.Eej.ee.schema.SerializableSchemaComponent {
        private Saxon.Hej.om.StructuredQName modelGroupName;
        private Saxon.Eej.ee.schema.ModelGroupDefinition redefinedModelGroupDefinition;
        public ModelGroupDefinition(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Ejavax.xml.transform.SourceLocator locator)  {
            setLocator(locator);
            setConfiguration(config);
        }
        public void setRedefinedModelGroupDefinition(Saxon.Eej.ee.schema.ModelGroupDefinition other) {
            redefinedModelGroupDefinition = other;
        }
        public void setModelGroupName(Saxon.Hej.om.StructuredQName nameCode) {
            this.modelGroupName = nameCode;
        }
        public Saxon.Hej.om.StructuredQName getModelGroupName() {
            return modelGroupName;
        }
        public string getName() {
            return modelGroupName.getLocalPart();
        }
        public Saxon.Hej.om.NamespaceUri getTargetNamespace() {
            return modelGroupName.getNamespaceUri();
        }
        public override bool isEmptiable() {
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                if (!(p.isEmptiable())) {
                    return false;
                }
            }
            return true;
        }
        public override bool isPointless(Saxon.Eej.ee.schema.ModelGroup container) {
            return (contentModel.Count==0 || (contentModel.Count == 1));
        }
        public override void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (references.Contains(this)) {
                string err = (("The definition of group "+Saxon.Hej.trans.Err.wrap(getModelGroupName().getDisplayName()))+" is circular");
                contentModel = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Particle>(20);
                throw new Saxon.Hej.type.SchemaException(err, this);
            }
            base.lookForCycles(references, compiler);
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result = true;
            if (!(isValidationNeeded())) {
                return true;
            }
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
            if (((contentModel == null) || contentModel.Count==0)) {
                string err = "The group should contain 'all' or 'sequence' or 'choice'";
                compiler.error(err, this);
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            if ((redefinedModelGroupDefinition != null)) {
                Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(Saxon.Hej.om.StandardNames.XS_ANY_TYPE, getConfiguration(), this);
                @ref.setTarget(Saxon.Hej.type.AnyType.getInstance());
                Saxon.Eej.ee.schema.UserComplexType r = new Saxon.Eej.ee.schema.UserComplexType(compiler.getConfiguration(), this);
                Saxon.Eej.ee.schema.ModelGroupParticle rp = new Saxon.Eej.ee.schema.ModelGroupParticle(compiler.getConfiguration(), null);
                rp.setTarget(this);
                r.setParticle(rp);
                r.setVariety(Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY);
                r.setBaseTypeReference(@ref);
                r.validate(compiler);
                Saxon.Eej.ee.schema.UserComplexType b = new Saxon.Eej.ee.schema.UserComplexType(compiler.getConfiguration(), redefinedModelGroupDefinition);
                Saxon.Eej.ee.schema.ModelGroupParticle bp = new Saxon.Eej.ee.schema.ModelGroupParticle(compiler.getConfiguration(), null);
                bp.setTarget(redefinedModelGroupDefinition);
                b.setParticle(bp);
                b.setVariety(Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY);
                b.setBaseTypeReference(@ref);
                b.validate(compiler);
                string problem = r.isSubsumedBy(b, compiler);
                if ((problem != null)) {
                    compiler.error(((("Group "+getName())+" is not a valid restriction of the group that it redefines. ")+problem), this);
                    result = false;
                }
            }
            result &= base.validate(compiler);
            setValidationStatus((result ? /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED : /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID));
            return result;
        }
        public bool isSameGroup(Saxon.Eej.ee.schema.ModelGroupDefinition other) {
            return ((this == other) || ((getModelGroupName().Equals(other.getModelGroupName()) && hasSameLocation(other))));
        }
        public override void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            string id = serializer.getId(this, true);
            serializer.startElement("modelGroupDefinition");
            serializer.emitAttribute("id", id);
            serializer.emitAttribute("name", getName());
            if (((getTargetNamespace() != null) && !(getTargetNamespace().isEmpty()))) {
                serializer.emitAttribute("targetNamespace", getTargetNamespace().ToString());
            }
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                p.serializeParticle(serializer);
            }
            serializer.endElement();
        }
        public override string getCompositorName() {
            return "modelGroupDefinition";
        }
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            Saxon.Eej.ee.schema.ModelGroupDefinition thisDefinition = this;
            Saxon.Hej.om.FunctionItem modelGroupFunction = base.getComponentAsFunction();
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Model Group Definition");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.ModelGroupDefinition>(thisDefinition);
                  case "name":
                    return new Saxon.Hej.value.StringValue(getName(), Saxon.Hej.type.BuiltInAtomicType.NCNAME);
                  case "target namespace":
                    return new Saxon.Hej.value.AnyURIValue(getTargetNamespace().toUnicodeString());
                  case "model group":
                    return modelGroupFunction;
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}