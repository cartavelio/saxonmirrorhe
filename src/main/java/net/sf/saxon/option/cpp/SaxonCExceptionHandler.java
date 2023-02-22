package net.sf.saxon.option.cpp;

import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;

public class SaxonCExceptionHandler implements CEntryPoint.ExceptionHandler {

    public static long handle(Throwable e) {
        ObjectHandle handle = SaxonCAPI.objectHandles.create(new SaxonCException(e));
        if(SaxonCAPI.debug) {
            System.err.println("DEBUG: Exception thrown message: "+e.getMessage());
        }
        return handle.rawValue();
   	}
}
