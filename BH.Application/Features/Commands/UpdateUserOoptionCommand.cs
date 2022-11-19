using BH.Application.Interface;
using BH.Domain.Model;
using MediatR;

namespace BH.Application.Features.Commands
{
    /// <summary>
    /// Allows the user to set options, mainly for names
    /// </summary>
    public class UpdateUserOptionCommand : IRequest<bool>
    {
        public UpdateUserOptionCommand(UpdateUserOptionDto userOptionDto)
        {
            UserOptionDto = userOptionDto;
        }

        public UpdateUserOptionDto UserOptionDto { get; }
    }

    public class UpdateUserOptionCommandHandler : IRequestHandler<UpdateUserOptionCommand, bool>
    {
        private readonly IRepository repository;

        public UpdateUserOptionCommandHandler(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<bool> Handle(UpdateUserOptionCommand request, CancellationToken cancellationToken)
        {
            bool updated = false;

            if(request.UserOptionDto.UserId > 0)
            {
                var user = await repository.GetByIdAsync<User>(request.UserOptionDto.UserId);
                if (user == null)
                    user = await repository.AddAsync(new User() { Id = request.UserOptionDto.UserId, GodsSon="", Anointed ="", DevineName ="" });

                if (!string.IsNullOrWhiteSpace(request.UserOptionDto?.DevineName))
                {
                    user.DevineName = request.UserOptionDto?.DevineName;
                    updated = true;
                }

                if (!string.IsNullOrWhiteSpace(request.UserOptionDto?.GodsSon))
                {
                    user.GodsSon = request.UserOptionDto?.GodsSon;
                    updated = true;
                }

                if (!string.IsNullOrWhiteSpace(request.UserOptionDto?.Anointed))
                {
                    user.Anointed = request.UserOptionDto?.Anointed;
                    updated = true;
                }

                if(updated)
                    await repository.UpdateAsync(user);
            }                       

            return updated;
        }
    }

    public class UpdateUserOptionDto
    {
        public long UserId { get; set; }
        public string DevineName { get; set; }
        public string GodsSon { get; set; }
        public string Anointed { get; set; }
    }
}
