// <copyright file="FileStorageService.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Services;

/// <summary>
/// Provides file storage operations for assembly files.
/// </summary>
public class FileStorageService : IFileStorage
{
    private readonly string uploadsDir;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageService"/> class.
    /// </summary>
    /// <param name="env">The web host environment.</param>
    public FileStorageService(IWebHostEnvironment env)
    {
        this.uploadsDir = Path.Combine(env.ContentRootPath, "Uploads");
        Directory.CreateDirectory(this.uploadsDir);
    }

    /// <inheritdoc />
    public async Task<string> SaveAssembliesAsync(IFormFileCollection files)
    {
        var runId = Guid.NewGuid().ToString("N");
        var runDir = Path.Combine(this.uploadsDir, runId);
        Directory.CreateDirectory(runDir);

        foreach (var file in files)
        {
            if (string.IsNullOrEmpty(file.FileName) || !file.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var targetPath = Path.Combine(runDir, file.FileName);
            await using var stream = new FileStream(targetPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        return runId;
    }

    /// <inheritdoc />
    public string? GetUploadsPath(string runId)
    {
        var path = Path.Combine(this.uploadsDir, runId);
        return Directory.Exists(path) ? path : null;
    }

    /// <inheritdoc />
    public bool RunDirectoryExists(string runId)
    {
        return Directory.Exists(Path.Combine(this.uploadsDir, runId));
    }
}
