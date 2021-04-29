namespace SimpleAuth.Shared
{
    using System;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the generic option type.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of result.</typeparam>
    public abstract class Option<T>
    {
        /// <summary>
        /// Defines the successful result.
        /// </summary>
        public class Result : Option<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Option{T}.Result"/> class.
            /// </summary>
            /// <param name="item"></param>
            public Result(T item)
            {
                Item = item;
            }

            /// <summary>
            /// Gets the result item.
            /// </summary>
            public T Item { get; }
        }

        /// <summary>
        /// Defines the error result.
        /// </summary>
        public class Error : Option<T>, IEquatable<Option<T>.Error>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Option{T}.Error"/> class.
            /// </summary>
            /// <param name="details">The <see cref="ErrorDetails"/> description.</param>
            /// <param name="state">The state description.</param>
            public Error(ErrorDetails details, string? state = null)
            {
                Details = details;
                State = state;
            }

            /// <summary>
            /// Gets the error details.
            /// </summary>
            public ErrorDetails Details { get; }

            /// <summary>
            /// Gets the state.
            /// </summary>
            public string? State { get; }

            /// <inheritdoc />
            public bool Equals(Error? other)
            {
                if (other is null)
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return Details.Equals(other.Details) && State == other.State;
            }

            /// <inheritdoc />
            public override bool Equals(object? obj)
            {
                return Equals(obj as Error);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return HashCode.Combine(Details, State);
            }

            public static bool operator ==(Error? left, Error? right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Error? left, Error? right)
            {
                return !Equals(left, right);
            }
        }
    }

    /// <summary>
    /// Defines the option type.
    /// </summary>
    public abstract class Option
    {
        /// <summary>
        /// Defines the success result.
        /// </summary>
        public class Success : Option { }

        /// <summary>
        /// Defines the error result.
        /// </summary>
        public class Error : Option
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Option.Error"/> class.
            /// </summary>
            /// <param name="details">The <see cref="ErrorDetails"/> description.</param>
            /// <param name="state">The state description.</param>
            public Error(ErrorDetails details, string? state = null)
            {
                Details = details;
                State = state;
            }

            /// <summary>
            /// Gets the error details.
            /// </summary>
            public ErrorDetails Details { get; }

            /// <summary>
            /// Gets the state.
            /// </summary>
            public string? State { get; }
        }
    }
}