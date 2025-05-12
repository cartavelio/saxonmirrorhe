namespace Saxon.Callbacks {
    
    /// <summary>
    /// Interface representing a ResourceResolver with properties that can be set and examined
    /// </summary>
    public interface ConfigurableResourceResolver {
        object getFeature (Org.XmlResolver.Features.ResolverFeature feature);
        void setFeature (Org.XmlResolver.Features.ResolverFeature feature, object value);
    }
}