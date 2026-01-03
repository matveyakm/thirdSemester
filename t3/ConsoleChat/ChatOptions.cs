// <copyright file="ChatOptions.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace NetworkChat;

using System;

/// <summary>
/// Парсер аргументов командной строки и вывод справки.
/// </summary>
public static class ChatOptions
{
    /// <summary>
    /// Парсит аргументы терминала и возвращает конфигурацию запуска.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    /// <returns>Объект с параметрами запуска.</returns>
    /// <exception cref="ArgumentException">При неверных аргументах.</exception>
    public static ChatOptionsResult Parse(string[] args)
    {
        string? host = null;
        int? port = null;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].ToLowerInvariant();

            if (arg is "--host" or "-h")
            {
                if (++i >= args.Length)
                {
                    throw new ArgumentException("Не указан хост после --host.");
                }

                host = args[i];
            }
            else if (arg is "--port" or "-p")
            {
                if (++i >= args.Length)
                {
                    throw new ArgumentException("Не указан порт после --port.");
                }

                if (!int.TryParse(args[i], out int p) || p < 1 || p > 65535)
                {
                    throw new ArgumentException("Порт должен быть числом от 1 до 65535.");
                }

                port = p;
            }
            else
            {
                throw new ArgumentException($"Неизвестный аргумент: {args[i]}");
            }
        }

        if (!port.HasValue)
        {
            throw new ArgumentException("Порт обязателен (--port).");
        }

        bool isServer = host == null;
        if (!isServer && string.IsNullOrWhiteSpace(host))
        {
            throw new ArgumentException("Хост не может быть пустым.");
        }

        return new ChatOptionsResult(isServer, host, port.Value);
    }

    /// <summary>
    /// Выводит справку по использованию программы.
    /// </summary>
    public static void PrintUsage()
    {
        Console.WriteLine();
        Console.WriteLine("Использование:");
        Console.WriteLine("  Как сервер: dotnet run -- --port <порт>");
        Console.WriteLine("  Как клиент: dotnet run -- --host <ip_или_хост> --port <порт>");
        Console.WriteLine();
        Console.WriteLine("Примеры:");
        Console.WriteLine("  dotnet run -- --port 8080");
        Console.WriteLine("  dotnet run -- --host 127.0.0.1 --port 8080");
    }

    /// <summary>
    /// Результат парсинга параметров запуска.
    /// </summary>
    public sealed class ChatOptionsResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatOptionsResult"/> class.
        /// Инициализирует новый экземпляр результата парсинга параметров.
        /// </summary>
        /// <param name="isServer">Признак режима сервера.</param>
        /// <param name="host">Хост (null для сервера).</param>
        /// <param name="port">Порт.</param>
        internal ChatOptionsResult(bool isServer, string? host, int port)
        {
            this.IsServer = isServer;
            this.Host = host;
            this.Port = port;
        }

        /// <summary>
        /// Gets a value indicating whether признак, что приложение должно запуститься как сервер.
        /// </summary>
        public bool IsServer { get; }

        /// <summary>
        /// Gets iP-адрес или имя хоста сервера (null для режима сервера).
        /// </summary>
        public string? Host { get; }

        /// <summary>
        /// Gets порт для подключения или прослушивания.
        /// </summary>
        public int Port { get; }
    }
}