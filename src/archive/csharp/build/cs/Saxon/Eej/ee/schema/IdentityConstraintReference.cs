using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class IdentityConstraintReference : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.ComponentReference {
        private Saxon.Hej.om.StructuredQName targetQName;
        private bool dangling;
        private Saxon.Eej.ee.schema.IdentityConstraint target;
        private int expectedKind;
        public IdentityConstraintReference()  {
        }
        public IdentityConstraintReference(Saxon.Hej.om.StructuredQName targetQName, int kind, Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Ejavax.xml.transform.SourceLocator locator)  {
            setConfiguration(config);
            this.targetQName = targetQName;
            this.expectedKind = kind;
            if ((locator != null)) {
                setLocator(locator);
            }
        }
        public int getExpectedKindOfConstraint() {
            return expectedKind;
        }
        public bool isResolved() {
            return (target != null);
        }
        public bool isDangling() {
            return dangling;
        }
        public void setTarget(Saxon.Eej.ee.schema.IdentityConstraint target) {
            this.target = target;
            this.targetQName = target.getConstraintName();
            if ((target is Saxon.Eej.ee.schema.Key)) {
                expectedKind = Saxon.Hej.om.StandardNames.XS_KEY;
            } else {
                if ((target is Saxon.Eej.ee.schema.Unique)) {
                    expectedKind = Saxon.Hej.om.StandardNames.XS_UNIQUE;
                } else {
                    if ((target is Saxon.Eej.ee.schema.KeyRef)) {
                        expectedKind = Saxon.Hej.om.StandardNames.XS_KEYREF;
                    }
                }
            }
        }
        public Saxon.Hej.om.StructuredQName getTargetComponentName() {
            return targetQName;
        }
        public int getSymbolSpace() {
            return Saxon.Eej.ee.schema.SymbolSpace.IDENTITY_CONSTRAINT;
        }
        public Saxon.Hej.type.SchemaComponent getTarget() {
            if ((target == null)) {
                throw new Saxon.Eej.ee.schema.MissingComponentExceptionImpl(this);
            }
            return target;
        }
        public void tryToResolve(Saxon.Eej.ee.schema.SchemaCompiler compiler, bool fatal) {
            if (!(dangling)) {
                if ((target == null)) {
                    target = compiler.getPreparedSchema().getIdentityConstraint(getTargetComponentName());
                }
                if ((target == null)) {
                    target = getConfiguration().getSuperSchema().getIdentityConstraint(getTargetComponentName());
                }
                if ((target == null)) {
                    dangling = true;
                }
            }
        }
    
    }
}