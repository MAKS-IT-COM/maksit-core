using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MaksIT.Core.Security.JWK;
public class OrderedJwk {
  [JsonPropertyName("e")]
  public string E { get; set; } = default!;

  [JsonPropertyName("kty")]
  public string Kty { get; set; } = default!;

  [JsonPropertyName("n")]
  public string N { get; set; } = default!;
}
