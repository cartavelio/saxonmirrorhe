using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pull {
    public class PullPushCopier {
        private readonly Saxon.Hej.pull.PullProvider @in;
        private readonly Saxon.Hej.@event.Receiver @out;
        public PullPushCopier(Saxon.Hej.pull.PullProvider @in, Saxon.Hej.@event.Receiver @out)  {
            this.@out = @out;
            this.@in = @in;
        }
        public void copy() {
            @out.open();
            Saxon.Hej.pull.PullPushTee tee = new Saxon.Hej.pull.PullPushTee(@in, @out);
            new Saxon.Hej.pull.PullConsumer(tee).consume();
            @out.close();
        }
        public void append() {
            Saxon.Hej.pull.PullPushTee tee = new Saxon.Hej.pull.PullPushTee(@in, @out);
            new Saxon.Hej.pull.PullConsumer(tee).consume();
        }
    
    }
}