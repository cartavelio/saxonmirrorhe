////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;


import com.oracle.svm.core.Uninterruptible;
import org.graalvm.nativeimage.c.function.CEntryPoint;

/*
 * This class is to use with SaxonC.
 * Here we handle exceptions from Java methods which returns a long value to the calling C++ code.
 */
public class SaxonCExceptionLongHandler implements CEntryPoint.ExceptionHandler {

    /**
     * handle method captures the throwable object as a static variable in the
     * SaxonCException class This object can be retrieved later. Here we return
     * a integer value which prevent the program from crashing.
     * @param e - throwable object captured from the calling Java method
     * @return  long value -2 indicating an exception has been thrown
     */
    @Uninterruptible(reason = "exception handler")
    public static long handle(Throwable e) {
        SaxonCException.throwable = e;
        return SaxonCAPI.STATUS_EXCEPTION;
   	}
}
