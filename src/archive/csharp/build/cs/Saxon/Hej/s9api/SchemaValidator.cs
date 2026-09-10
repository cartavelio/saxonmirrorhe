using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    abstract internal class SchemaValidator : Saxon.Hej.s9api.AbstractDestination {
        public abstract void setLax(bool lax);
        public abstract bool isLax();
        public abstract void setInvalidityHandler(Saxon.Hej.lib.InvalidityHandler handler);
        public abstract Saxon.Hej.lib.InvalidityHandler getInvalidityHandler();
        public abstract void setCollectStatistics(bool collect);
        public abstract bool isCollectStatistics();
        public abstract void reportValidationStatistics(Saxon.Hej.s9api.Destination destination);
        public abstract void setValidityReporting(Saxon.Hej.s9api.Destination destination);
        public abstract void setUseXsiSchemaLocation(bool recognize);
        public abstract bool isUseXsiSchemaLocation();
        public abstract void setDestination(Saxon.Hej.s9api.Destination destination);
        public abstract Saxon.Hej.s9api.Destination getDestination();
        public abstract void setDocumentElementName(Saxon.Api.QName name);
        public abstract Saxon.Api.QName getDocumentElementName();
        public abstract void setDocumentElementTypeName(Saxon.Api.QName name);
        public abstract Saxon.Api.QName getDocumentElementTypeName();
        protected internal abstract Saxon.Hej.type.SchemaType getDocumentElementType();
        public abstract void setExpandAttributeDefaults(bool expand);
        public abstract bool isExpandAttributeDefaults();
        public abstract void setParameter(Saxon.Api.QName name, Saxon.Api.XdmValue value);
        public abstract Saxon.Api.XdmValue getParameter(Saxon.Api.QName name);
        public abstract void validate(Saxon.Ejavax.xml.transform.Source source);
        public abstract void validateMultiple(System.Collections.Generic.IEnumerable<Saxon.Ejavax.xml.transform.Source> sources);
        public abstract override Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params);
        public abstract override void close();
    
    }
}