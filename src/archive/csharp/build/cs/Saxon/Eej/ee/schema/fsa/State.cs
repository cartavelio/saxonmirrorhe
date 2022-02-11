using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.fsa {
    public interface State {
        Saxon.Eej.ee.schema.fsa.Edge getTransition(int token, Saxon.Eej.ee.schema.UserComplexType type);
        bool isFinalState();
        string listAllowedElements();
        Saxon.Eej.ee.schema.fsa.Edge[] getWildcardEdges();
        System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.fsa.Edge> getEdges();
    
    }
}