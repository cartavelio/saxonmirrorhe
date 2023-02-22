using System.Buffers.Text;
using Saxon.Hej.value;

namespace Saxon.Api {
    public class XdmExternalObject<T> : XdmItem {
        public XdmExternalObject(T value) : base(new ObjectValue<T>(value)) {
            
        }

        public override bool IsAtomic() {
            return false;
        }

        public T GetObject() {
            return ((ObjectValue<T>)Unwrap()).getObject();
        }
    }
}