using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class KeyDefinition : Saxon.Hej.expr.instruct.Actor, Saxon.Hej.expr.ContextOriginator {
        private readonly Saxon.Hej.trans.SymbolicName symbolicName;
        private readonly Saxon.Hej.pattern.Pattern match;
        private Saxon.Hej.type.BuiltInAtomicType useType;
        private readonly Saxon.Hej.lib.StringCollator collation;
        private readonly string collationName;
        private bool backwardsCompatible = false;
        private bool strictComparison = false;
        private bool convertUntypedToOther = false;
        private bool rangeKey = false;
        private bool composite = false;
        public KeyDefinition(Saxon.Hej.trans.SymbolicName symbolicName, Saxon.Hej.pattern.Pattern match, Saxon.Hej.expr.Expression use, string collationName, Saxon.Hej.lib.StringCollator collation)  {
            this.symbolicName = symbolicName;
            this.match = match;
            setBody(use);
            this.collation = collation;
            this.collationName = collationName;
        }
        public override Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return symbolicName;
        }
        public void setRangeKey(bool rangeKey) {
            this.rangeKey = rangeKey;
        }
        public bool isRangeKey() {
            return rangeKey;
        }
        public void setComposite(bool composite) {
            this.composite = composite;
        }
        public bool isComposite() {
            return composite;
        }
        public void setIndexedItemType(Saxon.Hej.type.BuiltInAtomicType itemType) {
            useType = itemType;
        }
        public Saxon.Hej.type.BuiltInAtomicType getIndexedItemType() {
            if ((useType == null)) {
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            } else {
                return useType;
            }
        }
        public void setBackwardsCompatible(bool bc) {
            backwardsCompatible = bc;
        }
        public bool isBackwardsCompatible() {
            return backwardsCompatible;
        }
        public void setStrictComparison(bool strict) {
            strictComparison = strict;
        }
        public bool isStrictComparison() {
            return strictComparison;
        }
        public void setConvertUntypedToOther(bool convertToOther) {
            convertUntypedToOther = convertToOther;
        }
        public bool isConvertUntypedToOther() {
            return convertUntypedToOther;
        }
        public override void setStackFrameMap(Saxon.Hej.expr.instruct.SlotManager map) {
            if ((map != null)) {
                base.setStackFrameMap(map);
            }
        }
        public override void allocateAllBindingSlots(Saxon.Hej.style.StylesheetPackage pack) {
            base.allocateAllBindingSlots(pack);
            allocateBindingSlotsRecursive(pack, this, match, getDeclaringComponent().getComponentBindings());
        }
        public void setLocation(Saxon.Hej.s9api.Location loc) {
            setSystemId(loc.getSystemId());
            setLineNumber(loc.getLineNumber());
            setColumnNumber(loc.getColumnNumber());
        }
        public Saxon.Hej.pattern.Pattern getMatch() {
            return match;
        }
        public Saxon.Hej.expr.Expression getUse() {
            return getBody();
        }
        public string getCollationName() {
            return collationName;
        }
        public Saxon.Hej.lib.StringCollator getCollation() {
            return collation;
        }
        public Saxon.Hej.om.StructuredQName getObjectName() {
            return symbolicName.getComponentName();
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter @out, bool reusable, System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,int> componentIdMap) {
            @out.startElement("key");
            @out.emitAttribute("name", getObjectName());
            if (!(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI.Equals(collationName))) {
                @out.emitAttribute("collation", collationName);
            }
            @out.emitAttribute("line", (getLineNumber()+""));
            @out.emitAttribute("module", getSystemId());
            if (((getStackFrameMap() != null) && (getStackFrameMap().getNumberOfVariables() != 0))) {
                @out.emitAttribute("slots", (getStackFrameMap().getNumberOfVariables()+""));
            }
            if ((componentIdMap != null)) {
                @out.emitAttribute("binds", (""+getDeclaringComponent().listComponentReferences(componentIdMap)));
            }
            string flags = "";
            if (backwardsCompatible) {
                flags += "b";
            }
            if (isRangeKey()) {
                flags += "r";
                @out.emitAttribute("range", "1");
            }
            if (match.getUType().overlaps(Saxon.Hej.type.UType.ATTRIBUTE)) {
                flags += "a";
            }
            if (match.getUType().overlaps(Saxon.Hej.type.UType.NAMESPACE)) {
                flags += "n";
            }
            if (composite) {
                flags += "c";
            }
            if (reusable) {
                flags += "u";
            }
            if (convertUntypedToOther) {
                flags += "v";
            }
            if (strictComparison) {
                flags += "s";
            }
            if (!("".Equals(flags))) {
                @out.emitAttribute("flags", flags);
            }
            getMatch().export(@out);
            getBody().export(@out);
            @out.endElement();
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
    
    }
}