using Saxon.Hej.lib;

namespace Saxon.Impl.Overrides {
    public class DotNetLocalizerFactory : LocalizerFactory {
        public override Numberer getNumberer(string language, string country) {
            return new LocalNumberer(language, country);
        }
    }
}