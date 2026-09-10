// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.cpp;

import org.graalvm.nativeimage.c.CContext;
import org.graalvm.nativeimage.c.struct.CField;
import org.graalvm.nativeimage.c.struct.CStruct;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.word.PointerBase;

/**
 * * The SaxonCErrorHandler class is used to handle exceptions thrown in the Java code of Saxon
 */
@CContext(SaxonCHeaders.class)
@CStruct("saxon_error_t")
public interface SaxonError extends PointerBase {
    @CField("status")
    int getStatus();
    @CField("status")
    void setStatus(int v);

    @CField("error_code")
    CCharPointer getErrorCode();
    @CField("error_code")
    void setErrorCode(CCharPointer v);

    @CField("message")
    CCharPointer getMessage();
    @CField("message")
    void setMessage(CCharPointer v);

    @CField("system_id")
    CCharPointer getSystemId();
    @CField("system_id")
    void setSystemId(CCharPointer v);

    @CField("line_number")
    int getLineNumber();
    @CField("line_number")
    void setLineNumber(int v);

    @CField("num_of_errors")
    int getNumOfErrors();
    @CField("num_of_errors")
    void setNumOfErrors(int v);
}


