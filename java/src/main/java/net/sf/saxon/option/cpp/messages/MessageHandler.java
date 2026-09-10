package net.sf.saxon.option.cpp.messages;

import net.sf.saxon.option.cpp.SaxonCAPI;
import net.sf.saxon.option.cpp.SaxonCDebug;
import net.sf.saxon.s9api.Message;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CLongPointer;
import org.graalvm.word.WordFactory;

import java.util.List;
import java.util.function.Consumer;

public interface MessageHandler extends Consumer<Message> {
    static final List<Message> emptyMessages = List.of();

    default List<Message> getMessages() {
        return emptyMessages;
    }

    default void clear() {}

    default void close() throws Exception {}

    /**
     * Get the xsl:messages  as an array of XdmNode objects in the C memory space
     * @param thread - Graalvm runtime data structure for the thread
     * @param alloc  - Allocator function to create C memory space passed from the C++ application
     * @param messageHandlerRef - reference to the MessageHandler object in the ObjectHandles pool
     * @return pointer to the array of XdmNode objects in the C memory space
     */
    @CEntryPoint(name = "j_getXslMessages")
    public static CLongPointer getXslMessages(IsolateThread thread, SaxonCAPI.AllocatorFn alloc, ObjectHandle messageHandlerRef) {
        MessageHandler handler = SaxonCAPI.objectHandles.get(messageHandlerRef);
        List<Message> xslMessages = handler.getMessages();
        if(SaxonCDebug.debug) {
            SaxonCDebug.err("SaxonCMessageListener getXslMessage size = " + xslMessages.size());
        }
        if( xslMessages.size() == 0 ) {
            return WordFactory.nullPointer();
        }
        long[] refs= xslMessages.stream().mapToLong( message -> {
            return SaxonCAPI.createHandle(message.getContent()).rawValue();
        }).toArray();
        CLongPointer addr = SaxonCAPI.longArrayToNativeWithLength(alloc,refs);
        return addr;
    }

    /**
     * Clear the xsl:messages  held in the listener
     * @param thread - Graalvm runtime data structure for the thread
     * @param messageHandlerRef - reference to the SaxonCMessageListener object in the ObjectHandles pool
     */
    @CEntryPoint(name = "j_clearXslMessages")
    public static void  clearXslMessages(IsolateThread thread, ObjectHandle messageHandlerRef) {
        MessageHandler handler = SaxonCAPI.objectHandles.get(messageHandlerRef);
        if(SaxonCDebug.debug) {
            SaxonCDebug.err("Before clear - SaxonCMessageListener getXslMessage size = " + handler.getMessages().size());
        }
        handler.clear();
        if(SaxonCDebug.debug) {
            SaxonCDebug.err("After clear - SaxonCMessageListener getXslMessage size = " + handler.getMessages().size());
        }

    }
}
