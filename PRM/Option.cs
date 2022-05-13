using System;

namespace PRM {
    /// <summary>
    /// Represents a value that may or may not exist. Can either be Some or None.
    /// 
    /// Create a Some value with the constructor, or a None with the default constructor or default.
    /// </summary>
    /// <typeparam name="T">The type of the value that this option may contain.</typeparam>
    public struct Option<T> {
        /// <summary>
        /// The actual value. May be uninitialized if this is a None option
        /// </summary>
        T value;
        /// <summary>
        /// 0 if None, not 0 if Some
        /// </summary>
        byte discriminant;

        /// <summary>
        /// Create a new Some option with the given value
        /// </summary>
        /// <param name="value">The value to assign</param>
        public Option(T value) {
            this.value = value;
            this.discriminant = 1;
        }

        /// <summary>
        /// Attempt to get the value from this option.
        /// </summary>
        /// <returns>The value of this option.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if this option contains no value.</exception>
        public T Unwrap() {
            if (this.discriminant != 0) {
                return this.value;
            } else {
                throw new InvalidOperationException("Attempted to unwrap None option");
            }
        }

        /// <summary>
        /// Get whether this is a Some or a None option.
        /// </summary>
        /// <returns>True if Some, False if None.</returns>
        public bool IsSome() {
            return this.discriminant != 0;
        }
    }
}