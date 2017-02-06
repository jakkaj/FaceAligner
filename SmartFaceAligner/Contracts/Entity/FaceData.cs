using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;

namespace Contracts.Entity
{
    public class FaceData
    {
        public string FileName { get; set; }
        public bool? HasFace { get; set; }

        public Face Face { get; set; }

        [JsonIgnore]
        public Project Project { get; set; }
    }
}
