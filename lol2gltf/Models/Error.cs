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
    }
}
