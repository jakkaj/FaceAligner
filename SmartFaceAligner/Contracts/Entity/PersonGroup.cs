using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Contracts.Entity
{
    public class IdentityPerson
    {
        public Guid? PersonId { get; set; }
        public string PersonName { get; set; }

        [JsonIgnore]
        public string FolderPath { get; set; }
        [JsonIgnore]
        public List<FaceData> Faces { get; set; }
        [JsonIgnore]
        public Project Project { get; set; }
        
    }
}
