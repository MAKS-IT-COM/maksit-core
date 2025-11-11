using MaksIT.Core.Abstractions;


namespace MaksIT.Core.Security.JWK;

public sealed class JwkAlgorithm : Enumeration {
  public static readonly JwkAlgorithm Rs256 = new(1, "RS256");
  public static readonly JwkAlgorithm Rs512 = new(2, "RS512");
  public static readonly JwkAlgorithm Es256 = new(3, "ES256");
  public static readonly JwkAlgorithm Es384 = new(4, "ES384");
  public static readonly JwkAlgorithm Es512 = new(5, "ES512");
  public static readonly JwkAlgorithm A128Gcm = new(6, "A128GCM");
  public static readonly JwkAlgorithm A256Gcm = new(7, "A256GCM");
  public static readonly JwkAlgorithm A512Gcm = new(8, "A512GCM");

  private JwkAlgorithm(int id, string name) : base(id, name) { }
}