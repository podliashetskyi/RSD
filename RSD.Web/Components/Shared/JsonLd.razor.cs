#pragma warning disable S1144, S4487, S2933
using Microsoft.AspNetCore.Components;

namespace RSD.Web.Components.Shared;

public partial class JsonLd
{
    [Parameter, EditorRequired] public string Json { get; set; } = "";

    private bool HasJson => Json.Length > 0;

    // Blazor SSR drops literal <script> elements inside components; emit as markup.
    // Safe: callers build Json via System.Text.Json, whose encoder escapes angle brackets.
    private MarkupString ScriptHtml => new($"<script type=\"application/ld+json\">{Json}</script>");
}
