using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public interface SerializableSchemaComponent {
        void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer);
    
    }
}