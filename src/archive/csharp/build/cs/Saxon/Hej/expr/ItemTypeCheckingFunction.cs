using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    public class ItemTypeCheckingFunction : Saxon.Hej.expr.ItemMappingFunction {
        private readonly Saxon.Hej.type.ItemType requiredItemType;
        private readonly Saxon.Hej.expr.parser.RoleDiagnostic role;
        private readonly Saxon.Hej.s9api.Location location;
        private Saxon.Hej.expr.Expression failingExpression;
        private Saxon.Hej.Configuration config = null;
        public ItemTypeCheckingFunction(Saxon.Hej.type.ItemType requiredItemType, Saxon.Hej.expr.parser.RoleDiagnostic role, Saxon.Hej.expr.Expression failingExpression, Saxon.Hej.Configuration config)  {
            this.requiredItemType = requiredItemType;
            this.role = role;
            this.failingExpression = failingExpression;
            this.location = failingExpression.getLocation();
            this.config = config;
        }
        public ItemTypeCheckingFunction(Saxon.Hej.type.ItemType requiredItemType, Saxon.Hej.expr.parser.RoleDiagnostic role, Saxon.Hej.s9api.Location locator, Saxon.Hej.Configuration config)  {
            this.requiredItemType = requiredItemType;
            this.role = role;
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
                    string message = role.composeErrorMessage(requiredItemType, item, th);
                    string errorCode = role.getErrorCode();
                    if ("XPDY0050".Equals(errorCode)) {
                        Saxon.Hej.trans.XPathException te = new Saxon.Hej.trans.XPathException(message, errorCode);
                        te.setFailingExpression(failingExpression);
                        te.setLocator(location);
                        te.setIsTypeError(false);
                        throw te;
                    } else {
                        Saxon.Hej.trans.XPathException te = new Saxon.Hej.trans.XPathException(message, errorCode);
                        te.setFailingExpression(failingExpression);
                        te.setLocator(location);
                        te.setIsTypeError(true);
                        throw te;
                    }
                }
            }
        }
    
    }
}