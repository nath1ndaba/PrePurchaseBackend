using BackendServices.Exceptions;

namespace BackendServices.Validators.ValidationData
{
    public interface IValidationData
    {
        public IValidationData PreviousValidationData { get; }

        public T GetValidationDataOfType<T>() where T : IValidationData
        {
            if (this is T t || (PreviousValidationData is T _t && (t = _t) is not null))
                return t;

            if (PreviousValidationData == null)
                throw new NoValidationResultException();

            return PreviousValidationData.GetValidationDataOfType<T>();
        }
    }
}
