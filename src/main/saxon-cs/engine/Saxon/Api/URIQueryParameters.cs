using System;

namespace Saxon.Api {
    public class URIQueryParameters {
        private net.sf.saxon.functions.URIQueryParameters uriQueryParameters;

        public const int ON_ERROR_FAIL = 1;
        public const int ON_ERROR_WARNING = 2;
        public const int ON_ERROR_IGNORE = 3;

        public URIQueryParameters(String query, Processor proc) {
            uriQueryParameters = new net.sf.saxon.functions.URIQueryParameters(query, proc.Implementation);
        }

        internal net.sf.saxon.functions.URIQueryParameters Implementation() {
            return uriQueryParameters;
        }

        public int ValidationMode() {
            return uriQueryParameters.getValidationMode().intValue();
        }

        public bool GetRecurse() {
            return uriQueryParameters.getRecurse().booleanValue();
        }

        public int GetOnError() {
            return uriQueryParameters.getOnError().intValue();
        }


        public bool GetXInclude() {
            return uriQueryParameters.getXInclude().booleanValue();
        }


        public bool GetMetaData() {
            return uriQueryParameters.getMetaData().booleanValue();
        }


        public string ContentType() {
            return uriQueryParameters.getContentType();
        }

        public bool GetStable() {
            return uriQueryParameters.getStable().booleanValue();
        }
    }
}