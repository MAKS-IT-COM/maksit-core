namespace MaksIT.Core.Cli;


public static class Program {
  public static int Main(string[] args) {
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    return CommandFactory.CreateRootCommand().Parse(args).Invoke();
  }
}
