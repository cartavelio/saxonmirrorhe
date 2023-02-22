using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public class Statistics {
        private int treesCreated = 0;
        private readonly int[] last10Nodes = new int[10];
        private readonly int[] last10Attributes = new int[10];
        private readonly int[] last10Namespaces = new int[10];
        private readonly int[] last10Characters = new int[10];
        public Statistics() : this (4000, 100, 20, 4000) {
        }
        public Statistics(int nodes, int atts, int namespaces, int chars)  {
            Saxon.Ejava.util.Arrays.fill(last10Nodes, nodes);
            Saxon.Ejava.util.Arrays.fill(last10Attributes, atts);
            Saxon.Ejava.util.Arrays.fill(last10Namespaces, namespaces);
            Saxon.Ejava.util.Arrays.fill(last10Characters, chars);
        }
        private int getUpperBound(int[] last10) {
            int bits = 0;
            for (int i = 0; (i<10); i++) {
                bits |= last10[i];
            }
            return bits;
        }
        public int getAverageNodes() {
            return getUpperBound(last10Nodes);
        }
        public int getAverageAttributes() {
            return getUpperBound(last10Attributes);
        }
        public int getAverageNamespaces() {
            return getUpperBound(last10Namespaces);
        }
        public int getAverageCharacters() {
            return getUpperBound(last10Characters);
        }
        public void updateStatistics(int numberOfNodes, int numberOfAttributes, int numberOfNamespaces, Saxon.Hej.str.LargeTextBuffer textBuffer) { lock(this) {
            int n0 = treesCreated;
            if ((n0<1000000)) {
                int n = (treesCreated++%10);
                last10Nodes[n] = numberOfNodes;
                last10Attributes[n] = numberOfAttributes;
                last10Namespaces[n] = numberOfNamespaces;
                last10Characters[n] = System.Math.Max(textBuffer.length(), 65536);
            }
        }}
        public override string ToString() {
            return (((((((((treesCreated+"(")+getAverageNodes())+",")+getAverageAttributes())+",")+getAverageNamespaces())+",")+getAverageCharacters())+")");
        }
    
    }
}