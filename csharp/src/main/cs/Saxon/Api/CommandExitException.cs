using System;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej.trans;

namespace Saxon.Api {
    /// <summary>
    /// The <c>SaxonApiException</c> class contains information about a static or dynamic error detected during
    /// compilation or evaluation of a stylesheet, query, or XPath expression, or other errors that occur
    /// when using the Saxon API.
    /// </summary>

    [Serializable]
    public class CommandExitException : Exception
    {
        private readonly int code;
        public int getCode() {
            return code;
        }

        public CommandExitException(int code, string message) : base (message) {
            this.code = code;
        }
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited