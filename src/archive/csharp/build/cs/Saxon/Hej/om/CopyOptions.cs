using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public abstract class CopyOptions {
        public const int ALL_NAMESPACES = 2;
        public const int TYPE_ANNOTATIONS = 4;
        public const int FOR_UPDATE = 8;
        public static bool includes(int options, int option) {
            return (((options&option)) == option);
        }
        public static int getStartDocumentProperties(int copyOptions) {
            return (Saxon.Hej.om.CopyOptions.includes(copyOptions, Saxon.Hej.om.CopyOptions.FOR_UPDATE) ? Saxon.Hej.@event.ReceiverOption.MUTABLE_TREE : Saxon.Hej.@event.ReceiverOption.NONE);
        }
    
    }
}