using System;
using System.Threading.Tasks;
using AutoMapper;
using Bloggy.API.Data;
using Bloggy.API.Entities;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bloggy.API.Features.Comments
{
    public class DetailsQ
    {
        public class Query : IRequest<Result<Model>>
        {
            public int PostId { get; set; }
            public int Id { get; set; }
        }

        public class Model
        {
            public int Id { get; set; }
            public string Body { get; set; }
            public ApplicationUser Author { get; set; }
            public DateTime CreatedDate { get; set; }

            public class ApplicationUser
            {
                public int Id { get; set; }
                public string Username { get; set; }
            }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator ()
            {
                RuleFor (c => c.PostId).NotNull ();
                RuleFor (c => c.Id).NotNull ();
            }
        }

        public class Handler : AsyncRequestHandler<Query, Result<Model>>
        {
            private readonly BloggyContext _context;
            private readonly IMapper _mapper;

            public Handler (BloggyContext context,
                IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            protected override async Task<Result<Model>> HandleCore (Query message)
            {
                var comment = await SingleAsync (message.Id);

                if (comment == null)
                    return Result.Fail<Model> ("Comment does not exit");

                var model = _mapper.Map<Comment, Model> (comment);

                return Result.Ok (model);
            }

            private async Task<Comment> SingleAsync (int id)
            {
                return await _context.Comments
                    .Include (c => c.Author)
                    .AsNoTracking ()
                    .SingleOrDefaultAsync (p => p.Id == id);
            }
        }
    }
}
