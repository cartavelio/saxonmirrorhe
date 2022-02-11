using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class SymbolSpace {
        public const int ELEMENT = 801;
        public const int ATTRIBUTE = 802;
        public const int TYPE = 803;
        public const int GROUP = 804;
        public const int ATTRIBUTE_GROUP = 805;
        public const int NOTATION = 806;
        public const int IDENTITY_CONSTRAINT = 807;
        private SymbolSpace()  {
        }
        public static string getSymbolSpaceName(int space) {
            switch (space) {
              case ELEMENT:
                return "element";
              case ATTRIBUTE:
                return "attribute";
              case TYPE:
                return "type";
              case GROUP:
                return "group";
              case ATTRIBUTE_GROUP:
                return "attributeGroup";
              case NOTATION:
                return "notation";
              case IDENTITY_CONSTRAINT:
                return "identityConstraint";
            }
            throw new Saxon.Ejava.lang.IllegalArgumentException();
        }
    
    }
}