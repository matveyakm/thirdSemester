// <copyright file="MessageReceiverTests.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace NetworkChat;

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Tests for the MessageReceiver class.
/// </summary>
public static class MessageReceiverTests
{
    /// <summary>
    /// Asynchronously receives messages from the specified stream.
    /// </summary>
    /// <param name="stream">The stream to read messages from.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync(Stream stream)
    {
        var buffer = new byte[4096];
        var sb = new StringBuilder();

        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0)
                {
                    Console.WriteLine("\nСобеседник отключился.");
                    break;
                }

                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                sb.Append(data);

                string content = sb.ToString();
                int newlineIndex;
                while ((newlineIndex = content.IndexOf('\n')) >= 0)
                {
                    string message = content.Substring(0, newlineIndex);
                    Console.WriteLine($"> {message.TrimEnd('\r', '\n')}");
                    content = content.Substring(newlineIndex + 1);
                }

                sb.Clear();
                sb.Append(content);
            }
        }
        catch
        {
            Console.WriteLine("\nСоединение разорвано.");
        }
    }
}