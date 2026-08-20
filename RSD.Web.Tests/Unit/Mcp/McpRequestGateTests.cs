using System.Net;
using FluentAssertions;
using RSD.Web.Services.Mcp;

namespace RSD.Web.Tests.Unit.Mcp;

/// <summary>
/// The MCP endpoint must be unreachable through the public listener and from
/// non-local networks: requests pass only on the dedicated MCP port from
/// loopback or private (Docker-bridge) addresses.
/// </summary>
public sealed class McpRequestGateTests
{
    private const int McpPort = 8081;

    [Fact]
    public void WrongLocalPort_IsBlocked_EvenFromLoopback()
    {
        McpRequestGate.ShouldAllow(localPort: 8080, configuredPort: McpPort, IPAddress.Loopback).Should().BeFalse();
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    [InlineData("172.18.0.1")]     // Docker bridge gateway
    [InlineData("192.168.1.50")]
    [InlineData("10.0.0.9")]
    [InlineData("::ffff:172.18.0.1")]
    public void McpPort_FromLoopbackOrPrivateAddress_IsAllowed(string remote)
    {
        McpRequestGate.ShouldAllow(McpPort, McpPort, IPAddress.Parse(remote)).Should().BeTrue();
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("203.0.113.7")]
    public void McpPort_FromPublicAddress_IsBlocked(string remote)
    {
        McpRequestGate.ShouldAllow(McpPort, McpPort, IPAddress.Parse(remote)).Should().BeFalse();
    }

    [Fact]
    public void MissingRemoteAddress_OrUnconfiguredPort_IsBlocked()
    {
        McpRequestGate.ShouldAllow(McpPort, McpPort, null).Should().BeFalse();
        McpRequestGate.ShouldAllow(McpPort, configuredPort: 0, IPAddress.Loopback).Should().BeFalse();
    }
}
