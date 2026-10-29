using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;
using Cobranca.Cidada.Domain.Modules.Voting.Enums;
using Microsoft.Extensions.Configuration;

namespace Cobranca.Cidada.Application.Common;

/// <summary>Regras de exibição de votos: as contagens são sempre públicas (os campos "Hidden" ficam falsos).</summary>
public class VotePolicy
{
    public const int DefaultMinimumPublicVotes = 20;
    public const string Disclaimer = "Opinião de pessoas com e-mail verificado. Não é pesquisa estatística.";

    public int MinimumPublicVotes { get; }

    public VotePolicy(IConfiguration configuration)
    {
        MinimumPublicVotes = int.TryParse(configuration["Voting:MinimumPublicVotes"], out var min) && min >= 1
            ? min
            : DefaultMinimumPublicVotes;
    }

    public VoteSummaryResponse Summarize(VoteTally? tally)
    {
        return new VoteSummaryResponse(
            tally?.Charges ?? 0,
            false,
            tally?.Good ?? 0,
            tally?.Bad ?? 0,
            false);
    }

    public static MyVoteResponse? ToMyVote(VoteView? view)
        => view is null ? null : new MyVoteResponse(RatingToText(view.Rating), view.Charge);

    public static string? RatingToText(VoteRating? rating) => rating switch
    {
        VoteRating.Good => "boa",
        VoteRating.Bad => "ma",
        _ => null
    };

    public static bool TryParseRating(string? text, out VoteRating? rating)
    {
        switch (text?.Trim().ToLowerInvariant())
        {
            case null or "":
                rating = null;
                return true;
            case "boa":
                rating = VoteRating.Good;
                return true;
            case "ma" or "má":
                rating = VoteRating.Bad;
                return true;
            default:
                rating = null;
                return false;
        }
    }
}
