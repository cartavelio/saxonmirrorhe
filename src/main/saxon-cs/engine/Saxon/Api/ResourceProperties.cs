namespace Saxon.Api {
    
    /// <summary>
    /// A bundle of information made available when requesting resource resolution using
    /// a <c>ResourceResolver</c>. Any of these properties may be absent.
    /// </summary>
    public class ResourceProperties {
        public System.Uri BaseUri { get; set; }
        public string RelativeUri { get; set; }
        public System.Text.Encoding Encoding { get; set; }
        public string Nature { get; set; }
        public string Purpose { get; set; }


    }
}