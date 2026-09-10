using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public sealed class AttributeGroupReference : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.ComponentReference, Saxon.Hej.type.SchemaComponent {
        private Saxon.Hej.om.StructuredQName qName;
        private Saxon.Eej.ee.schema.AttributeGroupDecl target;
        private bool dangling = false;
        public AttributeGroupReference(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Hej.om.StructuredQName target)  {
            setConfiguration(config);
            setTargetComponentName(target);
        }
        public bool isResolved() {
            return (target != null);
        }
        public bool isDangling() {
            return dangling;
        }
        public int getSymbolSpace() {
            return Saxon.Eej.ee.schema.SymbolSpace.ATTRIBUTE_GROUP;
        }
        public void setTargetComponentName(Saxon.Hej.om.StructuredQName target) {
            this.qName = target;
        }
        public Saxon.Hej.om.StructuredQName getTargetComponentName() {
            return qName;
        }
        internal Saxon.Eej.ee.schema.AttributeWildcard getAttributeWildcard(Saxon.Eej.ee.schema.SchemaCompiler compiler, System.Collections.Generic.Stack<Saxon.Eej.ee.schema.AttributeGroupDecl> stack) {
            if ((target == null)) {
                throw new Saxon.Eej.ee.schema.MissingComponentExceptionImpl(this);
            }
            if (stack.Contains(target)) {
                return null;
            }
            return target.getAttributeWildcard(compiler, stack);
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Eej.ee.schema.AttributeGroupDecl decl = (Saxon.Eej.ee.schema.AttributeGroupDecl)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(this, compiler, false));
            if ((decl == null)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            lookForCycles(new System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent>(), compiler);
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                bool b = fixup(compiler);
                if (b) {
                    setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
                    return true;
                } else {
                    setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                }
            }
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID)) {
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD11)) {
                return ;
            }
            try {
                if (references.Contains(this)) {
                    throw new Saxon.Hej.type.SchemaException((("The definition of attribute group "+Saxon.Hej.trans.Err.wrap(getTargetComponentName().getDisplayName()))+" is circular"));
                }
                Saxon.Hej.om.StructuredQName fp = getTargetComponentName();
                if ((fp == null)) {
                    return ;
                }
                Saxon.Eej.ee.schema.AttributeGroupDecl group = compiler.getPreparedSchema().getAttributeGroup(fp);
                if ((group != null)) {
                    references.Push(this);
                    group.lookForCycles(references, compiler);
                    references.Pop();
                }
            } catch (Saxon.Hej.type.SchemaException err) {
                setTargetComponentName(Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME);
                throw;
            }
        }
        public void tryToResolve(Saxon.Eej.ee.schema.SchemaCompiler compiler, bool fatal) {
            if (!(dangling)) {
                Saxon.Eej.ee.schema.PreparedSchema schema = compiler.getPreparedSchema();
                if ((target == null)) {
                    target = schema.getAttributeGroup(getTargetComponentName());
                }
                if ((target == null)) {
                    target = getConfiguration().getSuperSchema().getAttributeGroup(getTargetComponentName());
                }
                if ((target == null)) {
                    if (compiler.isAllowDanglingReferences()) {
                        compiler.warning((("Unknown attribute group "+qName.getEQName())+" (substituting an empty attribute group)"), this);
                        dangling = true;
                        target = new Saxon.Eej.ee.schema.AttributeGroupDecl(compiler.getConfiguration());
                        target.addAttributeGroupReference(this);
                        target.setAttributeGroupName(qName);
                    } else {
                        throw new Saxon.Hej.type.SchemaException(("Unknown type "+qName.getEQName()), this);
                    }
                }
            }
        }
        public Saxon.Hej.type.SchemaComponent getTarget() {
            if ((target == null)) {
                throw new Saxon.Eej.ee.schema.MissingComponentExceptionImpl(this);
            }
            return target;
        }
        public override void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public override Saxon.Hej.om.Function getComponentAsFunction() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
    
    }
}