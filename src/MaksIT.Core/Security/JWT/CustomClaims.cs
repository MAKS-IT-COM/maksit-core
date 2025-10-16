﻿using MaksIT.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaksIT.Core.Security.JWT;
public class CustomClaims : Enumeration {
  public static readonly CustomClaims AclEntry = new(1, "acl_entry");
  private CustomClaims(int id, string name) : base(id, name) { }
}
