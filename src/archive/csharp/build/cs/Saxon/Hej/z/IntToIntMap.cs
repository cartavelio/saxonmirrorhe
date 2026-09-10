using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.z {
    public interface IntToIntMap {
        void setDefaultValue(int defaultValue);
        int getDefaultValue();
        void clear();
        bool contains(int key);
        int get(int key);
        int size();
        bool remove(int key);
        void put(int key, int value);
        Saxon.Hej.z.IntIterator keyIterator();
    
    }
}