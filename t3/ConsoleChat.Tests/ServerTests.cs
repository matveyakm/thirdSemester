// <copyright file="ServerTests.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ConsoleChat.Tests;

using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkChat;
using NUnit.Framework;

/// <summary>
/// Tests for the Server class.
/// </summary>
[TestFixture]
public class ServerTests
{
    private const int TestPort = 50001;

    /// <summary>
    /// Tests that the server accepts a client connection.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RunAsync_AcceptsClientConnection()
    {
        var serverTask = Server.RunAsync(TestPort);

        await Task.Delay(100);

        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", TestPort);

        Assert.That(client.Connected, Is.True);
    }
}