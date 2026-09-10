using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Parts : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {

        static Parts() {
            register(Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, "year", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.DateTimeValue)(a)).getYear()));
            register(Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, "month", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.DateTimeValue)(a)).getMonth()));
            register(Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, "day", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.DateTimeValue)(a)).getDay()));
            register(Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, "hours", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.DateTimeValue)(a)).getHour()));
            register(Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, "minutes", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.DateTimeValue)(a)).getMinute()));
            register(Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, "seconds", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => {
                Singulink.Numerics.BigDecimal d = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(((Saxon.Hej.value.DateTimeValue)(a)).getNanosecond());
                d = Singulink.Numerics.BigDecimal.Divide(d, Saxon.Hej.value.BigDecimalValue.BIG_DECIMAL_ONE_BILLION, 6, Singulink.Numerics.RoundingMode.MidpointAwayFromZero);
                d = (d+Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(((Saxon.Hej.value.DateTimeValue)(a)).getSecond()));
                return oneDec(d);
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.DATE_TIME, "timezone", Saxon.Hej.value.SequenceType.OPTIONAL_DAY_TIME_DURATION, a => timezone((Saxon.Hej.value.DateTimeValue)(a)));
            register(Saxon.Hej.type.BuiltInAtomicType.DATE, "year", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.DateValue)(a)).getYear()));
            register(Saxon.Hej.type.BuiltInAtomicType.DATE, "month", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.DateValue)(a)).getMonth()));
            register(Saxon.Hej.type.BuiltInAtomicType.DATE, "day", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.DateValue)(a)).getDay()));
            register(Saxon.Hej.type.BuiltInAtomicType.DATE, "timezone", Saxon.Hej.value.SequenceType.OPTIONAL_DAY_TIME_DURATION, a => timezone((Saxon.Hej.value.DateValue)(a)));
            register(Saxon.Hej.type.BuiltInAtomicType.TIME, "hours", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.TimeValue)(a)).getHour()));
            register(Saxon.Hej.type.BuiltInAtomicType.TIME, "minutes", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.TimeValue)(a)).getMinute()));
            register(Saxon.Hej.type.BuiltInAtomicType.TIME, "seconds", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => {
                Singulink.Numerics.BigDecimal d = Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(((Saxon.Hej.value.TimeValue)(a)).getNanosecond());
                d = Singulink.Numerics.BigDecimal.Divide(d, Saxon.Hej.value.BigDecimalValue.BIG_DECIMAL_ONE_BILLION, 6, Singulink.Numerics.RoundingMode.MidpointAwayFromZero);
                d = (d+Saxon.Impl.Helpers.BigDecimalUtils.ValueOf(((Saxon.Hej.value.TimeValue)(a)).getSecond()));
                return oneDec(d);
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.TIME, "timezone", Saxon.Hej.value.SequenceType.OPTIONAL_DAY_TIME_DURATION, a => timezone((Saxon.Hej.value.TimeValue)(a)));
            register(Saxon.Hej.type.BuiltInAtomicType.G_YEAR, "year", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.GDateValue)(a)).getYear()));
            register(Saxon.Hej.type.BuiltInAtomicType.G_YEAR, "timezone", Saxon.Hej.value.SequenceType.OPTIONAL_DAY_TIME_DURATION, a => timezone((Saxon.Hej.value.GDateValue)(a)));
            register(Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH, "year", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.GDateValue)(a)).getYear()));
            register(Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH, "month", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.GDateValue)(a)).getMonth()));
            register(Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH, "timezone", Saxon.Hej.value.SequenceType.OPTIONAL_DAY_TIME_DURATION, a => timezone((Saxon.Hej.value.GDateValue)(a)));
            register(Saxon.Hej.type.BuiltInAtomicType.G_MONTH, "month", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.GDateValue)(a)).getMonth()));
            register(Saxon.Hej.type.BuiltInAtomicType.G_MONTH, "timezone", Saxon.Hej.value.SequenceType.OPTIONAL_DAY_TIME_DURATION, a => timezone((Saxon.Hej.value.GDateValue)(a)));
            register(Saxon.Hej.type.BuiltInAtomicType.G_MONTH_DAY, "month", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.GDateValue)(a)).getMonth()));
            register(Saxon.Hej.type.BuiltInAtomicType.G_MONTH_DAY, "day", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.GDateValue)(a)).getDay()));
            register(Saxon.Hej.type.BuiltInAtomicType.G_MONTH_DAY, "timezone", Saxon.Hej.value.SequenceType.OPTIONAL_DAY_TIME_DURATION, a => timezone((Saxon.Hej.value.GDateValue)(a)));
            register(Saxon.Hej.type.BuiltInAtomicType.G_DAY, "day", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((Saxon.Hej.value.GDateValue)(a)).getDay()));
            register(Saxon.Hej.type.BuiltInAtomicType.G_DAY, "timezone", Saxon.Hej.value.SequenceType.OPTIONAL_DAY_TIME_DURATION, a => timezone((Saxon.Hej.value.GDateValue)(a)));
            register(Saxon.Hej.type.BuiltInAtomicType.DURATION, "years", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((long)(((Saxon.Hej.value.DurationValue)(a)).getYears())*((Saxon.Hej.value.DurationValue)(a)).signum())));
            register(Saxon.Hej.type.BuiltInAtomicType.DURATION, "months", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((long)(((Saxon.Hej.value.DurationValue)(a)).getMonths())*((Saxon.Hej.value.DurationValue)(a)).signum())));
            register(Saxon.Hej.type.BuiltInAtomicType.DURATION, "days", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((long)(((Saxon.Hej.value.DurationValue)(a)).getDays())*((Saxon.Hej.value.DurationValue)(a)).signum())));
            register(Saxon.Hej.type.BuiltInAtomicType.DURATION, "hours", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((long)(((Saxon.Hej.value.DurationValue)(a)).getHours())*((Saxon.Hej.value.DurationValue)(a)).signum())));
            register(Saxon.Hej.type.BuiltInAtomicType.DURATION, "minutes", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => oneInt(((long)(((Saxon.Hej.value.DurationValue)(a)).getMinutes())*((Saxon.Hej.value.DurationValue)(a)).signum())));
            register(Saxon.Hej.type.BuiltInAtomicType.DURATION, "seconds", Saxon.Hej.value.SequenceType.SINGLE_INTEGER, a => Saxon.Hej.tree.iter.SingletonIterator.makeIterator(((Saxon.Hej.value.DurationValue)(a)).getComponent(Saxon.Hej.functions.AccessorFn.Component.SECONDS)));
            register(Saxon.Hej.type.BuiltInAtomicType.HEX_BINARY, "octets", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.type.BuiltInAtomicType.UNSIGNED_BYTE, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE), a => {
                System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> result = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                foreach (byte b in ((Saxon.Hej.value.HexBinaryValue)(a)).getBinaryValue()) {
                    result.Add(new Saxon.Hej.value.Int64Value((b&0xFF), Saxon.Hej.type.BuiltInAtomicType.UNSIGNED_BYTE));
                }
                return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.value.AtomicValue>(result);
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY, "octets", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.type.BuiltInAtomicType.UNSIGNED_BYTE, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE), a => {
                System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> result = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
                foreach (byte b in ((Saxon.Hej.value.Base64BinaryValue)(a)).getBinaryValue()) {
                    result.Add(new Saxon.Hej.value.Int64Value((b&0xFF), Saxon.Hej.type.BuiltInAtomicType.UNSIGNED_BYTE));
                }
                return new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.value.AtomicValue>(result);
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.QNAME, "prefix", Saxon.Hej.value.SequenceType.OPTIONAL_STRING, a => {
                string prefix = ((Saxon.Hej.value.QNameValue)(a)).getStructuredQName().getPrefix();
                return (prefix.Length==0 ? Saxon.Hej.tree.iter.EmptyIterator.ofAtomic() : Saxon.Hej.tree.iter.SingletonIterator.makeIterator(new Saxon.Hej.value.StringValue(prefix, Saxon.Hej.type.BuiltInAtomicType.NCNAME)));
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.QNAME, "namespace-uri", Saxon.Hej.value.SequenceType.OPTIONAL_STRING, a => {
                string uri = ((Saxon.Hej.value.QNameValue)(a)).getStructuredQName().getURI();
                return (uri.Length==0 ? Saxon.Hej.tree.iter.EmptyIterator.ofAtomic() : Saxon.Hej.tree.iter.SingletonIterator.makeIterator(new Saxon.Hej.value.StringValue(uri, Saxon.Hej.type.BuiltInAtomicType.ANY_URI)));
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.QNAME, "local-name", Saxon.Hej.value.SequenceType.SINGLE_STRING, a => {
                string local = ((Saxon.Hej.value.QNameValue)(a)).getStructuredQName().getLocalPart();
                return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(new Saxon.Hej.value.StringValue(local, Saxon.Hej.type.BuiltInAtomicType.NCNAME));
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.ANY_URI, "scheme", Saxon.Hej.value.SequenceType.OPTIONAL_STRING, a => {
                Saxon.Hej.ma.map.MapItem map = uriComponents(a.getStringValue());
                return Saxon.Hej.tree.iter.SingletonIterator.makeIterator((Saxon.Hej.value.StringValue)(map.get(new Saxon.Hej.value.StringValue("scheme"))));
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.ANY_URI, "authority", Saxon.Hej.value.SequenceType.OPTIONAL_STRING, a => {
                Saxon.Hej.ma.map.MapItem map = uriComponents(a.getStringValue());
                return Saxon.Hej.tree.iter.SingletonIterator.makeIterator((Saxon.Hej.value.StringValue)(map.get(new Saxon.Hej.value.StringValue("authority"))));
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.ANY_URI, "path", Saxon.Hej.value.SequenceType.OPTIONAL_STRING, a => {
                Saxon.Hej.ma.map.MapItem map = uriComponents(a.getStringValue());
                return Saxon.Hej.tree.iter.SingletonIterator.makeIterator((Saxon.Hej.value.StringValue)(map.get(new Saxon.Hej.value.StringValue("path"))));
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.ANY_URI, "query", Saxon.Hej.value.SequenceType.OPTIONAL_STRING, a => {
                Saxon.Hej.ma.map.MapItem map = uriComponents(a.getStringValue());
                return Saxon.Hej.tree.iter.SingletonIterator.makeIterator((Saxon.Hej.value.StringValue)(map.get(new Saxon.Hej.value.StringValue("query"))));
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.ANY_URI, "fragment", Saxon.Hej.value.SequenceType.OPTIONAL_STRING, a => {
                Saxon.Hej.ma.map.MapItem map = uriComponents(a.getStringValue());
                return Saxon.Hej.tree.iter.SingletonIterator.makeIterator((Saxon.Hej.value.StringValue)(map.get(new Saxon.Hej.value.StringValue("fragment"))));
            } );
            register(Saxon.Hej.type.BuiltInAtomicType.ANY_URI, "query-parameters", Saxon.Hej.ma.map.MapType.SINGLE_MAP_ITEM, a => {
                Saxon.Hej.ma.map.MapItem map = uriComponents(a.getStringValue());
                return Saxon.Hej.tree.iter.SingletonIterator.makeIterator((Saxon.Hej.ma.map.MapItem)(map.get(new Saxon.Hej.value.StringValue("queryParams"))));
            } );
            buildRecordTypes();
        }
        public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
            Saxon.Hej.type.AtomicType argType = ((/*1430*/Saxon.Hej.type.AtomicType)args[0].getStaticType().getPrimaryType().getAtomizedItemType().getPrimitiveItemType());
            if ((argType == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) {
                return base.getResultItemType(args);
            }
            while ((argType.getBaseType() != Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) {
                argType = (Saxon.Hej.type.AtomicType)(argType.getBaseType());
            }
            Saxon.Hej.ma.map.RecordTest resultType = Saxon.Impl.Helpers.MapUtils.Get(resultTypeMap, argType);
            if ((resultType == null)) {
                return base.getResultItemType(args);
            } else {
                return resultType;
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.ma.map.DictionaryMap result = new Saxon.Hej.ma.map.DictionaryMap();
            Saxon.Hej.value.AtomicValue subject = (Saxon.Hej.value.AtomicValue)(args[0].head());
            Saxon.Hej.type.AtomicType type = subject.getPrimitiveType();
            if ((type.getBaseType() == Saxon.Hej.type.BuiltInAtomicType.DURATION)) {
                type = Saxon.Hej.type.BuiltInAtomicType.DURATION;
            }
            System.Collections.Generic.IDictionary<string,Saxon.Hej.functions.Parts.ComponentDetails> details = Saxon.Impl.Helpers.MapUtils.Get(componentMap, type);
            if ((details == null)) {
                details = new System.Collections.Generic.Dictionary<string, Saxon.Hej.functions.Parts.ComponentDetails>();
            }
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.functions.Parts.ComponentDetails> component in details) {
                result.initialPut(component.Key, Saxon.Hej.om.SequenceTool.toGroundedValue(component.Value.supplier(subject)));
            }
            result.initialPut("value", subject);
            return result;
        }
        private class ComponentDetails {
            internal Saxon.Hej.value.SequenceType componentType;
            internal System.Func<Saxon.Hej.value.AtomicValue,Saxon.Hej.om.SequenceIterator> supplier;
        
        }
        private readonly static System.Collections.Generic.IDictionary<Saxon.Hej.type.AtomicType,System.Collections.Generic.IDictionary<string,Saxon.Hej.functions.Parts.ComponentDetails>> componentMap = new System.Collections.Generic.Dictionary<Saxon.Hej.type.AtomicType, System.Collections.Generic.IDictionary<string, Saxon.Hej.functions.Parts.ComponentDetails>>();
        private static void register(Saxon.Hej.type.BuiltInAtomicType type, string fieldName, Saxon.Hej.value.SequenceType componentType, System.Func<Saxon.Hej.value.AtomicValue,Saxon.Hej.om.SequenceIterator> supplier) {
            System.Collections.Generic.IDictionary<string,Saxon.Hej.functions.Parts.ComponentDetails> typeData = Saxon.Impl.Helpers.MapUtils.ComputeIfAbsent(componentMap, type, k => new System.Collections.Generic.Dictionary<string,Saxon.Hej.functions.Parts.ComponentDetails>());
            Saxon.Hej.functions.Parts.ComponentDetails details = new Saxon.Hej.functions.Parts.ComponentDetails();
            details.componentType = componentType;
            details.supplier = supplier;
            Saxon.Impl.Helpers.MapUtils.Put(typeData, fieldName, details);
        }
        static Saxon.Hej.om.SequenceIterator oneInt(long x) {
            return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(Saxon.Hej.value.Int64Value.makeIntegerValue(x));
        }
        static Saxon.Hej.om.SequenceIterator oneDec(Singulink.Numerics.BigDecimal x) {
            return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(new Saxon.Hej.value.BigDecimalValue(x));
        }
        static Saxon.Hej.om.SequenceIterator timezone(Saxon.Hej.value.CalendarValue a) {
            if (a.hasTimezone()) {
                try {
                    return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(Saxon.Hej.value.DayTimeDurationValue.fromMilliseconds((60000L*a.getTimezoneInMinutes())));
                } catch (Saxon.Hej.type.ValidationException e) {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofAtomic();
                }
            } else {
                return Saxon.Hej.tree.iter.EmptyIterator.ofAtomic();
            }
        }
        private static System.Collections.Generic.IDictionary<Saxon.Hej.type.AtomicType,Saxon.Hej.ma.map.RecordTest> resultTypeMap;
        private static void buildRecordTypes() {
            resultTypeMap = new System.Collections.Generic.Dictionary<Saxon.Hej.type.AtomicType, Saxon.Hej.ma.map.RecordTest>();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.type.AtomicType,System.Collections.Generic.IDictionary<string,Saxon.Hej.functions.Parts.ComponentDetails>> entry in componentMap) {
                System.Collections.Generic.IList<string> names = new System.Collections.Generic.List<string>();
                System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> types = new System.Collections.Generic.List<Saxon.Hej.value.SequenceType>();
                System.Collections.Generic.IList<string> optionalFields = new System.Collections.Generic.List<string>();
                foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.functions.Parts.ComponentDetails> details in entry.Value) {
                    names.Add(details.Key);
                    types.Add(details.Value.componentType);
                    if (Saxon.Hej.value.Cardinality.allowsZero(details.Value.componentType.getCardinality())) {
                        optionalFields.Add(details.Key);
                    }
                }
                names.Add("value");
                types.Add(Saxon.Hej.value.SequenceType.makeSequenceType(entry.Key, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
                Saxon.Impl.Helpers.MapUtils.Put(resultTypeMap, entry.Key, new Saxon.Hej.ma.map.RecordTest(names, types, optionalFields, false));
            }
        }
        public static Saxon.Hej.value.SequenceType getComponentType(Saxon.Hej.type.BuiltInAtomicType type, string componentName) {
            if (((type == Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION) || (type == Saxon.Hej.type.BuiltInAtomicType.YEAR_MONTH_DURATION))) {
                type = Saxon.Hej.type.BuiltInAtomicType.DURATION;
            }
            System.Collections.Generic.IDictionary<string,Saxon.Hej.functions.Parts.ComponentDetails> typeData = Saxon.Impl.Helpers.MapUtils.Get(componentMap, type);
            if ((typeData == null)) {
                return null;
            } else {
                Saxon.Hej.functions.Parts.ComponentDetails details = Saxon.Impl.Helpers.MapUtils.Get(typeData, componentName);
                return ((details == null) ? null : details.componentType);
            }
        }
        public static Saxon.Hej.om.SequenceIterator getComponentValue(Saxon.Hej.value.AtomicValue value, string componentName) {
            Saxon.Hej.type.AtomicType type = value.getPrimitiveType();
            if (((type == Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION) || (type == Saxon.Hej.type.BuiltInAtomicType.YEAR_MONTH_DURATION))) {
                type = Saxon.Hej.type.BuiltInAtomicType.DURATION;
            }
            System.Collections.Generic.IDictionary<string,Saxon.Hej.functions.Parts.ComponentDetails> typeData = Saxon.Impl.Helpers.MapUtils.Get(componentMap, type);
            if ((typeData == null)) {
                return Saxon.Hej.tree.iter.EmptyIterator.ofAtomic();
            }
            System.Func<Saxon.Hej.value.AtomicValue,Saxon.Hej.om.SequenceIterator> supplier = Saxon.Impl.Helpers.MapUtils.Get(typeData, componentName).supplier;
            return supplier(value);
        }
        public static Saxon.Hej.ma.map.DictionaryMap uriComponents(string arg) {
            Saxon.Hej.ma.map.DictionaryMap dict = new Saxon.Hej.ma.map.DictionaryMap();
            try {
                System.Uri uri = new System.Uri(arg, System.UriKind.RelativeOrAbsolute);
                dict.initialPut("isValid", Saxon.Hej.value.BooleanValue.TRUE);
                bool absolute = uri.IsAbsoluteUri;
                dict.initialPut("isAbsolute", Saxon.Hej.value.BooleanValue.get(absolute));
                dict.initialPut("isOpaque", Saxon.Hej.value.BooleanValue.get(Saxon.Impl.Helpers.UriUtils.IsOpaque(uri)));
                System.Uri surrogateURI = (absolute ? uri : getSurrogateURI(uri));
                string val = (absolute ? uri.Authority : null);
                if ((val != null)) {
                    dict.initialPut("authority", new Saxon.Hej.value.StringValue(val));
                }
                val = (absolute ? uri.Authority : null);
                if ((val != null)) {
                    dict.initialPut("rawAuthority", new Saxon.Hej.value.StringValue(val));
                }
                val = (absolute ? uri.UserInfo : null);
                if ((val != null)) {
                    dict.initialPut("userInfo", new Saxon.Hej.value.StringValue(val));
                }
                val = (absolute ? uri.UserInfo : null);
                if ((val != null)) {
                    dict.initialPut("rawUserInfo", new Saxon.Hej.value.StringValue(val));
                }
                val = (absolute ? uri.Scheme : null);
                if ((val != null)) {
                    dict.initialPut("scheme", new Saxon.Hej.value.StringValue(val));
                }
                val = (absolute ? uri.Host : null);
                if ((val != null)) {
                    dict.initialPut("host", new Saxon.Hej.value.StringValue(val));
                }
                val = surrogateURI.GetComponents(System.UriComponents.PathAndQuery, System.UriFormat.Unescaped);
                if ((val != null)) {
                    dict.initialPut("schemeSpecificPart", new Saxon.Hej.value.StringValue(val));
                }
                val = surrogateURI.PathAndQuery;
                if ((val != null)) {
                    dict.initialPut("rawSchemeSpecificPart", new Saxon.Hej.value.StringValue(val));
                }
                if (absolute) {
                    int port = uri.Port;
                    if ((port>=0)) {
                        dict.initialPut("port", new Saxon.Hej.value.Int64Value(port));
                    }
                }
                val = surrogateURI.LocalPath;
                if ((val != null)) {
                    if ((val.Equals("/") && !(arg.EndsWith("/")))) {
                        val = "";
                    }
                    if (((!(absolute) && val.StartsWith("/")) && !(arg.StartsWith("/")))) {
                        val = val.Substring(1);
                    }
                    dict.initialPut("path", new Saxon.Hej.value.StringValue(val));
                }
                val = surrogateURI.LocalPath;
                if ((val != null)) {
                    dict.initialPut("rawPath", new Saxon.Hej.value.StringValue(val));
                }
                val = surrogateURI.Query;
                if ((val != null)) {
                    dict.initialPut("rawQuery", new Saxon.Hej.value.StringValue(val));
                }
                val = Saxon.Impl.Helpers.UriUtils.GetQuery(surrogateURI);
                if ((val != null)) {
                    dict.initialPut("query", new Saxon.Hej.value.StringValue(val));
                    Saxon.Hej.ma.map.DictionaryMap @params = new Saxon.Hej.ma.map.DictionaryMap();
                    Saxon.Ejava.util.StringTokenizer t = new Saxon.Ejava.util.StringTokenizer(val, ";&");
                    while (t.hasMoreTokens()) {
                        string tok = t.nextToken();
                        int eq = tok.IndexOf('=');
                        if ((eq>=0)) {
                            string keyword = tok.Substring(0, eq - 0);
                            string value = tok.Substring((eq+1));
                            @params.initialAppend(keyword, new Saxon.Hej.value.StringValue(value));
                        } else {
                            @params.initialAppend(tok, Saxon.Hej.value.BooleanValue.TRUE);
                        }
                    }
                    dict.initialPut("queryParams", @params);
                }
                val = surrogateURI.Fragment;
                if ((val != null)) {
                    dict.initialPut("rawFragment", new Saxon.Hej.value.StringValue(val));
                }
                val = Saxon.Impl.Helpers.UriUtils.GetFragment(surrogateURI);
                if ((val != null)) {
                    dict.initialPut("fragment", new Saxon.Hej.value.StringValue(val));
                }
                val = uri.OriginalString;
                if ((val != null)) {
                    dict.initialPut("asciiString", new Saxon.Hej.value.StringValue(val));
                }
                return dict;
            } catch (System.UriFormatException e) {
                dict.initialPut("isValid", Saxon.Hej.value.BooleanValue.FALSE);
                dict.initialPut("error", new Saxon.Hej.value.StringValue(e.Message));
                return dict;
            }
        }
        private static System.Uri getSurrogateURI(System.Uri relative) {
            return new System.Uri(new System.Uri("http://dummy.uri/"), relative);
        }
    
    }
}