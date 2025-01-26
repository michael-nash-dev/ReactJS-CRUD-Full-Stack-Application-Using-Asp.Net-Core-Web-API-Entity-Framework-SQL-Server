namespace MovieAPIDemo.Models
{
    public class BaseResponseModel
    {
        public bool status {  get; set; }
        public string message { get; set; }
        public object Data { get; set; }
    }
}
