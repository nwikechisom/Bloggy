using System;
using System.Threading.Tasks;
using AutoMapper;
using Bloggy.API.Data;
using Bloggy.API.Entities;
using Bloggy.API.Services.Interfaces;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bloggy.API.Features.Comments
{
    public class Create
    {
        public class Command : IRequest<Result<Model>>
        {
            public int PostId { get; set; }
            public string Body { get; set; }
        }

        public class Model
        {
            public int Id { get; set; }
            public string Body { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator ()
            {
                RuleFor (c => c.PostId).NotNull ();
                RuleFor (c => c.Body).NotEmpty ();
            }
        }

        public class Handler : AsyncRequestHandler<Command, Result<Model>>
        {
            private readonly BloggyContext _context;
            private readonly ICurrentUserAccessor _currentUserAccessor;
            private readonly IMapper _mapper;

            public Handler (BloggyContext context,
                ICurrentUserAccessor currentUserAccessor,
                IMapper mapper)
            {
                _context = context;
                _currentUserAccessor = currentUserAccessor;
                _mapper = mapper;
            }

            protected override async Task<Result<Model>> HandleCore (Command message)
            {
                var post = await SinglePostAsync (message.PostId);

                if (post == null)
                    return Result.Fail<Model> ("Post does not exit");

                var comment = new Comment
                {
                    Author = await SingleUserAsync (_currentUserAccessor.GetCurrentUsername ()),
                    Body = message.Body,
                    CreatedDate = DateTime.UtcNow
                };

                await _context.Comments.AddAsync (comment);

                post.Comments.Add (comment);

                await _context.SaveChangesAsync ();

                var model = _mapper.Map<Comment, Model> (comment);

                return Result.Ok (model);
            }

            private async Task<Post> SinglePostAsync (int id)
            {
                return await _context.Posts
                    .Include (p => p.Comments)
                    .SingleOrDefaultAsync (p => p.Id == id);
            }

            private async Task<ApplicationUser> SingleUserAsync (string username)
            {
                return await _context.Users
                    .SingleOrDefaultAsync (u => u.Username == username);
            }
        }
    }
}
