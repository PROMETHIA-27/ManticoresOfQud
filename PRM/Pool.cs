using System.Collections.Generic;

namespace PRM {
    /// <summary>
    /// An efficient Pool for reusing heap objects without reallocating memory.
    /// 
    /// Items retrieved should be considered to have garbage data.
    /// 
    /// The pool must be constructed with an initial count.
    /// </summary>
    public struct Pool<T> where T : new() {
        /// <summary>
        /// The internal storage of reused values
        /// </summary>
        Stack<T> stack;

        /// <summary>
        /// Initialize the pool 
        /// </summary>
        /// <param name="initialCount">The number of default elements to fill the pool with.</param>
        public Pool(int initialCount) {
            this.stack = new Stack<T>(initialCount);
            for (int i = 0; i < initialCount; i++) {
                this.stack.Push(new T());
            }
        }

        /// <summary>
        /// Take an element from the pool, or create one if it's empty.
        /// </summary>
        /// <returns>A new element, filled with potentially garbage data.</returns>
        public T Take() {
            if (this.stack.Count > 0) {
                return this.stack.Pop();
            } else {
                return new T();
            }
        }

        /// <summary>
        /// Put an item in the pool
        /// </summary>
        /// <param name="value">The item to put in the pool</param>
        public void Return(T value) {
            this.stack.Push(value);
        }
    }
}