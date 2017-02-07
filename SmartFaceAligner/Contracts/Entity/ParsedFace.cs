using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;

namespace Contracts.Entity
{
    public class ParsedFace
    {
        public Face Face { get; set; }
        public IdentityPerson IdentityPerson { get; set; }
    }
}
