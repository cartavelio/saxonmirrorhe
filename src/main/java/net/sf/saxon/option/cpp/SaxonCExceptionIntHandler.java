package net.sf.saxon.option.cpp;

import com.oracle.svm.core.Uninterruptible;
import org.graalvm.nativeimage.c.function.CEntryPoint;

/*
 * This class is to use with SaxonC.
 * Here we handle exceptions from Java methods which returns a int value to the calling C++ code.
 */
public class SaxonCExceptionIntHandler implements CEntryPoint.ExceptionHandler {

    /**
     * handle method captures the throwable object as a static variable in the
     * SaxonCException class This object can be retrieved later. Here we return
     * a integer value which prevent the program from crashing.
     * @param e - throwable object captured from the calling Java method
     * @return  int value -2 indicating an exception has been thrown
     */
    @Uninterruptible(reason = "exception handler")
    public static int handle(Throwable e) {
        SaxonCException.throwable = e;
        return -2;
   	}
}