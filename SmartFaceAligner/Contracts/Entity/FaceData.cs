using System;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;

namespace Contracts.Entity
{
    public class FaceData
    {
        public string FileName { get; set; }
        public string Hash { get; set; }
        public bool HasBeenScanned { get; set; }
        public Guid? PersistedFaceId { get; set; }
        public ParsedFace[] ParsedFaces { get; set; }
        public DateTime DateTaken { get; set; }

        [JsonIgnore]
        public Project Project { get; set; }
    }
}
