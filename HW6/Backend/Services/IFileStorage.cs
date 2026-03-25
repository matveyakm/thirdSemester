// <copyright file="IFileStorage.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Services;

/// <summary>
/// Defines methods for file storage operations.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Saves the uploaded assembly files.
    /// </summary>
    /// <param name="files">The files to save.</param>
    /// <returns>The run ID.</returns>
    Task<string> SaveAssembliesAsync(IFormFileCollection files);

    /// <summary>
    /// Gets the uploads path for a specific run.
    /// </summary>
    /// <param name="runId">The run ID.</param>
    /// <returns>The path, or null if not found.</returns>
    string? GetUploadsPath(string runId);

    /// <summary>
    /// Checks if the run directory exists.
    /// </summary>
    /// <param name="runId">The run ID.</param>
    /// <returns>True if exists, otherwise false.</returns>
    bool RunDirectoryExists(string runId);
}
