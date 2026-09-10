using Saxon.Ejava.util.function;

namespace Saxon.Api.Support {
    public class InvalidityDelegate : IInvalidityHandler {

        private readonly Consumer<ValidationFailure> listener;

        public InvalidityDelegate(Consumer<ValidationFailure> listener) {
            this.listener = listener;
        }

        public void startReporting(string systemId) {
            // no action
        }

        public void reportInvalidity(ValidationFailure failure) {
            listener(failure);
        }

        public XdmValue endReporting() {
            // no action
            return null;
        }
    }
}