using Microsoft.Extensions.Configuration;

namespace Cobranca.Cidada.Application.Modules.Voting.Settings;

/// <summary>Limiares da detecção de rajadas de votos (seção <c>Voting:Burst</c>).</summary>
public class VoteBurstSettings
{
    /// <summary>Ações de voto por pessoa na janela curta antes de bloquear.</summary>
    public int VoterMaxActions { get; }
    public TimeSpan VoterWindow { get; }
    public TimeSpan VoterBlock { get; }

    /// <summary>Ações de voto vindas do mesmo IP (qualquer pessoa) na janela curta.</summary>
    public int IpMaxActions { get; }
    public TimeSpan IpWindow { get; }
    public TimeSpan IpBlock { get; }

    /// <summary>Pessoas distintas votando do mesmo IP numa janela longa (indício de contas em massa).</summary>
    public int IpMaxDistinctVoters { get; }
    public TimeSpan IpDistinctWindow { get; }
    public TimeSpan IpDistinctBlock { get; }

    /// <summary>Ações de voto num mesmo compromisso numa janela média. Só gera alerta para a curadoria; não bloqueia.</summary>
    public int CommitmentSpikeMaxActions { get; }
    public TimeSpan CommitmentSpikeWindow { get; }
    public TimeSpan CommitmentSpikeAlertInterval { get; }

    public VoteBurstSettings(IConfiguration configuration)
    {
        var section = configuration.GetSection("Voting:Burst");

        VoterMaxActions = Int(section, "VoterMaxActions", 10);
        VoterWindow = Seconds(section, "VoterWindowSeconds", 10);
        VoterBlock = Seconds(section, "VoterBlockSeconds", 300);

        IpMaxActions = Int(section, "IpMaxActions", 15);
        IpWindow = Seconds(section, "IpWindowSeconds", 10);
        IpBlock = Seconds(section, "IpBlockSeconds", 300);

        IpMaxDistinctVoters = Int(section, "IpMaxDistinctVoters", 10);
        IpDistinctWindow = Seconds(section, "IpDistinctWindowSeconds", 600);
        IpDistinctBlock = Seconds(section, "IpDistinctBlockSeconds", 1800);

        CommitmentSpikeMaxActions = Int(section, "CommitmentSpikeMaxActions", 100);
        CommitmentSpikeWindow = Seconds(section, "CommitmentSpikeWindowSeconds", 300);
        CommitmentSpikeAlertInterval = Seconds(section, "CommitmentSpikeAlertSeconds", 3600);
    }

    private static int Int(IConfigurationSection section, string key, int fallback)
        => int.TryParse(section[key], out var value) && value > 0 ? value : fallback;

    private static TimeSpan Seconds(IConfigurationSection section, string key, int fallback)
        => TimeSpan.FromSeconds(Int(section, key, fallback));
}
