using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFaceAligner.Processor.Entity
{
    public static class Constants
    {
        public static class CogServices
        {
            public const string PersonGroupPattern = "{0}_group";
            public const string DefaultPerson = "DefaultPerson";
        }

        public static class Cache
        {
            public const string Thumbnail = "Thumbnail";
            public const string FaceData = "FaceData";
        }
        public static class FileNames
        {
            public const string ProjectRoot = "faceProject.json";
            public const string FolderRoot = "faceFolder.json";
        }

        public static class Settings
        {
            public const string SubsKeys = "SubsKeys";
            public const string BingKeys = "BingKeys";
        }
    }
}
