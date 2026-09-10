using System;
using System.Collections.Generic;
using XmlResolver.Features;

namespace Saxon.Impl.Helpers
{
#nullable enable

    public static class ResolverFeatureHandler
    {
        public static Saxon.Hej.trans.XmlProcessingIncident? Configure(Saxon.Hej.Configuration targetConfig,
            string name, string value)
        {
            var config = (Saxon.Callbacks.CatalogResourceResolver)targetConfig.getResourceResolver();
            ResolverFeature feature = ResolverFeature.ByName(name);
            if (feature.GetType() == typeof(BoolResolverFeature))
            {
                if (value is "true" or "false" or "yes" or "no" or "1" or "0")
                {
                    config.setFeature(feature, value is "true" or "yes" or "1");
                }
                else
                {
                    return new Saxon.Hej.trans.XmlProcessingIncident("Invalid XML resolver feature value for " + name +
                                                                     ". Supplied value: '" + value +
                                                                     "'; required: 'boolean'");
                }
            }
            else if (feature.GetType() == typeof(StringResolverFeature))
            {
                config.setFeature(feature, value);
            }
            else if (feature.GetType() == typeof(ListOfStringResolverFeature))
            {
                List<String> values = new List<String>(value.Split(";"));
                config.setFeature(feature, values);
            }
            else
            {
                return new Saxon.Hej.trans.XmlProcessingIncident("XML resolver feature cannot be configured from configuration file: " + name);
            }

            return null;
        }
   }
}