package net.sf.saxon.option.cpp;

import com.oracle.svm.core.Uninterruptible;
import org.graalvm.nativeimage.c.function.CEntryPoint;

/*
 * This class is to use with SaxonC.
 * Here we handle exceptions from Java methods which returns a long value to the calling C++ code.
 */
public class SaxonCExceptionDoubleHandler implements CEntryPoint.ExceptionHandler {

    /**
     * handle method captures the throwable object as a static variable in the
     * SaxonCException class This object can be retrieved later. Here we return
     * a integer value which prevent the program from crashing.
     * @param e - throwable object captured from the calling Java method
     * @return  long value -2 indicating an exception has been thrown
     */
    @Uninterruptible(reason = "exception handler")
    public static double handle(Throwable e) {
        SaxonCCurrentException.getInstance().setThrowable(e);
        return SaxonCAPI.STATUS_EXCEPTION;
    }
}

