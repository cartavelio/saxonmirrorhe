using System;
using System.Collections.Generic;
using net.sf.saxon.lib;
using net.sf.saxon.om;

namespace Saxon.Api {
    /// <summary>
    /// <para>If an application does <em>not</em> register its own custom
    /// <code>InvalidityHandler</code>, the default <code>InvalidityGatherer</code>
    /// is used which keeps track of all warnings and errors in a list,
    /// and does not throw any <code>Exception</code>s.
    /// Applications are <em>strongly</em> encouraged to register and use
    /// <code>InvalidityHandler</code>s that insure proper behavior for warnings and
    /// errors.</para>
    /// </summary>
    [Serializable]
    internal class InvalidityGatherer : InvalidityHandler
    {


        private IList<ValidationFailure> errorList;



        /// <summary>
        /// Initializes a new instance of the <see cref="Saxon.Api.InvalidityGatherer"/> class.
        /// </summary>

        public InvalidityGatherer(IList<ValidationFailure> errorList)
        {
            this.errorList = errorList;
        }

        public void startReporting(String systemId) {
            //invalidityHandler.startReporting (systemId);
        }

        public Sequence endReporting() {
            //return invalidityHandler.endReporting ();
            return null;
        }

        /// <summary>
        /// List of errors. The caller may supply an empty list before calling <c>Compile</c>;
        /// the processor will then populate the list with error information obtained during
        /// the schema compilation. Each error will be included as an object of type <c>StaticError</c>.
        /// If no error list is supplied by the caller, error information will be written to
        /// the standard error stream.
        /// </summary>
        /// <remarks>
        /// <para>By supplying a custom <c>List</c> with a user-written <c>add()</c> method, it is possible to
        /// intercept error conditions as they occur.</para>
        /// <para>Note that this error list is used only for errors detected while 
        /// using the schema to validate a source document. It is not used to report errors
        /// in the schema itself.</para>
        /// </remarks>

        public IList<ValidationFailure> ErrorList
        {
            set
            {
                errorList = value;
            }
            get
            {
                return errorList;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="failure">net.sf.saxon.type.ValidationFailure.</param>
        public void reportInvalidity(Invalidity failure)
        {
            ValidationFailure se = new ValidationFailure((net.sf.saxon.type.ValidationFailure)failure);
            errorList.Add(se);

            //invalidityHandler.reportInvalidity (se);
        }


    }
}