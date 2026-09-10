package net.sf.saxon.option.cpp.messages;

import net.sf.saxon.option.cpp.SaxonCAPI;
import net.sf.saxon.option.cpp.SaxonError;
import net.sf.saxon.s9api.Message;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;

import java.util.List;

public class NullHandler implements MessageHandler {
    @CEntryPoint(name = "j_xsltExecutable_messageHandler_null")
    public static long createHandler(IsolateThread thread, SaxonError saxonError) {
        MessageHandler handler = new NullHandler();

        ObjectHandle handle = SaxonCAPI.createHandle(handler);

        return handle.rawValue();
    }

    @Override
    public void accept(Message message) {
        // We just throw them away
    }
}
