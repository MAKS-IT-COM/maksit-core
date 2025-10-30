using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace MaksIT.Core.Tests;

/// <summary>
/// Simple implementation of IHostEnvironment for testing purposes.
/// </summary>
public class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Production;
    public string ApplicationName { get; set; } = "";
    public string ContentRootPath { get; set; } = "";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}