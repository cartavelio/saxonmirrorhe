package net.sf.saxon.option.cpp.messages;

import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.option.cpp.SaxonCAPI;
import net.sf.saxon.option.cpp.SaxonCExceptionHandler;
import net.sf.saxon.option.cpp.SaxonError;
import net.sf.saxon.s9api.Message;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import org.graalvm.nativeimage.IsolateThread;
import org.graalvm.nativeimage.ObjectHandle;
import org.graalvm.nativeimage.c.function.CEntryPoint;
import org.graalvm.nativeimage.c.type.CCharPointer;
import org.graalvm.word.WordFactory;

import java.io.*;
import java.util.List;

public class FileHandler implements MessageHandler, AutoCloseable {
    private final File output;
    private final FileWriter fileWriter;
    private final BufferedWriter bufferedWriter;
    private final PrintWriter out;

    @CEntryPoint(name = "j_xsltExecutable_messageHandler_file")
    public static long createHandler(IsolateThread thread, ObjectHandle procRef, CCharPointer cCwd, CCharPointer cFilename, SaxonError saxonError) {
        Processor processor = SaxonCAPI.objectHandles.get(procRef);
        String cwd = null;

        try {
            if (cCwd != WordFactory.nullPointer()) {
                cwd = SaxonCAPI.toJavaString(cCwd, WordFactory.nullPointer());
            }
            String filename = SaxonCAPI.toJavaString(cFilename, WordFactory.nullPointer());
            File sourceFile = SaxonCAPI.resolveFile(processor, cwd, filename, ResourceRequest.XML_NATURE);

            MessageHandler handler = new FileHandler(sourceFile);

            ObjectHandle handle = SaxonCAPI.createHandle(handler);

            return handle.rawValue();
        } catch (IOException | SaxonApiException e) {
            SaxonCExceptionHandler.setErrorDetails(e, saxonError, null);
            return SaxonCAPI.STATUS_EXCEPTION;
        }
    }

    public FileHandler(File f) throws IOException {
        output =  f;
        fileWriter = new FileWriter(output, true);
        bufferedWriter = new BufferedWriter(fileWriter);
        out = new PrintWriter(bufferedWriter);
    }

    @Override
    public void close() throws Exception {
        bufferedWriter.close();
    }

    @Override
    public void accept(Message message) {
        out.println(message.toString());
    }
}
