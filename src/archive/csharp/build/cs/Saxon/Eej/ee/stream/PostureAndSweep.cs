using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream {
    public class PostureAndSweep {
        private readonly Saxon.Eej.ee.stream.Posture posture;
        private readonly Saxon.Eej.ee.stream.Sweep sweep;
        public PostureAndSweep(Saxon.Eej.ee.stream.Posture posture, Saxon.Eej.ee.stream.Sweep sweep)  {
            this.posture = posture;
            this.sweep = sweep;
        }
        public Saxon.Eej.ee.stream.Posture getPosture() {
            return posture;
        }
        public Saxon.Eej.ee.stream.Sweep getSweep() {
            return sweep;
        }
        public override int GetHashCode() {
            return ((posture.GetHashCode()<<16)^sweep.GetHashCode());
        }
        public override bool Equals(System.Object obj) {
            return (((obj is Saxon.Eej.ee.stream.PostureAndSweep) && ((Saxon.Eej.ee.stream.PostureAndSweep)(obj)).posture.Equals(posture)) && ((Saxon.Eej.ee.stream.PostureAndSweep)(obj)).sweep.Equals(sweep));
        }
        public override string ToString() {
            return ((posture.ToString()+" and ")+sweep.ToString());
        }
        public static readonly Saxon.Eej.ee.stream.PostureAndSweep ROAMING_AND_FREE_RANGING = new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.ROAMING, Saxon.Eej.ee.stream.Sweep.FREE_RANGING);
        public static readonly Saxon.Eej.ee.stream.PostureAndSweep GROUNDED_AND_MOTIONLESS = new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.GROUNDED, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
        public static readonly Saxon.Eej.ee.stream.PostureAndSweep CLIMBING_AND_MOTIONLESS = new Saxon.Eej.ee.stream.PostureAndSweep(Saxon.Eej.ee.stream.Posture.CLIMBING, Saxon.Eej.ee.stream.Sweep.MOTIONLESS);
    
    }
}