using BackendServices.Exceptions;

namespace BackendServices.Validators.ValidationData
{
    public interface IValidationResult
    {
        public IValidationResult PreviousValidationResult { get; }

        public T GetValidationResultOfType<T>() where T : IValidationResult
        {
            if (this is T t || (PreviousValidationResult is T _t && (t = _t) is not null))
                return t;

            if (PreviousValidationResult == null)
                throw new NoValidationResultException();

            return PreviousValidationResult.GetValidationResultOfType<T>();
        }
    }
}
