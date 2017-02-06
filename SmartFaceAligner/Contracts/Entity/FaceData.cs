using Microsoft.ProjectOxford.Face.Contract;

namespace Contracts.Entity
{
    public class FaceData
    {
        public string FileName { get; set; }
        public bool? HasFace { get; set; }

        public Face Face { get; set; }
    }
}
