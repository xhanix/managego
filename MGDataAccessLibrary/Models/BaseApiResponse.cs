using System;
namespace MGDataAccessLibrary.Models
{
    public class BaseApiResponse<T>
    {
        public ResponseStatus Status { get; set; }
        public string ErrorMessage { get; set; }
        public T Result { get; set; }
    }
}
