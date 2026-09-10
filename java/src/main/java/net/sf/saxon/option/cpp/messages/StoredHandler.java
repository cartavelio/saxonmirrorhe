package net.sf.saxon.option.cpp.messages;

import net.sf.saxon.option.cpp.SaxonCAPI;
import net.sf.saxon.option.cpp.SaxonError;
import net.sf.saxon.s9api.Message;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;

import java.util.ArrayList;
import java.util.List;

public class StoredHandler implements MessageHandler {
    private final List<Message> messages = new ArrayList<>();

    @CEntryPoint(name = "j_xsltExecutable_messageHandler_store")
    public static long createHandler(IsolateThread thread, SaxonError saxonError) {
        MessageHandler handler = new StoredHandler();

        ObjectHandle handle = SaxonCAPI.createHandle(handler);

        return handle.rawValue();
    }

    @Override
    public void accept(Message message) {
        messages.add(message);
    }

    @Override
    public List<Message> getMessages() {
        return messages;
    }

    @Override
    public void clear() {
        messages.clear();
    }
}
