using BH.Application.Interface;
using BH.Domain.Model;
using MediatR;

namespace BH.Application.Features.Commands
{
    /// <summary>
    /// Adds verses to database
    /// </summary>
    public class AddVersesCommand : IRequest<Unit>
    {
        public AddVersesCommand(IEnumerable<Verse> verses)
        {
            Verses = verses;
        }

        public IEnumerable<Verse> Verses { get; }
    }

    public class AddVersesCommandHandler : IRequestHandler<AddVersesCommand, Unit>
    {
        private readonly IRepository repository;

        public AddVersesCommandHandler(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Unit> Handle(AddVersesCommand request, CancellationToken cancellationToken)
        {
            foreach (var verse in request.Verses)
            {
                await repository.AddNoSaveAsync(verse);
            }

            await repository.SaveChangesAsync();

            return new Unit();
        }
    }
}
