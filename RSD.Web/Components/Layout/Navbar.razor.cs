#pragma warning disable S1144, S4487, S2933
namespace RSD.Web.Components.Layout;

public partial class Navbar
{
    private bool _isMenuOpen;

    private void ToggleMenu() => _isMenuOpen = !_isMenuOpen;
}
