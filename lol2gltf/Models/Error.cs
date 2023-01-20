using System;

namespace lol2gltf.Models
{
    public class Error
    {
        public string Title { get; set; }
        public string Message { get; set; }

        public Error(string title, string message)
        {
            this.Title = title;
            this.Message = message;
        }

        public static Error FromException(Exception exception)
        {
            if (exception.InnerException is Exception innerException)
            {
                return new(exception.Message, innerException.Message);
            }
            else
            {
                return new("Error", exception.Message);
            }
        }
    }
}
