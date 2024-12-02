using System.Text.Json.Serialization;


namespace Sant.News
{
    public class Result
    {
        public bool Success => Code == ResultCode.Ok;

        public IEnumerable<ErrorMessage> Errors { get; set; }

        [JsonIgnore]
        public ResultCode Code { get; set; }

        public static Result Ok()
        {
            return new Result()
            {
                Code = ResultCode.Ok
            };
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>()
            {
                Value = value,
                Code = ResultCode.Ok
            };
        }

        public static Result BadRequest(string message)
        {
            var result = new Result
            {
                Code = ResultCode.BadRequest,
                Errors = new List<ErrorMessage>()
                {
                    new ErrorMessage()
                    {
                        Message = message
                    }
                }
            };

            return result;
        }

        public static Result BadRequest(IEnumerable<ErrorMessage> messages)
        {
            var result = new Result
            {
                Code = ResultCode.BadRequest,
                Errors = messages
            };


            return result;
        }

        public static Result<T> BadRequest<T>(string message)
        {
            var result = new Result<T>
            {
                Code = ResultCode.BadRequest,
                Errors = new List<ErrorMessage>()
                {
                    new ErrorMessage()
                    {
                        Message = message
                    }
                }
            };

            return result;
        }

        public static Result<T> BadRequest<T>(IEnumerable<ErrorMessage> messages)
        {
            var result = new Result<T>
            {
                Code = ResultCode.BadRequest,
                Errors = messages
            };


            return result;
        }


        public static Result<T> NotFound<T>(Guid id)
        {
            var result = new Result<T>
            {
                Code = ResultCode.NotFound,
                Errors = new List<ErrorMessage>()
                {
                    new ErrorMessage()
                    {
                        Message = $"Nie ma obiektu o id {id}"
                    }
                }
            };


            return result;
        }


        public static Result<T> NotFound<T>(string message)
        {
            var result = new Result<T>
            {
                Code = ResultCode.NotFound,
                Errors = new List<ErrorMessage>()
                {
                    new ErrorMessage()
                    {
                        Message = $"Nie ma obiektu {message}"
                    }
                }
            };


            return result;
        }
        public static Result<T> NotFound<T>()
        {
            var result = new Result<T>
            {
                Code = ResultCode.NotFound,
                Errors = new List<ErrorMessage>()
                {
                    new ErrorMessage()
                    {
                        Message = $"Wyszukiwany obiekt nie został znaleziony"
                    }
                }
            };


            return result;
        }

        public static Result NotFound(Guid id)
        {
            var result = new Result
            {
                Code = ResultCode.NotFound,
                Errors = new List<ErrorMessage>()
                {
                    new ErrorMessage()
                    {
                        Message = $"Nie ma obiektu o id {id}"
                    }
                }
            };
            return result;
        }



    }
    public class Result<T> : Result
    {
        public T Value { get; set; }
    }
    public class ErrorMessage
    {
        public string PropertyName { get; set; }

        public string Message { get; set; }
    }

    public enum ResultCode
    {
        Ok,
        BadRequest,
        NotFound,
        AccessDenied

    }
}
