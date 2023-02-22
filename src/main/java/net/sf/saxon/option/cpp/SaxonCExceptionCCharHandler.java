package net.sf.saxon.option.cpp;

import com.oracle.svm.core.Uninterruptible;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.word.WordFactory;

/*
 * This class is to use with SaxonC.
 * Here we handle exceptions from Java methods which returns a
 * char pointer.
 */
public class SaxonCExceptionCCharHandler implements CEntryPoint.ExceptionHandler {

    /**
     * handle method captures the throwable object as a static variable in the
     * SaxonCException class This object can be retrieved later. Here we return
     * a null pointer which prevent the program from crashing.
     * @param e - throwable object captured from the calling Java method
     * @return  null pointer
     */
    @Uninterruptible(reason = "exception handler")
    public static CCharPointer handle(Throwable e) {
        SaxonCException.throwable = e;
        return WordFactory.nullPointer();
   	}
}
