using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class ContextItemStaticInfo {
        private readonly Saxon.Hej.type.ItemType itemType;
        private readonly bool contextMaybeUndefined;
        private Saxon.Hej.expr.Expression contextSettingExpression;
        private bool parentless;
        public ContextItemStaticInfo(Saxon.Hej.type.ItemType itemType, bool maybeUndefined)  {
            this.itemType = itemType;
            this.contextMaybeUndefined = maybeUndefined;
        }
        public void setContextSettingExpression(Saxon.Hej.expr.Expression setter) {
            contextSettingExpression = setter;
        }
        public Saxon.Hej.expr.Expression getContextSettingExpression() {
            return contextSettingExpression;
        }
        public Saxon.Hej.type.ItemType getItemType() {
            return itemType;
        }
        public Saxon.Hej.type.UType getContextItemUType() {
            return itemType.getUType();
        }
        public bool isPossiblyAbsent() {
            return contextMaybeUndefined;
        }
        public virtual void setContextPostureStriding() {
        }
        public virtual void setContextPostureGrounded() {
        }
        public virtual bool isStrictStreamabilityRules() {
            return false;
        }
        public void setParentless(bool parentless) {
            this.parentless = parentless;
        }
        public bool isParentless() {
            return parentless;
        }
        public readonly static Saxon.Hej.expr.parser.ContextItemStaticInfo DEFAULT = new Saxon.Hej.expr.parser.ContextItemStaticInfo(Saxon.Hej.type.AnyItemType.getInstance(), true);
        public readonly static Saxon.Hej.expr.parser.ContextItemStaticInfo ABSENT = new Saxon.Hej.expr.parser.ContextItemStaticInfo(Saxon.Hej.type.ErrorType.getInstance(), true);
    
    }
}