// <copyright file="Server.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace NetworkChat;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Логика работы чата в режиме сервера.
/// </summary>
public static class Server
{
    /// <summary>
    /// Запускает сервер, ожидает подключение клиента и начинает двусторонний чат.
    /// </summary>
    /// <param name="port">Порт, на котором сервер будет принимать подключения.</param>
    /// <returns>Задача, завершающаяся по окончании работы сервера.</returns>
    public static async Task RunAsync(int port)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Сервер запущен. Ожидание подключения на порту {port}...");

        TcpClient client;
        try
        {
            client = await listener.AcceptTcpClientAsync();
        }
        finally
        {
            listener.Stop();
        }

        Console.WriteLine("Клиент подключился. Можно начинать чат. Для выхода введите 'exit'.");

        await ChatHandler.HandleAsync(client);
    }
}