////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;


import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.trans.LicenseException;
import org.graalvm.nativeimage.IsolateThread;

import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;
import com.oracle.svm.core.Uninterruptible;

import java.io.*;

/**
 * * This class is to use with Saxon on C++
 */
public class SaxonCException  {

    //public static Throwable throwable = null;
    public static String errorOutputFile = null;
    //public static SaxonCErrorReporter errorReporter = new SaxonCErrorReporter();

    //public static String ecwd = null;

    @Uninterruptible(reason = "exception handler")
    public SaxonCException(Throwable e) {
        //throwable = e;

    }

    @Uninterruptible(reason = "exception handler")
    public static SaxonCException getInstance(Throwable e){
        return new SaxonCException(e);
    }



    public void setErrorOutput(PrintStream out){
        //TODO remove this method
        //errorReporter.setStandardErrorOutput(out);
    }

    
    @CEntryPoint(name = "j_getErrorMessage")
    public static CCharPointer j_getErrorMessage(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc){
        return WordFactory.nullPointer();
    }


    @CEntryPoint(name = "j_getErrorMessageWithErrorCode")
    public static CCharPointer j_getErrorMessageWithErrorCode(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc){
        return WordFactory.nullPointer();
    }


    @CEntryPoint(name = "j_getErrorMessageInJavaMem")
    public static CCharPointer j_getErrorMessageInJavaMem(IsolateThread thread){
        Throwable throwable = SaxonCCurrentException.get();
        SaxonApiException exception = null;
        if(throwable == null) {
            return WordFactory.nullPointer();
        }
        if(throwable instanceof SaxonApiException) {
           exception = (SaxonApiException)throwable;
        } if(throwable instanceof NullPointerException) {
            StringWriter sw = new StringWriter();
            PrintWriter pw = new PrintWriter(sw);
            throwable.printStackTrace(pw);
          exception = new SaxonApiException("NullPointer exception found: "+ sw.toString());
        } else {
            exception = new SaxonApiException(throwable);
        }
        if(exception.getMessage() == null) {
            return WordFactory.nullPointer();
        }
        //Convert Java String to C *char. Data held in Java memory. Caller has to copy string
        try(final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(exception.getMessage())){

            final CCharPointer result=holder.get();
            return result;
        }
    }



    @CEntryPoint(name = "j_getCombinedStaticErrorMessages")
    public static CCharPointer j_getCombinedStaticErrorMessages(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc) {
        return WordFactory.nullPointer();
    }



    @CEntryPoint(name = "j_getLineNumber")
    public static int getLinenumber(IsolateThread thread){
        Throwable throwable = SaxonCCurrentException.get();
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


    @CEntryPoint(name = "j_getErrorCode")
    public static CCharPointer getErrorCode(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc){
        Throwable throwable = SaxonCCurrentException.get();
        SaxonApiException exception = null;
        if(throwable == null) {
            return WordFactory.nullPointer();
        }
        if(throwable instanceof SaxonApiException) {
           exception = (SaxonApiException)throwable;
        } else {
            exception = new SaxonApiException(throwable);
        }
        if(exception.getErrorCode() == null) {
            return WordFactory.nullPointer();
        }
        String qnameStr = exception.getErrorCode().getClarkName();

        //Convert Java String to C *char. Data held in Java memory. Caller has to copy string
        try(final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(qnameStr)){

            final CCharPointer result=holder.get();
            return result;
        }
        //CCharPointer a = SaxonCAPI.stringToNative2(alloc, qnameStr);
        //return a;
    }



    @CEntryPoint(name = "j_getSystemId")
    public static CCharPointer getSystemId(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc){
        Throwable throwable = SaxonCCurrentException.get();
        SaxonApiException exception = null;
        if(throwable == null) {
            return WordFactory.nullPointer();
        }
        if(throwable instanceof SaxonApiException) {
           exception = (SaxonApiException)throwable;
        } else {
            exception = new SaxonApiException(throwable);
        }
        String systemId = exception.getSystemId();
        if(systemId == null || systemId.isEmpty()) {
            return WordFactory.nullPointer();
        }

        //Convert Java String to C *char. Data held in Java memory. Caller has to copy string
        try(final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(systemId)){

            final CCharPointer result=holder.get();
            return result;
        }

        //CCharPointer a = SaxonCAPI.stringToNative2(alloc, systemId);
        //return a;
    }

    @CEntryPoint(name = "j_clearException")
    public static void clearException(IsolateThread thread) {
        SaxonCCurrentException.clear();
        //errorReporter.reset(); // The resetting of the errorReporter is important because it accumulates errors for each query and stylesheet static error.
        //errorOutputFile = null;
    }

    @CEntryPoint(name = "j_checkForException")
    public static byte checkForException(IsolateThread thread) {
        Throwable throwable = SaxonCCurrentException.get();
        return CTypeConversion.toCBoolean(throwable != null);
    }



    @CEntryPoint(name = "j_staticErrorCount")
    public static int staticErrorCount(IsolateThread thread) {
        //TODO fix this up
        //return errorReporter.getNumberOfErrors();
        return 0;
    }


}
