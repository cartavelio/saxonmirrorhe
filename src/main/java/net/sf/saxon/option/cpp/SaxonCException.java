////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2014 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XmlProcessingError;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.ObjectHandles;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;
import com.oracle.svm.core.Uninterruptible;

import java.io.PrintWriter;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.List;

/**
 * * This class is to use with Saxon on C++
 */
public class SaxonCException  {

    public static Throwable throwable = null;
    public static List<? super XmlProcessingError> errorList = new ArrayList<>();
    
    @Uninterruptible(reason = "exception handler")
    public SaxonCException(Throwable e) {
        throwable = e;

    }
    
    @CEntryPoint(name = "j_getErrorMessage")
    public static CCharPointer j_getErrorMessage(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc){
        SaxonApiException exception = null;
        if(throwable == null) {
            if(SaxonCAPI.debug) {
                System.err.println("Java DEBUG: j_getErrorMessage throwable is null");
            }
            return WordFactory.nullPointer();
        }
        if(SaxonCAPI.debug) {
            System.err.println("Java DEBUG: j_getErrorMessage = " + throwable.getClass().getName());
        }
        if(throwable instanceof SaxonApiException) {
           exception = (SaxonApiException)throwable;
        } else if(throwable instanceof NullPointerException) {
            StringWriter sw = new StringWriter();
            PrintWriter pw = new PrintWriter(sw);
            throwable.printStackTrace(pw);
          exception = new SaxonApiException("NullPointer exception found: "+ sw.toString());
        } else {
            exception = new SaxonApiException(throwable);
        }
        if(exception.getMessage() == null) {
            if(SaxonCAPI.debug) {
                System.err.println("Exception thrown but getMessage() is null");
                System.err.println(exception.getCause().toString());
            }
            StringWriter sw = new StringWriter();
            PrintWriter pw = new PrintWriter(sw);
            throwable.printStackTrace(pw);
            exception = new SaxonApiException("NullPointer exception found: "+ sw.toString());

            //return WordFactory.nullPointer();
        }

        CCharPointer a = SaxonCAPI.stringToNative2(alloc, exception.getMessage());
        return a;
    }


    @CEntryPoint(name = "j_getErrorMessageWithErrorCode")
    public static CCharPointer j_getErrorMessageWithErrorCode(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc){
        SaxonApiException exception = null;
        if(throwable == null) {
            if(SaxonCAPI.debug) {
                System.err.println("Java DEBUG: j_getErrorMessage throwable is null");
            }
            return WordFactory.nullPointer();
        }
        if(throwable instanceof SaxonApiException) {
           exception = (SaxonApiException)throwable;
        } else if(throwable instanceof NullPointerException) {
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
        if(SaxonCAPI.debug) {
            String messageStr = exception.getMessage();
            System.err.println("Java Debug: Exception found: " + messageStr);
            if(messageStr.contains("Invalid handle")){
                System.err.println("Found Invalid Handle error");
                StringWriter sw = new StringWriter();
               PrintWriter pw = new PrintWriter(sw);
               throwable.printStackTrace(pw);
               System.err.println(sw.toString());
            }

        }
        CCharPointer a = null;
        int lineNumber = exception.getLineNumber();
        if(exception.getErrorCode() == null) {
            a = SaxonCAPI.stringToNative2(alloc, exception.getMessage()+". Line number: "+lineNumber);
        } else {
            a = SaxonCAPI.stringToNative2(alloc,exception.getErrorCode().getLocalName() + ": " + exception.getMessage() +
                    ". Line number: "+lineNumber);
        }

        return a;
    }


    @CEntryPoint(name = "j_getErrorMessageInJavaMem")
    public static CCharPointer j_getErrorMessageInJavaMem(IsolateThread thread){
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

        final CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(exception.getMessage());
        return holder.get();
    }



    @CEntryPoint(name = "j_getCombinedStaticErrorMessages")
    public static CCharPointer j_getCombinedStaticErrorMessages(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc){

        String message = new String();

        for(Object item : errorList) {
            XmlProcessingError error = (XmlProcessingError)item;
            if(error.getErrorCode() == null) {
                message+=(((XmlProcessingError)item).getMessage() + "\n");
            } else {
                message+=(error.getErrorCode().getLocalName() + ": " +((XmlProcessingError)item).getMessage() + "\n");
            }

        }

        if(SaxonCAPI.debug) {
            System.err.println("Debug - j_getCombinedStaticErrorMessages errorList.length: "+errorList.size());
            System.err.println("Debug - j_getCombinedStaticErrorMessages: message length:"+message.length());
        }

        if(message.isEmpty()) {
            return j_getErrorMessageWithErrorCode(thread, alloc);
        } else {
            CCharPointer a = SaxonCAPI.stringToNative2(alloc, message);
            return a;
        }

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


    @CEntryPoint(name = "j_getErrorCode")
    public static CCharPointer getErrorCode(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc){
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
        CCharPointer a = SaxonCAPI.stringToNative2(alloc, qnameStr);
        return a;
    }



    @CEntryPoint(name = "j_getSystemId")
    public static CCharPointer getSystemId(IsolateThread thread, SaxonCAPI.AllocatorCCharFn alloc){
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
        CCharPointer a = SaxonCAPI.stringToNative2(alloc, systemId);
        return a;
    }

    @CEntryPoint(name = "j_clearException")
    public static void clearException(IsolateThread thread) {
        errorList.clear();
        throwable = null;
    }



    @CEntryPoint(name = "j_checkForException")
    public static byte checkForException(IsolateThread thread) {
        return CTypeConversion.toCBoolean(throwable != null);
    }



    @CEntryPoint(name = "j_staticErrorCount")
    public static int staticErrorCount(IsolateThread thread) {
        return errorList.size();
    }


}
