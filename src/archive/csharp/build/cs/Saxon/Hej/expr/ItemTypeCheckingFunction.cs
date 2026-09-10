using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ItemTypeCheckingFunction : Saxon.Hej.expr.ItemMappingFunction {
        private readonly Saxon.Hej.type.ItemType requiredItemType;
        private readonly Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier;
        private readonly Saxon.Hej.s9api.Location location;
        private Saxon.Hej.Configuration config = null;
        public ItemTypeCheckingFunction(Saxon.Hej.type.ItemType requiredItemType, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> roleSupplier, Saxon.Hej.s9api.Location locator, Saxon.Hej.Configuration config)  {
            this.requiredItemType = requiredItemType;
            this.roleSupplier = roleSupplier;
            this.location = locator;
            this.config = config;
        }
        public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
            testConformance(item, config);
            return item;
        }
        private void testConformance(Saxon.Hej.om.Item item, Saxon.Hej.Configuration config) {
            Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
            if (requiredItemType.matches(item, th)) {
            } else {
                if ((requiredItemType.getUType().subsumes(Saxon.Hej.type.UType.STRING) && Saxon.Hej.type.BuiltInAtomicType.ANY_URI.matches(item, th))) {
                } else {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = roleSupplier();
                    string message = role.composeErrorMessage(requiredItemType, item, th);
                    string errorCode = role.getErrorCode();
                    if ("XPDY0050".Equals(errorCode)) {
                        Saxon.Hej.trans.XPathException te = new Saxon.Hej.trans.XPathException(message, errorCode);
                        te.setLocator(location);
                        te.setIsTypeError(false);
                        throw te;
                    } else {
                        Saxon.Hej.trans.XPathException te = new Saxon.Hej.trans.XPathException(message, errorCode);
                        te.setLocator(location);
                        te.setIsTypeError(true);
                        throw te;
                    }
                }
            }
        }
    
    }
}