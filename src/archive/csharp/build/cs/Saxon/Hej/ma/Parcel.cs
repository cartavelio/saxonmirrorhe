using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma {
    public class Parcel : Saxon.Hej.ma.map.SingleEntryMap {
        public static readonly Saxon.Hej.value.StringValue parcelKey = new Saxon.Hej.value.StringValue(new Saxon.Hej.str.Twine8("value"));
        public static Saxon.Hej.ma.map.RecordTest TYPE = Saxon.Hej.ma.map.RecordTest.nonExtensible(new Saxon.Hej.ma.map.RecordTest.Field("value", Saxon.Hej.value.SequenceType.ANY_SEQUENCE, false));
        public Parcel(Saxon.Hej.om.GroundedValue content) : base (parcelKey, content) {
        }
    
    }
}