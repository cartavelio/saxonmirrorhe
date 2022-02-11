using System;
using Saxon.Ejava.util.function;

namespace Saxon.Api.Support {
    public class InvalidityDelegate : IInvalidityHandler {

        private readonly Action<ValidationFailure> listener;

        public InvalidityDelegate(Action<ValidationFailure> listener) {
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