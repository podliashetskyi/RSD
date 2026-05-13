using Ganss.Xss;

namespace RSD.Web.Services.Imaging;

public sealed class SvgSanitizer
{
    private readonly HtmlSanitizer Inner = BuildSanitizer();

    public string Sanitize(string svg) => Inner.Sanitize(svg);

    private static HtmlSanitizer BuildSanitizer()
    {
        var s = new HtmlSanitizer();
        s.AllowedTags.Clear();
        s.AllowedAttributes.Clear();
        s.AllowedCssProperties.Clear();
        s.AllowedSchemes.Clear();
        s.AllowedSchemes.Add("data");

        foreach (var tag in AllowedSvgTags) s.AllowedTags.Add(tag);
        foreach (var attr in AllowedSvgAttributes) s.AllowedAttributes.Add(attr);
        return s;
    }

    private static readonly string[] AllowedSvgTags =
    [
        "svg", "g", "title", "desc", "defs", "symbol", "use",
        "path", "rect", "circle", "ellipse", "line", "polyline", "polygon",
        "text", "tspan", "textpath",
        "linearGradient", "radialGradient", "stop",
        "clipPath", "mask", "pattern",
    ];

    private static readonly string[] AllowedSvgAttributes =
    [
        "id", "class", "style",
        "viewbox", "width", "height", "preserveaspectratio",
        "x", "y", "x1", "y1", "x2", "y2", "cx", "cy", "r", "rx", "ry",
        "d", "points", "transform", "transform-origin",
        "fill", "fill-opacity", "fill-rule", "stroke", "stroke-width",
        "stroke-linecap", "stroke-linejoin", "stroke-opacity", "stroke-dasharray",
        "stop-color", "stop-opacity", "offset",
        "gradientunits", "gradienttransform", "spreadmethod",
        "clip-path", "mask", "opacity",
        "xmlns", "xmlns:xlink", "xlink:href", "href",
    ];
}
