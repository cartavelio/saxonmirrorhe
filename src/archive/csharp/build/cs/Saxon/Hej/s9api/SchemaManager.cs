using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    abstract internal class SchemaManager {
        public SchemaManager()  {
        }
        public abstract void setXsdVersion(string version);
        public abstract string getXsdVersion();
        public abstract void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter);
        public abstract Saxon.Hej.lib.ErrorReporter getErrorReporter();
        public abstract void setSchemaURIResolver(Saxon.Hej.lib.SchemaURIResolver resolver);
        public abstract Saxon.Hej.lib.SchemaURIResolver getSchemaURIResolver();
        public abstract void load(Saxon.Ejavax.xml.transform.Source source);
        public void load(Saxon.Ejava.io.File file) {
            load(new Saxon.Ejavax.xml.transform.stream.StreamSource(file));
        }
        public abstract void importComponents(Saxon.Ejavax.xml.transform.Source source);
        public abstract void exportComponents(Saxon.Hej.s9api.Destination destination);
        public abstract Saxon.Hej.s9api.SchemaValidator newSchemaValidator();
    
    }
}