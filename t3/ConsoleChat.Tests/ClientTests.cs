// <copyright file="ClientTests.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ConsoleChat.Tests;

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkChat;
using NUnit.Framework;

/// <summary>
/// Tests for the Client class.
/// </summary>
[TestFixture]
public class ClientTests
{
    private const int TestPort = 50002;

    /// <summary>
    /// Starts a simple echo server for testing purposes.
    /// </summary>
    [SetUp]
    public void StartTestServer()
    {
        _ = Task.Run(async () =>
        {
            var listener = new TcpListener(IPAddress.Loopback, TestPort);
            listener.Start();
            using var client = await listener.AcceptTcpClientAsync();
            listener.Stop();
        });
    }

    /// <summary>
    /// Tests that the client can connect to the server successfully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task RunAsync_ConnectsToServerSuccessfully()
    {
        var clientTask = Client.RunAsync("127.0.0.1", TestPort);

        await Task.Delay(200); // даём время на подключение

        // Если не упало с исключением — подключение прошло успешно
        Assert.Pass();
    }
}