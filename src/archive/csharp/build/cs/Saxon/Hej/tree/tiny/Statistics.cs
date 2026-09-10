using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public class Statistics {
        private int treesCreated = 5;
        private double averageNodes = 4000.0;
        private double averageAttributes = 100.0;
        private double averageNamespaces = 20.0;
        private double averageCharacters = 4000.0;
        public Statistics()  {
        }
        public Statistics(int nodes, int atts, int namespaces, int chars)  {
            this.averageNodes = nodes;
            this.averageAttributes = atts;
            this.averageNamespaces = namespaces;
            this.averageCharacters = chars;
        }
        public double getAverageNodes() {
            return averageNodes;
        }
        public double getAverageAttributes() {
            return averageAttributes;
        }
        public double getAverageNamespaces() {
            return averageNamespaces;
        }
        public double getAverageCharacters() {
            return averageCharacters;
        }
        public void updateStatistics(int numberOfNodes, int numberOfAttributes, int numberOfNamespaces, int chars) { lock(this) {
            int n0 = treesCreated;
            if ((n0<1000000)) {
                int n1 = (treesCreated+1);
                treesCreated = n1;
                averageNodes = (((((averageNodes*n0))+numberOfNodes))/n1);
                if ((averageNodes<10.0)) {
                    averageNodes = 10.0;
                }
                averageAttributes = (((((averageAttributes*n0))+numberOfAttributes))/n1);
                if ((averageAttributes<10.0)) {
                    averageAttributes = 10.0;
                }
                averageNamespaces = (((((averageNamespaces*n0))+numberOfNamespaces))/n1);
                if ((averageNamespaces<5.0)) {
                    averageNamespaces = 5.0;
                }
                averageCharacters = (((((averageCharacters*n0))+chars))/n1);
                if ((averageCharacters<100.0)) {
                    averageCharacters = 100.0;
                }
            }
        }}
        public override string ToString() {
            return (((((((((treesCreated+"(")+averageNodes)+",")+averageAttributes)+",")+averageNamespaces)+",")+averageCharacters)+")");
        }
    
    }
}