using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface ComplexType : Saxon.Hej.type.SchemaType {
        Saxon.Hej.type.ComplexVariety getVariety();
        bool isAbstract();
        bool isComplexContent();
        bool isSimpleContent();
        bool isAllContent();
        Saxon.Hej.type.SimpleType getSimpleContentType();
        bool isRestricted();
        bool isEmptyContent();
        bool isEmptiable();
        bool isMixedContent();
        Saxon.Hej.type.SchemaType getElementParticleType(int elementName, bool considerExtensions);
        int getElementParticleCardinality(int elementName, bool considerExtensions);
        Saxon.Hej.type.SimpleType getAttributeUseType(Saxon.Hej.om.StructuredQName attributeName);
        int getAttributeUseCardinality(Saxon.Hej.om.StructuredQName attributeName);
        bool allowsAttributes();
        void gatherAllPermittedChildren(Saxon.Hej.z.IntHashSet children, bool ignoreWildcards);
        void gatherAllPermittedDescendants(Saxon.Hej.z.IntHashSet descendants);
        Saxon.Hej.type.SchemaType getDescendantElementType(int fingerprint);
        int getDescendantElementCardinality(int elementFingerprint);
        bool containsElementWildcard();
        bool hasAssertions();
    
    }
}