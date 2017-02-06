using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using ImageMagick;

namespace StartFaceAligner.FaceSmarts
{
    public static class LocalFaceDetector
    {
        public static bool HasFace(string fileName)
        {

            Debug.WriteLine($"Detecting: {fileName}");
            try
            {
                using (var img = Image.FromFile(fileName))
                {
                    using (var imgResized = ResizeImage(img, img.Width / 4, img.Height / 4))
                    {
                        var f = new FileInfo(Path.GetTempFileName());

                        imgResized.Save(f.FullName);

                        IImage image = new UMat(f.FullName, ImreadModes.Color);

                        var result = Detect(image);

                        f.Delete();

                        if (result.Item1?.Count > 0)
                        {
                            return true;
                        }
                        return false;
                    }


                }
            }
            catch
            {
                
            }

            return false;

        }

        public static Image ResizeImage(Image img, int width, int height)
        {
            Bitmap b = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage((Image)b))
            {
                g.DrawImage(img, 0, 0, width, height);
            }

            return (Image)b;
        }

        public static (List<Rectangle>, List<Rectangle>) Detect(
            IInputArray image)
        {
            Stopwatch watch;
            List<Rectangle> faces = new List<Rectangle>();
            List<Rectangle> eyes = new List<Rectangle>();
            using (InputArray iaImage = image.GetInputArray())
            {

#if !(__IOS__ || NETFX_CORE)
                if (iaImage.Kind == InputArray.Type.CudaGpuMat && CudaInvoke.HasCuda)
                {
                    using (CudaCascadeClassifier face = new CudaCascadeClassifier("haarcascade_frontalface_default.xml"))
                    using (CudaCascadeClassifier eye = new CudaCascadeClassifier("haarcascade_eye.xml"))
                    {
                        face.ScaleFactor = 1.1;
                        face.MinNeighbors = 10;
                        face.MinObjectSize = Size.Empty;
                        eye.ScaleFactor = 1.1;
                        eye.MinNeighbors = 10;
                        eye.MinObjectSize = Size.Empty;
                        watch = Stopwatch.StartNew();
                        using (CudaImage<Bgr, Byte> gpuImage = new CudaImage<Bgr, byte>(image))
                        using (CudaImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
                        using (GpuMat region = new GpuMat())
                        {
                            face.DetectMultiScale(gpuGray, region);
                            Rectangle[] faceRegion = face.Convert(region);
                            faces.AddRange(faceRegion);
                            foreach (Rectangle f in faceRegion)
                            {
                                using (CudaImage<Gray, Byte> faceImg = gpuGray.GetSubRect(f))
                                {
                                    //For some reason a clone is required.
                                    //Might be a bug of CudaCascadeClassifier in opencv
                                    using (CudaImage<Gray, Byte> clone = faceImg.Clone(null))
                                    using (GpuMat eyeRegionMat = new GpuMat())
                                    {
                                        eye.DetectMultiScale(clone, eyeRegionMat);
                                        Rectangle[] eyeRegion = eye.Convert(eyeRegionMat);
                                        foreach (Rectangle e in eyeRegion)
                                        {
                                            Rectangle eyeRect = e;
                                            eyeRect.Offset(f.X, f.Y);
                                            eyes.Add(eyeRect);
                                        }
                                    }
                                }
                            }
                        }
                        watch.Stop();
                    }
                }
                else
#endif
                {
                    //Read the HaarCascade objects
                    using (CascadeClassifier face = new CascadeClassifier("haarcascade_frontalface_default.xml"))
                    using (CascadeClassifier eye = new CascadeClassifier("haarcascade_eye.xml"))
                    {
                        watch = Stopwatch.StartNew();

                        using (UMat ugray = new UMat())
                        {
                            CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                            //normalizes brightness and increases contrast of the image
                            CvInvoke.EqualizeHist(ugray, ugray);

                            //Detect the faces  from the gray scale image and store the locations as rectangle
                            //The first dimensional is the channel
                            //The second dimension is the index of the rectangle in the specific channel                     
                            Rectangle[] facesDetected = face.DetectMultiScale(
                               ugray,
                               1.1,
                               10,
                               new Size(20, 20));

                            faces.AddRange(facesDetected);

                            foreach (Rectangle f in facesDetected)
                            {
                                //Get the region of interest on the faces
                                using (UMat faceRegion = new UMat(ugray, f))
                                {
                                    Rectangle[] eyesDetected = eye.DetectMultiScale(
                                       faceRegion,
                                       1.1,
                                       10,
                                       new Size(20, 20));

                                    foreach (Rectangle e in eyesDetected)
                                    {
                                        Rectangle eyeRect = e;
                                        eyeRect.Offset(f.X, f.Y);
                                        eyes.Add(eyeRect);
                                    }
                                }
                            }
                        }
                        watch.Stop();
                    }
                }

                return (faces, eyes);
            }
        }
    }
}
