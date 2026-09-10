package net.sf.saxon.option.cpp.messages;

import net.sf.saxon.s9api.*;
import net.sf.saxon.str.StringView;
import net.sf.saxon.test.NullLocation;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.Type;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.io.TempDir;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import java.util.stream.Collectors;

import static org.junit.jupiter.api.Assertions.*;

public class MessageHandlerTest {
    final Processor processor = new Processor(false);
    final QName defaultErrorCode =  new QName("", "XTMM9000");
    final Location nullLocation = new NullLocation();
    private Message messageFixture;

    public Message buildMessageFixture(XdmNode content, QName errorCode, boolean terminate, Location location) {
        return new Message(content, errorCode, terminate, location);
    }

    public XdmNode messageContent(String content) throws SaxonApiException {
        Orphan nodeInfo = new net.sf.saxon.tree.util.Orphan(processor.getUnderlyingConfiguration());
        nodeInfo.setNodeKind(Type.TEXT);
        nodeInfo.setStringValue(StringView.wrap(content));
        return new XdmNode(nodeInfo);
    }

    @BeforeEach
    public void setUp() throws SaxonApiException {
        messageFixture = buildMessageFixture(
            messageContent("Hello, World!"),
            defaultErrorCode,
            false,
            nullLocation
        );
    }

    @Nested
    public class NullHandlerTest {
        private NullHandler handler;

        @Test
        @DisplayName("NullHandler should just eat the message and ignore it")
        public void testNullHandler() {
            handler = new NullHandler();

            List.of(messageFixture).forEach(handler);
        }

        @Test
        @DisplayName("Returns null when asked for messages")
        void gettingMessages() {
            handler = new NullHandler();

            List.of(messageFixture).forEach(handler);

            assertIterableEquals(List.of(), handler.getMessages());
        }
    }

    @Nested
    public class FileHandlerTest {
        private MessageHandler handler;

        @Test
        @DisplayName("FileHandler should write the message contents to the specified file")
        void testFileHandler(@TempDir Path temp) throws Exception {
            Path f = temp.resolve("messages.txt");

            try (FileHandler fh = new FileHandler(f.toFile())) {
                handler = fh;
                List.of(messageFixture).forEach(handler);
            }

            assertEquals(messageFixture.toString() + "\n", Files.readString(f));
        }

        @Test
        @DisplayName("Returns null when asked for messages")
        void gettingMessages(@TempDir Path temp) throws Exception {
            Path f = temp.resolve("messages.txt");

            try (FileHandler fh = new FileHandler(f.toFile())) {
                handler = fh;
                List.of(messageFixture).forEach(handler);
            }

            assertIterableEquals(List.of(), handler.getMessages());
        }

        @Test
        @DisplayName("Can be closed outside of a try-with-resources block")
        void canBeSeparatelyClosed(@TempDir Path temp) throws Exception {
            Path f = temp.resolve("messages.txt");

            FileHandler fh = new FileHandler(f.toFile());
            handler = fh;
            List.of(messageFixture).forEach(handler);

            handler.close();

            assertEquals(messageFixture.toString() + "\n", Files.readString(f));
        }


        @Test
        @DisplayName("Messages are flushed to the file immediately")
        void messagesAreFlushedToFileImmediately(@TempDir Path temp) throws Exception {
            Path f = temp.resolve("messages.txt");

            FileHandler fh = new FileHandler(f.toFile());
            handler = fh;
            List.of(messageFixture).forEach(handler);

            assertEquals(messageFixture.toString() + "\n", Files.readString(f));
        }

        @Test
        @DisplayName("A closed FileHandler will automatically re-open its file and append to it if asked to consume more messages")
        void canBeReused(@TempDir Path temp) throws Exception {
            Path f = temp.resolve("messages.txt");

            FileHandler fh = new FileHandler(f.toFile());
            handler = fh;
            List.of(messageFixture).forEach(handler);

            handler.close();

            List.of(messageFixture).forEach(handler);
            String expected = List.of(messageFixture, messageFixture).stream().map(m -> m.toString() + "\n")
                    .collect(Collectors.joining());
            assertEquals(expected, Files.readString(f));
        }
    }

    @Nested
    class StoredHandlerTest {
        private StoredHandler handler;

        @Test
        public void messagesAreRetrievable() {
            handler = new StoredHandler();

            List.of(messageFixture).forEach(handler);

            assertIterableEquals(List.of(messageFixture), handler.getMessages(), "handler should contain the sent messages");
        }

        @Test
        void messagesAreClearable() {
            handler = new StoredHandler();
            List.of(messageFixture).forEach(handler);

            handler.clear();

            assertIterableEquals(List.of(), handler.getMessages(), "handler should no longer contain any messages");
        }
    }
}
