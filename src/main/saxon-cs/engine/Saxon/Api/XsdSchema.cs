namespace Saxon.Api {
    public abstract class XsdSchema {

        public abstract Saxon.Api.SchemaValidator NewValidator();

        public abstract XsdSchema Combine(XsdSchema other);

        public abstract void ExportComponents(IDestination destination);
    }
}