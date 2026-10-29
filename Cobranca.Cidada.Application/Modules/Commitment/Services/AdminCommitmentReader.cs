using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Cobranca.Cidada.Application.Modules.Commitment.Mappers;
using Microsoft.AspNetCore.Identity;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Application.Modules.Commitment.Services;

/// <summary>Monta a visão completa de um compromisso para o painel, resolvendo o nome de quem editou.</summary>
public class AdminCommitmentReader(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
{
    public async Task<ErrorOr<AdminCommitmentDetailResponse>> GetAsync(string slug, CancellationToken cancellationToken)
    {
        var commitment = await unitOfWork.CommitmentRepository.GetForAdminAsync(slug.Trim(), cancellationToken);
        if (commitment is null)
            return Error.NotFound("Commitment.NotFound", "Compromisso não encontrado.");

        var authorIds = commitment.StatusEntries.Select(e => e.AuthorId)
            .Concat(commitment.Revisions.Select(r => r.AuthorId))
            .Distinct()
            .ToList();
        var names = new Dictionary<string, string>();
        foreach (var id in authorIds)
        {
            if (await userManager.FindByIdAsync(id) is { } user)
                names[id] = user.Name;
        }

        string Author(string id) => names.GetValueOrDefault(id) ?? (id == "seed" ? "Carga inicial" : id);

        return commitment.ToAdminDetail(Author);
    }
}
