using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class GlobalContextRequirement {
        private bool mayBeOmitted = true;
        private bool absentFocus;
        private bool external;
        private readonly System.Collections.Generic.IList<Saxon.Hej.type.ItemType> requiredItemTypes = new System.Collections.Generic.List<Saxon.Hej.type.ItemType>();
        private Saxon.Hej.expr.Expression defaultValue = null;
        public Saxon.Hej.type.ItemType getRequiredItemType() {
            if (requiredItemTypes.Count==0) {
                return Saxon.Hej.type.AnyItemType.getInstance();
            } else {
                return requiredItemTypes[0];
            }
        }
        public System.Collections.Generic.IList<Saxon.Hej.type.ItemType> getRequiredItemTypes() {
            return requiredItemTypes;
        }
        public void addRequiredItemType(Saxon.Hej.type.ItemType requiredItemType) {
            requiredItemTypes.Add(requiredItemType);
        }
        public Saxon.Hej.expr.Expression getDefaultValue() {
            return defaultValue;
        }
        public void setDefaultValue(Saxon.Hej.expr.Expression defaultValue) {
            this.defaultValue = defaultValue;
        }
        public void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("glob");
            string use;
            if (isMayBeOmitted()) {
                if (isAbsentFocus()) {
                    use = "pro";
                } else {
                    use = "opt";
                }
            } else {
                use = "req";
            }
            @out.emitAttribute("use", use);
            if (!(getRequiredItemType().Equals(Saxon.Hej.type.AnyItemType.getInstance()))) {
                @out.emitAttribute("type", getRequiredItemType().toExportString());
            }
            @out.endElement();
        }
        public void setAbsentFocus(bool absent) {
            this.absentFocus = absent;
        }
        public bool isAbsentFocus() {
            return absentFocus;
        }
        public void setMayBeOmitted(bool mayOmit) {
            this.mayBeOmitted = mayOmit;
        }
        public bool isMayBeOmitted() {
            return mayBeOmitted;
        }
        public void setExternal(bool external) {
            this.external = external;
        }
        public bool isExternal() {
            return external;
        }
    
    }
}