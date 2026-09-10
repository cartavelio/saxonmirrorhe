package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.SaxonApiException;
import org.graalvm.nativeimage.c.type.CTypeConversion;
import org.graalvm.word.WordFactory;

public class SaxonCExceptionHandler {


    public static void setErrorDetails(Throwable throwable, SaxonError errorStruct, SaxonCErrorReporter errorReporter) {


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
            }

            int lineNumber = -1;
            long status = SaxonCAPI.STATUS_EXCEPTION;
            String errorCode = null;

            if (throwable instanceof SaxonApiException) {
                exception = (SaxonApiException) throwable;
                lineNumber = exception.getLineNumber();

            } else {
                exception = new SaxonApiException(throwable);
            }

            errorStruct.setLineNumber(lineNumber);
            errorStruct.setStatus((int) status);
            errorStruct.setNumOfErrors(1);

            QName errorCodeQName = exception.getErrorCode();
            if (errorCodeQName != null) {
                errorCode = errorCodeQName.toString();

            }

            setErrorMessage(errorStruct, exception.getMessage(), "Unknown exception");

            if (errorCode != null) {
                try (final CTypeConversion.CCharPointerHolder holder = CTypeConversion.toCString(errorCode)) {
                    errorStruct.setMessage(holder.get());
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

        try(final CTypeConversion.CCharPointerHolder holder=CTypeConversion.toCString(message != null ? message : defaultMessage)) {
            errorStruct.setMessage(holder.get());
        }
    }
}
