using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Contracts.Entity
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public List<IdentityPerson> IdentityPeople { get; set; }

        [JsonIgnore]
        public string FilePath { get; set; }
    }
}
