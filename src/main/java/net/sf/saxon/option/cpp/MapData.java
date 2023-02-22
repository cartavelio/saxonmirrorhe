////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmAtomicValue;
import net.sf.saxon.s9api.XdmMap;
import net.sf.saxon.s9api.XdmValue;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;

import java.util.HashMap;
import java.util.Map;

public class MapData {
    private Map<XdmAtomicValue, XdmValue> primitiveMap;

    public MapData() {
        primitiveMap = new HashMap<XdmAtomicValue, XdmValue>();
    }


    public MapData(int cap) {
        primitiveMap = new HashMap<XdmAtomicValue, XdmValue>(cap);
    }


    @CEntryPoint(name = "j_create_mapData", exceptionHandler = SaxonCExceptionCLongPointerHandler.class)
    public static long j_createMapData(IsolateThread thread) throws SaxonApiException {
        ObjectHandle handle = SaxonCAPI.createHandle(new MapData());
        return handle.rawValue();
    }

    @CEntryPoint(name = "j_create_mapDataWithCapacity", exceptionHandler = SaxonCExceptionCLongPointerHandler.class)
    public static long j_createMapDataWithCapacity(IsolateThread thread, int cap) throws SaxonApiException {
        ObjectHandle handle = SaxonCAPI.createHandle(new MapData(cap));
        return handle.rawValue();
    }

    @CEntryPoint(name = "j_addMapPair", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_addMapPair(IsolateThread thread, ObjectHandle mapDataRef, ObjectHandle keyRef, ObjectHandle valueRef) {
        MapData mapData = SaxonCAPI.objectHandles.get(mapDataRef);
        XdmAtomicValue key =  SaxonCAPI.objectHandles.get(keyRef);
        XdmValue value =  SaxonCAPI.objectHandles.get(valueRef);
        mapData.primitiveMap.put(key, value);

        return SaxonCAPI.STATUS_OK;
    }



    @CEntryPoint(name = "j_makeXdmMap", exceptionHandler = SaxonCExceptionLongHandler.class)
    public static long j_createXdmMap(IsolateThread thread, ObjectHandle mapDataRef) {
        MapData mapData = SaxonCAPI.objectHandles.get(mapDataRef);
        XdmMap xdmMap = XdmMap.makeMap(mapData.primitiveMap);

        ObjectHandle handle = SaxonCAPI.createHandle(xdmMap);

        return handle.rawValue();
    }
}
