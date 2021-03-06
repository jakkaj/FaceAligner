﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFaceAligner.Processor.Entity
{
    public static class Constants
    {
        public static class Filters
        {
            public const string Female = "female";
            public const string Male = "male";
        }

        public static class Errors
        {
            public const string RateLimitExceeded = "RateLimitExceeded";
        }
        public static class CogServices
        {
            public const string PersonGroupPattern = "{0}_group";
            public const string DefaultPerson = "DefaultPerson";
        }

        public static class Cache
        {
            public const string Thumbnail = "Thumbnail";
            public const string FaceData = "FaceData";
            public const string HashCache = "HashCache";

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
            public const string LastProject = "LastProject";
            public const string Ffmpeg = "Ffmpeg";
        }
    }
}
