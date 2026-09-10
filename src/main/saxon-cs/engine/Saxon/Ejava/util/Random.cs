using System;
using System.Collections.Generic;
namespace Saxon.Ejava.util {
    public class Random {
        internal System.Random _rand;
        public Random() {
            _rand = new System.Random(); 
        }
        public Random(long seed) {
            _rand = new System.Random((int)seed ^ (int)(seed>>32)); 
        }
        public double nextDouble() {
            return _rand.NextDouble();
        }
        public long nextLong() {
            return ((long)_rand.Next()<<32) | (long)(_rand.Next());
        }

        public int nextInt(int bound) {
            return _rand.Next(bound);
        }
    }
}