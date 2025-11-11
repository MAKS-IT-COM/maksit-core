using MaksIT.Core.Abstractions;


namespace MaksIT.Core.Security.JWK;

public sealed class JwkCurve : Enumeration {
    public static readonly JwkCurve P256 = new(1, "P-256");
    public static readonly JwkCurve P384 = new(2, "P-384");
    public static readonly JwkCurve P521 = new(3, "P-521");

    private JwkCurve(int id, string name) : base(id, name) { }
}