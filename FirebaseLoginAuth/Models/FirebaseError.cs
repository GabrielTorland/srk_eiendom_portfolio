namespace FirebaseLoginAuth.Models
{
    public class Error
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Error> errors { get; set; }
    }

    public class FirebaseError
    {
        public Error error { get; set; }
    }
}    
