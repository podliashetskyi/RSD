using System.Net;

namespace RSD.Web.Services.Mcp;

/// <summary>
/// Local-only gate for the MCP endpoint. Host-side isolation comes from publishing the
/// MCP port only on 127.0.0.1; this gate is defense-in-depth: the endpoint answers only
/// on the dedicated MCP port (never the public listener) and only to loopback/private
/// remote addresses (in-container "loopback" is the Docker bridge, hence private ranges).
/// </summary>
internal static class McpRequestGate
{
    internal static bool ShouldAllow(int localPort, int configuredPort, IPAddress? remote)
    {
        if (configuredPort <= 0 || localPort != configuredPort || remote is null) return false;
        var ip = remote.IsIPv4MappedToIPv6 ? remote.MapToIPv4() : remote;
        return IPAddress.IsLoopback(ip) || IsPrivate(ip);
    }

    private static bool IsPrivate(IPAddress ip) =>
        ip.AddressFamily switch
        {
            System.Net.Sockets.AddressFamily.InterNetwork => IsPrivateV4(ip.GetAddressBytes()),
            System.Net.Sockets.AddressFamily.InterNetworkV6 => IsPrivateV6(ip.GetAddressBytes()),
            _ => false,
        };

    private static bool IsPrivateV4(byte[] b) =>
        b[0] == 10 || (b[0] == 172 && b[1] >= 16 && b[1] <= 31) || (b[0] == 192 && b[1] == 168);

    // fc00::/7 — IPv6 unique local addresses
    private static bool IsPrivateV6(byte[] b) => (b[0] & 0xFE) == 0xFC;
}
