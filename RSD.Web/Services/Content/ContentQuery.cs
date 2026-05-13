using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Content;

public record ContentQuery(
    ContentStatus? Status = null,
    bool IncludeDeleted = false,
    string Search = "",
    int Page = 1,
    int PageSize = 50);
