package net.sf.saxon.query;

import net.sf.saxon.transpile.CSharpInjectMembers;
import net.sf.saxon.transpile.CSharpReplaceBody;

import java.io.BufferedInputStream;
import java.io.InputStream;

/**
 * Allows the position of an input stream to be marked and reset.
 * This class provides an abstraction that can be mapped to the different
 * ways of doing this on Java and .NET
 */

@CSharpInjectMembers(code="private long markerPosition;")
public class InputStreamMarker {

    private final InputStream stream;

    public InputStreamMarker(InputStream stream) {
        this.stream = stream;
    }

    public static InputStream ensureMarkSupported(InputStream stream) {
        if (isMarkSupported(stream)) {
            return stream;
        } else {
            return new BufferedInputStream(stream);
        }
    }

    @CSharpReplaceBody(code = "return stream.CanSeek;")
    private static boolean isMarkSupported(InputStream stream) {
        return stream.markSupported();
    }

    @CSharpReplaceBody(code="return stream.CanSeek;")
    public boolean isMarkSupported() {
        return stream.markSupported();
    }

    @CSharpReplaceBody(code = "markerPosition = stream.Position;")
    public void mark(int readLimit) {
        stream.mark(readLimit);
    }

    @CSharpReplaceBody(code = "stream.Position = markerPosition;")
    public void reset() throws java.io.IOException {
        stream.reset();
    }
}

