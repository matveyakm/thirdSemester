// <copyright file="Client.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace NetworkChat;

using System;
using System.Net.Sockets;
using System.Threading.Tasks;

/// <summary>
/// Содержит логику работы чата в режиме клиента.
/// </summary>
internal static class Client
{
    /// <summary>
    /// Подключается к серверу и начинает двусторонний чат.
    /// </summary>
    /// <param name="host">IP-адрес или имя хоста сервера.</param>
    /// <param name="port">Порт сервера.</param>
    /// <returns>Задача, завершающаяся по окончании работы клиента.</returns>
    public static async Task RunAsync(string host, int port)
    {
        using var client = new TcpClient();

        Console.WriteLine($"Подключение к {host}:{port}...");
        await client.ConnectAsync(host, port);

        Console.WriteLine("Подключено к серверу. Можно начинать чат. Для выхода введите 'exit'.");

        await ChatHandler.HandleAsync(client);
    }
}