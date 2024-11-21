using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MaksIT.Core.Networking;

/// <summary>
/// Provides network-related utility methods.
/// </summary>
public static class PingPort {
  /// <summary>
  /// Tries to ping a host on a specified TCP port.
  /// </summary>
  /// <param name="hostUri">The host URI.</param>
  /// <param name="portNumber">The port number.</param>
  /// <param name="errorMessage">The error message if the operation fails.</param>
  /// <returns>True if the host is reachable on the specified port; otherwise, false.</returns>
  public static bool TryHostPort(string hostUri, int portNumber, out string? errorMessage) {
    if (string.IsNullOrEmpty(hostUri)) {
      errorMessage = "Host URI cannot be null or empty.";
      return false;
    }

    try {
      using (var client = new TcpClient()) {
        var result = client.BeginConnect(hostUri, portNumber, null, null);
        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
        if (!success) {
          errorMessage = "Connection timed out.";
          return false;
        }

        client.EndConnect(result);
        errorMessage = null;
        return true;
      }
    }
    catch (SocketException ex) {
      errorMessage = ex.Message;
      return false;
    }
    catch (Exception ex) {
      // Log or handle other exceptions as needed
      errorMessage = ex.Message;
      return false;
    }
  }

  /// <summary>
  /// Tries to ping a host on a specified UDP port.
  /// </summary>
  /// <param name="hostUri">The host URI.</param>
  /// <param name="portNumber">The port number.</param>
  /// <param name="errorMessage">The error message if the operation fails.</param>
  /// <returns>True if the host is reachable on the specified port; otherwise, false.</returns>
  public static bool TryUDPPort(string hostUri, int portNumber, out string? errorMessage) {
    if (string.IsNullOrEmpty(hostUri)) {
      errorMessage = "Host URI cannot be null or empty.";
      return false;
    }

    using (var udpClient = new UdpClient()) {
      try {
        udpClient.Connect(hostUri, portNumber);

        // Sends a message to the host to which you have connected.
        byte[] sendBytes = Encoding.ASCII.GetBytes("Is anybody there?");
        udpClient.Send(sendBytes, sendBytes.Length);

        // IPEndPoint object will allow us to read datagrams sent from any source.
        IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        // Set a receive timeout to avoid blocking indefinitely
        udpClient.Client.ReceiveTimeout = 5000;

        // Blocks until a message returns on this socket from a remote host.
        byte[] receiveBytes = udpClient.Receive(ref remoteIpEndPoint);
        string returnData = Encoding.ASCII.GetString(receiveBytes);

        errorMessage = null;
        return true;
      }
      catch (SocketException ex) {
        errorMessage = ex.Message;
        return false;
      }
      catch (Exception ex) {
        // Log or handle other exceptions as needed
        errorMessage = ex.Message;
        return false;
      }
    }
  }
}
