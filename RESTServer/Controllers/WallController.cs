using Microsoft.AspNetCore.Mvc;

namespace RESTServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WallController : Controller
    {
        public class Post
        {
            public string name { get; set; }
            public string content { get; set; }
            public DateTime time { get; set; }
            public string ip { get; set; }
        }

        public class PostDTO
        {
            public string name { get; set; }
            public string content { get; set; }
        }

        const int maxPosts = 5;
        public static List<Post> posts;

        static WallController()
        {
            posts = new List<Post>();
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(posts);
        }

        [HttpPost]
        public IActionResult Submit([FromBody] PostDTO post)
        {
            Post newPost = new Post()
            {
                name = post.name,
                content = post.content,
                time = DateTime.Now,
                ip = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            posts.Add(newPost);

            if (posts.Count > maxPosts)
                posts.RemoveRange(0, posts.Count - maxPosts);

            return Ok();
        }
    }
}
