using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;


namespace MaksIT.Core.Networking.Windows;

public class NetworkConnection : IDisposable {
  private readonly ILogger<NetworkConnection> _logger;
  private readonly string _networkName;

  private NetworkConnection(ILogger<NetworkConnection> logger, string networkName) {
    _logger = logger;
    _networkName = networkName;
  }

  public static bool TryCreate(
    ILogger<NetworkConnection> logger,
    string networkName,
    NetworkCredential credentials,
    [NotNullWhen(true)] out NetworkConnection? networkConnection,
    [NotNullWhen(false)] out string? errorMessage
  ) {
    try {
      if (!OperatingSystem.IsWindows()) {
        throw new PlatformNotSupportedException("NetworkConnection is only supported on Windows.");
      }

      if (logger == null) throw new ArgumentNullException(nameof(logger));
      if (networkName == null) throw new ArgumentNullException(nameof(networkName));
      if (credentials == null) throw new ArgumentNullException(nameof(credentials));

      var netResource = new NetResource {
        Scope = ResourceScope.GlobalNetwork,
        ResourceType = ResourceType.Disk,
        DisplayType = ResourceDisplayType.Share,
        RemoteName = networkName
      };

      var result = WNetAddConnection2(netResource, credentials.Password, credentials.UserName, 0);

      if (result != 0) {
        throw new InvalidOperationException($"Error connecting to remote share: {result}");
      }

      networkConnection = new NetworkConnection(logger, networkName);
      errorMessage = null;
      return true;
    }
    catch (Exception ex) {
      networkConnection = null;
      errorMessage = ex.Message;
      return false;
    }
  }

  ~NetworkConnection() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    if (OperatingSystem.IsWindows()) {
      WNetCancelConnection2(_networkName, 0, true);
    }
  }

  [DllImport("mpr.dll")]
  private static extern int WNetAddConnection2(NetResource netResource, string? password, string? username, int flags);

  [DllImport("mpr.dll")]
  private static extern int WNetCancelConnection2(string name, int flags, bool force);

  [StructLayout(LayoutKind.Sequential)]
  public class NetResource {
    public ResourceScope Scope;
    public ResourceType ResourceType;
    public ResourceDisplayType DisplayType;
    public int Usage;
    public string? LocalName;
    public string? RemoteName;
    public string? Comment;
    public string? Provider;
  }

  public enum ResourceScope : int {
    Connected = 1,
    GlobalNetwork,
    Remembered,
    Recent,
    Context
  }

  public enum ResourceType : int {
    Any = 0,
    Disk = 1,
    Print = 2,
    Reserved = 8
  }

  public enum ResourceDisplayType : int {
    Generic = 0x0,
    Domain = 0x01,
    Server = 0x02,
    Share = 0x03,
    File = 0x04,
    Group = 0x05,
    Network = 0x06,
    Root = 0x07,
    Shareadmin = 0x08,
    Directory = 0x09,
    Tree = 0x0a,
    Ndscontainer = 0x0b
  }
}
