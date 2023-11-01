namespace DotnetAPI.DTOs
{
    public class PostToAdd
    {
        public string PostTitle { get; set; }
        public string PostContent { get; set; }

        public PostToAdd()
        {
            PostTitle ??= "";
            PostContent ??= "";
        }
    }
}