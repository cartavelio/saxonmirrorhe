using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public class  Axis  {
        public static readonly Axis ANCESTOR = new Axis("ANCESTOR", InnerEnum.ANCESTOR, Saxon.Hej.om.AxisInfo.ANCESTOR);
        public static readonly Axis ANCESTOR_OR_SELF = new Axis("ANCESTOR_OR_SELF", InnerEnum.ANCESTOR_OR_SELF, Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF);
        public static readonly Axis ATTRIBUTE = new Axis("ATTRIBUTE", InnerEnum.ATTRIBUTE, Saxon.Hej.om.AxisInfo.ATTRIBUTE);
        public static readonly Axis CHILD = new Axis("CHILD", InnerEnum.CHILD, Saxon.Hej.om.AxisInfo.CHILD);
        public static readonly Axis DESCENDANT = new Axis("DESCENDANT", InnerEnum.DESCENDANT, Saxon.Hej.om.AxisInfo.DESCENDANT);
        public static readonly Axis DESCENDANT_OR_SELF = new Axis("DESCENDANT_OR_SELF", InnerEnum.DESCENDANT_OR_SELF, Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF);
        public static readonly Axis FOLLOWING = new Axis("FOLLOWING", InnerEnum.FOLLOWING, Saxon.Hej.om.AxisInfo.FOLLOWING);
        public static readonly Axis FOLLOWING_SIBLING = new Axis("FOLLOWING_SIBLING", InnerEnum.FOLLOWING_SIBLING, Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING);
        public static readonly Axis PARENT = new Axis("PARENT", InnerEnum.PARENT, Saxon.Hej.om.AxisInfo.PARENT);
        public static readonly Axis PRECEDING = new Axis("PRECEDING", InnerEnum.PRECEDING, Saxon.Hej.om.AxisInfo.PRECEDING);
        public static readonly Axis PRECEDING_SIBLING = new Axis("PRECEDING_SIBLING", InnerEnum.PRECEDING_SIBLING, Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING);
        public static readonly Axis SELF = new Axis("SELF", InnerEnum.SELF, Saxon.Hej.om.AxisInfo.SELF);
        public static readonly Axis NAMESPACE = new Axis("NAMESPACE", InnerEnum.NAMESPACE, Saxon.Hej.om.AxisInfo.NAMESPACE);
        private static readonly List<Axis> valueList = new List<Axis>();
        static Axis() {
            valueList.Add(ANCESTOR);
            valueList.Add(ANCESTOR_OR_SELF);
            valueList.Add(ATTRIBUTE);
            valueList.Add(CHILD);
            valueList.Add(DESCENDANT);
            valueList.Add(DESCENDANT_OR_SELF);
            valueList.Add(FOLLOWING);
            valueList.Add(FOLLOWING_SIBLING);
            valueList.Add(PARENT);
            valueList.Add(PRECEDING);
            valueList.Add(PRECEDING_SIBLING);
            valueList.Add(SELF);
            valueList.Add(NAMESPACE);
        }
        public static IList<Axis> values() {
            return valueList;
        }
        public enum InnerEnum {
            ANCESTOR,
            ANCESTOR_OR_SELF,
            ATTRIBUTE,
            CHILD,
            DESCENDANT,
            DESCENDANT_OR_SELF,
            FOLLOWING,
            FOLLOWING_SIBLING,
            PARENT,
            PRECEDING,
            PRECEDING_SIBLING,
            SELF,
            NAMESPACE,
        }
        public readonly InnerEnum innerEnumValue;
        public readonly string nameValue;
        public readonly int ordinalValue;
        public static int nextOrdinal = 0;
        internal Axis(string name, InnerEnum innerEnum, int axisNumber) { 
            this.axisNumber = axisNumber;
            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
    this.axisNumber = axisNumber;
        } 
        private readonly int axisNumber;
        public int getAxisNumber() {
            return axisNumber;
        }
    }
}