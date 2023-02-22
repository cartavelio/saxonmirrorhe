namespace java.io {
    public class StringReader : Reader {
        private System.IO.StringReader _sr;

        public StringReader(string str) {
            this._sr = new System.IO.StringReader(str);
        }

        public string ToString() {
            return this._sr.ToString();
        }
    }
}