namespace SimpleAuth.Shared
{
    using System;
    using SimpleAuth.Shared.Models;

    public abstract class Option<T>
    {
        public class Result : Option<T>
        {
            public Result(T item)
            {
                Item = item;
            }

            public T Item { get; }
        }

        public class Error : Option<T>, IEquatable<Option<T>.Error>
        {
            public Error(ErrorDetails details, string? state = null)
            {
                Details = details;
                State = state;
            }

            public ErrorDetails Details { get; }
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

    public abstract class Option
    {
        public class Success : Option { }

        public class Error : Option
        {
            public Error(ErrorDetails details, string? state = null)
            {
                Details = details;
                State = state;
            }

            public ErrorDetails Details { get; }
            public string? State { get; }
        }
    }
}