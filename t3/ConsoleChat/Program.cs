// <copyright file="Program.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace NetworkChat;

using System;
using System.Threading.Tasks;

/// <summary>
/// Определяет режим работы (сервер или клиент) по аргументам командной строки.
/// </summary>
public class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var options = ChatOptions.Parse(args);

            if (options.IsServer)
            {
                await Server.RunAsync(options.Port);
            }
            else
            {
                await Client.RunAsync(options.Host!, options.Port);
            }
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            Console.WriteLine($"Ошибка в параметрах: {ex.Message}");
            ChatOptions.PrintUsage();
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
            Environment.Exit(1);
        }
    }
}