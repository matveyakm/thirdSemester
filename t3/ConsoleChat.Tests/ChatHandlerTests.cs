// <copyright file="ChatHandlerTests.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ConsoleChat.Tests;

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkChat;
using NUnit.Framework;

/// <summary>
/// Tests for the ChatHandler class.
/// </summary>
[TestFixture]
public class ChatHandlerTests
{
    /// <summary>
    /// Проверяет, что HandleAsync успешно запускает обработку соединения
    /// и не бросает исключений при установке соединения между клиентом и сервером.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Test]
    public async Task HandleAsync_EstablishesConnectionWithoutExceptions()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var serverTask = Task.Run(async () =>
        {
            using var serverClient = await listener.AcceptTcpClientAsync();
            _ = ChatHandler.HandleAsync(serverClient);
        });

        await Task.Delay(50);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port);

        var clientHandleTask = ChatHandler.HandleAsync(client);

        await Task.Delay(200);

        listener.Stop();

        Assert.Pass("ChatHandler успешно запустил обработку соединения с обеих сторон.");
    }
}