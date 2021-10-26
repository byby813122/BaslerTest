using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Basler.Pylon;
using OpenCvSharp;

namespace BaslerTest
{
    class BaslerCamera
    {
        #region Singleton
        private static readonly object thisLock = new object();
        private static BaslerCamera instance;
        public static BaslerCamera Instance
        {
            get
            {
                if (null == instance)
                {
                    lock (thisLock)
                    {
                        if (null == instance)
                        {
                            instance = new BaslerCamera();
                        }
                    }
                }

                return instance;
            }
            private set { }
        }
        #endregion

        //The number of camera connections
        public int CameraNumber = CameraFinder.Enumerate().Count;

        //Delegate-event-callback function to pass images captured by the camera
        public delegate void CameraImage(Bitmap bmp);
        public event CameraImage CameraImageEvent;

        //Let out a Camera
        Camera camera;

        //Basler is used to convert images captured by the camera into bitmaps
        PixelDataConverter pxConvert = new PixelDataConverter();

        //Controls the process by which the camera captures images
        bool GrabOver = false;

        private string cameraState;
        public string CameraState
        {
            get => cameraState;
            set
            {
                cameraState = value;
            }
        }

        public BaslerCamera()
        {
            CameraState = null;

            camera = new Camera();
            Initial();
        }

        private void Initial()
        {
            camera.CameraOpened += Configuration.AcquireContinuous; //Free-to-run mode
            camera.ConnectionLost += Camera_ConnectionLost; //Disconnect event
            camera.StreamGrabber.GrabStarted += StreamGrabber_GrabStarted; //Grab the start event
            camera.StreamGrabber.ImageGrabbed += StreamGrabber_ImageGrabbed; //Grab a picture event
            camera.StreamGrabber.GrabStopped += StreamGrabber_GrabStopped; //End the crawl event
            OpenCamera();
        }

        private void StreamGrabber_GrabStopped(object sender, GrabStopEventArgs e)
        {
            GrabOver = false;
        }

        private void StreamGrabber_ImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            IGrabResult grabResult = e.GrabResult;
            if (grabResult.IsValid)
            {
                if (GrabOver)
                    CameraImageEvent(GrabResult2Bmp(grabResult));
            }
        }

        private void StreamGrabber_GrabStarted(object sender, EventArgs e)
        {
            GrabOver = true;
        }

        private void Camera_ConnectionLost(object sender, EventArgs e)
        {
            camera.StreamGrabber.Stop();
            DestroyCamera();
        }

        public void OneShot()
        {
            if (camera != null & CameraState is null)
            {
                if (camera.StreamGrabber.IsGrabbing)
                {
                    camera.StreamGrabber.Stop();
                }
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            else
            {
                CameraState = "The Basler camera is not open";
            }
        }

        public void OpenCamera()
        {
            if (camera != null)
            {
                camera.Open();
            }
        }

        public void KeepShot()
        {
            if (camera != null & CameraState == null)
            {
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            else
            {
                CameraState = "The Basler camera is not open";
            }
        }

        public void Stop()
        {
            if (camera != null)
            {
                camera.StreamGrabber.Stop();
            }
        }

        public void CloseCamera()
        {
            camera.Close();
            if (camera != null)
            {
                camera.Close();
            }
            else
            {
                CameraState = "The Basler camera is closed";
            }
        }

        //Converts the image captured by the camera into a Bitmap bitmap
        public Bitmap GrabResult2Bmp(IGrabResult grabResult)
        {
            Bitmap b = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, b.PixelFormat);
            pxConvert.OutputPixelFormat = PixelType.BGRA8packed;
            IntPtr bmpIntpr = bmpData.Scan0;
            pxConvert.Convert(bmpIntpr, bmpData.Stride * b.Height, grabResult);
            b.UnlockBits(bmpData);
            return b;
        }

        public void DestroyCamera()
        {
            if (camera != null)
            {
                camera.Close();
                camera.Dispose();
                camera = null;
            }
        }
    }

}
