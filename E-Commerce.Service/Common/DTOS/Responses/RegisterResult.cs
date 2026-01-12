namespace E_Commerce.Application.Common.DTOS.Responses;
    public class RegisterResult
    { 
            public bool success { get; set; }
            public string UserEmail { get; set; }
            public IEnumerable<string> Errors { get; set; }

            public static RegisterResult Success(string email) => new RegisterResult
            {
                success = true,
                UserEmail = email
            };

            public static RegisterResult Failure(IEnumerable<string> errors) => new RegisterResult
            {
                success = false,
                Errors = errors
            };
        }
    

