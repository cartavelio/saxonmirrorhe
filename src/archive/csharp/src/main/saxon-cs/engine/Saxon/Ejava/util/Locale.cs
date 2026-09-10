namespace Saxon.Ejava.util {
    public class Locale {
        public static Locale getDefault() {
            return new Locale();
        }

        public string getLanguage() {
            return "en-US";
        }

        public string getCountry() {
            return "us";
        }

        public string getDisplayName() {
            return getLanguage();
        }
    }
}