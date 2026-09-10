using System;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej.trans;

namespace Saxon.Api {
    /// <summary>
    /// The <c>CommandExitException</c> class is used to signal to the CLI runner that
    /// something untoward has happened and the process should exit non-zero with the
    /// status code from <c>getCode()</c>.
    /// </summary>

    [Serializable]
    public class CommandExitException : Exception
    {
        private readonly int code;

        /// <summary>
        /// Get the exit status code for this exception.
        /// </summary>
        /// <returns>The status code</returns>
        public int getCode() {
            return code;
        }

        /// <summary>
        /// Create a <c>CommandExitException</c>.
        /// </summary>
        /// <param name="code">The exit status code</param>
        /// <param name="message">The error message</param>
        public CommandExitException(int code, string message) : base (message) {
            this.code = code;
        }
    }
}

// Copyright (c) 2026 Saxonica Limited