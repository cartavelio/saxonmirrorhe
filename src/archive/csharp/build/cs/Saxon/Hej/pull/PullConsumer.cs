using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pull {
    public class PullConsumer {
        private readonly Saxon.Hej.pull.PullProvider @in;
        public PullConsumer(Saxon.Hej.pull.PullProvider @in)  {
            this.@in = @in;
        }
        public void consume() {
            while (true) {
                if ((@in.next() == Saxon.Hej.pull.PullEvent.END_OF_INPUT)) {
                    return ;
                }
            }
        }
    
    }
}