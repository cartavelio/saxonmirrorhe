using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class KeyRef : Saxon.Eej.ee.schema.IdentityConstraint, Saxon.Eej.ee.schema.ComponentReference {
        private Saxon.Hej.om.StructuredQName targetQName;
        private Saxon.Eej.ee.schema.IdentityConstraint target;
        private bool dangling = false;
        public KeyRef()  {
        }
        public KeyRef(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Hej.om.StructuredQName refer)  {
            setConfiguration(config);
            setTargetComponentName(refer);
        }
        public override string getConstraintCategory() {
            return "keyref";
        }
        public Saxon.Hej.om.StructuredQName getTargetComponentName() {
            return targetQName;
        }
        public void setTargetComponentName(Saxon.Hej.om.StructuredQName target) {
            this.targetQName = target;
        }
        public void setTargetKey(Saxon.Eej.ee.schema.IdentityConstraint key) {
            target = key;
        }
        public void tryToResolve(Saxon.Eej.ee.schema.SchemaCompiler compiler, bool fatal) {
            if (!(dangling)) {
                target = compiler.getPreparedSchema().getIdentityConstraint(getTargetComponentName());
                if ((target == null)) {
                    target = compiler.getConfiguration().getSuperSchema().getIdentityConstraint(getTargetComponentName());
                }
                if ((target == null)) {
                    dangling = true;
                }
            }
        }
        public bool isResolved() {
            return (target != null);
        }
        public bool isDangling() {
            return dangling;
        }
        public Saxon.Hej.type.SchemaComponent getTarget() {
            if ((target == null)) {
                throw new Saxon.Eej.ee.schema.MissingComponentExceptionImpl(this);
            }
            return target;
        }
        public int getSymbolSpace() {
            return Saxon.Eej.ee.schema.SymbolSpace.IDENTITY_CONSTRAINT;
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
                Saxon.Eej.ee.schema.IdentityConstraint key = (Saxon.Eej.ee.schema.IdentityConstraint)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(this, compiler, false));
                if ((key == null)) {
                    setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                } else {
                    if ((key is Saxon.Eej.ee.schema.KeyRef)) {
                        compiler.error((("The key reference "+Saxon.Hej.trans.Err.wrap(getTargetComponentName().getDisplayName()))+" refers to a key reference, not to a key or unique constraint"), this);
                        target = null;
                        setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                        return false;
                    } else {
                        key.fixup(compiler);
                    }
                }
            }
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result = base.validate(compiler);
            tryToResolve(compiler, false);
            if ((target == null)) {
                compiler.error((("Identity constraint "+Saxon.Hej.trans.Err.wrap(getTargetComponentName().getDisplayName()))+" is referenced, but has not been declared"), this);
                result = false;
            } else {
                if ((target is Saxon.Eej.ee.schema.KeyRef)) {
                    compiler.error((("The key reference "+Saxon.Hej.trans.Err.wrap(getTargetComponentName().getDisplayName()))+" refers to a key reference, not to a key or unique constraint"), this);
                    result = false;
                } else {
                    if ((getFields().Count != target.getFields().Count)) {
                        compiler.error((("The key reference "+Saxon.Hej.trans.Err.wrap(getTargetComponentName().getDisplayName()))+" has a different number of fields from the identity constraint that it references"), this);
                        result = false;
                    }
                }
            }
            return result;
        }
    
    }
}