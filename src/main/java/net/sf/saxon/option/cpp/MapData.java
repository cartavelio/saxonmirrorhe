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
    public static long j_CreateMapData(IsolateThread thread) throws SaxonApiException {
        ObjectHandle handle = SaxonCAPI.createHandle(new MapData());
        return handle.rawValue();
    }

    @CEntryPoint(name = "j_create_mapDataWithCapacity", exceptionHandler = SaxonCExceptionCLongPointerHandler.class)
    public static long j_CreateMapDataWithCapacity(IsolateThread thread, int cap) throws SaxonApiException {
        ObjectHandle handle = SaxonCAPI.createHandle(new MapData(cap));
        return handle.rawValue();
    }

    @CEntryPoint(name = "j_addMapPair")
    public void addMapPair(IsolateThread thread, XdmAtomicValue key, XdmValue value) {
        primitiveMap.put(key, value);
    }



    @CEntryPoint(name = "j_createXdmMap")
    public long createXdmMap(IsolateThread thread, ObjectHandle mapDataRef) {
        MapData mapData = SaxonCAPI.objectHandles.get(mapDataRef);
        XdmMap xdmmap = XdmMap.makeMap(primitiveMap);

        ObjectHandle handle = SaxonCAPI.createHandle(xdmmap);

        return handle.rawValue();
    }
}
