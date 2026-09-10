package com.saxonica.ee.extfn;

import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.type.AlphaCode;
import net.sf.saxon.value.StringValue;

public class ExperimentalSequenceTypeMarshaller {

    public static StringValue unmarshal(MapItem type) {
        return new StringValue(AlphaCode.fromXdmMap(type));
    }

//    public static MapItem marshal(StringValue type) {
//        String input = type.getStringValue();
//        return AlphaCode.toXdmMap(input);
//    }

}

// Copyright (c) 2020-2023 Saxonica Limited

