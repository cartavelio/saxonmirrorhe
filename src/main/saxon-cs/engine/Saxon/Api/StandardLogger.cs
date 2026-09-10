using System;
using System.IO;
using javax.xml.transform.stream;
using net.sf.saxon.lib;

namespace Saxon.Api {
    /// <summary>The default Logger used by Saxon on the .NET platform. All messages are written by
    /// default to System.err. The logger can be configured by setting a different output
    /// destination, and by setting a minimum threshold for the severity of messages to be output.</summary>

    public class StandardLogger : Logger {

        private TextWriter outi = Console.Error;
        TextWriter writer;
        int threshold = Logger.INFO;
        java.io.PrintStream stream;

        /// <summary>
        /// Default constructor that wraps a <c>TextWriter</c> to write Saxon messages
        /// </summary>
        public StandardLogger() {
            writer = outi;
        }

        /// <summary>
        /// Constructor method to supply a user defined TextWriter to the logger
        /// </summary>
        /// <param name="w"></param>
        public StandardLogger(TextWriter w) {
            writer = w;

        }

        /// <summary>
        /// Property to get the underlying TextWriter object.
        /// </summary>
        public TextWriter UnderlyingTextWriter {
            set {
                writer = value;
            }
            get {
                return writer;
            }
        }

        /// <summary> Set the minimum threshold for the severity of messages to be output. Defaults to
        /// <see cref="net.sf.saxon.lib.Logger#INFO"/>. Messages whose severity is below this threshold will be ignored. </summary>
        /// <param name="threshold">the minimum severity of messages to be output. </param>

        public int Threshold {

            set {
                threshold = value;
            }

            get {
                return threshold;
            }

        }

        /// <summary>
        /// Java internal streamResult object wrapping the TextWriter
        /// </summary>
        /// <returns></returns>
        public override StreamResult asStreamResult()
        {
            return new StreamResult(writer);
        }

        /// <summary>
        /// Write the message to the TextWriter object
        /// </summary>
        /// <param name="str">The message</param>
        /// <param name="severity">the severity of the error message</param>
        public override void println(string str, int severity)
        {
            if (severity >= threshold) {
                writer.Write(str);
            }
        }
    }
}