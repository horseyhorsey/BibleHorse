using BH.Application.Interface;
using BH.Domain.Model;
using MediatR;

namespace BH.Application.Features.Commands
{
    /// <summary>
    /// Gets or Adds a new user from a UserId
    /// </summary>
    public class GetOrAddUserCommand : IRequest<User>
    {
        public GetOrAddUserCommand(long userId)
        {
            UserId = userId;
        }

        public long UserId { get; }
    }

    public class GetOrAddUserCommandHandler : IRequestHandler<GetOrAddUserCommand, User>
    {
        private readonly IRepository repository;

        public GetOrAddUserCommandHandler(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<User> Handle(GetOrAddUserCommand request, CancellationToken cancellationToken)
        {
            User user = null;

            if(request.UserId > 0)
            {
                user = await repository.GetByIdAsync<User>(request.UserId);
                if (user == null) //create new user
                {
                    user = await repository.AddAsync(new User() { Id = request.UserId, DevineName = "", Anointed = "", GodsSon = ""});
                }
            }

            return user;
        }
    }
}
