#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Sections.Article;

public partial class ArticleBodyView : ComponentBase
{
    [Parameter, EditorRequired] public ArticleBody Body { get; set; } = new();
}
