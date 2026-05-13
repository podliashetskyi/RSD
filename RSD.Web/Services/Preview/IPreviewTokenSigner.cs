using RSD.Web.Services.Common;

namespace RSD.Web.Services.Preview;

public interface IPreviewTokenSigner
{
    string Sign(PreviewClaims claims);
    Result<PreviewClaims> Verify(string token);
}
