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


