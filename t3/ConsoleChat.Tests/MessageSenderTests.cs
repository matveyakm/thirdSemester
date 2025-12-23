// <copyright file="MessageSenderTests.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace NetworkChat;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Tests for the MessageSender class.
/// </summary>
public static class MessageSenderTests
{
    /// <summary>
    /// Asynchronously reads input from the console and sends it to the specified stream.
    /// </summary>
    /// <param name="stream">The stream to which the input messages will be sent.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task RunAsync(Stream stream)
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