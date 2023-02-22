////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.SaxonApiException;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.ObjectHandles;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

/**
 * * This class is to use with Saxon on C++
 */
public class SaxonCException  {

    Throwable throwable;

    public SaxonCException(Throwable e) {
        throwable = e;
    }

    public static long addException(SaxonApiException exception){
        ObjectHandle handle = SaxonCAPI.objectHandles.create(new SaxonCException(exception));

        return handle.rawValue();
    }

    public String getMessage(){
        return throwable.getMessage();
    }

    public Throwable getThrowable(){
        return throwable;
    }



    @CEntryPoint(name = "j_getErrorMessage")
    public static CCharPointer j_getErrorMessage(IsolateThread thread, ObjectHandle exceptionRef){
        SaxonCException exception = SaxonCAPI.objectHandles.get(exceptionRef);
        
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(exception.getMessage());
        return holder.get();
    }

    @CEntryPoint(name = "j_getLineNumber")
    public static int getLinenumber(IsolateThread thread, ObjectHandle exceptionRef){
        SaxonCException exception = SaxonCAPI.objectHandles.get(exceptionRef);
        Throwable throwable = exception.getThrowable();
        if(throwable instanceof SaxonApiException) {
            return ((SaxonApiException)throwable).getLineNumber();
        }
        return -1;
    }


    @CEntryPoint(name = "j_getErroCode")
    public static CCharPointer getErroCode(IsolateThread thread, ObjectHandle exceptionRef){
        SaxonCException exception = SaxonCAPI.objectHandles.get(exceptionRef);
        Throwable throwable = exception.getThrowable();
        if(throwable instanceof SaxonApiException) {
            String qnameStr =  ((SaxonApiException)throwable).getErrorCode().getClarkName();
            final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(qnameStr);
            return holder.get();
        }
        return WordFactory.nullPointer();
    }


}
