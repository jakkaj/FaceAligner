using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Contracts.Entity
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public string FilePath { get; set; }
    }
}
