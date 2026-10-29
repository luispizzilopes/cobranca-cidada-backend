using ErrorOr;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Seo.Queries.GetSeoFragment;

public enum SeoFragmentPart
{
    Head,
    Body
}

/// <summary>Trecho de HTML para a rota <paramref name="Path"/> do site; vazio quando a página usa o padrão do <c>index.html</c>.</summary>
public record GetSeoFragmentQuery(string? Path, SeoFragmentPart Part) : IRequest<ErrorOr<string>>;
