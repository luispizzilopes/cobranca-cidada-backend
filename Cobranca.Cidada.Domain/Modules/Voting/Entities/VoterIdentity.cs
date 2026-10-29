using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;

namespace Cobranca.Cidada.Domain.Modules.Voting.Entities;

/// <summary>
/// Votante com e-mail verificado que não criou conta. O Id é o VoterKey (HMAC do e-mail); o e-mail não é guardado.
/// Tokens emitidos antes de <see cref="TokensValidAfterUtc"/> são inválidos (revogação).
/// </summary>
public class VoterIdentity : Entity<string>
{
    public DateTime VerifiedAtUtc { get; private set; }
    public DateTime LastVerifiedAtUtc { get; private set; }
    public DateTime TokensValidAfterUtc { get; private set; }

    private VoterIdentity() { }

    private VoterIdentity(string voterKey) : base(voterKey)
    {
        var now = DateTime.UtcNow;
        VerifiedAtUtc = now;
        LastVerifiedAtUtc = now;
        TokensValidAfterUtc = now.AddSeconds(-1);
    }

    public static VoterIdentity Create(string voterKey) => new(voterKey);

    public void MarkVerified() => LastVerifiedAtUtc = DateTime.UtcNow;
}
