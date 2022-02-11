using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface AttributeMap : System.Collections.Generic.IEnumerable<Saxon.Hej.om.AttributeInfo> {
        int size();
        Saxon.Hej.om.AttributeInfo get(Saxon.Hej.om.NodeName name);
        Saxon.Hej.om.AttributeInfo get(string uri, string local);
        Saxon.Hej.om.AttributeInfo getByFingerprint(int fingerprint, Saxon.Hej.om.NamePool namePool);
        string getValue(string uri, string local);
        string getValue(string local);
        Saxon.Hej.om.AttributeMap put(Saxon.Hej.om.AttributeInfo att);
        Saxon.Hej.om.AttributeMap remove(Saxon.Hej.om.NodeName name);
        void verify();
        Saxon.Hej.om.AttributeMap apply(System.Func<Saxon.Hej.om.AttributeInfo,Saxon.Hej.om.AttributeInfo> mapper);
        System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo> asList();
        Saxon.Hej.om.AttributeInfo itemAt(int index);
    
    }
}