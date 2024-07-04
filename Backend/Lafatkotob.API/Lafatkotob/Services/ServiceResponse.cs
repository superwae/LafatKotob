namespace Lafatkotob.Services
{
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public T Data { get; set; }
        public int? TotalItems { get; set; }
    }

}
