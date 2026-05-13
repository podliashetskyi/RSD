#pragma warning disable S1144, S4487, S2933

using Microsoft.AspNetCore.Components;
using RSD.Web.Data.Entities;

namespace RSD.Web.Components.Admin.Shared;

public partial class StatusBadge : ComponentBase
{
    [Parameter, EditorRequired] public ContentStatus Status { get; set; }

    private string Label => Status.ToString();

    private string Classes => StatusClasses.GetValueOrDefault(Status, "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200");

    private static readonly Dictionary<ContentStatus, string> StatusClasses = new()
    {
        [ContentStatus.Draft] = "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300",
        [ContentStatus.Published] = "bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300",
        [ContentStatus.Archived] = "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300",
    };
}
