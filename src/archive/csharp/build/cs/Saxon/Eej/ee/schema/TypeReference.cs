using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class TypeReference : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.ComponentReference {
        private readonly Saxon.Hej.om.StructuredQName qName;
        private readonly int fingerprint;
        private bool dangling;
        private Saxon.Hej.type.SchemaType target;
        public TypeReference(int fingerprint, Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Ejavax.xml.transform.SourceLocator locator)  {
            this.fingerprint = fingerprint;
            this.qName = config.getNamePool().getStructuredQName(fingerprint);
            if ((locator != null)) {
                setLocator(locator);
            }
            if ((fingerprint<1024)) {
                setTarget(Saxon.Hej.type.BuiltInType.getSchemaType(fingerprint));
            }
            setConfiguration(config);
        }
        public bool isResolved() {
            return (target != null);
        }
        public void setTarget(Saxon.Hej.type.SchemaType target) {
            this.target = target;
        }
        public int getTargetFingerprint() {
            return fingerprint;
        }
        public Saxon.Hej.om.StructuredQName getTargetComponentName() {
            return qName;
        }
        public bool isDangling() {
            return dangling;
        }
        public int getSymbolSpace() {
            return Saxon.Eej.ee.schema.SymbolSpace.TYPE;
        }
        public bool isAnonymousType() {
            return ((getTargetFingerprint()>1023) && getTargetComponentName().hasURI(Saxon.Hej.lib.NamespaceConstant.ANONYMOUS));
        }
        public Saxon.Hej.type.SchemaComponent getTarget() {
            if ((target is Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)) {
                Saxon.Eej.ee.schema.UserSimpleType ust = ((Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition)(target)).getWorkingType();
                if ((ust != null)) {
                    target = ust;
                }
            }
            return target;
        }
        public void tryToResolve(Saxon.Eej.ee.schema.SchemaCompiler compiler, bool fatal) {
            if (!(dangling)) {
                if ((target == null)) {
                    target = compiler.getPreparedSchema().getType(qName);
                }
                if ((target == null)) {
                    target = getConfiguration().getSuperSchema().getType(qName);
                }
                if ((target == null)) {
                    if ((compiler.isAllowDanglingReferences() && !(fatal))) {
                        dangling = true;
                        compiler.warning((("Unknown type "+qName.getEQName())+" (substituting xs:error)"), this);
                        target = Saxon.Hej.type.ErrorType.getInstance();
                    } else {
                        dangling = true;
                        compiler.error(("Unknown type "+qName.getEQName()), this);
                    }
                }
            }
        }
    
    }
}