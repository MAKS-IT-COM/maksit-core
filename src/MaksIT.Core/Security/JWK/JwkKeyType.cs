using MaksIT.Core.Abstractions;


namespace MaksIT.Core.Security.JWK;

public sealed class JwkKeyType : Enumeration {
  public static readonly JwkKeyType Rsa = new(1, "RSA");
  public static readonly JwkKeyType Ec = new(2, "EC");
  public static readonly JwkKeyType Oct = new(3, "oct");

  private JwkKeyType(int id, string name) : base(id, name) { }
}