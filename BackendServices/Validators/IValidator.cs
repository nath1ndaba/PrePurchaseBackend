using BackendServices.Validators.ValidationData;
using System.Threading.Tasks;

namespace BackendServices.Validators
{
    public interface IValidator<TType, TResult> 
        where TType: struct, IValidationData
        where TResult : IValidationResult
    {
        public ValueTask<TResult> Validate(TType value);
    }
}
