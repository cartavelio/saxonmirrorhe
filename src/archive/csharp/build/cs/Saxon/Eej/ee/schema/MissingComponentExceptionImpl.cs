using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class MissingComponentExceptionImpl : Saxon.Hej.type.MissingComponentException {
        private readonly Saxon.Eej.ee.schema.ComponentReference reference;
        public MissingComponentExceptionImpl(Saxon.Eej.ee.schema.ComponentReference @ref) : base (("Unresolved reference to schema component "+(((@ref.getTargetComponentName() == null) ? "" : @ref.getTargetComponentName().getDisplayName())))) {
            this.reference = @ref;
        }
        public Saxon.Eej.ee.schema.ComponentReference getComponentReference() {
            return reference;
        }
        public override  /* REPLACING public */ string getMessage() {
            try {
                string space = /*134*/Saxon.Eej.ee.schema.SymbolSpace.getSymbolSpaceName(reference.getSymbolSpace());
                string component = reference.getTargetComponentName().getEQName();
                string module = reference.getSystemId();
                if (module.EndsWith("override.xsl")) {
                    module = "schema_document_resulting_from_override_processing";
                }
                return ((((((("The schema contains an unresolved reference to "+space)+' ')+component)+" at line ")+reference.getLineNumber())+" in ")+reference.getSystemId());
            } catch (System.Exception e) {
                return base.Message;
            }
        }
    
    }
}