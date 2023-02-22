package net.sf.saxon.option.cpp;

import net.sf.saxon.s9api.SaxonApiException;
import org.graalvm.nativeimage.ObjectHandle;
import com.oracle.svm.core.annotate.*;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.word.WordFactory;

public class SaxonCExceptionCCharHandler extends CEntryPoint.ExceptionHandler {

    @Uninterruptible(reason = "exception handler")
    public static CCharPointer handle(Throwable e) {
        SaxonCException ex = new SaxonCException(e);
        return WordFactory.nullPointer();
   	}
}
