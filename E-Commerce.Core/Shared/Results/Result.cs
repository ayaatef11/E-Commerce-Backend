namespace E_Commerce.Core.Shared.Results;
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    public string? Message;
    public List<Error> Errors { get; }

    public Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
    }
    protected Result(bool isSuccess, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors.ToList();
    }

    public Result(bool isSuccess, string successMessage)
    {
        if (!isSuccess || successMessage is null)
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = Error.None;
        Message = successMessage;
    }

    public static Result Success() => new(true, Error.None);

    public static Result Success(string successMessage) => new(true, successMessage);

    public static Result Failure(Error? error) => new(false, error);

    //public static Result Failure(ProblemDetails problemDetails)
    //{
    //    var error = new Error
    //    {
    //        Title = problemDetails.Title,
    //        StatusCode = problemDetails.Status ?? 500
    //    };
    //    return Failure(error);
    //}
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error? error) => new(default, false, error);
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);
     public static Result<T> Failure<T>(IEnumerable<Error> errors) => new(default, false, errors);

}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, Error? error) : base(isSuccess, error)
    {
        _value = value;
    }
    public Result(TValue? value, bool isSuccess, IEnumerable<Error> errors)
      : base(isSuccess, errors)
    {
        _value = value;
    }


    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Failure results cannot have value");
}/*
  using Microsoft.AspNetCore.Mvc;

namespace SharedKernal.ResultResponse
{
    public sealed class Result : Result<object>
    {
        public Result() // only for serializing
        {
        }

        internal Result(bool succeeded, string? message = null) : base(succeeded, message)
        {
        }

        public static new Result Success(string? message = null)
        {
            return new Result(true)
            {
                Message = message
            };
        }

        public static new Result Failure(string message)
        {
            return new Result(false, message);
        }

        public static new Result Failure(ProblemDetails problemDetails)
        {
            return new Result(false)
            {
                ProblemDetails = problemDetails
            };
        }
    }

    public class Result<TData>
    {
        public Result() // only for serializing
        {
        }

        internal Result(bool succeeded, string? message = null)
        {
            Succeeded = succeeded;
            Message = message;
        }

        public bool Succeeded { get; init; }

        public string Message { get; set; }
        public TData Data { get; set; }
        public ProblemDetails ProblemDetails { get; set; }

        public static Result<TData> Success(string? message = null)
        {
            return new Result<TData>(true)
            {
                Message = message
            };
        }

        public static Result<TData> Failure(string message)
        {
            return new Result<TData>(false, message);
        }

        public static Result<TData> Failure(ProblemDetails problemDetails)
        {
            return new Result<TData>(false)
            {
                ProblemDetails = problemDetails
            };
        }

        public Result<TData> WithData(TData data)
        {
            Data = data;
            return this;
        }

        public static implicit operator Result(Result<TData> result)
            => new Result
            {
                Succeeded = result.Succeeded,
                Message = result.Message,
                ProblemDetails = result.ProblemDetails
            };
        public static implicit operator Result<TData>(Result<object> result)
            => new Result<TData>
            {
                Succeeded = result.Succeeded,
                Message = result.Message,
                ProblemDetails = result.ProblemDetails
            };

        public static implicit operator Result<TData>(TData data)
           => new Result<TData>
           {
               Succeeded = true,
               Data = data
           };
    }
}

    public static class ResultUtility
    {
        public static Result<T> ToResult<T>(this Exception e)
        {
            var message = e.Message;
            if (e.InnerException is not null)
                message = $"{message} - inner details => {e.InnerException.Message}";

            return Result<T>.Failure(message);
        }

        public static void EnsureSuccess<T>(this Result<T> r, string errorMessage = null)
        {
            if (!r.Succeeded)
                throw new BusinessException(errorMessage ?? "received not success response", r);
        }
    }
}

*/