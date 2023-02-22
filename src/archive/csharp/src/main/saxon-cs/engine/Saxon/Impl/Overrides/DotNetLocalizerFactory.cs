using Saxon.Hej;
using Saxon.Hej.lib;

namespace Saxon.Impl.Overrides {
    public class DotNetLocalizerFactory : LocalizerFactory {

        private readonly Configuration config;

        public DotNetLocalizerFactory(Configuration config) {
            this.config = config;
        }
        public override Numberer getNumberer(string language, string country) {
            return new LocalNumberer(language??config.getDefaultLanguage(), country??config.getDefaultCountry());
        }
    }
}