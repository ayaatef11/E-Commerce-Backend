using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Extensions;
    public static class FluentValidationExtension
    {
        public static IServiceCollection AddFluentValidation(this IServiceCollection services)
        {
            services
                .AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }

    public static class ModelStateExtensions
    {
        public static List<ValidationFailure> ToFluentValidationFailures(this ModelStateDictionary modelState)
        {
            var errors = new List<ValidationFailure>();

            foreach (var keyValuePair in modelState)
            {
                foreach (var error in keyValuePair.Value.Errors)
                {
                    var errorMessage = string.IsNullOrEmpty(error.ErrorMessage) ? "Invalid data" : error.ErrorMessage;
                    //var failure = new ValidationFailure(keyValuePair.Key, errorMessage)
                    //{
                    //    AttemptedValue = keyValuePair.Value.RawValue,
                    //    ErrorMessage = errorMessage
                    //};

                    //errors.Add(failure);
                }
            }

            return errors;
        }
    }
