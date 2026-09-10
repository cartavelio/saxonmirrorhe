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
import com.oracle.svm.core.annotate.*;

/**
 * * This class is to use with Saxon on C++
 */
public class SaxonCException  {

    public static Throwable throwable = null;
    
    @Uninterruptible(reason = "exception handler")
    public SaxonCException(Throwable e) {
        throwable = e;

    }
    



    @CEntryPoint(name = "j_getErrorMessage")
    public static CCharPointer j_getErrorMessage(IsolateThread thread){
        SaxonApiException exception = null;
        if(throwable == null) {
            return WordFactory.nullPointer();
        }
        if(throwable instanceof SaxonApiException) {
           exception = (SaxonApiException)throwable;
        } else {
            exception = new SaxonApiException(throwable);
        }
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(exception.getMessage());
        return holder.get();
    }

    @CEntryPoint(name = "j_getLineNumber")
    public static int getLinenumber(IsolateThread thread){
        SaxonApiException exception = null;
        if(throwable == null) {
            return -1;
        }
        if(throwable instanceof SaxonApiException) {
           exception = (SaxonApiException)throwable;
        } else {
            exception = new SaxonApiException(throwable);
        }
        return exception.getLineNumber();
    }


    @CEntryPoint(name = "j_getErroCode")
    public static CCharPointer getErroCode(IsolateThread thread){
        SaxonApiException exception = null;
        if(throwable == null) {
            return WordFactory.nullPointer();
        }
        if(throwable instanceof SaxonApiException) {
           exception = (SaxonApiException)throwable;
        } else {
            exception = new SaxonApiException(throwable);
        }
        String qnameStr = exception.getErrorCode().getClarkName();
        final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(qnameStr);
        return holder.get();
    }

    @CEntryPoint(name = "j_clearException")
    public static void clearException(IsolateThread thread) {
        throwable = null;
    }


}
