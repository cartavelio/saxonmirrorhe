using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class SimpleComponentReference : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.ComponentReference {
        private Saxon.Hej.om.StructuredQName targetComponentName = null;
        private int targetFingerprint = -1;
        private bool dangling = false;
        protected internal Saxon.Hej.type.SchemaComponent target;
        protected internal int symbolSpace;
        protected internal SimpleComponentReference()  {
        }
        public SimpleComponentReference(int symbolSpace, Saxon.Hej.om.StructuredQName name, Saxon.Eej.config.EnterpriseConfiguration config)  {
            setConfiguration(config);
            this.symbolSpace = symbolSpace;
            setTargetComponentName(name);
        }
        public virtual bool isResolved() {
            return (target != null);
        }
        public void setTargetComponentName(Saxon.Hej.om.StructuredQName name) {
            this.targetComponentName = name;
        }
        public Saxon.Hej.om.StructuredQName getTargetComponentName() {
            return targetComponentName;
        }
        public int getTargetFingerprint() {
            if ((targetFingerprint == -1)) {
                targetFingerprint = getConfiguration().getNamePool().allocateFingerprint(targetComponentName.getNamespaceUri(), targetComponentName.getLocalPart());
            }
            return targetFingerprint;
        }
        public virtual int getSymbolSpace() {
            return symbolSpace;
        }
        public virtual Saxon.Hej.type.SchemaComponent getTarget() {
            if ((target == null)) {
                throw new Saxon.Eej.ee.schema.MissingComponentExceptionImpl(this);
            }
            return target;
        }
        public void setTarget(Saxon.Hej.type.SchemaComponent target) {
            this.target = target;
        }
        public virtual void tryToResolve(Saxon.Eej.ee.schema.SchemaCompiler compiler, bool fatal) {
            if (!(dangling)) {
                if ((target == null)) {
                    tryToResolve(compiler.getPreparedSchema());
                }
                if ((target == null)) {
                    tryToResolve(compiler.getConfiguration().getSuperSchema());
                }
                if ((target == null)) {
                    dangling = true;
                }
            }
        }
        private void tryToResolve(Saxon.Eej.ee.schema.PreparedSchema schema) {
            switch (symbolSpace) {
              case Saxon.Eej.ee.schema.SymbolSpace.ELEMENT:
                target = schema.getElementDecl(getTargetComponentName());
                return ;
              case Saxon.Eej.ee.schema.SymbolSpace.ATTRIBUTE:
                target = schema.getAttributeDecl(getTargetComponentName());
                return ;
              default:
                throw new Saxon.Ejava.lang.IllegalStateException();
            }
        }
        public bool isDangling() {
            return dangling;
        }
    
    }
}