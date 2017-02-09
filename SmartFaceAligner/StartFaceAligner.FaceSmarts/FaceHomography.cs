using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

/// <summary>
/// Jordan Knight @jakkaj https://github.com/jakkaj
/// </summary>

namespace StartFaceAligner.FaceSmarts
{
    public static class FaceHomography
    {

        public static List<PointF> GetPoints(ImageAligner.ProcessFaceData face)
        {
            List<PointF> points = new List<PointF>();

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.PupilLeft.X,
                (float)face.ParsedFace.FaceLandmarks.PupilLeft.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.PupilRight.X,
                (float)face.ParsedFace.FaceLandmarks.PupilRight.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeLeftBottom.X,
              (float)face.ParsedFace.FaceLandmarks.EyeLeftBottom.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeRightBottom.X,
                (float)face.ParsedFace.FaceLandmarks.EyeRightBottom.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeRightOuter.X,
                (float)face.ParsedFace.FaceLandmarks.EyeRightOuter.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeLeftOuter.X,
                (float)face.ParsedFace.FaceLandmarks.EyeLeftOuter.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeLeftInner.X,
                (float)face.ParsedFace.FaceLandmarks.EyeLeftInner.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeLeftTop.X,
                (float)face.ParsedFace.FaceLandmarks.EyeLeftTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeRightTop.X,
                (float)face.ParsedFace.FaceLandmarks.EyeRightTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeRightInner.X,
                (float)face.ParsedFace.FaceLandmarks.EyeRightInner.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyebrowLeftInner.X,
                (float)face.ParsedFace.FaceLandmarks.EyebrowLeftInner.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyebrowLeftOuter.X,
                (float)face.ParsedFace.FaceLandmarks.EyebrowLeftOuter.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyebrowRightInner.X,
                (float)face.ParsedFace.FaceLandmarks.EyebrowRightInner.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyebrowRightOuter.X,
                (float)face.ParsedFace.FaceLandmarks.EyebrowRightOuter.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.UnderLipTop.X,
              (float)face.ParsedFace.FaceLandmarks.UnderLipTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.UnderLipBottom.X,
                (float)face.ParsedFace.FaceLandmarks.UnderLipBottom.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.UpperLipBottom.X,
                (float)face.ParsedFace.FaceLandmarks.UpperLipBottom.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.UpperLipTop.X,
                (float)face.ParsedFace.FaceLandmarks.UpperLipTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.MouthRight.X,
                (float)face.ParsedFace.FaceLandmarks.MouthRight.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.MouthLeft.X,
                (float)face.ParsedFace.FaceLandmarks.MouthLeft.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceRectangle.Left,
                (float)face.ParsedFace.FaceRectangle.Top));

            points.Add(new PointF((float)face.ParsedFace.FaceRectangle.Left + face.ParsedFace.FaceRectangle.Width,
                (float)face.ParsedFace.FaceRectangle.Top + face.ParsedFace.FaceRectangle.Height));




            

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseTip.X,
                 (float)face.ParsedFace.FaceLandmarks.NoseTip.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseLeftAlarTop.X,
                (float)face.ParsedFace.FaceLandmarks.NoseLeftAlarTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseRightAlarTop.X,
                (float)face.ParsedFace.FaceLandmarks.NoseRightAlarTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseLeftAlarOutTip.X,
                (float)face.ParsedFace.FaceLandmarks.NoseLeftAlarOutTip.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseRightAlarOutTip.X,
               (float)face.ParsedFace.FaceLandmarks.NoseRightAlarOutTip.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseRootLeft.X,
                (float)face.ParsedFace.FaceLandmarks.NoseRootLeft.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseRootRight.X,
                (float)face.ParsedFace.FaceLandmarks.NoseRootRight.Y));



            return points;
        }

        public static List<PointF> GetPointsFeatures(ImageAligner.ProcessFaceData face)
        {
            List<PointF> points = new List<PointF>();

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.PupilLeft.X,
                (float)face.ParsedFace.FaceLandmarks.PupilLeft.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.PupilRight.X,
                (float)face.ParsedFace.FaceLandmarks.PupilRight.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeLeftBottom.X,
              (float)face.ParsedFace.FaceLandmarks.EyeLeftBottom.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeRightBottom.X,
                (float)face.ParsedFace.FaceLandmarks.EyeRightBottom.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeRightOuter.X,
                (float)face.ParsedFace.FaceLandmarks.EyeRightOuter.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeLeftOuter.X,
                (float)face.ParsedFace.FaceLandmarks.EyeLeftOuter.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeLeftInner.X,
                (float)face.ParsedFace.FaceLandmarks.EyeLeftInner.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeLeftTop.X,
                (float)face.ParsedFace.FaceLandmarks.EyeLeftTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeRightTop.X,
                (float)face.ParsedFace.FaceLandmarks.EyeRightTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyeRightInner.X,
                (float)face.ParsedFace.FaceLandmarks.EyeRightInner.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyebrowLeftInner.X,
                (float)face.ParsedFace.FaceLandmarks.EyebrowLeftInner.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyebrowLeftOuter.X,
                (float)face.ParsedFace.FaceLandmarks.EyebrowLeftOuter.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyebrowRightInner.X,
                (float)face.ParsedFace.FaceLandmarks.EyebrowRightInner.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.EyebrowRightOuter.X,
                (float)face.ParsedFace.FaceLandmarks.EyebrowRightOuter.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.UnderLipTop.X,
              (float)face.ParsedFace.FaceLandmarks.UnderLipTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.UnderLipBottom.X,
                (float)face.ParsedFace.FaceLandmarks.UnderLipBottom.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.UpperLipBottom.X,
                (float)face.ParsedFace.FaceLandmarks.UpperLipBottom.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.UpperLipTop.X,
                (float)face.ParsedFace.FaceLandmarks.UpperLipTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.MouthRight.X,
                (float)face.ParsedFace.FaceLandmarks.MouthRight.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.MouthLeft.X,
                (float)face.ParsedFace.FaceLandmarks.MouthLeft.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceRectangle.Left,
                (float)face.ParsedFace.FaceRectangle.Top));

            points.Add(new PointF((float)face.ParsedFace.FaceRectangle.Left + face.ParsedFace.FaceRectangle.Width,
                (float)face.ParsedFace.FaceRectangle.Top + face.ParsedFace.FaceRectangle.Height));


            




            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseTip.X,
                 (float)face.ParsedFace.FaceLandmarks.NoseTip.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseLeftAlarTop.X,
                (float)face.ParsedFace.FaceLandmarks.NoseLeftAlarTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseRightAlarTop.X,
                (float)face.ParsedFace.FaceLandmarks.NoseRightAlarTop.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseLeftAlarOutTip.X,
                (float)face.ParsedFace.FaceLandmarks.NoseLeftAlarOutTip.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseRightAlarOutTip.X,
               (float)face.ParsedFace.FaceLandmarks.NoseRightAlarOutTip.Y));

            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseRootLeft.X,
                (float)face.ParsedFace.FaceLandmarks.NoseRootLeft.Y));


            points.Add(new PointF((float)face.ParsedFace.FaceLandmarks.NoseRootRight.X,
                (float)face.ParsedFace.FaceLandmarks.NoseRootRight.Y));


          


            return points;
        }

        public static UMat MapFaces(ImageAligner.ProcessFaceData face1, ImageAligner.ProcessFaceData face2)
        {
            var points = GetPoints(face1);
            var points2 = GetPoints(face2);



            var mOutputHomog = new UMat();


            CvInvoke.FindHomography(points2.ToArray(), points.ToArray(), mOutputHomog, HomographyMethod.Default,3D, null);

            return mOutputHomog;


        }



        public static UMat Test()
        {
            List<PointF> points = new List<PointF>();
            //face7
            points.Add(new PointF(532, 895));
            points.Add(new PointF(843, 919));
            points.Add(new PointF(704, 1136));
            points.Add(new PointF(541, 1185));
            points.Add(new PointF(843, 1212));

            List<PointF> points3 = new List<PointF>();
            //face7
            points3.Add(new PointF(532, 896));
            points3.Add(new PointF(843, 929));
            points3.Add(new PointF(704, 1146));
            points3.Add(new PointF(541, 1185));
            points3.Add(new PointF(843, 1212));

            List<PointF> points2 = new List<PointF>();

            //face4
            //points2.Add(new PointF(1701, 619));
            //points2.Add(new PointF(1859, 606));
            //points2.Add(new PointF(1789, 712));
            //points2.Add(new PointF(1702, 765));
            //points2.Add(new PointF(1865, 753));

            //face3
            points2.Add(new PointF(369, 921));
            points2.Add(new PointF(766, 951));
            points2.Add(new PointF(609, 1239));
            points2.Add(new PointF(309, 1259));
            points2.Add(new PointF(696, 1277));

            var mOutputHomog = new UMat();


            var v = new VectorOfKeyPoint();


            var pArray = points.ToArray();
            var pArray2 = points2.ToArray();

            float[,] sourcePoints = { { 30, 470 }, { 620, 370 }, { 610, 130 }, { 30, 1 } };

            float[,] destPoints = { { 30, 470 }, { 620, 470 }, { 610, 30 }, { 30, 1 } };
            Emgu.CV.Matrix<float> sourceMat = new Matrix<float>(sourcePoints);
            Emgu.CV.Matrix<float> destMat = new Matrix<float>(destPoints);


            //CvInvoke.FindHomography(points.ToArray(), points.ToArray(), mOutputHomog, HomographyMethod.Default, 3D, null);
            CvInvoke.FindHomography(points.ToArray(), points2.ToArray(), mOutputHomog, HomographyMethod.Default, 3D, null);

            return mOutputHomog;
        }
    }
}
