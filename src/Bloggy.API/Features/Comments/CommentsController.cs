using System.Threading.Tasks;
using Bloggy.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bloggy.API.Features.Comments
{
    [Route ("api/posts/{postId}/comments")]
    public class CommentsController : Controller
    {
        private readonly IMediator _mediator;

        public CommentsController (IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> List ([FromRoute] int postId)
        {
            var query = new List.Query { PostId = postId };
            var result = await _mediator.Send (query);

            return result.IsSuccess
                ? (IActionResult)Ok(result)
                : (IActionResult)BadRequest(result.Error);
        }

        [HttpGet ("{id}", Name = "Details")]
        public async Task<IActionResult> Details ([FromRoute] int postId, [FromRoute] int id)
        {
            var query = new Details.Query { PostId = postId, Id = id };
            var result = await _mediator.Send (query);

            return result.IsSuccess
                ? (IActionResult)Ok()
                : (IActionResult)BadRequest(result.Error);
        }

        [HttpPost]
        [Authorize (AuthenticationSchemes = JwtIssuerOptions.Schemes)]
        public async Task<IActionResult> Create ([FromRoute] int postId, [FromBody] Create.Command command)
        {
            command.PostId = postid;
            var result = await _mediator.Send (command);

            return result.IsSuccess
                ? (IActionResult)CreatedAtRoute ("Details", new { controller = "Comments", id = result.Id }, result)
                : (IActionResult)BadRequest(result.Error);
        }

        [HttpPut ("{id}")]
        [Authorize (AuthenticationSchemes = JwtIssuerOptions.Schemes)]
        public async Task<IActionResult> Edit ([FromRoute] int postId, [FromQuery] int id, [FromBody] Edit.Command command)
        {
            command.PostId = postid;
            command.Id = id;
            var result = await _mediator.Send (command);

            return result.IsSuccess
                ? (IActionResult)NoContent()
                : (IActionResult)BadRequest(result.Error);
        }

        [HttpDelete ("{id}")]
        [Authorize (AuthenticationSchemes = JwtIssuerOptions.Schemes)]
        public async Task Delete ([FromRoute] int postId, [FromRoute] int id)
        {
            var command = new Delete.Command { PostId = postId, Id = id };
            var result = await _mediator.Send (command);

            return result.IsSuccess
                ? (IActionResult)NoContent()
                : (IActionResult)BadRequest(result.Error);
        }
    }
}
