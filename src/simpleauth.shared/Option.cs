namespace SimpleAuth.Shared
{
    using SimpleAuth.Shared.Models;

    public abstract class Option
    {
        public class Success : Option { }

        public class Success<T> : Option
        {
            public Success(T item)
            {
                Item = item;
            }

            public T Item { get; }
        }

        public class Error : Option
        {
            public Error(ErrorDetails details)
            {
                Details = details;
            }

            public ErrorDetails Details { get; }
        }
    }
}