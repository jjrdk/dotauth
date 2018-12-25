namespace SimpleAuth.Validators
{
    public interface IParametersValidator
    {
        void ValidateLocationPattern(string locationPattern);
    }
}