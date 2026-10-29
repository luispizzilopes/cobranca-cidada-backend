using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.Commitment.Rules;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Commitment.Entities;

public class CommitmentSource : Entity<CommitmentSourceId>
{
    public string Name { get; private set; } = null!;
    public string Url { get; private set; } = null!;
    public string? Page { get; private set; }
    public SourceKind Kind { get; private set; }
    public int Position { get; private set; }

    private CommitmentSource() { }

    private CommitmentSource(string name, string url, string? page, SourceKind kind, int position)
        : base(CommitmentSourceId.CreateIdentifier())
    {
        Name = name;
        Url = url;
        Page = page;
        Kind = kind;
        Position = position;
    }

    public static CommitmentSource Create(string name, string url, string? page, SourceKind kind, int position)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O nome da fonte é obrigatório.");
        if (!StatusRules.IsValidHttpUrl(url)) throw new DomainException($"URL de fonte inválida: {url}");
        return new CommitmentSource(name.Trim(), url.Trim(), string.IsNullOrWhiteSpace(page) ? null : page.Trim(), kind, position);
    }
}
