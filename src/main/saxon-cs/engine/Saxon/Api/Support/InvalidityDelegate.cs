using Saxon.Ejava.util.function;

namespace Saxon.Api.Support {
    public class InvalidityDelegate : IInvalidityHandler {

        private readonly Consumer<ValidationFailure> listener;

        public InvalidityDelegate(Consumer<ValidationFailure> listener) {
            this.listener = listener;
        }

        public void StartReporting(string systemId) {
            // no action
        }

        public void ReportInvalidity(ValidationFailure failure) {
            listener(failure);
        }

        public XdmValue EndReporting() {
            // no action
            return null;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited