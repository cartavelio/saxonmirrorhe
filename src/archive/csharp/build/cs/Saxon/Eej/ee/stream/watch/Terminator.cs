using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class Terminator {
        private static readonly Saxon.Eej.ee.stream.watch.Terminator THE_INSTANCE = new Saxon.Eej.ee.stream.watch.Terminator();
        public static Saxon.Eej.ee.stream.watch.Terminator getInstance() {
            return THE_INSTANCE;
        }
        public virtual  /* REPLACING public */ void terminate() {
        }
    
    }
}