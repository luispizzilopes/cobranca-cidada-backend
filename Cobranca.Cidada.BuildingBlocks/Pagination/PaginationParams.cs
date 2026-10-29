namespace Cobranca.Cidada.BuildingBlocks.Pagination;

public class PaginationParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int MaxSize { get; set; } = 200;

    public int FinalSize => PageSize > MaxSize ? MaxSize : PageSize;
}
