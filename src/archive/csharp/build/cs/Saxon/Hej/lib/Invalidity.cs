using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface Invalidity : Saxon.Ejavax.xml.transform.SourceLocator {
        int getSchemaPart();
        string getConstraintName();
        string getConstraintClauseNumber();
        string getConstraintReference();
        Saxon.Hej.om.NodeInfo getInvalidNode();
        Saxon.Hej.om.AbsolutePath getPath();
        Saxon.Hej.om.AbsolutePath getContextPath();
        string getMessage();
        string getErrorCode();
    
    }
}