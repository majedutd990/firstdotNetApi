using System.Data;
using Dapper;
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
            DynamicParameters sqlParam = new DynamicParameters();
            if (postId != 0)
            {
                sqlParam.Add("@PostIdParam", postId, DbType.Int32);
                parameter += ", @PostId = @PostIdParam";
            }

            if (userId != 0)
            {
                sqlParam.Add("@UserIdParam", userId, DbType.Int32);
                parameter += ", @UserId = @UserIdParam";
            }

            if (searchParam.ToLower() != "none")
            {
                sqlParam.Add("@SearchValueParam", searchParam, DbType.String);
                parameter += ", @SearchValue = @SearchValueParam";
            }

            if (parameter.Length > 0)
            {
                sqlQuery += parameter.Substring(1);
            }

            return _dapper.LoadDataWithParameters<Post>(sqlQuery, sqlParam);
        }


        [HttpGet("MyPosts")]
        public IEnumerable<Post> MyPosts()
        {
            string sqlQuery = @"EXEC TutorialAppSchema.spPosts_Get @UserId= @UserIdParam";
            DynamicParameters sqlParam = new DynamicParameters();
            sqlParam.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);

            return _dapper.LoadDataWithParameters<Post>(sqlQuery, sqlParam);
        }


        [HttpPut("Upsert")]
        public IActionResult Upsert(Post post)
        {
            string sqlInsertPost = @"
                EXEC TutorialAppSchema.spPosts_Upsert @UserId = @UserIdParam"
                                   + ", @PostTitle = @PostTitleParam"
                                   + ", @PostContent = @PostContentParam ";
            string parameters = "";

            DynamicParameters sqlParam = new DynamicParameters();
            sqlParam.Add("@PostTitleParam", post.PostTitle, DbType.String);
            sqlParam.Add("@PostContentParam", post.PostContent, DbType.String);
            sqlParam.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);

            if (post.PostId > 0)
            {
                sqlParam.Add("@PostIdParam", post.PostId, DbType.Int32);
                parameters += ", @PostId = @PostIdParam";
            }

            sqlInsertPost += parameters;

            if (!_dapper.ExecuteSqlWithParams(sqlInsertPost, sqlParam))
                throw new InvalidOperationException("failed to add post");
            return Ok();
        }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"EXEC TutorialAppSchema.spPost_DELETE @PostId= @PostIdParam, @UserId = @UserIdParam";
            DynamicParameters sqlParam = new DynamicParameters();
            sqlParam.Add("@PostIdParam", postId, DbType.Int32);
            sqlParam.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
            if (!_dapper.ExecuteSqlWithParams(sql, sqlParam))
                throw new InvalidOperationException("could not delete post");
            return Ok();
        }
    }
}