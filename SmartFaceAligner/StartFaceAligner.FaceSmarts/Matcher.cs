//----------------------------------------------------------------------------
//  Copyright (C) 2004-2014 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace StartFaceAligner.FaceSmarts
{
    public static class DrawMatches
    {
        public static void FindMatch(UMat modelImage,
            UMat observedImage,
            out long matchTime,
            out VectorOfKeyPoint modelKeyPoints,
            out VectorOfKeyPoint observedKeyPoints,
            VectorOfVectorOfDMatch matches,
            out Mat mask,
            out Mat homography,
            Feature2D surfCPU = null,
            int k = 2, double uniquenessThreshold = 0.8, double hessianThresh = 280)
        {

            if (surfCPU == null)
            {
                surfCPU = new ORBDetector(2400, 1.2f, 10, 31, 0, 2, ORBDetector.ScoreType.Harris, 31, 20);
            }

            Stopwatch watch;
            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();

            

            using (UMat uModelImage = modelImage)
            using (UMat uObservedImage = observedImage)
            {

                UMat modelDescriptors = new UMat();
                surfCPU.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);

                watch = Stopwatch.StartNew();

                // extract features from the observed image

                /*BFMatcher::BFMatcher(int normType=NORM_L2, bool crossCheck=false )
Parameters:	
normType � One of NORM_L1, NORM_L2, NORM_HAMMING, NORM_HAMMING2. L1 and L2 norms are preferable choices for SIFT and SURF descriptors, NORM_HAMMING should be used with ORB, BRISK and BRIEF, NORM_HAMMING2 should be used with ORB when WTA_K==3 or 4 (see ORB::ORB constructor description).
crossCheck � If it is false, this is will be default BFMatcher behaviour when it finds the k nearest neighbors for each query descriptor. If crossCheck==true, then the knnMatch() method with k=1 will only return pairs (i,j) such that for i-th query descriptor the j-th descriptor in the matcher�s collection is the nearest and vice versa, i.e. the BFMatcher will only return consistent pairs. Such technique usually produces best results with minimal number of outliers when there are enough matches. This is alternative to the ratio test, used by D. Lowe in SIFT paper.
*/

                UMat observedDescriptors = new UMat();
                surfCPU.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
                var o = observedDescriptors.GetOutputArray();
                //modelKeyPoints = _extract(modelKeyPoints);
                //observedKeyPoints = _extract()

                BFMatcher matcher = new BFMatcher(DistanceType.Hamming);
                matcher.Add(modelDescriptors);

                matcher.KnnMatch(observedDescriptors, matches, k, null);

                mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));
                Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                int nonZeroCount = CvInvoke.CountNonZero(mask);
                if (nonZeroCount >= 4)
                {
                    //seems that scale increment small and rotations bits large is good. 
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                       matches, mask, 1.01, 60);
                    if (nonZeroCount >= 4)
                    {
                       
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                          observedKeyPoints, matches, mask, 2);
                    }
                       
                }

                watch.Stop();

            }
            matchTime = watch.ElapsedMilliseconds;
        }

        static VectorOfKeyPoint _extract(VectorOfKeyPoint cp)
        {
            var l = new List<MKeyPoint>();

            

            for (var i = 0; i < cp.Size; i++)
            {
                var current = cp[i];

                var n = new MKeyPoint
                {
                    Angle = 0,
                    ClassId = current.ClassId,
                    Octave = current.Octave,
                    Point = new Point(1, 1),
                    Response = 0,
                    Size = current.Size
                };
                l.Add(n);
            }

            var nv = new VectorOfKeyPoint();
            nv.Push(l.ToArray());
            return nv;
        }

        /// <summary>
        /// Draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="modelImage">The model image</param>
        /// <param name="observedImage">The observed image</param>
        /// <param name="matchTime">The output total time for computing the homography matrix.</param>
        /// <returns>The model image and observed image, the matched features and homography projection.</returns>
        public static Mat Draw(UMat modelImage,
           UMat observedImage,
           out long matchTime,
           out Mat homography,
           Feature2D surfCPU = null,
           int k = 3, double uniquenessThreshold = 0.8, double hessianThresh = 280)
        {

            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {

                Mat mask;

                FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                   out mask, out homography, surfCPU, k, uniquenessThreshold, hessianThresh);

                //Draw the matched keypoints
                Mat result = new Mat();
               // Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                 //  matches, result, new MCvScalar(255, 0, 0), new MCvScalar(255, 0, 0), mask);

                #region draw the projected region on the image

               // if (homography != null)
               // {
               //     //draw a rectangle along the projected model
               //     Rectangle rect = modelImage.ROI;
               //     PointF[] pts = new PointF[]
               //{
               //   new PointF(rect.Left, rect.Bottom),
               //   new PointF(rect.Right, rect.Bottom),
               //   new PointF(rect.Right, rect.Top),
               //   new PointF(rect.Left, rect.Top)
               //};
               //     pts = CvInvoke.PerspectiveTransform(pts, homography);

               //     Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
               //     using (VectorOfPoint vp = new VectorOfPoint(points))
               //     {
               //         CvInvoke.Polylines(result, vp, true, new MCvScalar(255, 0, 0, 255), 5);
               //     }
               //     //result.DrawPolyline(, true, new Bgr(Color.Red), 5);
               // }

                #endregion

                return result;

            }
        }
        public static Tuple<byte[], byte[]> Align(ImageAligner.ProcessFaceData baseFace, ImageAligner.ProcessFaceData newFace, Feature2D surfCPU = null,
           int k = 3, double uniquenessThreshold = 0.8, double hessianThresh = 280)
        {

            long time;
            UMat homography;

            var tempFile1 = new FileInfo(Path.GetTempFileName());
            var tempFile2 = new FileInfo(Path.GetTempFileName());

            File.WriteAllBytes(tempFile1.FullName, baseFace.FaceImage);
            File.WriteAllBytes(tempFile2.FullName, newFace.FaceImage);
            
            using (var baseImage = new Image<Bgr, Byte>(tempFile1.FullName))
            using (Mat otherImage = CvInvoke.Imread(tempFile2.FullName, ImreadModes.AnyColor))
            {


                //using (Mat result =
                //  DrawMatches.Draw(otherImageGray, baseImageGray, out time, out homography, surfCPU, k, uniquenessThreshold, hessianThresh))
                // {
                //byte[] renderedResult = null;//result.ToImage<Bgr, Byte>().ToJpegData(85);
                //return new Tuple<byte[], byte[]>(otherImage.ToJpegData(85), null);
                homography = FaceHomography.MapFaces(baseFace, newFace);

                if (homography != null)
                {
                    var l = homography.GetOutputArray();
                    var matrix =  new HomographyMatrix();
                    homography.CopyTo(matrix);

                    //if (!_niceHomography(matrix))
                    //{
                    //    return new Tuple<byte[], byte[]>(null, renderedResult);
                    //}
                    var outputImage = new Mat();

                    CvInvoke.WarpPerspective(otherImage, outputImage, matrix, baseImage.Size,
                       global::Emgu.CV.CvEnum.Inter.Linear, global::Emgu.CV.CvEnum.Warp.Default, global::Emgu.CV.CvEnum.BorderType.Constant);

                    //var transformed = otherImage.WarpPerspective(matrix,
                    //   baseImage.Size.Width, baseImage.Size.Height,
                    //   global::Emgu.CV.CvEnum.Inter.Linear, global::Emgu.CV.CvEnum.Warp.Default, global::Emgu.CV.CvEnum.BorderType.Constant, new Bgr());
                    //
                    var transformed = outputImage.ToImage<Bgr, Byte>();
                    var imageResult = transformed.ToJpegData(85);
                    if (tempFile1.Exists)
                    {
                        tempFile1.Delete();
                    }

                    if (tempFile2.Exists)
                    {
                        tempFile2.Delete();
                    }
                    return new Tuple<byte[], byte[]>(imageResult, null);

                }
                else
                    Debug.WriteLine("Failed");

                if (tempFile1.Exists)
                {
                    tempFile1.Delete();
                }

                if (tempFile2.Exists)
                {
                    tempFile2.Delete();
                }

                return new Tuple<byte[], byte[]>(null, null);
            }

        }

        static bool _niceHomography(HomographyMatrix mat)
        {
            var det = mat[0, 0] * mat[1, 1] - mat[1, 0] * mat[0, 1];
            if (det < 0)
            {
                return false;
            }

            var n1 = Math.Sqrt(mat[0, 0] * mat[0, 0] + mat[1, 0] * mat[1, 0]);

            if (n1 > 4 || n1 < 0.1)
            {
                return false;
            }

            var n2 = Math.Sqrt(mat[0, 1] * mat[0, 1] + mat[1, 1] * mat[1, 1]);

            if (n2 > 4 || n1 < 0.1)
            {
                return false;
            }

            var n3 = Math.Sqrt(mat[2, 0] * mat[2, 0] + mat[2, 1] * mat[2, 1]);

            if (n3 > 0.002)
            {
                return false;
            }

            return true;

            /*
             * 
             * bool niceHomography(const CvMat * H)
{
                                  const double det = cvmGet(H, 0, 0) * cvmGet(H, 1, 1) - cvmGet(H, 1, 0) * cvmGet(H, 0, 1);
                                  if (det < 0)
                                    return false;

                                      const double N1 = sqrt(cvmGet(H, 0, 0) * cvmGet(H, 0, 0) + cvmGet(H, 1, 0) * cvmGet(H, 1, 0));
                                      if (N1 > 4 || N1 < 0.1)
                                        return false;

  const double N2 = sqrt(cvmGet(H, 0, 1) * cvmGet(H, 0, 1) + cvmGet(H, 1, 1) * cvmGet(H, 1, 1));
  if (N2 > 4 || N2 < 0.1)
    return false;

  const double N3 = sqrt(cvmGet(H, 2, 0) * cvmGet(H, 2, 0) + cvmGet(H, 2, 1) * cvmGet(H, 2, 1));
  if (N3 > 0.002)
    return false;

  return true;
}*/



        }
    }


}
