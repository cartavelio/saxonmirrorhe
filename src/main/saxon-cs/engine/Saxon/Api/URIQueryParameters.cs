using System;

namespace Saxon.Api {
    public class URIQueryParameters
    {

        private net.sf.saxon.functions.URIQueryParameters uriQueryParameters;


        public const int ON_ERROR_FAIL = 1;
        public const int ON_ERROR_WARNING = 2;
        public const int ON_ERROR_IGNORE = 3;

        public URIQueryParameters(String query, Processor proc)
        {
            uriQueryParameters = new net.sf.saxon.functions.URIQueryParameters(query, proc.Implementation);

        }

        internal net.sf.saxon.functions.URIQueryParameters Implementation() {
            return uriQueryParameters;
        } 

        /*
        public static FileNameFilter MakeGlobFilter(String value) {
            return new FileNameFilter(net.sf.saxon.functions.URIQueryParameters.makeGlobFilter(value));
        } */

        public int ValidationMode() {
            return uriQueryParameters.getValidationMode().intValue();
        }

        /*
        public FileNameFilter FileNameFilter
        {
            get {
                return new FileNameFilter(uriQueryParameters.getFilenameFilter());
            }
            
        }*/

        public bool GetRecurse() {
            return uriQueryParameters.getRecurse().booleanValue();
        }

        public int GetOnError() {
            return uriQueryParameters.getOnError().intValue();
        }


        public bool GetXInclude()
        {
            return uriQueryParameters.getXInclude().booleanValue();
        }


        public bool GetMeataData()
        {
            return uriQueryParameters.getMetaData().booleanValue();
        }


        public String ContentType()
        {
            return uriQueryParameters.getContentType();
        }

        public bool GetStable()
        {
            return uriQueryParameters.getStable().booleanValue();
        }

    }
}