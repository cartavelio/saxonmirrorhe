package net.sf.saxon.dotnet;

import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.AtomicValue;

/**
 * A couple of static methods providing an interface for C# to call, because for some reasons the direct calls
 * didn't work
 */

public class DotNetMapUtils {

    public static MapItem addEntry(MapItem map, AtomicValue key, GroundedValue value) {
        return map.addEntry(key, value);
    }

    public static MapItem remove(MapItem map, AtomicValue key) {
        return map.remove(key);
    }
}

