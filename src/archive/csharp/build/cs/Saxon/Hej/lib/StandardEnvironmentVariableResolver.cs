using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class StandardEnvironmentVariableResolver : Saxon.Hej.lib.EnvironmentVariableResolver {
        public System.Collections.Generic.ISet<string> getAvailableEnvironmentVariables() {
            System.Collections.Generic.IDictionary<string,string> vars = Saxon.Ejava.lang.System.getenv();
            return Saxon.Impl.Helpers.MapUtils.KeySet(vars);
        }
        public string getEnvironmentVariable(string name) {
            return Saxon.Ejava.lang.System.getenv(name);
        }
    
    }
}