using RSD.Web.Data.Entities;
using RSD.Web.Data.Entities.ArticleBlocks;

namespace RSD.Web.Components.Admin.Shared.BlockRows;

public static class BlockRowFactory
{
    public static IBlockRow Create(BlockKind kind)
    {
        var id = NewId();
        return kind switch
        {
            BlockKind.Subsection => new SubsectionRow { Id = id },
            BlockKind.StatsRow => new StatsRowRow { Id = id },
            BlockKind.Gallery => new GalleryRow { Id = id },
            BlockKind.BulletList => new BulletListRow { Id = id },
            BlockKind.Quote => new QuoteRow { Id = id },
            BlockKind.Image => new ImageRow { Id = id },
            BlockKind.RichText => new RichTextRow { Id = id },
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    public static IBlockRow From(ArticleBlock block) => block switch
    {
        SubsectionBlock s => SubsectionRow.From(s),
        StatsRowBlock st => StatsRowRow.From(st),
        GalleryBlock g => GalleryRow.From(g),
        BulletListBlock bl => BulletListRow.From(bl),
        QuoteBlock q => QuoteRow.From(q),
        ImageBlock i => ImageRow.From(i),
        RichTextBlock rt => RichTextRow.From(rt),
        _ => throw new ArgumentOutOfRangeException(nameof(block)),
    };

    private static string NewId() => $"b-{Guid.NewGuid():N}"[..10];
}
