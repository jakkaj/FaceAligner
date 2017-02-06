using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;

namespace StartFaceAligner.FaceSmarts
{
    public class ImageAligner
    {
        public async Task<byte[]> AlignImages(byte[] baseImage, byte[] newImage, Face baseFace, Face targetFace)
        {
            var faceDataBase = new ProcessFaceData
            {
                FaceImage = baseImage,
                ParsedFace = baseFace
            };

            var faceDataCompare = new ProcessFaceData
            {
                FaceImage = newImage,
                ParsedFace = targetFace
            };

            var result = DrawMatches.Align(faceDataBase, faceDataCompare);

            return result?.Item1;
        }

        public  class ProcessFaceData
        {
            public Microsoft.ProjectOxford.Face.Contract.Face ParsedFace { get; set; }
            public byte[] FaceImage { get; set; }
        }
    }
}
