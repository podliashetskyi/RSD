using RSD.Web.Data.Entities;

namespace RSD.Web.Services.Audit;

public record AuditQuery(
    string UserId = "",
    string UserEmail = "",
    string EntityType = "",
    AuditAction? Action = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = 50);
