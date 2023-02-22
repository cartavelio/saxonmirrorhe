using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class Particle : Saxon.Eej.ee.schema.SimpleComponentReference, Saxon.Eej.ee.schema.UserSchemaComponent {
        private int maxOccurs = 1;
        private int minOccurs = 1;
        private bool vulnerable = false;
        public const int UNBOUNDED = -1;
        public int getMaxOccurs() {
            return maxOccurs;
        }
        public int getMinOccurs() {
            return minOccurs;
        }
        public void setMaxOccurs(int maxOccurs) {
            this.maxOccurs = maxOccurs;
        }
        public void setMinOccurs(int minOccurs) {
            this.minOccurs = minOccurs;
        }
        public abstract Saxon.Eej.ee.schema.Term getTerm();
        public void setVulnerable(bool vulnerable) {
            this.vulnerable = vulnerable;
        }
        public virtual void markVulnerableSubParticles() {
        }
        public bool isVulnerable() {
            return vulnerable;
        }
        public static int computeParticleCardinality(int minOccurs, int maxOccurs) {
            if (((maxOccurs == -1) || (maxOccurs>1))) {
                if ((minOccurs == 1)) {
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE;
                } else {
                    return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
                }
            } else {
                if ((maxOccurs == 1)) {
                    if ((minOccurs == 0)) {
                        return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
                    } else {
                        return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
                    }
                } else {
                    return 0;
                }
            }
        }
        public abstract bool containsElementWildcard();
        public abstract void gatherAllPermittedElements(Saxon.Hej.z.IntHashSet result, bool ignoreWildcards);
        public abstract int getElementParticleCardinality(int elementName);
        public abstract bool isEmptiable();
        public abstract Saxon.Hej.type.SchemaType getElementParticleType(int fingerprint);
        public abstract override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler);
        public abstract override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler);
        public abstract void compile(Saxon.Eej.ee.schema.SchemaCompiler compiler);
        public abstract bool isPointless(Saxon.Eej.ee.schema.ModelGroup container);
        public abstract void serializeParticle(Saxon.Eej.ee.schema.SchemaModelSerializer serializer);
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            Saxon.Eej.ee.schema.Particle thisParticle = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Particle");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.Particle>(thisParticle);
                  case "min occurs":
                    return new Saxon.Hej.value.Int64Value(getMinOccurs(), Saxon.Hej.type.BuiltInAtomicType.NON_NEGATIVE_INTEGER, false);
                  case "max occurs":
                    if ((getMaxOccurs()<0)) {
                        return Saxon.Hej.value.StringValue.bmp("unbounded");
                    } else {
                        return new Saxon.Hej.value.Int64Value(getMaxOccurs(), Saxon.Hej.type.BuiltInAtomicType.NON_NEGATIVE_INTEGER, false);
                    }
                  case "term":
                    return ((Saxon.Hej.type.SchemaComponent)(getTerm())).getComponentAsFunction();
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}