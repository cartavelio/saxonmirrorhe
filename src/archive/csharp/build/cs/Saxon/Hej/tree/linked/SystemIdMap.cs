using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.linked {
    public class SystemIdMap {
        private int[] sequenceNumbers;
        private string[] uris;
        private int allocated;
        public SystemIdMap()  {
            sequenceNumbers = new int[4];
            uris = new string[4];
            allocated = 0;
        }
        public void setSystemId(int sequence, string uri) {
            if ((allocated>0)) {
                if (uri.Equals(uris[(allocated-1)])) {
                    return ;
                }
                if ((sequence<=sequenceNumbers[(allocated-1)])) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("System IDs of nodes are immutable");
                }
            }
            if ((sequenceNumbers.Length<=(allocated+1))) {
                sequenceNumbers = Saxon.Ejava.util.Arrays.copyOf(sequenceNumbers, (allocated*2));
                uris = Saxon.Ejava.util.Arrays.copyOf(uris, (allocated*2));
            }
            sequenceNumbers[allocated] = sequence;
            uris[allocated] = uri;
            allocated++;
        }
        public string getSystemId(int sequence) {
            if ((allocated == 0)) {
                return null;
            }
            for (int i = 1; (i<allocated); i++) {
                if ((sequenceNumbers[i]>sequence)) {
                    return uris[(i-1)];
                }
            }
            return uris[(allocated-1)];
        }
    
    }
}