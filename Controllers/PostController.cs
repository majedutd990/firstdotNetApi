using DotnetAPI.Data;
using DotnetAPI.DTOs;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public PostController(IConfiguration configuration)
        {
            _dapper = new DataContextDapper(configuration);
        }

        [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
        {
            string sqlQuery = @"EXEC TutorialAppSchema.spPosts_Get";
            string parameter = "";
            if (postId != 0)
            {
                parameter += ", @PostId = " + postId.ToString();
            }

            if (userId != 0)
            {
                parameter += ", @UserId = " + userId.ToString();
            }

            if (searchParam.ToLower() != "none")
            {
                parameter += ", @SearchValue = '" + searchParam + "'";
            }

            if (parameter.Length > 0)
            {
                sqlQuery += parameter.Substring(1);
            }

            return _dapper.LoadData<Post>(sqlQuery);
        }


        [HttpGet("MyPosts")]
        public IEnumerable<Post> MyPosts()
        {
            string sqlQuery = @"EXEC TutorialAppSchema.spPosts_Get @UserId=" + this.User.FindFirst("userId")?.Value +
                              "";

            return _dapper.LoadData<Post>(sqlQuery);
        }


        [HttpPut("Upsert")]
        public IActionResult Upsert(Post post)
        {
            string sqlInsertPost = @"
                EXEC TutorialAppSchema.spPosts_Upsert @UserId =" + this.User.FindFirst("userId")?.Value + ","
                                   + "@PostTitle = '" + post.PostTitle + "',"
                                   + "@PostContent = '" + post.PostContent + "'";
            string parameters = "";

            if (post.PostId > 0)
            {
                parameters += ", @PostId = " + post.PostId;
            }

            sqlInsertPost += parameters;

            if (!_dapper.ExecuteSql(sqlInsertPost)) throw new InvalidOperationException("failed to add post");
            return Ok();
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"DELETE FROM TutorialAppSchema.Posts WHERE PostId = '" + postId.ToString() + "'" +
                         "AND UserId = '" + this.User.FindFirst("userId").Value + "'";
            if (!_dapper.ExecuteSql(sql)) throw new InvalidOperationException("could not delete post");
            return Ok();
        }
    }
}