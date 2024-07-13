using BackendServices.Models;

namespace BackendServices.Validators.ValidationData
{
    internal struct ShiftValidationResult : IValidationResult
    {
        public IValidationResult PreviousValidationResult { get; }
        public ShiftValidationData ValidationData { get; }
        public Response Response { get; }

        public ShiftValidationResult(IValidationResult previousValidationResult, ShiftValidationData validationData, Response response)
        {
            PreviousValidationResult = previousValidationResult;
            ValidationData = validationData;
            Response = response;
        }
    }
}
