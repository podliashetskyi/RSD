namespace RSD.Web.Data;

/// <summary>
/// Single source of truth for every column length used by an admin input.
/// EF configurations (<see cref="Configurations"/>), input-model
/// <see cref="System.ComponentModel.DataAnnotations.StringLengthAttribute"/>s,
/// and the <c>maxlength</c> attribute on every admin <c>&lt;InputText&gt;</c>
/// / <c>&lt;InputTextArea&gt;</c> all read from here. Changes propagate to FE
/// + BE + schema in one place.
/// </summary>
public static class FieldLimits
{
    public const int Slug = 200;
    public const int Status = 20;

    public static class Seo
    {
        public const int MetaTitle = 200;
        public const int MetaDescription = 500;
        public const int OgImagePath = 500;
        public const int OgImageAlt = 200;
    }

    public static class BlogPost
    {
        public const int Title = 300;
        public const int Summary = 280;
        public const int Description = 2000;
        public const int Category = 100;
        public const int CoverImagePath = 500;
        public const int CoverImageAlt = 200;
        public const int Intro = 4000;
    }

    public static class Case
    {
        public const int Name = 300;
        public const int Summary = 280;
        public const int Industry = 100;
        public const int Description = 2000;
        public const int CoverImagePath = 500;
        public const int CoverImageAlt = 200;
    }

    public static class Product
    {
        public const int Name = 300;
        public const int Summary = 280;
        public const int Subtitle = 300;
        public const int Price = 100;
        public const int Description = 2000;
        public const int CoverImagePath = 500;
        public const int CoverImageAlt = 200;
        public const int TryForFreeHref = 500;
        public const int LearnMoreHref = 500;
    }

    public static class Service
    {
        public const int Title = 300;
        public const int Summary = 280;
        public const int Description = 2000;
        public const int CoverImagePath = 500;
        public const int CoverImageAlt = 200;
        public const int DetailsHref = 500;
        public const int Intro = 4000;
    }

    public static class Team
    {
        public const int Name = 200;
        public const int Role = 200;
        public const int AvatarPath = 500;
        public const int SocialUrl = 500;
        public const int Email = 320;
    }

    public static class Testimonial
    {
        public const int Title = 200;
        public const int Quote = 2000;
        public const int AvatarPath = 500;
        public const int AuthorName = 200;
        public const int AuthorRole = 200;
    }

    public static class Partner
    {
        public const int Name = 200;
        public const int Role = 200;
        public const int PhotoPath = 500;
        public const int ContactHref = 500;
    }

    public static class Value
    {
        public const int Title = 200;
        public const int Description = 1000;
        public const int IconPath = 500;
    }

    public static class TechStackItem
    {
        public const int Label = 100;
        public const int LogoPath = 500;
    }

    public static class SocialLink
    {
        public const int Label = 100;
        public const int IconPath = 500;
        public const int Href = 500;
        public const int Scope = 20;
    }

    public static class MessengerLink
    {
        public const int Label = 100;
        public const int LargeIconPath = 500;
        public const int SmallIconPath = 500;
        public const int BgColor = 20;
        public const int Href = 500;
    }

    public static class MissionStat
    {
        public const int Label = 200;
        public const int Number = 20;
        public const int Symbol = 5;
    }

    public static class ContactPoint
    {
        public const int Label = 100;
    }

    public static class TermsOfService
    {
        public const int Title = 200;
    }

    public static class PrivacyPolicy
    {
        public const int Title = 200;
    }

    public static class ContactSubmission
    {
        public const int Name = 200;
        public const int Email = 320;
        public const int Subject = 500;
        public const int Message = 8000;
    }

    public static class ProjectEstimate
    {
        public const int EnumLabel = 40;
        public const int ContactName = 200;
        public const int ContactEmail = 320;
        public const int Company = 200;
        public const int ProjectDescription = 8000;
    }

    public static class UploadedFile
    {
        public const int Path = 500;
        public const int OriginalName = 500;
        public const int ContentType = 100;
        public const int UploadedByUserId = 450;
    }

    public static class AuditLogEntry
    {
        public const int UserId = 450;
        public const int UserEmail = 320;
        public const int EntityType = 100;
        public const int Action = 20;
    }
}
