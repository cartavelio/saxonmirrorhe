// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.SaxonApiException;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

import java.io.IOException;
import java.io.PrintWriter;

public class SaxonCExceptionHandler {


    public static void setErrorDetails(Throwable throwable, SaxonError errorStruct, SaxonCErrorReporter errorReporter) {


        String errorMessage = null;
        if(errorReporter != null && errorReporter.getNumberOfErrors() > 0) {
            if(SaxonCAPI.debug) {
                System.err.println("Java DEBUG: setStaticErrorDetails called = " + errorReporter.getErrors().toString());
            }

            setErrorMessage(errorStruct, errorReporter.getErrors().toString(), "Unknown exception");
            int lineNumber = -1;
            long status = SaxonCAPI.STATUS_STATIC_EXCEPTION;

            errorStruct.setLineNumber(lineNumber);
            errorStruct.setStatus((int)status);
            errorStruct.setNumOfErrors(errorReporter.getNumberOfErrors());

        } else {

            SaxonApiException exception;

            if (SaxonCAPI.debug) {
                System.err.println("Java DEBUG: j_getErrorMessage = " + throwable.getClass().getName());
                System.err.println("Java DEBUG: j_getErrorMessage = " + throwable.getMessage());
                String stackTrace;
                try (java.io.StringWriter sw = new java.io.StringWriter();
                     PrintWriter pw = new PrintWriter(sw)) {

                    throwable.printStackTrace(pw);
                    stackTrace = sw.toString();
                    System.err.println("Java DEBUG: stackTrace = " + stackTrace);
                } catch (IOException e) {

                }

            }

            int lineNumber = -1;
            long status = SaxonCAPI.STATUS_EXCEPTION;
            String errorCode = null;

            if(throwable instanceof NullPointerException) {

                createStandardErrorResult(errorStruct, "NullPointerException");
                return;

            }

            errorMessage = getAllCausedMessages(throwable);

            if (throwable instanceof SaxonApiException) {
                if (SaxonCAPI.debug) {
                    System.err.println("Java DEBUG: SaxonCExceptionHandler SaxonApiException found" );
                }
                exception = (SaxonApiException) throwable;
                lineNumber = exception.getLineNumber();


            } else  {
                if (SaxonCAPI.debug) {
                    System.err.println("Java DEBUG: SaxonCExceptionHandler not SaxonApiException found. Create new one!" );
                }
                exception = new SaxonApiException(throwable);
            }

            errorStruct.setLineNumber(lineNumber);
            errorStruct.setStatus((int) status);
            errorStruct.setNumOfErrors(1);

            QName errorCodeQName = exception.getErrorCode();
            if (errorCodeQName != null) {
                errorCode = errorCodeQName.toString();
                if (SaxonCAPI.debug) {
                    System.err.println("Java DEBUG: SaxonCExceptionHandler errorCode found: " + errorCode );
                }

            }

            setErrorMessage(errorStruct, errorMessage, "Unknown exception");

            if (errorCode != null) {
                try (final CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(errorCode)) {
                    errorStruct.setErrorCode(holder.get());
                }
            }

            errorStruct.setLineNumber(lineNumber);

            if (exception.getSystemId() != null) {
                try (final CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(exception.getSystemId())) {
                    errorStruct.setSystemId(holder.get());
                }
            }
        }

    }



    public static void createStandardErrorResult(SaxonError errorStruct, String message) {
        setErrorMessage(errorStruct, message, "Unknown exception");
        errorStruct.setLineNumber(-1);
        errorStruct.setStatus((int)SaxonCAPI.STATUS_EXCEPTION);
        errorStruct.setErrorCode(WordFactory.nullPointer());
    }


    public static void setErrorMessage(SaxonError errorStruct, String message, String defaultMessage) {
        if (SaxonCAPI.debug) {
            System.err.println("Java DEBUG: SaxonCExceptionHandler.setErrorMessage = " +message);
        }
        try(final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(message != null ? message : defaultMessage)) {
            errorStruct.setMessage(holder.get());
        }
    }

    private static String getAllCausedMessages(Throwable throwable) {
        StringBuilder builder = new StringBuilder();
        String previous = null;

        for (Throwable t = throwable; t != null; t = t.getCause()) {

            String message = t.getMessage();

            if (message == null || message.isEmpty()) {
                message = t.getClass().getSimpleName();
            }
            if (!message.equals(previous)) {

                if (!builder.isEmpty()) {
                    builder.append("\nCaused by: ");
                }
                builder.append(message);
                previous = message;
            }
        }
        return builder.toString();
    }
}
