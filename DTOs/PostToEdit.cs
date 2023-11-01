namespace DotnetAPI.DTOs
{
    public class PostToEdit
    {
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public string PostContent { get; set; }

        public PostToEdit()
        {
            PostTitle ??= "";
            PostContent ??= "";
        }
    }
}