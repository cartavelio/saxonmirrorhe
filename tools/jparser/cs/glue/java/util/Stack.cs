using System;
//using System.Collections.Generic;
using NG = System.Collections.Generic;

namespace java.util {
    public class Stack<E> {
        private readonly NG.Stack<E> _stack;

        public Stack() {
            this._stack = new NG.Stack<E>();
        }

        public int size() {
            return _stack.Count;
        }

        public bool isEmpty() {
            return _stack.Count == 0;
        }

        public void push(E entry) {
            _stack.Push(entry);
        }

        public E pop() {
            return _stack.Pop();
        }

        public E peek() {
            return _stack.Peek();
        }

        public bool contains(E value) {
            return _stack.Contains(value);
        }

        public E get(int index) {
            return _stack.ToArray()[index];
        }
    }
}