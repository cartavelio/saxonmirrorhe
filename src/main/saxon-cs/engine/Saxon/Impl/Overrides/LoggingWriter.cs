namespace Saxon.Impl.Overrides {
    
    /// <summary>
    /// This class provides an interface to the Saxon Logger in the form of a <c>TextWriter</c>
    /// </summary>
    public class LoggingWriter : System.IO.TextWriter {
        
        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        
        private readonly Saxon.Ejava.lang.StringBuilder builder = new Saxon.Ejava.lang.StringBuilder();
        private readonly Saxon.Hej.lib.Logger logger;

        public LoggingWriter(Saxon.Hej.lib.Logger logger) {
            this.logger = logger;
        }

        public override void Write(char[] cbuf, int off, int len) {
            for (int i = 0; (i < len); i++) {
                char ch = cbuf[(off + i)];
                Write(ch);
            }
        }

        public override void Write(char ch) {
            if ((ch == '\n')) {
                logger.println(builder.ToString(), Saxon.Hej.lib.Logger.INFO);
                builder.setLength(0);
            }
            else {
                builder.append(ch);
            }
        }

        public override void Flush() {
            if ((builder.length() > 0)) {
                logger.println(builder.ToString(), Saxon.Hej.lib.Logger.INFO);
                builder.setLength(0);
            }
        }

        public override void Close() {
            Flush();
        }
    }
}