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
        public int getCode() {
            return code;
        }

        public CommandExitException(int code, string message) : base (message) {
            this.code = code;
        }
    }
}

// Copyright (c) 2026 Saxonica Limited