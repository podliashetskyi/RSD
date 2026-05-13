namespace RSD.Web.Services.Content;

public enum ContactSubmissionFilter
{
    All,
    Open,
    Handled
}

public record ContactSubmissionQuery(
    ContactSubmissionFilter Filter = ContactSubmissionFilter.All,
    string Search = "",
    int Page = 1,
    int PageSize = 25);
