using Microsoft.ProjectOxford.Face.Contract;

namespace SmartFaceAligner.Processor.Services.FaceSmarts
{
    public static class FaceFilter
    {

        public static bool SimpleCheck(Face originalFace, Face currentFace, double maxAgeGap = 3)
        {
            var post = currentFace.FaceAttributes.HeadPose;
            var orig = originalFace.FaceAttributes.HeadPose;

            if (!_isGood(post.Pitch, orig.Pitch) || !_isGood(post.Roll, orig.Roll) || !_isGood(post.Yaw, orig.Yaw))
            {
                return false;
            }

            var age = originalFace.FaceAttributes.Age;
            var sex = originalFace.FaceAttributes.Gender;

            var ageCompare = currentFace.FaceAttributes.Age - age;
            var ageCompare2 = age - currentFace.FaceAttributes.Age;

            return ((ageCompare < maxAgeGap && ageCompare > 0) || (ageCompare2 < maxAgeGap && ageCompare2 > 0)) && currentFace.FaceAttributes.Gender == sex;
        }

        const int angleLimit = 30;

        static bool _isGood(double compare, double item)
        {
            if (compare > item && compare > item + angleLimit)
            {
                return false;
            }

            if (compare < item && compare < item - angleLimit)
            {
                return false;
            }

            return true;
        }
    }
}
