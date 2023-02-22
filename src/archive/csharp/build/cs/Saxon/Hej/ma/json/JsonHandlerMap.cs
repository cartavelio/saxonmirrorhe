using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.json {
    public class JsonHandlerMap : Saxon.Hej.ma.json.JsonHandler {
        internal System.Collections.Generic.Stack<Saxon.Hej.om.Sequence> stack;
        protected internal System.Collections.Generic.Stack<string> keys;
        public JsonHandlerMap(Saxon.Hej.expr.XPathContext context, int flags)  {
            setContext(context);
            stack = new System.Collections.Generic.Stack<Saxon.Hej.om.Sequence>();
            keys = new System.Collections.Generic.Stack<string>();
            escape = (((flags&Saxon.Hej.ma.json.JsonParser.ESCAPE)) != 0);
            charChecker = context.getConfiguration().getValidCharacterChecker();
        }
        public override Saxon.Hej.om.Sequence getResult() {
            return stack.Peek();
        }
        public override bool setKey(string unEscaped, string reEscaped) {
            this.keys.Push(reEscaped);
            Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(stack.Peek());
            return (map.get(new Saxon.Hej.value.StringValue(reEscaped)) != null);
        }
        public override void startArray() {
            System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> memberList = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
            Saxon.Hej.ma.arrays.ArrayItem map = new Saxon.Hej.ma.arrays.SimpleArrayItem(memberList);
            stack.Push(map);
        }
        public override void endArray() {
            Saxon.Hej.ma.arrays.ArrayItem map = (Saxon.Hej.ma.arrays.ArrayItem)(stack.Pop());
            if ((stack.Count==0)) {
                stack.Push(map);
            } else {
                writeItem(map);
            }
        }
        public override void startMap() {
            Saxon.Hej.ma.map.DictionaryMap map = new Saxon.Hej.ma.map.DictionaryMap();
            stack.Push(map);
        }
        public override void endMap() {
            Saxon.Hej.ma.map.DictionaryMap map = (Saxon.Hej.ma.map.DictionaryMap)(stack.Pop());
            if ((stack.Count==0)) {
                stack.Push(map);
            } else {
                writeItem(map);
            }
        }
        private void writeItem(Saxon.Hej.om.GroundedValue val) {
            if ((stack.Count==0)) {
                stack.Push(val);
            } else {
                if ((stack.Peek() is Saxon.Hej.ma.arrays.ArrayItem)) {
                    Saxon.Hej.ma.arrays.SimpleArrayItem array = (Saxon.Hej.ma.arrays.SimpleArrayItem)(stack.Peek());
                    array.getMembers().Add(val.materialize());
                } else {
                    Saxon.Hej.ma.map.DictionaryMap map = (Saxon.Hej.ma.map.DictionaryMap)(stack.Peek());
                    map.initialPut(keys.Pop(), val);
                }
            }
        }
        public override void writeNumeric(string asString, Saxon.Hej.value.AtomicValue parsedValue) {
            writeItem(parsedValue);
        }
        public override void writeString(string val) {
            writeItem(new Saxon.Hej.value.StringValue(reEscape(val)));
        }
        public override void writeBoolean(bool value) {
            writeItem(Saxon.Hej.value.BooleanValue.get(value));
        }
        public override void writeNull() {
            writeItem(Saxon.Hej.value.EmptySequence.getInstance());
        }
    
    }
}