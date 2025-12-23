// <copyright file="MessageSender.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace NetworkChat;

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Чтение ввода и отправка сообщений.
/// </summary>
internal static class MessageSender
{
    /// <summary>
    /// Асинхронно читает ввод и отправляет сообщения.
    /// </summary>
    /// <param name="stream">Сетевой поток.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task RunAsync(NetworkStream stream)
    {
        while (true)
        {
            string? input = Console.ReadLine();

            if (input == null)
            {
                break;
            }

            if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            string message = input + "\n";
            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data);
            await stream.FlushAsync();
        }
    }
}