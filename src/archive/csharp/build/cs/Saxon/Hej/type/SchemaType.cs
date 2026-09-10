using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public interface SchemaType : Saxon.Hej.type.SchemaComponent {
        string getName();
        Saxon.Hej.om.NamespaceUri getTargetNamespace();
        int getFingerprint();
        string getDisplayName();
        Saxon.Hej.om.StructuredQName getStructuredQName();
        string getEQName();
        bool isComplexType();
        bool isSimpleType();
        bool isAtomicType();
        bool isAnonymousType();
        int getBlock();
        Saxon.Hej.type.SchemaType getBaseType();
        int getDerivationMethod();
        int getFinalProhibitions();
        bool allowsDerivation(int derivation);
        void analyzeContentExpression(Saxon.Hej.expr.Expression expression, int kind);
        Saxon.Hej.om.AtomicSequence atomize(Saxon.Hej.om.NodeInfo node);
        bool isSameType(Saxon.Hej.type.SchemaType other);
        string getDescription();
        void checkTypeDerivationIsOK(Saxon.Hej.type.SchemaType @base, int block);
        string getSystemId();
        bool isIdType();
        bool isIdRefType();
    
    }
}