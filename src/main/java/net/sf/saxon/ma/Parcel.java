package net.sf.saxon.ma;

import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.ma.map.SingleEntryMap;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class Parcel extends SingleEntryMap {

    private static <T> List<T> singletonList(T value) {
        List<T> result = new ArrayList<>(1);
        result.add(value);
        return result;
    }

    public static final StringValue parcelKey = new StringValue(new Twine8("value"));
    public static RecordTest TYPE = new RecordTest(
            singletonList("value"),
            singletonList(SequenceType.ANY_SEQUENCE),
            Collections.emptyList(),
            false);

    public Parcel(GroundedValue content) {
        super(parcelKey, content);
    }

}

