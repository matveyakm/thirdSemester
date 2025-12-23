// <copyright file="ChatOptionsTests.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ConsoleChat.Tests;

using System;
using NetworkChat;
using NUnit.Framework;

/// <summary>
/// Tests for the ChatOptions class.
/// </summary>
[TestFixture]
public class ChatOptionsTests
{
    /// <summary>
    /// Tests that the Parse method returns server mode when only the port is provided.
    /// </summary>
    [Test]
    public void Parse_OnlyPort_ReturnsServerMode()
    {
        var args = new[] { "--port", "8080" };
        var result = ChatOptions.Parse(args);

        Assert.That(result.IsServer, Is.True);
        Assert.That(result.Host, Is.Null);
        Assert.That(result.Port, Is.EqualTo(8080));
    }

    /// <summary>
    /// Tests that the Parse method returns client mode when both host and port are provided.
    /// </summary>
    [Test]
    public void Parse_HostAndPort_ReturnsClientMode()
    {
        var args = new[] { "--host", "127.0.0.1", "--port", "5000" };
        var result = ChatOptions.Parse(args);

        Assert.That(result.IsServer, Is.False);
        Assert.That(result.Host, Is.EqualTo("127.0.0.1"));
        Assert.That(result.Port, Is.EqualTo(5000));
    }

    /// <summary>
    /// Tests that the Parse method correctly handles short flags for host and port.
    /// </summary>
    [Test]
    public void Parse_ShortFlags_WorkCorrectly()
    {
        var args = new[] { "-h", "localhost", "-p", "12345" };
        var result = ChatOptions.Parse(args);

        Assert.That(result.Host, Is.EqualTo("localhost"));
        Assert.That(result.Port, Is.EqualTo(12345));
    }

    /// <summary>
    /// Tests that the Parse method throws an exception when no port is provided.
    /// </summary>
    [Test]
    public void Parse_MissingPort_Throws()
    {
        var args = Array.Empty<string>();
        Assert.Throws<ArgumentException>(() => ChatOptions.Parse(args));
    }

    /// <summary>
    /// Tests that the Parse method throws an exception when an invalid port is provided.
    /// </summary>
    [Test]
    public void Parse_InvalidPort_Throws()
    {
        var args = new[] { "--port", "999999" };
        Assert.Throws<ArgumentException>(() => ChatOptions.Parse(args));
    }

    /// <summary>
    /// Tests that the Parse method throws an exception when the host is empty.
    /// </summary>
    [Test]
    public void Parse_EmptyHost_Throws()
    {
        var args = new[] { "--host", string.Empty, "--port", "8080" };
        Assert.Throws<ArgumentException>(() => ChatOptions.Parse(args));
    }
}