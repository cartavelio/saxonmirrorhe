using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class CharacterMapIndex : System.Collections.Generic.IEnumerable<Saxon.Hej.serialize.CharacterMap> {
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.serialize.CharacterMap> index = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.serialize.CharacterMap>(10);
        public CharacterMapIndex()  {
        }
        public Saxon.Hej.serialize.CharacterMap getCharacterMap(Saxon.Hej.om.StructuredQName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(index, name);
        }
        public void putCharacterMap(Saxon.Hej.om.StructuredQName name, Saxon.Hej.serialize.CharacterMap charMap) {
            Saxon.Impl.Helpers.MapUtils.Put(index, name, charMap);
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.serialize.CharacterMap> GetEnumerator() {
            return index.Values.GetEnumerator();
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public bool isEmpty() {
            return index.Count==0;
        }
        public Saxon.Hej.serialize.CharacterMapIndex copy() {
            Saxon.Hej.serialize.CharacterMapIndex copy = new Saxon.Hej.serialize.CharacterMapIndex();
            copy.index = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.serialize.CharacterMap>(this.index);
            return copy;
        }
        public Saxon.Hej.serialize.CharacterMapExpander makeCharacterMapExpander(string useMaps, Saxon.Hej.@event.Receiver next, Saxon.Hej.lib.SerializerFactory sf) {
            Saxon.Hej.serialize.CharacterMapExpander characterMapExpander = null;
            System.Collections.Generic.IList<Saxon.Hej.serialize.CharacterMap> characterMaps = new System.Collections.Generic.List<Saxon.Hej.serialize.CharacterMap>(5);
            Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(useMaps, " \t\n\r", false);
            while (st.hasMoreTokens()) {
                string expandedName = st.nextToken();
                Saxon.Hej.om.StructuredQName qName = Saxon.Hej.om.StructuredQName.fromClarkName(expandedName);
                Saxon.Hej.serialize.CharacterMap map = getCharacterMap(qName);
                if ((map == null)) {
                    throw new Saxon.Hej.trans.XPathException((("Character map '"+expandedName)+"' has not been defined"), "SEPM0016");
                }
                characterMaps.Add(map);
            }
            if (!(characterMaps.Count==0)) {
                characterMapExpander = sf.newCharacterMapExpander(next);
                if ((characterMaps.Count == 1)) {
                    characterMapExpander.setCharacterMap(characterMaps[0]);
                } else {
                    characterMapExpander.setCharacterMap(new Saxon.Hej.serialize.CharacterMap(characterMaps));
                }
            }
            return characterMapExpander;
        }
    
    }
}