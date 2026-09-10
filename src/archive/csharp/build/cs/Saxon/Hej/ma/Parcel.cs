using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma {
    public class Parcel : Saxon.Hej.ma.map.SingleEntryMap {
        private static System.Collections.Generic.IList<T> singletonList<T> (T value) {
            System.Collections.Generic.IList<T> result = new System.Collections.Generic.List<T>(1);
            result.Add(value);
            return result;
        }
        public static readonly Saxon.Hej.value.StringValue parcelKey = new Saxon.Hej.value.StringValue(new Saxon.Hej.str.Twine8("value"));
        public static Saxon.Hej.ma.map.RecordTest TYPE = new Saxon.Hej.ma.map.RecordTest(singletonList("value"), singletonList(Saxon.Hej.value.SequenceType.ANY_SEQUENCE), new System.Collections.Generic.List<string>(), false);
        public Parcel(Saxon.Hej.om.GroundedValue content) : base (parcelKey, content) {
        }
    
    }
}