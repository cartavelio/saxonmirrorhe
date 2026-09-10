using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class InputStreamMarker {
    // Injecting C# code
        private long markerPosition;
    // End of injected C# code
        private readonly System.IO.Stream stream;
        public InputStreamMarker(System.IO.Stream stream)  {
            this.stream = stream;
        }
        public static System.IO.Stream ensureMarkSupported(System.IO.Stream stream) {
            if (isMarkSupported(stream)) {
                return stream;
            } else {
                return new System.IO.BufferedStream(stream);
            }
        }
        private static bool isMarkSupported(System.IO.Stream stream) {
            return stream.CanSeek;
        }
        public bool isMarkSupported() {
            return stream.CanSeek;
        }
        public void mark(int readLimit) {
            markerPosition = stream.Position;
        }
        public void reset() {
            stream.Position = markerPosition;
        }
    
    }
}