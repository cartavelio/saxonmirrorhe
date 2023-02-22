using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class AccessorFn : Saxon.Hej.functions.ScalarSystemFunction {
        public enum Component {
            YEAR,
            MONTH,
            DAY,
            HOURS,
            MINUTES,
            SECONDS,
            TIMEZONE,
            LOCALNAME,
            NAMESPACE,
            PREFIX,
            MICROSECONDS,
            NANOSECONDS,
            WHOLE_SECONDS,
            YEAR_ALLOWING_ZERO,
        }
        public abstract Saxon.Hej.functions.AccessorFn.Component getComponentId();
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            switch (getComponentId()) {
              case Saxon.Hej.functions.AccessorFn.Component.YEAR:
                return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.makeIntegerValue(-100000), Saxon.Hej.value.Int64Value.makeIntegerValue(+(100000))};
              case Saxon.Hej.functions.AccessorFn.Component.MONTH:
                return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.makeIntegerValue(-11), Saxon.Hej.value.Int64Value.makeIntegerValue(+(11))};
              case Saxon.Hej.functions.AccessorFn.Component.DAY:
                return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.makeIntegerValue(-31), Saxon.Hej.value.Int64Value.makeIntegerValue(+(31))};
              case Saxon.Hej.functions.AccessorFn.Component.HOURS:
                return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.makeIntegerValue(-24), Saxon.Hej.value.Int64Value.makeIntegerValue(+(24))};
              case Saxon.Hej.functions.AccessorFn.Component.MINUTES:
                return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.makeIntegerValue(-59), Saxon.Hej.value.Int64Value.makeIntegerValue(+(59))};
              case Saxon.Hej.functions.AccessorFn.Component.SECONDS:
                return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.makeIntegerValue(-59), Saxon.Hej.value.Int64Value.makeIntegerValue(+(59))};
              default:
                return null;
            }
        }
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            return ((Saxon.Hej.value.AtomicValue)(item)).getComponent(getComponentId());
        }
        public override string getCompilerName() {
            return "AccessorFnCompiler";
        }
        public class YearFromDateTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.YEAR;
            }
        
        }
        public class MonthFromDateTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.MONTH;
            }
        
        }
        public class DayFromDateTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.DAY;
            }
        
        }
        public class HoursFromDateTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.HOURS;
            }
        
        }
        public class MinutesFromDateTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.MINUTES;
            }
        
        }
        public class SecondsFromDateTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.SECONDS;
            }
        
        }
        public class TimezoneFromDateTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.TIMEZONE;
            }
        
        }
        public class YearFromDate : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.YEAR;
            }
        
        }
        public class MonthFromDate : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.MONTH;
            }
        
        }
        public class DayFromDate : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.DAY;
            }
        
        }
        public class TimezoneFromDate : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.TIMEZONE;
            }
        
        }
        public class HoursFromTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.HOURS;
            }
        
        }
        public class MinutesFromTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.MINUTES;
            }
        
        }
        public class SecondsFromTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.SECONDS;
            }
        
        }
        public class TimezoneFromTime : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.TIMEZONE;
            }
        
        }
        public class YearsFromDuration : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.YEAR;
            }
        
        }
        public class MonthsFromDuration : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.MONTH;
            }
        
        }
        public class DaysFromDuration : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.DAY;
            }
        
        }
        public class HoursFromDuration : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.HOURS;
            }
        
        }
        public class MinutesFromDuration : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.MINUTES;
            }
        
        }
        public class SecondsFromDuration : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.SECONDS;
            }
        
        }
        public class LocalNameFromQName : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.LOCALNAME;
            }
        
        }
        public class PrefixFromQName : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.PREFIX;
            }
        
        }
        public class NamespaceUriFromQName : Saxon.Hej.functions.AccessorFn {
            public override Saxon.Hej.functions.AccessorFn.Component getComponentId() {
                return Saxon.Hej.functions.AccessorFn.Component.NAMESPACE;
            }
        
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.AccessorFn.AccessorFnElaborator();
        }
        public class AccessorFnElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.functions.AccessorFn fn = (Saxon.Hej.functions.AccessorFn)(fnc.getTargetFunction());
                Saxon.Hej.functions.AccessorFn.Component component = fn.getComponentId();
                Saxon.Hej.expr.elab.ItemEvaluator argEval = fnc.getArg(0).makeElaborator().elaborateForItem();
                bool nullable = Saxon.Hej.value.Cardinality.allowsZero(fnc.getArg((0)).getCardinality());
                return context => {
                    Saxon.Hej.value.AtomicValue @base = ((Saxon.Hej.value.AtomicValue)(/*Delegate.eval()*/argEval(context)));
                    if ((nullable && (@base == null))) {
                        return null;
                    }
                    return @base.getComponent(component);
                } ;
            }
        
        }
    
    }
}