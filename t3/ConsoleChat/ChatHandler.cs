// <copyright file="ChatHandler.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace NetworkChat;

using System.Net.Sockets;
using System.Threading.Tasks;

/// <summary>
/// Обработчик двустороннего чата. общий для и сервера и для клиента.
/// </summary>
internal static class ChatHandler
{
    /// <summary>
    /// Запускает задачи отправки и приёма одновременно.
    /// </summary>
    /// <param name="client">Подключённый <see cref="TcpClient"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task HandleAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();

        var receiveTask = MessageReceiver.RunAsync(stream);
        var sendTask = MessageSender.RunAsync(stream);

        await Task.WhenAny(receiveTask, sendTask);

        stream.Close();
        client.Close();
    }
}