package net.sf.saxon.option.cpp;

import com.oracle.svm.core.annotate.*;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CLongPointer;
import org.graalvm.word.WordFactory;

public class SaxonCExceptionCLongPointerHandler implements CEntryPoint.ExceptionHandler {

    @Uninterruptible(reason = "exception handler")
    public static CLongPointer handle(Throwable e) {
        SaxonCException.throwable = e;
        return WordFactory.nullPointer();
   	}
}