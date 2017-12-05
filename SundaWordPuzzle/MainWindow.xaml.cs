//#define SREENSHOT_PAUSE
//#define HIT_RADIUS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Kinect;

using System.Media;
using System.Windows.Media.Animation;
using System.IO;


namespace SundaWordPuzzle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.Timer timer1;
        System.Windows.Forms.Timer timer2;
        int counter = 300;
        int invincible = 0;
        bool stopable = true;
        bool lvlcomplete = true;
        bool sound = true;
        string txtLevel, txtComplete, txtStart, txtTime, txtScore;
        public MainWindow()
        {
            InitializeComponent();

            // Game Timer
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // 1 second

            // invincible Timer
            timer2 = new System.Windows.Forms.Timer();
            timer2.Tick += new EventHandler(timer2_Tick);
            timer2.Interval = 1000;

            txtLevel = "TINGKAT ";
            txtComplete = " RÉNGSÉ";
            txtStart = " DIKAWITAN";

            //splash screen
            if (lvlcomplete)
            {
                timer1.Stop();
                lvlcomplete = false;
                stopable = false;
                counter = 3;
                timer1.Start();
            }
        }

        #region timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            counter--;
            if (stopable)
            {
                tbTimer.Text = counter.ToString();
                if (counter == 0)
                {
                    timer1.Stop();
                    gameOv();
                    stage = 11;
                }
            }
            else
            {
                if (counter == 2 && !lvlcomplete && stage < 8)
                {
                    tblComplete.Text = txtLevel + Convert.ToString(stage + 1) + txtStart;
                }
                else if (counter == 0)
                {
                    timer1.Stop();
                    hilang();
                }
            }
        }
        
        private void timer2_Tick(object sender, EventArgs e)
        {
            invincible--;
            if (invincible == 0)
            {
                timer2.Stop();
            }
        }
        #endregion

        #region hid
        void hilang()
        {
            HiasanAddedTimer.Visibility = Visibility.Hidden;
            AddedTimerItf.Visibility = Visibility.Hidden;
            switch (stage)
            {
                case -1:
                    {
                        flash.Visibility = Visibility.Hidden;
                        stopable = true;
                        break;
                    }
                case 11:
                    {
                        gameOver.Visibility = Visibility.Hidden;
                        stopable = true;
                        break;
                    }
                case 12:
                    {
                        FinalBoard.Visibility = Visibility.Hidden;
                        FinalScore.Visibility = Visibility.Hidden;
                        FinalText.Visibility = Visibility.Hidden;
                        stopable = true;
                        break;
                    }
                default:
                    {
                        pictComplete.Visibility = Visibility.Hidden;
                        tblComplete.Visibility = Visibility.Hidden;
                        stopable = true;

                        break;
                    }
            }
        }
        #endregion

#if SREENSHOT_PAUSE

        // Used to pause the screen after a number of skeleton tracked events
        // This is so that I can take screenshots 
        // The trackcount and limit
        int trackCount = 0;
        int trackLimit = 200;

#endif

        #region Background

        BitmapSource gameImageBitmap;
        Byte[] gameImageBytes;
        void setupGameImage()
        {
            gameImageBitmap = new BitmapImage(new Uri("TampilanAwal.png", UriKind.RelativeOrAbsolute));
            
            gameImageBytes = new byte[gameImageBitmap.PixelWidth * gameImageBitmap.PixelHeight * 4];

            gameImageBitmap.CopyPixels(gameImageBytes, 1280, 0);
        }

        #endregion

        #region Background Isolation

        //byte[] maskBytes = null;

        void setColorImage(short[] depthValues, int width, int height)
        {
            /// Create a new mask buffer if this is the first time we have been called
            /* if (maskBytes == null)
                maskBytes = new byte[gameImageBytes.Length];

            // Copy a clean version of the game page into the mask
            Buffer.BlockCopy(gameImageBytes, 0, maskBytes, 0, gameImageBytes.Length);
            */
            //int gameImagePos;

            for (int depthPos = 0; depthPos < depthValues.Length; depthPos++)
            {
                int depthValue = depthValues[depthPos];

                int playerNo = depthValue & DepthImageFrame.PlayerIndexBitmask;
                depthValue = depthValue >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (playerNo != 0)
                {
                    // have a player - make them show through the mask

                    // find the X and Y positions of the depth point
                    int x = depthPos % width;
                    int y = depthPos / width;

                    // get the X and Y positions in the video feed
                    ColorImagePoint playerPoint = myKinect.MapDepthToColorImagePoint(DepthImageFormat.Resolution320x240Fps30, x, y, depthValues[depthPos], ColorImageFormat.RgbResolution640x480Fps30);

                    playerPoint.X /= 2;
                    playerPoint.Y /= 2;

                    if (playerPoint.X < 0 || playerPoint.X >= 320 || playerPoint.Y < 0 || playerPoint.Y >= 240)
                        continue;

                   /* gameImagePos = (playerPoint.X + (playerPoint.Y * width)) * 4;

                    maskBytes[gameImagePos] = 0;
                    gameImagePos++;
                    // Green
                    maskBytes[gameImagePos] = 0;
                    gameImagePos++;
                    // Red
                    maskBytes[gameImagePos] = 0;
                    gameImagePos++;
                    // transparency
                    maskBytes[gameImagePos] = 0;
                    gameImagePos++;
                    */
                }
            }
        }


        short[] depthData = null;

        void myKinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {

#if SREENSHOT_PAUSE
            if (trackCount == trackLimit) return;
#endif

            using (DepthImageFrame df = e.OpenDepthImageFrame())
            {
                if (df != null)
                {
                    if (depthData == null)
                        depthData = new short[df.Width * df.Height];

                    df.CopyPixelDataTo(depthData);

                    setColorImage(depthData, df.Width, df.Height);

                    /* gameImage.Source = BitmapSource.Create(
                        df.Width, df.Height,
                        96, 96,
                        PixelFormats.Pbgra32, null,
                        maskBytes, df.Width * 4); */
                }
            }

        }

        #endregion

        #region Kinect Video Display

        byte[] colorData = null;

        WriteableBitmap colorImage = null;

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        void myKinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
#if SREENSHOT_PAUSE
            if (trackCount == trackLimit) return;
#endif

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    if (colorData == null)
                        colorData = new byte[colorFrame.PixelDataLength];

                    colorFrame.CopyPixelDataTo(colorData);

                    if (colorImage == null)
                        this.colorImage = new WriteableBitmap(
                            colorFrame.Width,
                            colorFrame.Height,
                            96,  // DpiX
                            96,  // DpiY
                            PixelFormats.Bgr32,
                            null);

                    this.colorImage.WritePixels(
                        new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                        this.colorData,
                        colorFrame.Width * Bgr32BytesPerPixel,
                        0);

                    kinectVideoImage.Source = colorImage;
                }
            }
            
        }

        #endregion

        #region Kinect Skeleton Display

        #region Kinect Skeleton Tracking

        void myKinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            malletValid = false;

            string message = "No Skeleton";
            string qualityMessage = "";

#if SREENSHOT_PAUSE
            if (trackCount == trackLimit) return;
#endif
            // Remove the old skeleton
            malletCanvas.Children.Clear();
            Brush brush = new SolidColorBrush(Colors.Red);

            Skeleton[] skeletons = null;

            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);
                }
            }

            if (skeletons == null) return;

            foreach (Skeleton skeleton in skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {

#if SREENSHOT_PAUSE
                    // got a tracked skeleton, update the counter
                    trackCount++;
#endif
                    message = "Tracked";
                    Joint headJoint = skeleton.Joints[JointType.Head];
                    Joint hipCenter = skeleton.Joints[JointType.HipCenter];

                    if (skeleton.ClippedEdges == 0)
                    {
                        qualityMessage = "Good Quality";
                    }
                    else
                    {
                        if ((skeleton.ClippedEdges & FrameEdges.Bottom) != 0)
                            qualityMessage += "Move up ";
                        if ((skeleton.ClippedEdges & FrameEdges.Top) != 0)
                            qualityMessage += "Move down ";
                        if ((skeleton.ClippedEdges & FrameEdges.Right) != 0)
                            qualityMessage += "Move left ";
                        if ((skeleton.ClippedEdges & FrameEdges.Left) != 0)
                            qualityMessage += "Move right ";
                    }

                    //drawSkeleton(skeleton);

                    updateMallet(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);
                }
            }

            StatusTextBlock.Text = message;
            QualityTextBlock.Text = qualityMessage;
        }

        #endregion

        #region Mallet drawing
        int stage = -1;
        Brush malletHandleBrush = new SolidColorBrush(Colors.Black);     
        Brush malletHeadBrush = new SolidColorBrush(Colors.Yellow);
        Brush malletHandleBrush2 = new SolidColorBrush(Colors.Black);        
        Brush malletHeadBrush2 = new SolidColorBrush(Colors.Yellow);
        
        float malletLength = 20;
        float headLength = 20;

        Vector malletPosition;
        Vector malletPosition2;
        float malletHitRadius = 30;
        bool malletValid = false;

        float angle = 0;

        void updateMallet(Joint j1, Joint j2, Joint j3, Joint j4)
        {
            if (j1.TrackingState != JointTrackingState.Tracked ||
                j2.TrackingState != JointTrackingState.Tracked ||
                j3.TrackingState != JointTrackingState.Tracked ||
                j4.TrackingState != JointTrackingState.Tracked)
                return;

            // Get the start and end positions of the mallet vector

            ColorImagePoint j1P = myKinect.MapSkeletonPointToColor(j1.Position, ColorImageFormat.RgbResolution640x480Fps30);
            ColorImagePoint j2P = myKinect.MapSkeletonPointToColor(j2.Position, ColorImageFormat.RgbResolution640x480Fps30);
            ColorImagePoint j3P = myKinect.MapSkeletonPointToColor(j3.Position, ColorImageFormat.RgbResolution640x480Fps30);
            ColorImagePoint j4P = myKinect.MapSkeletonPointToColor(j4.Position, ColorImageFormat.RgbResolution640x480Fps30);

            int dX = j2P.X - j1P.X;
            int dY = j2P.Y - j1P.Y;
            int eX = j4P.X - j3P.X;
            int eY = j4P.Y - j3P.Y;

            Vector malletDirection = new Vector(dX, dY);
            Vector malletDirection2 = new Vector(eX, eY);

            if (malletDirection.Length < 1 || malletDirection2.Length < 1) return;

            // Convert into a vector of length 1 unit
            malletDirection.Normalize();
            malletDirection2.Normalize();
            // now set the length of the mallet 
            Vector handleVector = malletDirection * malletLength;
            Vector handleVector2 = malletDirection2 * malletLength;

            Line handleLine = new Line();

            handleLine.Stroke = malletHandleBrush;
            handleLine.StrokeThickness = 0;

            handleLine.X1 = j1P.X;
            handleLine.Y1 = j1P.Y;

            handleLine.X2 = j1P.X + handleVector.X;
            handleLine.Y2 = j1P.Y + handleVector.Y;

            Line handleLine2 = new Line();

            handleLine2.Stroke = malletHandleBrush2;
            handleLine2.StrokeThickness = 0;

            handleLine2.X1 = j3P.X;
            handleLine2.Y1 = j3P.Y;

            handleLine2.X2 = j3P.X + handleVector2.X;
            handleLine2.Y2 = j3P.Y + handleVector2.Y;

            malletCanvas.Children.Add(handleLine);
            malletCanvas.Children.Add(handleLine2);


            Line headLine = new Line();

            headLine.Stroke = malletHeadBrush;
            headLine.StrokeThickness = 10;

            Vector headVector = malletDirection * headLength;

            headLine.X1 = handleLine.X2;
            headLine.Y1 = handleLine.Y2;

            headLine.X2 = handleLine.X2 + headVector.X;
            headLine.Y2 = handleLine.Y2 + headVector.Y;
            malletCanvas.Children.Add(headLine);
            headLine.Visibility = Visibility.Hidden;
            malletPosition = new Vector(j1P.X, j1P.Y);

            malletPosition = malletPosition + (malletDirection * (malletLength + (headLength / 2)));
            
            //kiri
            Line headLine2 = new Line();

            headLine2.Stroke = malletHeadBrush2;
            headLine2.StrokeThickness = 50;

            Vector headVector2 = malletDirection2 * headLength;

            headLine2.X1 = handleLine2.X2;
            headLine2.Y1 = handleLine2.Y2;

            headLine2.X2 = handleLine2.X2 + headVector2.X;
            headLine2.Y2 = handleLine2.Y2 + headVector2.Y;
            malletCanvas.Children.Add(headLine2);
            headLine2.Visibility = Visibility.Hidden;

            malletPosition2 = new Vector(j3P.X, j3P.Y);

            malletPosition2 = malletPosition2 + (malletDirection2 * (malletLength + (headLength / 2)));
            //malletPosition2 = malletPosition2 + (malletDirection2 * (headLength / 2));
//#if HIT_RADIUS

            // draw an ellipse around the mallet to show the hit radius
            
            Ellipse hitSpot = new Ellipse();
            hitSpot.Width = malletHitRadius * 2;
            hitSpot.Height = malletHitRadius * 2;
            hitSpot.Stroke = new SolidColorBrush(Colors.Yellow);//warna lingkaran tangan kanan luar
            hitSpot.StrokeThickness = 5;            
            hitSpot.StrokeDashArray = new DoubleCollection() { 6 };
            malletCanvas.Children.Add(hitSpot);
            Canvas.SetLeft(hitSpot, malletPosition.X - malletHitRadius);
            Canvas.SetTop(hitSpot, malletPosition.Y - malletHitRadius);


            angle += 3 % 360;
            hitSpot.RenderTransform = new RotateTransform(angle,hitSpot.Width/2,hitSpot.Height/2);

            Ellipse hitSpota = new Ellipse();
            hitSpota.Width = malletHitRadius/2 ;
            hitSpota.Height = malletHitRadius/2;
            hitSpota.Fill = new SolidColorBrush(Colors.Yellow);//warna lingkaran tangan kanan dalam
            malletCanvas.Children.Add(hitSpota);
            Canvas.SetLeft(hitSpota, malletPosition.X - malletHitRadius/4);
            Canvas.SetTop(hitSpota, malletPosition.Y - malletHitRadius/4);

            Ellipse hitSpot2 = new Ellipse();
            hitSpot2.Width = malletHitRadius * 2;
            hitSpot2.Height = malletHitRadius * 2;
            hitSpot2.Stroke = new SolidColorBrush(Colors.Yellow);//warna lingkaran tangan kiri luar
            hitSpot2.StrokeThickness = 5;
            hitSpot2.StrokeDashArray = new DoubleCollection() { 6 };
            malletCanvas.Children.Add(hitSpot2);
            Canvas.SetLeft(hitSpot2, malletPosition2.X - malletHitRadius);
            Canvas.SetTop(hitSpot2, malletPosition2.Y - malletHitRadius);
            hitSpot2.RenderTransform = new RotateTransform(angle, hitSpot2.Width / 2, hitSpot2.Height / 2);


            Ellipse hitSpotb = new Ellipse();
            hitSpotb.Width = malletHitRadius/2;
            hitSpotb.Height = malletHitRadius/2;
            hitSpotb.Fill = new SolidColorBrush(Colors.Yellow);//warana lingkaran tangan kiri dalam
            malletCanvas.Children.Add(hitSpotb);
            Canvas.SetLeft(hitSpotb, malletPosition2.X - malletHitRadius/4);
            Canvas.SetTop(hitSpotb, malletPosition2.Y - malletHitRadius/4);
//#endif
            if (invincible > 0)
            {
                hitSpota.Fill = Brushes.Gray;
                hitSpotb.Fill = Brushes.Gray;
                hitSpot.Stroke = Brushes.Gray;
                hitSpot2.Stroke = Brushes.Gray;
            }
            if (invincible > 2)
            {
                hitSpot.Fill = Brushes.Red;
                hitSpot.Fill = Brushes.Red;
                
            }
            malletValid = true;
        }
        #endregion

        #region Skeleton Drawing primitives


        Brush skeletonBrush = new SolidColorBrush(Colors.Red);

        void addLine(Joint j1, Joint j2)
        {
            Line boneLine = new Line();
            boneLine.Stroke = skeletonBrush;
            boneLine.StrokeThickness = 5;

            ColorImagePoint j1P = myKinect.MapSkeletonPointToColor(j1.Position, ColorImageFormat.RgbResolution640x480Fps30);
            boneLine.X1 = j1P.X;
            boneLine.Y1 = j1P.Y;

            ColorImagePoint j2P = myKinect.MapSkeletonPointToColor(j2.Position, ColorImageFormat.RgbResolution640x480Fps30);
            boneLine.X2 = j2P.X;
            boneLine.Y2 = j2P.Y;

            malletCanvas.Children.Add(boneLine);
        }

        #endregion
        
        void drawSkeleton(Skeleton skeleton)
        {
            // Spine
            addLine(skeleton.Joints[JointType.Head], skeleton.Joints[JointType.ShoulderCenter]);
            addLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Spine]);

            // Left leg
            addLine(skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.HipCenter]);
            addLine(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipLeft]);
            addLine(skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft]);
            addLine(skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.AnkleLeft]);
            addLine(skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.FootLeft]);

            // Right leg
            addLine(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipRight]);
            addLine(skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight]);
            addLine(skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.AnkleRight]);
            addLine(skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.FootRight]);

            // Left arm
            addLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft]);
            addLine(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft]);
            addLine(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft]);
            addLine(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);

            // Right arm
            addLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight]);
            addLine(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight]);
            addLine(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]);
            addLine(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight]);

        }

        #endregion

        #region Kinect setup

        KinectSensor myKinect;

        void setupKinect()
        {

            // Check to see if a Kinect is available
            if (KinectSensor.KinectSensors.Count == 0)
            {
                System.Windows.MessageBox.Show("No Kinects detected", "Camera Viewer");
                this.Close();
            }

            // Get the first Kinect on the computer
            myKinect = KinectSensor.KinectSensors[0];

            // Start the Kinect running and select the depth camera
            try
            {
                myKinect.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                myKinect.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                myKinect.SkeletonStream.Enable();
            }
            catch
            {
                System.Windows.MessageBox.Show("Kinect initialize failed", "Camera Viewer");
                System.Windows.Application.Current.Shutdown();
            }

            // connect a handler to the event that fires when new frames are available

            myKinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(myKinect_SkeletonFrameReady);
            myKinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(myKinect_ColorFrameReady);
            myKinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(myKinect_DepthFrameReady);

            try
            {
                myKinect.Start();
            }
            catch
            {
                System.Windows.MessageBox.Show("Camera start failed", "Camera Viewer");
                this.Close();
            }
        }


        void shutdownKinect()
        {
            if (myKinect != null)
                myKinect.Stop();
        }

        #endregion
        
        #region Game drawing

        private void startGameThread()
        {
            Thread game = new Thread(gameLoop);
            game.Start();
        }

        private void gameLoop()
        {
            while (true)
            {
                Dispatcher.Invoke(new Action(() => updateGame()));
                Thread.Sleep(17);
            }
        }

        struct HitVector
        {
            public Vector ob1;
            public Vector ob2;
            public Vector ob3;
            public Vector ob4;
            public Vector ob5;
            
            public Vector getob1()
            {
                return ob1;
            }
            public Vector getob2()
            {
                return ob2;
            }
            public Vector getob3()
            {
                return ob3;
            }
            public Vector getob4()
            {
                return ob4;
            }
            public Vector getob5()
            {
                return ob5;
            }
        }

        struct StatePoint
        {
            public byte ob1;
            public byte ob2;
            public byte ob3;
            public byte ob4;
            public byte ob5;
            public void zeros()
            {
                ob1 = 0;
                ob2 = 0;
                ob3 = 0;
                ob4 = 0;
                ob5 = 0;
            }
        }

        Random rand = new Random(1);

        int lives = 3;
        int score1 = 0;
        int score2 = 0;
        int score3 = 0;
        int score4 = 0;
        int score5 = 0;
        int score6 = 0;
        int score7 = 0;
        int score8 = 0;
        int gscore = 0; // gscore for interface-only, 
        int fscore = 0; // while fscore used for calculating real score
        int tmerge = 0; 

        SoundPlayer scorePlayer;
        SoundPlayer musik;
        
        HitVector HitVectorTx;
        HitVector HitVectorIm;

        StatePoint lv1;
        StatePoint lv2;
        StatePoint lv3;
        StatePoint lv4;
        StatePoint lv5;
        StatePoint lv6;
        StatePoint lv7;
        StatePoint lv8;

        private void updateGame()
        {
            //Canvas.SetTop(bugImage, bugY);
            //bugY = bugY + bugYSpeed;
            
            // check for collisions           

            if (malletValid)
            {
                //double bugCenterX = bugX + (bugImage.Width / 2);
                //double bugCenterY = bugY + (bugImage.Height / 2);

                //stage 0
                double startX = Canvas.GetLeft(start) + (start.Width / 2);
                double startY = Canvas.GetTop(start) + (start.Height / 2);
                double start2X = Canvas.GetLeft(start2) + (start2.Width / 2);
                double start2Y = Canvas.GetTop(start2) + (start2.Height / 2);

                double exitX = Canvas.GetLeft(exit) + (exit.Width / 2);
                double exitY = Canvas.GetTop(exit) + (exit.Height / 2);

                double settingX = Canvas.GetLeft(setting) + (setting.Width / 2);
                double settingY = Canvas.GetTop(setting) + (setting.Height / 2);

                double developerX = Canvas.GetLeft(developer) + (developer.Width / 2);
                double developerY = Canvas.GetTop(developer) + (developer.Height / 2);

                //language
                double sunX = Canvas.GetLeft(sun) + (sun.Width / 2);
                double sunY = Canvas.GetTop(sun) + (sun.Height / 2);

                double idX = Canvas.GetLeft(id) + (id.Width / 2);
                double idY = Canvas.GetTop(id) + (id.Height / 2);

                double usX = Canvas.GetLeft(us) + (us.Width / 2);
                double usY = Canvas.GetTop(us) + (us.Height / 2);

                //credits
                double backX = Canvas.GetLeft(back) + (back.Width / 2);
                double backY = Canvas.GetTop(back) + (back.Height / 2);

                //settings
                double speaker_onX = Canvas.GetLeft(speaker_on) + (speaker_on.Width / 2);
                double speaker_onY = Canvas.GetTop(speaker_on) + (speaker_on.Height / 2);

                //how to
                double howX = Canvas.GetLeft(cara_maen) + (cara_maen.Width / 2);
                double howY = Canvas.GetTop(cara_maen) + (cara_maen.Height / 2);

                //stage 1
                double g_lv1ob1X = Canvas.GetLeft(g_lv1ob1) + (g_lv1ob1.Width / 2);
                double g_lv1ob1Y = Canvas.GetTop(g_lv1ob1) + (g_lv1ob1.Height / 2);
                double t_lv1ob1X = Canvas.GetLeft(t_lv1ob1) + (t_lv1ob1.Width / 2);
                double t_lv1ob1Y = Canvas.GetTop(t_lv1ob1) + (t_lv1ob1.Height / 2);
                double g_lv1ob2X = Canvas.GetLeft(g_lv1ob2) + (g_lv1ob2.Width / 2);
                double g_lv1ob2Y = Canvas.GetTop(g_lv1ob2) + (g_lv1ob2.Height / 2);
                double t_lv1ob2X = Canvas.GetLeft(t_lv1ob2) + (t_lv1ob2.Width / 2);
                double t_lv1ob2Y = Canvas.GetTop(t_lv1ob2) + (t_lv1ob2.Height / 2);
                double g_lv1ob3X = Canvas.GetLeft(g_lv1ob3) + (g_lv1ob3.Width / 2);
                double g_lv1ob3Y = Canvas.GetTop(g_lv1ob3) + (g_lv1ob3.Height / 2);
                double t_lv1ob3X = Canvas.GetLeft(t_lv1ob3) + (t_lv1ob3.Width / 2);
                double t_lv1ob3Y = Canvas.GetTop(t_lv1ob3) + (t_lv1ob3.Height / 2);
                double g_lv1ob4X = Canvas.GetLeft(g_lv1ob4) + (g_lv1ob4.Width / 2);
                double g_lv1ob4Y = Canvas.GetTop(g_lv1ob4) + (g_lv1ob4.Height / 2);
                double t_lv1ob4X = Canvas.GetLeft(t_lv1ob4) + (t_lv1ob4.Width / 2);
                double t_lv1ob4Y = Canvas.GetTop(t_lv1ob4) + (t_lv1ob4.Height / 2);
                double g_lv1ob5X = Canvas.GetLeft(g_lv1ob5) + (g_lv1ob5.Width / 2);
                double g_lv1ob5Y = Canvas.GetTop(g_lv1ob5) + (g_lv1ob5.Height / 2);
                double t_lv1ob5X = Canvas.GetLeft(t_lv1ob5) + (t_lv1ob5.Width / 2);
                double t_lv1ob5Y = Canvas.GetTop(t_lv1ob5) + (t_lv1ob5.Height / 2);

                //stage 2
                double g_lv2ob1X = Canvas.GetLeft(g_lv2ob1) + (g_lv2ob1.Width / 2);
                double g_lv2ob1Y = Canvas.GetTop(g_lv2ob1) + (g_lv2ob1.Height / 2);
                double t_lv2ob1X = Canvas.GetLeft(t_lv2ob1) + (t_lv2ob1.Width / 2);
                double t_lv2ob1Y = Canvas.GetTop(t_lv2ob1) + (t_lv2ob1.Height / 2);
                double g_lv2ob2X = Canvas.GetLeft(g_lv2ob2) + (g_lv2ob2.Width / 2);
                double g_lv2ob2Y = Canvas.GetTop(g_lv2ob2) + (g_lv2ob2.Height / 2);
                double t_lv2ob2X = Canvas.GetLeft(t_lv2ob2) + (t_lv2ob2.Width / 2);
                double t_lv2ob2Y = Canvas.GetTop(t_lv2ob2) + (t_lv2ob2.Height / 2);
                double g_lv2ob3X = Canvas.GetLeft(g_lv2ob3) + (g_lv2ob3.Width / 2);
                double g_lv2ob3Y = Canvas.GetTop(g_lv2ob3) + (g_lv2ob3.Height / 2);
                double t_lv2ob3X = Canvas.GetLeft(t_lv2ob3) + (t_lv2ob3.Width / 2);
                double t_lv2ob3Y = Canvas.GetTop(t_lv2ob3) + (t_lv2ob3.Height / 2);
                double g_lv2ob4X = Canvas.GetLeft(g_lv2ob4) + (g_lv2ob4.Width / 2);
                double g_lv2ob4Y = Canvas.GetTop(g_lv2ob4) + (g_lv2ob4.Height / 2);
                double t_lv2ob4X = Canvas.GetLeft(t_lv2ob4) + (t_lv2ob4.Width / 2);
                double t_lv2ob4Y = Canvas.GetTop(t_lv2ob4) + (t_lv2ob4.Height / 2);
                double g_lv2ob5X = Canvas.GetLeft(g_lv2ob5) + (g_lv2ob5.Width / 2);
                double g_lv2ob5Y = Canvas.GetTop(g_lv2ob5) + (g_lv2ob5.Height / 2);
                double t_lv2ob5X = Canvas.GetLeft(t_lv2ob5) + (t_lv2ob5.Width / 2);
                double t_lv2ob5Y = Canvas.GetTop(t_lv2ob5) + (t_lv2ob5.Height / 2);
                
                //stage 3
                double g_lv3ob1X = Canvas.GetLeft(g_lv3ob1) + (g_lv3ob1.Width / 2);
                double g_lv3ob1Y = Canvas.GetTop(g_lv3ob1) + (g_lv3ob1.Height / 2);
                double t_lv3ob1X = Canvas.GetLeft(t_lv3ob1) + (t_lv3ob1.Width / 2);
                double t_lv3ob1Y = Canvas.GetTop(t_lv3ob1) + (t_lv3ob1.Height / 2);
                double g_lv3ob2X = Canvas.GetLeft(g_lv3ob2) + (g_lv3ob2.Width / 2);
                double g_lv3ob2Y = Canvas.GetTop(g_lv3ob2) + (g_lv3ob2.Height / 2);
                double t_lv3ob2X = Canvas.GetLeft(t_lv3ob2) + (t_lv3ob2.Width / 2);
                double t_lv3ob2Y = Canvas.GetTop(t_lv3ob2) + (t_lv3ob2.Height / 2);
                double g_lv3ob3X = Canvas.GetLeft(g_lv3ob3) + (g_lv3ob3.Width / 2);
                double g_lv3ob3Y = Canvas.GetTop(g_lv3ob3) + (g_lv3ob3.Height / 2);
                double t_lv3ob3X = Canvas.GetLeft(t_lv3ob3) + (t_lv3ob3.Width / 2);
                double t_lv3ob3Y = Canvas.GetTop(t_lv3ob3) + (t_lv3ob3.Height / 2);
                double g_lv3ob4X = Canvas.GetLeft(g_lv3ob4) + (g_lv3ob4.Width / 2);
                double g_lv3ob4Y = Canvas.GetTop(g_lv3ob4) + (g_lv3ob4.Height / 2);
                double t_lv3ob4X = Canvas.GetLeft(t_lv3ob4) + (t_lv3ob4.Width / 2);
                double t_lv3ob4Y = Canvas.GetTop(t_lv3ob4) + (t_lv3ob4.Height / 2);
                double g_lv3ob5X = Canvas.GetLeft(g_lv3ob5) + (g_lv3ob5.Width / 2);
                double g_lv3ob5Y = Canvas.GetTop(g_lv3ob5) + (g_lv3ob5.Height / 2);
                double t_lv3ob5X = Canvas.GetLeft(t_lv3ob5) + (t_lv3ob5.Width / 2);
                double t_lv3ob5Y = Canvas.GetTop(t_lv3ob5) + (t_lv3ob5.Height / 2);

                //stage 4
                double g_lv4ob1X = Canvas.GetLeft(g_lv4ob1) + (g_lv4ob1.Width / 2);
                double g_lv4ob1Y = Canvas.GetTop(g_lv4ob1) + (g_lv4ob1.Height / 2);
                double t_lv4ob1X = Canvas.GetLeft(t_lv4ob1) + (t_lv4ob1.Width / 2);
                double t_lv4ob1Y = Canvas.GetTop(t_lv4ob1) + (t_lv4ob1.Height / 2);
                double g_lv4ob2X = Canvas.GetLeft(g_lv4ob2) + (g_lv4ob2.Width / 2);
                double g_lv4ob2Y = Canvas.GetTop(g_lv4ob2) + (g_lv4ob2.Height / 2);
                double t_lv4ob2X = Canvas.GetLeft(t_lv4ob2) + (t_lv4ob2.Width / 2);
                double t_lv4ob2Y = Canvas.GetTop(t_lv4ob2) + (t_lv4ob2.Height / 2);
                double g_lv4ob3X = Canvas.GetLeft(g_lv4ob3) + (g_lv4ob3.Width / 2);
                double g_lv4ob3Y = Canvas.GetTop(g_lv4ob3) + (g_lv4ob3.Height / 2);
                double t_lv4ob3X = Canvas.GetLeft(t_lv4ob3) + (t_lv4ob3.Width / 2);
                double t_lv4ob3Y = Canvas.GetTop(t_lv4ob3) + (t_lv4ob3.Height / 2);
                double g_lv4ob4X = Canvas.GetLeft(g_lv4ob4) + (g_lv4ob4.Width / 2);
                double g_lv4ob4Y = Canvas.GetTop(g_lv4ob4) + (g_lv4ob4.Height / 2);
                double t_lv4ob4X = Canvas.GetLeft(t_lv4ob4) + (t_lv4ob4.Width / 2);
                double t_lv4ob4Y = Canvas.GetTop(t_lv4ob4) + (t_lv4ob4.Height / 2);
                double g_lv4ob5X = Canvas.GetLeft(g_lv4ob5) + (g_lv4ob5.Width / 2);
                double g_lv4ob5Y = Canvas.GetTop(g_lv4ob5) + (g_lv4ob5.Height / 2);
                double t_lv4ob5X = Canvas.GetLeft(t_lv4ob5) + (t_lv4ob5.Width / 2);
                double t_lv4ob5Y = Canvas.GetTop(t_lv4ob5) + (t_lv4ob5.Height / 2);

                //stage 5
                double g_lv5ob1X = Canvas.GetLeft(g_lv5ob1) + (g_lv5ob1.Width / 2);
                double g_lv5ob1Y = Canvas.GetTop(g_lv5ob1) + (g_lv5ob1.Height / 2);
                double t_lv5ob1X = Canvas.GetLeft(t_lv5ob1) + (t_lv5ob1.Width / 2);
                double t_lv5ob1Y = Canvas.GetTop(t_lv5ob1) + (t_lv5ob1.Height / 2);
                double g_lv5ob2X = Canvas.GetLeft(g_lv5ob2) + (g_lv5ob2.Width / 2);
                double g_lv5ob2Y = Canvas.GetTop(g_lv5ob2) + (g_lv5ob2.Height / 2);
                double t_lv5ob2X = Canvas.GetLeft(t_lv5ob2) + (t_lv5ob2.Width / 2);
                double t_lv5ob2Y = Canvas.GetTop(t_lv5ob2) + (t_lv5ob2.Height / 2);
                double g_lv5ob3X = Canvas.GetLeft(g_lv5ob3) + (g_lv5ob3.Width / 2);
                double g_lv5ob3Y = Canvas.GetTop(g_lv5ob3) + (g_lv5ob3.Height / 2);
                double t_lv5ob3X = Canvas.GetLeft(t_lv5ob3) + (t_lv5ob3.Width / 2);
                double t_lv5ob3Y = Canvas.GetTop(t_lv5ob3) + (t_lv5ob3.Height / 2);
                double g_lv5ob4X = Canvas.GetLeft(g_lv5ob4) + (g_lv5ob4.Width / 2);
                double g_lv5ob4Y = Canvas.GetTop(g_lv5ob4) + (g_lv5ob4.Height / 2);
                double t_lv5ob4X = Canvas.GetLeft(t_lv5ob4) + (t_lv5ob4.Width / 2);
                double t_lv5ob4Y = Canvas.GetTop(t_lv5ob4) + (t_lv5ob4.Height / 2);
                double g_lv5ob5X = Canvas.GetLeft(g_lv5ob5) + (g_lv5ob5.Width / 2);
                double g_lv5ob5Y = Canvas.GetTop(g_lv5ob5) + (g_lv5ob5.Height / 2);
                double t_lv5ob5X = Canvas.GetLeft(t_lv5ob5) + (t_lv5ob5.Width / 2);
                double t_lv5ob5Y = Canvas.GetTop(t_lv5ob5) + (t_lv5ob5.Height / 2);

                //stage 6
                double g_lv6ob1X = Canvas.GetLeft(g_lv6ob1) + (g_lv6ob1.Width / 2);
                double g_lv6ob1Y = Canvas.GetTop(g_lv6ob1) + (g_lv6ob1.Height / 2);
                double t_lv6ob1X = Canvas.GetLeft(t_lv6ob1) + (t_lv6ob1.Width / 2);
                double t_lv6ob1Y = Canvas.GetTop(t_lv6ob1) + (t_lv6ob1.Height / 2);
                double g_lv6ob2X = Canvas.GetLeft(g_lv6ob2) + (g_lv6ob2.Width / 2);
                double g_lv6ob2Y = Canvas.GetTop(g_lv6ob2) + (g_lv6ob2.Height / 2);
                double t_lv6ob2X = Canvas.GetLeft(t_lv6ob2) + (t_lv6ob2.Width / 2);
                double t_lv6ob2Y = Canvas.GetTop(t_lv6ob2) + (t_lv6ob2.Height / 2);
                double g_lv6ob3X = Canvas.GetLeft(g_lv6ob3) + (g_lv6ob3.Width / 2);
                double g_lv6ob3Y = Canvas.GetTop(g_lv6ob3) + (g_lv6ob3.Height / 2);
                double t_lv6ob3X = Canvas.GetLeft(t_lv6ob3) + (t_lv6ob3.Width / 2);
                double t_lv6ob3Y = Canvas.GetTop(t_lv6ob3) + (t_lv6ob3.Height / 2);
                double g_lv6ob4X = Canvas.GetLeft(g_lv6ob4) + (g_lv6ob4.Width / 2);
                double g_lv6ob4Y = Canvas.GetTop(g_lv6ob4) + (g_lv6ob4.Height / 2);
                double t_lv6ob4X = Canvas.GetLeft(t_lv6ob4) + (t_lv6ob4.Width / 2);
                double t_lv6ob4Y = Canvas.GetTop(t_lv6ob4) + (t_lv6ob4.Height / 2);
                double g_lv6ob5X = Canvas.GetLeft(g_lv6ob5) + (g_lv6ob5.Width / 2);
                double g_lv6ob5Y = Canvas.GetTop(g_lv6ob5) + (g_lv6ob5.Height / 2);
                double t_lv6ob5X = Canvas.GetLeft(t_lv6ob5) + (t_lv6ob5.Width / 2);
                double t_lv6ob5Y = Canvas.GetTop(t_lv6ob5) + (t_lv6ob5.Height / 2);

                //stage 7
                double g_lv7ob1X = Canvas.GetLeft(g_lv7ob1) + (g_lv7ob1.Width / 2);
                double g_lv7ob1Y = Canvas.GetTop(g_lv7ob1) + (g_lv7ob1.Height / 2);
                double t_lv7ob1X = Canvas.GetLeft(t_lv7ob1) + (t_lv7ob1.Width / 2);
                double t_lv7ob1Y = Canvas.GetTop(t_lv7ob1) + (t_lv7ob1.Height / 2);
                double g_lv7ob2X = Canvas.GetLeft(g_lv7ob2) + (g_lv7ob2.Width / 2);
                double g_lv7ob2Y = Canvas.GetTop(g_lv7ob2) + (g_lv7ob2.Height / 2);
                double t_lv7ob2X = Canvas.GetLeft(t_lv7ob2) + (t_lv7ob2.Width / 2);
                double t_lv7ob2Y = Canvas.GetTop(t_lv7ob2) + (t_lv7ob2.Height / 2);
                double g_lv7ob3X = Canvas.GetLeft(g_lv7ob3) + (g_lv7ob3.Width / 2);
                double g_lv7ob3Y = Canvas.GetTop(g_lv7ob3) + (g_lv7ob3.Height / 2);
                double t_lv7ob3X = Canvas.GetLeft(t_lv7ob3) + (t_lv7ob3.Width / 2);
                double t_lv7ob3Y = Canvas.GetTop(t_lv7ob3) + (t_lv7ob3.Height / 2);
                double g_lv7ob4X = Canvas.GetLeft(g_lv7ob4) + (g_lv7ob4.Width / 2);
                double g_lv7ob4Y = Canvas.GetTop(g_lv7ob4) + (g_lv7ob4.Height / 2);
                double t_lv7ob4X = Canvas.GetLeft(t_lv7ob4) + (t_lv7ob4.Width / 2);
                double t_lv7ob4Y = Canvas.GetTop(t_lv7ob4) + (t_lv7ob4.Height / 2);
                double g_lv7ob5X = Canvas.GetLeft(g_lv7ob5) + (g_lv7ob5.Width / 2);
                double g_lv7ob5Y = Canvas.GetTop(g_lv7ob5) + (g_lv7ob5.Height / 2);
                double t_lv7ob5X = Canvas.GetLeft(t_lv7ob5) + (t_lv7ob5.Width / 2);
                double t_lv7ob5Y = Canvas.GetTop(t_lv7ob5) + (t_lv7ob5.Height / 2);

                //stage 8
                double g_lv8ob1X = Canvas.GetLeft(g_lv8ob1) + (g_lv8ob1.Width / 2);
                double g_lv8ob1Y = Canvas.GetTop(g_lv8ob1) + (g_lv8ob1.Height / 2);
                double t_lv8ob1X = Canvas.GetLeft(t_lv8ob1) + (t_lv8ob1.Width / 2);
                double t_lv8ob1Y = Canvas.GetTop(t_lv8ob1) + (t_lv8ob1.Height / 2);
                double g_lv8ob2X = Canvas.GetLeft(g_lv8ob2) + (g_lv8ob2.Width / 2);
                double g_lv8ob2Y = Canvas.GetTop(g_lv8ob2) + (g_lv8ob2.Height / 2);
                double t_lv8ob2X = Canvas.GetLeft(t_lv8ob2) + (t_lv8ob2.Width / 2);
                double t_lv8ob2Y = Canvas.GetTop(t_lv8ob2) + (t_lv8ob2.Height / 2);
                double g_lv8ob3X = Canvas.GetLeft(g_lv8ob3) + (g_lv8ob3.Width / 2);
                double g_lv8ob3Y = Canvas.GetTop(g_lv8ob3) + (g_lv8ob3.Height / 2);
                double t_lv8ob3X = Canvas.GetLeft(t_lv8ob3) + (t_lv8ob3.Width / 2);
                double t_lv8ob3Y = Canvas.GetTop(t_lv8ob3) + (t_lv8ob3.Height / 2);
                double g_lv8ob4X = Canvas.GetLeft(g_lv8ob4) + (g_lv8ob4.Width / 2);
                double g_lv8ob4Y = Canvas.GetTop(g_lv8ob4) + (g_lv8ob4.Height / 2);
                double t_lv8ob4X = Canvas.GetLeft(t_lv8ob4) + (t_lv8ob4.Width / 2);
                double t_lv8ob4Y = Canvas.GetTop(t_lv8ob4) + (t_lv8ob4.Height / 2);
                double g_lv8ob5X = Canvas.GetLeft(g_lv8ob5) + (g_lv8ob5.Width / 2);
                double g_lv8ob5Y = Canvas.GetTop(g_lv8ob5) + (g_lv8ob5.Height / 2);
                double t_lv8ob5X = Canvas.GetLeft(t_lv8ob5) + (t_lv8ob5.Width / 2);
                double t_lv8ob5Y = Canvas.GetTop(t_lv8ob5) + (t_lv8ob5.Height / 2);
                
                if (stage == -1) // splash screen
                {
                    if (!flash.IsVisible)
                    {
                        stage = 0;
                        lvlcomplete = true;
                    }
                }
                else if (stage == 0) // main menu
                {
                    timer1.Stop();
                    tbTimer.Text = txtTime;
                    ScoreTextBlock.Text = txtScore;
                    start.Visibility = Visibility.Visible;
                    tblNgawitan.Visibility = Visibility.Visible;
                    start2.Visibility = Visibility.Visible;
                    Heart_isi1.Visibility = Visibility.Visible;
                    Heart_isi2.Visibility = Visibility.Visible;
                    Heart_isi3.Visibility = Visibility.Visible;
                    Heart_patah2.Visibility = Visibility.Hidden;
                    Heart_patah3.Visibility = Visibility.Hidden;
                    exit.Visibility = Visibility.Visible;
                    tblKaluar.Visibility = Visibility.Visible;

                    developer.Visibility = Visibility.Visible;
                    tblPamekar.Visibility = Visibility.Visible;

                    setting.Visibility = Visibility.Visible;
                    tblPangaturan.Visibility = Visibility.Visible;
                    
                    cara_maen.Visibility = Visibility.Visible;
                    tblCaraMaen.Visibility = Visibility.Visible;

                    Vector hitVector_start = new Vector(malletPosition.X - startX, malletPosition.Y - startY);
                    Vector hitVector2_start = new Vector(malletPosition2.X - start2X, malletPosition2.Y - start2Y);

                    Vector hitVector_setting = new Vector(malletPosition.X - settingX, malletPosition.Y - settingY);

                    Vector hitVector_developer = new Vector(malletPosition.X - developerX, malletPosition.Y - developerY);

                    Vector hitVector_how = new Vector(malletPosition.X - howX, malletPosition.Y - howY);
                    
                    Vector hitVector_exit = new Vector(malletPosition.X - exitX, malletPosition.Y - exitY);
                    if (hitVector_start.Length < malletHitRadius && hitVector2_start.Length < malletHitRadius)
                    {
                        lvlcomplete = true;
                        counter = 90;
                        timer1.Start();
                        tbTimer.Text = counter.ToString();

                        FinalBoard.Visibility = Visibility.Hidden;
                        FinalScore.Visibility = Visibility.Hidden;
                        FinalText.Visibility = Visibility.Hidden;
                        //gameCanvas.Children.Remove(start);
                        //gameCanvas.Children.Remove(start2);
                        start.Visibility = Visibility.Hidden;
                        tblNgawitan.Visibility = Visibility.Hidden;
                        start2.Visibility = Visibility.Hidden;
                        setting.Visibility = Visibility.Hidden;
                        tblPangaturan.Visibility = Visibility.Hidden;

                        developer.Visibility = Visibility.Hidden;
                        tblPamekar.Visibility = Visibility.Hidden;

                        cara_maen.Visibility = Visibility.Hidden;
                        tblCaraMaen.Visibility = Visibility.Hidden;

                        exit.Visibility = Visibility.Hidden;
                        tblKaluar.Visibility = Visibility.Hidden;

                        g_lv1ob1.Visibility = Visibility.Visible;
                        g_lv1ob2.Visibility = Visibility.Visible;
                        g_lv1ob3.Visibility = Visibility.Visible;
                        g_lv1ob4.Visibility = Visibility.Visible;
                        g_lv1ob5.Visibility = Visibility.Visible;

                        t_lv1ob1.Visibility = Visibility.Visible;
                        t_lv1ob2.Visibility = Visibility.Visible;
                        t_lv1ob3.Visibility = Visibility.Visible;
                        t_lv1ob4.Visibility = Visibility.Visible;
                        t_lv1ob5.Visibility = Visibility.Visible;

                        stage = 1;
                    }
                    if (hitVector_developer.Length < malletHitRadius && hitVector2_start.Length < malletHitRadius)
                    {
                        lvlcomplete = true;
                        stage = 13;
                        start.Visibility = Visibility.Hidden;
                        tblNgawitan.Visibility = Visibility.Hidden;
                    }
                    if (hitVector_setting.Length < malletHitRadius && hitVector2_start.Length < malletHitRadius)
                    {
                        //gameCanvas.Children.Remove(start);
                        //gameCanvas.Children.Remove(start2);
                        //lvlcomplete = true;
                        stage = 14;
                    }
                    
                    if (hitVector_how.Length < malletHitRadius && hitVector2_start.Length < malletHitRadius)
                    {
                        //lvlcomplete = true;
                        stage = 15;
                    }
                    if (hitVector_exit.Length < malletHitRadius && hitVector2_start.Length < malletHitRadius)
                    {
                        this.Close();
                    }
                }
                else if (stage == 1)
                {
                    HitVectorIm.ob1 = new Vector(malletPosition.X - g_lv1ob1X, malletPosition.Y - g_lv1ob1Y);
                    HitVectorIm.ob2 = new Vector(malletPosition.X - g_lv1ob2X, malletPosition.Y - g_lv1ob2Y);
                    HitVectorIm.ob3 = new Vector(malletPosition.X - g_lv1ob3X, malletPosition.Y - g_lv1ob3Y);
                    HitVectorIm.ob4 = new Vector(malletPosition.X - g_lv1ob4X, malletPosition.Y - g_lv1ob4Y);
                    HitVectorIm.ob5 = new Vector(malletPosition.X - g_lv1ob5X, malletPosition.Y - g_lv1ob5Y);

                    HitVectorTx.ob1 = new Vector(malletPosition2.X - t_lv1ob1X, malletPosition2.Y - t_lv1ob1Y);
                    HitVectorTx.ob2 = new Vector(malletPosition2.X - t_lv1ob2X, malletPosition2.Y - t_lv1ob2Y);
                    HitVectorTx.ob3 = new Vector(malletPosition2.X - t_lv1ob3X, malletPosition2.Y - t_lv1ob3Y);
                    HitVectorTx.ob4 = new Vector(malletPosition2.X - t_lv1ob4X, malletPosition2.Y - t_lv1ob4Y);
                    HitVectorTx.ob5 = new Vector(malletPosition2.X - t_lv1ob5X, malletPosition2.Y - t_lv1ob5Y);
                    
                    hitvectorInteract(ref lv1, HitVectorTx, HitVectorIm, g_lv1ob1, g_lv1ob2, g_lv1ob3, g_lv1ob4, g_lv1ob5, t_lv1ob1, t_lv1ob2, t_lv1ob3, t_lv1ob4, t_lv1ob5);
                    
                    score1 = (lv1.ob1 + lv1.ob2 + lv1.ob3 + lv1.ob4 + lv1.ob5) * 100;
                    fscore = gscore + (100 * lives);
                    ScoreTextBlock.Text = score1.ToString();

                    if ((lv1.ob1 + lv1.ob2 + lv1.ob3 + lv1.ob4 + lv1.ob5) == 5)
                    {
                        if (lvlcomplete)
                        {
                            lvlcomplete = false;
                            tmerge = ((counter + 10) / 15) * 5;
                            timer1.Stop();
                            pictComplete.Visibility = Visibility.Visible;
                            tblComplete.Visibility = Visibility.Visible;
                            tblComplete.Text = txtLevel + stage.ToString() + txtComplete;
                            stopable = false;
                            counter = 4;

                            HiasanAddedTimer.Visibility = Visibility.Visible;
                            AddedTimerItf.Visibility = Visibility.Visible;
                            AddedTimerItf.Text = txtTime+ " + " + tmerge.ToString();

                            timer1.Start();
                        }
                        if (!pictComplete.IsVisible)
                        {
                            lvlcomplete = true;
                            counter = 60 + tmerge;
                            timer1.Start();
                            tbTimer.Text = counter.ToString();
                            stage = 2;

                            g_lv2ob1.Visibility = Visibility.Visible;
                            g_lv2ob2.Visibility = Visibility.Visible;
                            g_lv2ob3.Visibility = Visibility.Visible;
                            g_lv2ob4.Visibility = Visibility.Visible;
                            g_lv2ob5.Visibility = Visibility.Visible;

                            t_lv2ob1.Visibility = Visibility.Visible;
                            t_lv2ob2.Visibility = Visibility.Visible;
                            t_lv2ob3.Visibility = Visibility.Visible;
                            t_lv2ob4.Visibility = Visibility.Visible;
                            t_lv2ob5.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (stage == 2)
                {
                    HitVectorIm.ob1 = new Vector(malletPosition.X - g_lv2ob1X, malletPosition.Y - g_lv2ob1Y);
                    HitVectorIm.ob2 = new Vector(malletPosition.X - g_lv2ob2X, malletPosition.Y - g_lv2ob2Y);
                    HitVectorIm.ob3 = new Vector(malletPosition.X - g_lv2ob3X, malletPosition.Y - g_lv2ob3Y);
                    HitVectorIm.ob4 = new Vector(malletPosition.X - g_lv2ob4X, malletPosition.Y - g_lv2ob4Y);
                    HitVectorIm.ob5 = new Vector(malletPosition.X - g_lv2ob5X, malletPosition.Y - g_lv2ob5Y);

                    HitVectorTx.ob1 = new Vector(malletPosition2.X - t_lv2ob1X, malletPosition2.Y - t_lv2ob1Y);
                    HitVectorTx.ob2 = new Vector(malletPosition2.X - t_lv2ob2X, malletPosition2.Y - t_lv2ob2Y);
                    HitVectorTx.ob3 = new Vector(malletPosition2.X - t_lv2ob3X, malletPosition2.Y - t_lv2ob3Y);
                    HitVectorTx.ob4 = new Vector(malletPosition2.X - t_lv2ob4X, malletPosition2.Y - t_lv2ob4Y);
                    HitVectorTx.ob5 = new Vector(malletPosition2.X - t_lv2ob5X, malletPosition2.Y - t_lv2ob5Y);

                    hitvectorInteract(ref lv2, HitVectorTx, HitVectorIm, g_lv2ob1, g_lv2ob2, g_lv2ob3, g_lv2ob4, g_lv2ob5, t_lv2ob1, t_lv2ob2, t_lv2ob3, t_lv2ob4, t_lv2ob5);

                    score2 = (lv2.ob1 + lv2.ob2 + lv2.ob3 + lv2.ob4 + lv2.ob5) * 100;
                    gscore = score1 + score2;
                    fscore = gscore + (100 * lives);
                    ScoreTextBlock.Text = gscore.ToString();
                    if (lv2.ob1 + lv2.ob2 + lv2.ob3 + lv2.ob4 + lv2.ob5 == 5)
                    {
                        if (lvlcomplete)
                        {
                            lvlcomplete = false;
                            tmerge = ((counter + 5) / 15) * 5;
                            timer1.Stop();
                            pictComplete.Visibility = Visibility.Visible;
                            tblComplete.Visibility = Visibility.Visible;
                            tblComplete.Text = txtLevel + stage.ToString() + txtComplete;
                            stopable = false;
                            counter = 4;
                            
                            HiasanAddedTimer.Visibility = Visibility.Visible;
                            AddedTimerItf.Visibility = Visibility.Visible;
                            AddedTimerItf.Text = "Waktos +" + tmerge.ToString();

                            timer1.Start();
                        }
                        if (!pictComplete.IsVisible)
                        {
                            lvlcomplete = true;
                            counter = 60 + tmerge;
                            timer1.Start();
                            tbTimer.Text = counter.ToString();
                            stage = 3;
                            g_lv3ob1.Visibility = Visibility.Visible;
                            g_lv3ob2.Visibility = Visibility.Visible;
                            g_lv3ob3.Visibility = Visibility.Visible;
                            g_lv3ob4.Visibility = Visibility.Visible;
                            g_lv3ob5.Visibility = Visibility.Visible;

                            t_lv3ob1.Visibility = Visibility.Visible;
                            t_lv3ob2.Visibility = Visibility.Visible;
                            t_lv3ob3.Visibility = Visibility.Visible;
                            t_lv3ob4.Visibility = Visibility.Visible;
                            t_lv3ob5.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (stage == 3)
                {
                    HitVectorIm.ob1 = new Vector(malletPosition.X - g_lv3ob1X, malletPosition.Y - g_lv3ob1Y);
                    HitVectorIm.ob2 = new Vector(malletPosition.X - g_lv3ob2X, malletPosition.Y - g_lv3ob2Y);
                    HitVectorIm.ob3 = new Vector(malletPosition.X - g_lv3ob3X, malletPosition.Y - g_lv3ob3Y);
                    HitVectorIm.ob4 = new Vector(malletPosition.X - g_lv3ob4X, malletPosition.Y - g_lv3ob4Y);
                    HitVectorIm.ob5 = new Vector(malletPosition.X - g_lv3ob5X, malletPosition.Y - g_lv3ob5Y);

                    HitVectorTx.ob1 = new Vector(malletPosition2.X - t_lv3ob1X, malletPosition2.Y - t_lv3ob1Y);
                    HitVectorTx.ob2 = new Vector(malletPosition2.X - t_lv3ob2X, malletPosition2.Y - t_lv3ob2Y);
                    HitVectorTx.ob3 = new Vector(malletPosition2.X - t_lv3ob3X, malletPosition2.Y - t_lv3ob3Y);
                    HitVectorTx.ob4 = new Vector(malletPosition2.X - t_lv3ob4X, malletPosition2.Y - t_lv3ob4Y);
                    HitVectorTx.ob5 = new Vector(malletPosition2.X - t_lv3ob5X, malletPosition2.Y - t_lv3ob5Y);

                    hitvectorInteract(ref lv3, HitVectorTx, HitVectorIm, g_lv3ob1, g_lv3ob2, g_lv3ob3, g_lv3ob4, g_lv3ob5, t_lv3ob1, t_lv3ob2, t_lv3ob3, t_lv3ob4, t_lv3ob5);

                    score3 = (lv3.ob1 + lv3.ob2 + lv3.ob3 + lv3.ob4 + lv3.ob5) * 100;
                    gscore = score1 + score2 + score3;
                    fscore = gscore + counter + (100 * lives);
                    ScoreTextBlock.Text = gscore.ToString();
                    if (lv3.ob1 + lv3.ob2 + lv3.ob3 + lv3.ob4 + lv3.ob5 == 5)
                    {
                        if (lvlcomplete)
                        {
                            lvlcomplete = false;
                            tmerge = ((counter + 5) / 15) * 5;
                            fscore = gscore + (100 * lives);
                            timer1.Stop();
                            pictComplete.Visibility = Visibility.Visible;
                            tblComplete.Visibility = Visibility.Visible;
                            tblComplete.Text = txtLevel + stage.ToString() + txtComplete;
                            stopable = false;
                            counter = 4;

                            HiasanAddedTimer.Visibility = Visibility.Visible;
                            AddedTimerItf.Visibility = Visibility.Visible;
                            AddedTimerItf.Text = "Waktos +" + tmerge.ToString();

                            timer1.Start();
                        }
                        if (!pictComplete.IsVisible)
                        {
                            lvlcomplete = true;
                            counter = 60 + tmerge;
                            timer1.Start();
                            tbTimer.Text = counter.ToString();
                            stage = 4;
                            g_lv4ob1.Visibility = Visibility.Visible;
                            g_lv4ob2.Visibility = Visibility.Visible;
                            g_lv4ob3.Visibility = Visibility.Visible;
                            g_lv4ob4.Visibility = Visibility.Visible;
                            g_lv4ob5.Visibility = Visibility.Visible;

                            t_lv4ob1.Visibility = Visibility.Visible;
                            t_lv4ob2.Visibility = Visibility.Visible;
                            t_lv4ob3.Visibility = Visibility.Visible;
                            t_lv4ob4.Visibility = Visibility.Visible;
                            t_lv4ob5.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (stage == 4)
                {
                    HitVectorIm.ob1 = new Vector(malletPosition.X - g_lv4ob1X, malletPosition.Y - g_lv4ob1Y);
                    HitVectorIm.ob2 = new Vector(malletPosition.X - g_lv4ob2X, malletPosition.Y - g_lv4ob2Y);
                    HitVectorIm.ob3 = new Vector(malletPosition.X - g_lv4ob3X, malletPosition.Y - g_lv4ob3Y);
                    HitVectorIm.ob4 = new Vector(malletPosition.X - g_lv4ob4X, malletPosition.Y - g_lv4ob4Y);
                    HitVectorIm.ob5 = new Vector(malletPosition.X - g_lv4ob5X, malletPosition.Y - g_lv4ob5Y);

                    HitVectorTx.ob1 = new Vector(malletPosition2.X - t_lv4ob1X, malletPosition2.Y - t_lv4ob1Y);
                    HitVectorTx.ob2 = new Vector(malletPosition2.X - t_lv4ob2X, malletPosition2.Y - t_lv4ob2Y);
                    HitVectorTx.ob3 = new Vector(malletPosition2.X - t_lv4ob3X, malletPosition2.Y - t_lv4ob3Y);
                    HitVectorTx.ob4 = new Vector(malletPosition2.X - t_lv4ob4X, malletPosition2.Y - t_lv4ob4Y);
                    HitVectorTx.ob5 = new Vector(malletPosition2.X - t_lv4ob5X, malletPosition2.Y - t_lv4ob5Y);

                    hitvectorInteract(ref lv4, HitVectorTx, HitVectorIm, g_lv4ob1, g_lv4ob2, g_lv4ob3, g_lv4ob4, g_lv4ob5, t_lv4ob1, t_lv4ob2, t_lv4ob3, t_lv4ob4, t_lv4ob5);

                    score4 = (lv4.ob1 + lv4.ob2 + lv4.ob3 + lv4.ob4 + lv4.ob5) * 100;
                    gscore = score1 + score2 + score3 + score4;
                    fscore = gscore + (100 * lives);
                    ScoreTextBlock.Text = gscore.ToString();
                    if (lv4.ob1 + lv4.ob2 + lv4.ob3 + lv4.ob4 + lv4.ob5 == 5)
                    {
                        if (lvlcomplete)
                        {
                            lvlcomplete = false;
                            tmerge = ((counter + 5) / 15) * 5;
                            timer1.Stop();
                            pictComplete.Visibility = Visibility.Visible;
                            tblComplete.Visibility = Visibility.Visible;
                            tblComplete.Text = txtLevel + stage.ToString() + txtComplete;
                            stopable = false;
                            counter = 4;

                            HiasanAddedTimer.Visibility = Visibility.Visible;
                            AddedTimerItf.Visibility = Visibility.Visible;
                            AddedTimerItf.Text = "Waktos +" + tmerge.ToString();

                            timer1.Start();
                        }
                        if (!pictComplete.IsVisible)
                        {
                            lvlcomplete = true;
                            counter = 60 + tmerge;
                            timer1.Start();
                            tbTimer.Text = counter.ToString();
                            stage = 5;
                            g_lv5ob1.Visibility = Visibility.Visible;
                            g_lv5ob2.Visibility = Visibility.Visible;
                            g_lv5ob3.Visibility = Visibility.Visible;
                            g_lv5ob4.Visibility = Visibility.Visible;
                            g_lv5ob5.Visibility = Visibility.Visible;

                            t_lv5ob1.Visibility = Visibility.Visible;
                            t_lv5ob2.Visibility = Visibility.Visible;
                            t_lv5ob3.Visibility = Visibility.Visible;
                            t_lv5ob4.Visibility = Visibility.Visible;
                            t_lv5ob5.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (stage == 5)
                {
                    HitVectorIm.ob1 = new Vector(malletPosition.X - g_lv5ob1X, malletPosition.Y - g_lv5ob1Y);
                    HitVectorIm.ob2 = new Vector(malletPosition.X - g_lv5ob2X, malletPosition.Y - g_lv5ob2Y);
                    HitVectorIm.ob3 = new Vector(malletPosition.X - g_lv5ob3X, malletPosition.Y - g_lv5ob3Y);
                    HitVectorIm.ob4 = new Vector(malletPosition.X - g_lv5ob4X, malletPosition.Y - g_lv5ob4Y);
                    HitVectorIm.ob5 = new Vector(malletPosition.X - g_lv5ob5X, malletPosition.Y - g_lv5ob5Y);

                    HitVectorTx.ob1 = new Vector(malletPosition2.X - t_lv5ob1X, malletPosition2.Y - t_lv5ob1Y);
                    HitVectorTx.ob2 = new Vector(malletPosition2.X - t_lv5ob2X, malletPosition2.Y - t_lv5ob2Y);
                    HitVectorTx.ob3 = new Vector(malletPosition2.X - t_lv5ob3X, malletPosition2.Y - t_lv5ob3Y);
                    HitVectorTx.ob4 = new Vector(malletPosition2.X - t_lv5ob4X, malletPosition2.Y - t_lv5ob4Y);
                    HitVectorTx.ob5 = new Vector(malletPosition2.X - t_lv5ob5X, malletPosition2.Y - t_lv5ob5Y);

                    hitvectorInteract(ref lv5, HitVectorTx, HitVectorIm, g_lv5ob1, g_lv5ob2, g_lv5ob3, g_lv5ob4, g_lv5ob5, t_lv5ob1, t_lv5ob2, t_lv5ob3, t_lv5ob4, t_lv5ob5);

                    score5 = (lv5.ob1 + lv5.ob2 + lv5.ob3 + lv5.ob4 + lv5.ob5) * 100;
                    gscore = score1 + score2 + score3 + score4 + score5;
                    fscore = gscore + (100 * lives);
                    ScoreTextBlock.Text = gscore.ToString();
                    if (lv5.ob1 + lv5.ob2 + lv5.ob3 + lv5.ob4 + lv5.ob5 == 5)
                    {
                        if (lvlcomplete)
                        {
                            lvlcomplete = false;
                            tmerge = ((counter + 5) / 15) * 5;
                            timer1.Stop();
                            pictComplete.Visibility = Visibility.Visible;
                            tblComplete.Visibility = Visibility.Visible;
                            tblComplete.Text = txtLevel + stage.ToString() + txtComplete;
                            stopable = false;
                            counter = 4;

                            HiasanAddedTimer.Visibility = Visibility.Visible;
                            AddedTimerItf.Visibility = Visibility.Visible;
                            AddedTimerItf.Text = "Waktos +" + tmerge.ToString();

                            timer1.Start();
                        }
                        if (!pictComplete.IsVisible)
                        {
                            lvlcomplete = true;
                            counter = 60 + tmerge;
                            timer1.Start();
                            tbTimer.Text = counter.ToString();
                            stage = 6;
                            g_lv6ob1.Visibility = Visibility.Visible;
                            g_lv6ob2.Visibility = Visibility.Visible;
                            g_lv6ob3.Visibility = Visibility.Visible;
                            g_lv6ob4.Visibility = Visibility.Visible;
                            g_lv6ob5.Visibility = Visibility.Visible;

                            t_lv6ob1.Visibility = Visibility.Visible;
                            t_lv6ob2.Visibility = Visibility.Visible;
                            t_lv6ob3.Visibility = Visibility.Visible;
                            t_lv6ob4.Visibility = Visibility.Visible;
                            t_lv6ob5.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (stage == 6)
                {
                    HitVectorIm.ob1 = new Vector(malletPosition.X - g_lv6ob1X, malletPosition.Y - g_lv6ob1Y);
                    HitVectorIm.ob2 = new Vector(malletPosition.X - g_lv6ob2X, malletPosition.Y - g_lv6ob2Y);
                    HitVectorIm.ob3 = new Vector(malletPosition.X - g_lv6ob3X, malletPosition.Y - g_lv6ob3Y);
                    HitVectorIm.ob4 = new Vector(malletPosition.X - g_lv6ob4X, malletPosition.Y - g_lv6ob4Y);
                    HitVectorIm.ob5 = new Vector(malletPosition.X - g_lv6ob5X, malletPosition.Y - g_lv6ob5Y);

                    HitVectorTx.ob1 = new Vector(malletPosition2.X - t_lv6ob1X, malletPosition2.Y - t_lv6ob1Y);
                    HitVectorTx.ob2 = new Vector(malletPosition2.X - t_lv6ob2X, malletPosition2.Y - t_lv6ob2Y);
                    HitVectorTx.ob3 = new Vector(malletPosition2.X - t_lv6ob3X, malletPosition2.Y - t_lv6ob3Y);
                    HitVectorTx.ob4 = new Vector(malletPosition2.X - t_lv6ob4X, malletPosition2.Y - t_lv6ob4Y);
                    HitVectorTx.ob5 = new Vector(malletPosition2.X - t_lv6ob5X, malletPosition2.Y - t_lv6ob5Y);

                    hitvectorInteract(ref lv6, HitVectorTx, HitVectorIm, g_lv6ob1, g_lv6ob2, g_lv6ob3, g_lv6ob4, g_lv6ob5, t_lv6ob1, t_lv6ob2, t_lv6ob3, t_lv6ob4, t_lv6ob5);

                    score6 = (lv6.ob1 + lv6.ob2 + lv6.ob3 + lv6.ob4 + lv6.ob5) * 100;
                    gscore = score1 + score2 + score3 + score4 + score5 + score6;
                    fscore = gscore + (100 * lives);
                    ScoreTextBlock.Text = gscore.ToString();
                    if (lv6.ob1 + lv6.ob2 + lv6.ob3 + lv6.ob4 + lv6.ob5 == 5)
                    {
                        if (lvlcomplete)
                        {
                            lvlcomplete = false;
                            tmerge = ((counter + 5) / 15) * 5;
                            timer1.Stop();
                            pictComplete.Visibility = Visibility.Visible;
                            tblComplete.Visibility = Visibility.Visible;
                            tblComplete.Text = txtLevel + stage.ToString() + txtComplete;
                            stopable = false;
                            counter = 4;

                            HiasanAddedTimer.Visibility = Visibility.Visible;
                            AddedTimerItf.Visibility = Visibility.Visible;
                            AddedTimerItf.Text = "Waktos +" + tmerge.ToString();

                            timer1.Start();
                        }
                        if (!pictComplete.IsVisible)
                        {
                            lvlcomplete = true;
                            counter = 60 + tmerge;
                            timer1.Start();
                            tbTimer.Text = counter.ToString();
                            stage = 7;
                            g_lv7ob1.Visibility = Visibility.Visible;
                            g_lv7ob2.Visibility = Visibility.Visible;
                            g_lv7ob3.Visibility = Visibility.Visible;
                            g_lv7ob4.Visibility = Visibility.Visible;
                            g_lv7ob5.Visibility = Visibility.Visible;

                            t_lv7ob1.Visibility = Visibility.Visible;
                            t_lv7ob2.Visibility = Visibility.Visible;
                            t_lv7ob3.Visibility = Visibility.Visible;
                            t_lv7ob4.Visibility = Visibility.Visible;
                            t_lv7ob5.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (stage == 7)
                {
                    HitVectorIm.ob1 = new Vector(malletPosition.X - g_lv7ob1X, malletPosition.Y - g_lv7ob1Y);
                    HitVectorIm.ob2 = new Vector(malletPosition.X - g_lv7ob2X, malletPosition.Y - g_lv7ob2Y);
                    HitVectorIm.ob3 = new Vector(malletPosition.X - g_lv7ob3X, malletPosition.Y - g_lv7ob3Y);
                    HitVectorIm.ob4 = new Vector(malletPosition.X - g_lv7ob4X, malletPosition.Y - g_lv7ob4Y);
                    HitVectorIm.ob5 = new Vector(malletPosition.X - g_lv7ob5X, malletPosition.Y - g_lv7ob5Y);

                    HitVectorTx.ob1 = new Vector(malletPosition2.X - t_lv7ob1X, malletPosition2.Y - t_lv7ob1Y);
                    HitVectorTx.ob2 = new Vector(malletPosition2.X - t_lv7ob2X, malletPosition2.Y - t_lv7ob2Y);
                    HitVectorTx.ob3 = new Vector(malletPosition2.X - t_lv7ob3X, malletPosition2.Y - t_lv7ob3Y);
                    HitVectorTx.ob4 = new Vector(malletPosition2.X - t_lv7ob4X, malletPosition2.Y - t_lv7ob4Y);
                    HitVectorTx.ob5 = new Vector(malletPosition2.X - t_lv7ob5X, malletPosition2.Y - t_lv7ob5Y);

                    hitvectorInteract(ref lv7, HitVectorTx, HitVectorIm, g_lv7ob1, g_lv7ob2, g_lv7ob3, g_lv7ob4, g_lv7ob5, t_lv7ob1, t_lv7ob2, t_lv7ob3, t_lv7ob4, t_lv7ob5);

                    score7 = (lv7.ob1 + lv7.ob2 + lv7.ob3 + lv7.ob4 + lv7.ob5) * 100;
                    gscore = score1 + score2 + score3 + score4 + score5 + score6 + score7;
                    fscore = gscore + (100 * lives);
                    ScoreTextBlock.Text = gscore.ToString();
                    if (lv7.ob1 + lv7.ob2 + lv7.ob3 + lv7.ob4 + lv7.ob5 == 5)
                    {
                        if (lvlcomplete)
                        {
                            lvlcomplete = false;
                            tmerge = ((counter + 5) / 10) * 5;
                            timer1.Stop();
                            pictComplete.Visibility = Visibility.Visible;
                            tblComplete.Visibility = Visibility.Visible;
                            tblComplete.Text = txtLevel + stage.ToString() + txtComplete;
                            stopable = false;
                            counter = 4;

                            HiasanAddedTimer.Visibility = Visibility.Visible;
                            AddedTimerItf.Visibility = Visibility.Visible;
                            AddedTimerItf.Text = "Waktos +" + tmerge.ToString();

                            timer1.Start();
                        }
                        if (!pictComplete.IsVisible)
                        {
                            lvlcomplete = true;
                            counter = 60 + tmerge;
                            timer1.Start();
                            tbTimer.Text = counter.ToString();
                            stage = 8;
                            g_lv8ob1.Visibility = Visibility.Visible;
                            g_lv8ob2.Visibility = Visibility.Visible;
                            g_lv8ob3.Visibility = Visibility.Visible;
                            g_lv8ob4.Visibility = Visibility.Visible;
                            g_lv8ob5.Visibility = Visibility.Visible;

                            t_lv8ob1.Visibility = Visibility.Visible;
                            t_lv8ob2.Visibility = Visibility.Visible;
                            t_lv8ob3.Visibility = Visibility.Visible;
                            t_lv8ob4.Visibility = Visibility.Visible;
                            t_lv8ob5.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (stage == 8)
                {
                    HitVectorIm.ob1 = new Vector(malletPosition.X - g_lv8ob1X, malletPosition.Y - g_lv8ob1Y);
                    HitVectorIm.ob2 = new Vector(malletPosition.X - g_lv8ob2X, malletPosition.Y - g_lv8ob2Y);
                    HitVectorIm.ob3 = new Vector(malletPosition.X - g_lv8ob3X, malletPosition.Y - g_lv8ob3Y);
                    HitVectorIm.ob4 = new Vector(malletPosition.X - g_lv8ob4X, malletPosition.Y - g_lv8ob4Y);
                    HitVectorIm.ob5 = new Vector(malletPosition.X - g_lv8ob5X, malletPosition.Y - g_lv8ob5Y);

                    HitVectorTx.ob1 = new Vector(malletPosition2.X - t_lv8ob1X, malletPosition2.Y - t_lv8ob1Y);
                    HitVectorTx.ob2 = new Vector(malletPosition2.X - t_lv8ob2X, malletPosition2.Y - t_lv8ob2Y);
                    HitVectorTx.ob3 = new Vector(malletPosition2.X - t_lv8ob3X, malletPosition2.Y - t_lv8ob3Y);
                    HitVectorTx.ob4 = new Vector(malletPosition2.X - t_lv8ob4X, malletPosition2.Y - t_lv8ob4Y);
                    HitVectorTx.ob5 = new Vector(malletPosition2.X - t_lv8ob5X, malletPosition2.Y - t_lv8ob5Y);

                    hitvectorInteract(ref lv8, HitVectorTx, HitVectorIm, g_lv8ob1, g_lv8ob2, g_lv8ob3, g_lv8ob4, g_lv8ob5, t_lv8ob1, t_lv8ob2, t_lv8ob3, t_lv8ob4, t_lv8ob5);

                    score8 = (lv8.ob1 + lv8.ob2 + lv8.ob3 + lv8.ob4 + lv8.ob5) * 100;
                    gscore = score1 + score2 + score3 + score4 + score5 + score6 + score7 + score8;
                    fscore = gscore + (100 * lives);
                    ScoreTextBlock.Text = gscore.ToString();
                    if (lv8.ob1 + lv8.ob2 + lv8.ob3 + lv8.ob4 + lv8.ob5 == 5)
                    {
                        if (lvlcomplete)
                        {
                            lvlcomplete = false;
                            timer1.Stop();
                            pictComplete.Visibility = Visibility.Visible;
                            tblComplete.Visibility = Visibility.Visible;
                            tblComplete.Text = txtLevel + stage.ToString() + txtComplete;
                            stopable = false;
                            counter = 4;


                            timer1.Start();
                        }
                        if (!pictComplete.IsVisible)
                        {
                            lvlcomplete = true;

                            stage = 12;
                        }
                    }
                }
                else if (stage == 11)
                {
                    if (lvlcomplete)
                    {
                        lvlcomplete = false;
                        timer1.Stop();
                        gameOver.Visibility = Visibility.Visible;
                        HiasanAddedTimer.Visibility = Visibility.Visible;
                        FScoreGameOver.Visibility = Visibility.Visible;
                        FScoreGameOver.Text = "Penten = " + fscore.ToString();
                        stopable = false;
                        counter = 4;
                        timer1.Start();
                    }
                    if (!gameOver.IsVisible)
                    {
                        lvlcomplete = true;
                        lv1.zeros();
                        lv2.zeros();
                        lv3.zeros();

                        HiasanAddedTimer.Visibility = Visibility.Hidden;
                        FScoreGameOver.Visibility = Visibility.Hidden;
                        
                        lives = 3;
                        stage = 0;
                    }
                }
                else if (stage == 12) // all stage clear

                {
                    if (lvlcomplete)
                    {
                        lvlcomplete = false;
                        timer1.Stop();
                        FinalBoard.Visibility = Visibility.Visible;
                        FinalScore.Visibility = Visibility.Visible;
                        FinalText.Visibility = Visibility.Visible;
                        FinalScore.Text = "Peunten\n" + fscore.ToString();
                        stopable = false;
                        counter = 7;
                        timer1.Start();
                    }
                    if (!FinalBoard.IsVisible)
                    {
                        lvlcomplete = true;
                        lv1.zeros();
                        lv2.zeros();
                        lv3.zeros();

                        HiasanAddedTimer.Visibility = Visibility.Hidden;
                        FScoreGameOver.Visibility = Visibility.Hidden;
                        lives = 3;
                        stage = 0;
                    }

                }
                else if (stage == 13)//developer
                {
                    start.Visibility = Visibility.Hidden;
                    tblNgawitan.Visibility = Visibility.Hidden;
                    start2.Visibility = Visibility.Hidden;

                    setting.Visibility = Visibility.Hidden;
                    tblPangaturan.Visibility = Visibility.Hidden;

                    developer.Visibility = Visibility.Hidden;
                    tblPamekar.Visibility = Visibility.Hidden;

                    cara_maen.Visibility = Visibility.Hidden;
                    tblCaraMaen.Visibility = Visibility.Hidden;

                    exit.Visibility = Visibility.Hidden;
                    tblKaluar.Visibility = Visibility.Hidden;

                    pemekar.Visibility = Visibility.Visible;
                    back.Visibility = Visibility.Visible;
                    tblUih.Visibility = Visibility.Visible;

                    Vector hitVector_back = new Vector(malletPosition.X - backX, malletPosition.Y - backY);
                    if (hitVector_back.Length < malletHitRadius)
                    {
                        pemekar.Visibility = Visibility.Hidden;
                        back.Visibility = Visibility.Hidden;
                        tblUih.Visibility = Visibility.Hidden;
                        stage = 0;
                    }
                }
                else if (stage == 14)//settings
                {
                    start.Visibility = Visibility.Hidden;
                    tblNgawitan.Visibility = Visibility.Hidden;
                    setting.Visibility = Visibility.Hidden;
                    tblPangaturan.Visibility = Visibility.Hidden;
                    developer.Visibility = Visibility.Hidden;
                    tblPamekar.Visibility = Visibility.Hidden;
                    cara_maen.Visibility = Visibility.Hidden;
                    tblCaraMaen.Visibility = Visibility.Hidden;
                    exit.Visibility = Visibility.Hidden;
                    tblKaluar.Visibility = Visibility.Hidden;

                    back.Visibility = Visibility.Visible;
                    tblUih.Visibility = Visibility.Visible;
                    sun.Visibility = Visibility.Visible;
                    id.Visibility = Visibility.Visible;
                    us.Visibility = Visibility.Visible;
                    Vector hitVector2_start = new Vector(malletPosition2.X - start2X, malletPosition2.Y - start2Y);

                    if (sound)
                    {
                        speaker_on.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        speaker_off.Visibility = Visibility.Visible;
                    }
                    Vector hitVector_sun = new Vector(malletPosition.X - sunX, malletPosition.Y - sunY);
                    Vector hitVector_id = new Vector(malletPosition.X - idX, malletPosition.Y - idY);
                    Vector hitVector_us = new Vector(malletPosition.X - usX, malletPosition.Y - usY);
                    Vector hitVector_back = new Vector(malletPosition.X - backX, malletPosition.Y - backY);
                    Vector hitVector_speaker = new Vector(malletPosition.X - speaker_onX, malletPosition.Y - speaker_onY);
                    if (hitVector_speaker.Length < malletHitRadius && hitVector2_start.Length < malletHitRadius)
                    {
                        if (sound && invincible == 0)
                        {
                            musik.Stop();
                            sound = !sound;
                            speaker_on.Visibility = Visibility.Hidden;
                            speaker_off.Visibility = Visibility.Visible;
                            invincible = 2;
                            timer2.Start();
                            
                        }
                        else if(!sound && invincible == 0)
                        {
                            musik.PlayLooping();
                            sound = !sound;
                            speaker_off.Visibility = Visibility.Hidden;
                            speaker_on.Visibility = Visibility.Visible;
                            invincible = 1;
                            timer2.Start();
                        }
                    }
                    if (hitVector_sun.Length < malletHitRadius && hitVector2_start.Length < malletHitRadius)
                    {
                        changeLanguage(1);
                    }
                    if (hitVector_id.Length < malletHitRadius && hitVector2_start.Length < malletHitRadius)
                    {
                        changeLanguage(2);
                    }
                    if (hitVector_us.Length < malletHitRadius && hitVector2_start.Length < malletHitRadius)
                    {
                        changeLanguage(3);
                    }
                    if (hitVector_back.Length < malletHitRadius && hitVector2_start.Length < malletHitRadius)
                    {
                        if (sound)
                        {
                            speaker_on.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            speaker_off.Visibility = Visibility.Hidden;
                        }
                        back.Visibility = Visibility.Hidden;
                        tblUih.Visibility = Visibility.Hidden;
                        sun.Visibility = Visibility.Hidden;
                        id.Visibility = Visibility.Hidden;
                        us.Visibility = Visibility.Hidden;

                        stage = 0;
                    }

                }
                else if (stage == 15)//how to play
                {
                    start.Visibility = Visibility.Hidden;
                    tblNgawitan.Visibility = Visibility.Hidden;
                    start2.Visibility = Visibility.Hidden;
                    setting.Visibility = Visibility.Hidden;
                    tblPangaturan.Visibility = Visibility.Hidden;
                    developer.Visibility = Visibility.Hidden;
                    tblPamekar.Visibility = Visibility.Hidden;
                    cara_maen.Visibility = Visibility.Hidden;
                    tblCaraMaen.Visibility = Visibility.Hidden;
                    exit.Visibility = Visibility.Hidden;
                    tblKaluar.Visibility = Visibility.Hidden;
                    howPlay.Visibility = Visibility.Visible;
                    tbHP_hand.Visibility = Visibility.Visible;
                    tbHP_heart.Visibility = Visibility.Visible;
                    tbHP_break.Visibility = Visibility.Visible;
                    tbHP_timer.Visibility = Visibility.Visible;
                    back.Visibility = Visibility.Visible;
                    tblUih.Visibility = Visibility.Visible;

                    Vector hitVector_back = new Vector(malletPosition.X - backX, malletPosition.Y - backY);
                    if (hitVector_back.Length < malletHitRadius)
                    {
                        howPlay.Visibility = Visibility.Hidden;
                        tbHP_hand.Visibility = Visibility.Hidden;
                        tbHP_heart.Visibility = Visibility.Hidden;
                        tbHP_break.Visibility = Visibility.Hidden;
                        tbHP_timer.Visibility = Visibility.Hidden;
                        back.Visibility = Visibility.Hidden;
                        tblUih.Visibility = Visibility.Hidden;
                        stage = 0;
                    }
                }
            }

            //if (bugY > gameCanvas.Height)
            //{
            //    bugX = rand.Next(0, (int)gameCanvas.Width - (int)bugImage.Width);
            //    Canvas.SetLeft(bugImage, bugX);
            //    bugY = -bugImage.Height;
            //}
        }
        #endregion 

        #region kumpulan fungsi lainnya

        void hitvectorInteract(ref byte state, Vector v1, Vector v2, Image satu, Image dua)
        {
            if (v1.Length < malletHitRadius && v2.Length < malletHitRadius)
            {
                //.Children.Remove(satu);
                //gameCanvas.Children.Remove(dua);  
                satu.Visibility = Visibility.Hidden;
                dua.Visibility = Visibility.Hidden;

                state = 1;
                //scorePlayer.Play();
            }
        }

        void hitvectorInteract(Vector v1, Vector v2, Image satu, Image dua)
        {
            if (v1.Length < malletHitRadius && v2.Length < malletHitRadius)
            {
                if(satu.IsVisible && dua.IsVisible)
                {
                    if (lives == 3)
                    {
                        Heart_isi3.Visibility = Visibility.Hidden;
                        Heart_patah3.Visibility = Visibility.Visible;
                        lives--;
                        invincible = 3;
                        timer2.Start();
                    }
                    else if (lives == 2)
                    {
                        Heart_isi2.Visibility = Visibility.Hidden;
                        Heart_patah2.Visibility = Visibility.Visible;
                        lives--;
                        invincible = 3;
                        timer2.Start();
                    }
                    else if (lives == 1)
                    {
                        gameOv();
                        stage = 11;
                    }
                }
            }
        }

        void hitvectorInteract(ref StatePoint lev, HitVector vectorText, HitVector vectorImage,
            Image li1, Image li2, Image li3, Image li4, Image li5, Image ri1, Image ri2, Image ri3, Image ri4, Image ri5)
        {
            hitvectorInteract(ref lev.ob1, vectorText.getob1(), vectorImage.getob1(), li1, ri1);
            hitvectorInteract(ref lev.ob2, vectorText.getob2(), vectorImage.getob2(), li2, ri2);
            hitvectorInteract(ref lev.ob3, vectorText.getob3(), vectorImage.getob3(), li3, ri3);
            hitvectorInteract(ref lev.ob4, vectorText.getob4(), vectorImage.getob4(), li4, ri4);
            hitvectorInteract(ref lev.ob5, vectorText.getob5(), vectorImage.getob5(), li5, ri5);
            
            if (invincible == 0)
            {
                hitvectorInteract(vectorText.getob1(), vectorImage.getob2(), li1, ri2);
                hitvectorInteract(vectorText.getob1(), vectorImage.getob3(), li1, ri3);
                hitvectorInteract(vectorText.getob1(), vectorImage.getob4(), li1, ri4);
                hitvectorInteract(vectorText.getob1(), vectorImage.getob5(), li1, ri5);

                hitvectorInteract(vectorText.getob2(), vectorImage.getob1(), li2, ri1);
                hitvectorInteract(vectorText.getob2(), vectorImage.getob3(), li2, ri3);
                hitvectorInteract(vectorText.getob2(), vectorImage.getob4(), li2, ri4);
                hitvectorInteract(vectorText.getob2(), vectorImage.getob5(), li2, ri5);

                hitvectorInteract(vectorText.getob3(), vectorImage.getob1(), li3, ri1);
                hitvectorInteract(vectorText.getob3(), vectorImage.getob2(), li3, ri2);
                hitvectorInteract(vectorText.getob3(), vectorImage.getob4(), li3, ri4);
                hitvectorInteract(vectorText.getob3(), vectorImage.getob5(), li3, ri5);

                hitvectorInteract(vectorText.getob4(), vectorImage.getob1(), li4, ri1);
                hitvectorInteract(vectorText.getob4(), vectorImage.getob2(), li4, ri2);
                hitvectorInteract(vectorText.getob4(), vectorImage.getob3(), li4, ri3);
                hitvectorInteract(vectorText.getob4(), vectorImage.getob5(), li4, ri5);

                hitvectorInteract(vectorText.getob5(), vectorImage.getob1(), li5, ri1);
                hitvectorInteract(vectorText.getob5(), vectorImage.getob2(), li5, ri2);
                hitvectorInteract(vectorText.getob5(), vectorImage.getob3(), li5, ri3);
                hitvectorInteract(vectorText.getob5(), vectorImage.getob4(), li5, ri4);
            }
        }


        #endregion

        #region Event Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //setupGameImage();
            setupKinect();
            startGameThread();
            scorePlayer = new SoundPlayer("ding.wav");
            musik = new SoundPlayer("kacapisuling.wav");
            flash.Visibility = Visibility.Visible;
            musik.PlayLooping();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            shutdownKinect();
        }

        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (FileStream fileStream = new FileStream(string.Format("{0}.Jpg", Guid.NewGuid().ToString()), System.IO.FileMode.Create))
            {
                BitmapSource imageSource = (BitmapSource)kinectVideoImage.Source;
                JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.Frames.Add(BitmapFrame.Create(imageSource));
                jpegEncoder.Save(fileStream);
                fileStream.Close();
            }


        }
        private void gameOv()
        {

            Heart_isi1.Visibility = Visibility.Hidden;
            Heart_isi2.Visibility = Visibility.Hidden;
            Heart_isi3.Visibility = Visibility.Hidden;
            Heart_patah2.Visibility = Visibility.Hidden;
            Heart_patah3.Visibility = Visibility.Hidden;

            switch (stage)
            {
                case 1:
                    {
                        g_lv1ob1.Visibility = Visibility.Hidden;
                        g_lv1ob2.Visibility = Visibility.Hidden;
                        g_lv1ob3.Visibility = Visibility.Hidden;
                        g_lv1ob4.Visibility = Visibility.Hidden;
                        g_lv1ob5.Visibility = Visibility.Hidden;
                        t_lv1ob1.Visibility = Visibility.Hidden;
                        t_lv1ob2.Visibility = Visibility.Hidden;
                        t_lv1ob3.Visibility = Visibility.Hidden;
                        t_lv1ob4.Visibility = Visibility.Hidden;
                        t_lv1ob5.Visibility = Visibility.Hidden;
                        break;
                    }
                case 2:
                    {
                        g_lv2ob1.Visibility = Visibility.Hidden;
                        g_lv2ob2.Visibility = Visibility.Hidden;
                        g_lv2ob3.Visibility = Visibility.Hidden;
                        g_lv2ob4.Visibility = Visibility.Hidden;
                        g_lv2ob5.Visibility = Visibility.Hidden;
                        t_lv2ob1.Visibility = Visibility.Hidden;
                        t_lv2ob2.Visibility = Visibility.Hidden;
                        t_lv2ob3.Visibility = Visibility.Hidden;
                        t_lv2ob4.Visibility = Visibility.Hidden;
                        t_lv2ob5.Visibility = Visibility.Hidden;
                        break;
                    }
                case 3:
                    {
                        g_lv3ob1.Visibility = Visibility.Hidden;
                        g_lv3ob2.Visibility = Visibility.Hidden;
                        g_lv3ob3.Visibility = Visibility.Hidden;
                        g_lv3ob4.Visibility = Visibility.Hidden;
                        g_lv3ob5.Visibility = Visibility.Hidden;
                        t_lv3ob1.Visibility = Visibility.Hidden;
                        t_lv3ob2.Visibility = Visibility.Hidden;
                        t_lv3ob3.Visibility = Visibility.Hidden;
                        t_lv3ob4.Visibility = Visibility.Hidden;
                        t_lv3ob5.Visibility = Visibility.Hidden;
                        break;
                    }
                case 4:
                    {
                        g_lv4ob1.Visibility = Visibility.Hidden;
                        g_lv4ob2.Visibility = Visibility.Hidden;
                        g_lv4ob3.Visibility = Visibility.Hidden;
                        g_lv4ob4.Visibility = Visibility.Hidden;
                        g_lv4ob5.Visibility = Visibility.Hidden;
                        t_lv4ob1.Visibility = Visibility.Hidden;
                        t_lv4ob2.Visibility = Visibility.Hidden;
                        t_lv4ob3.Visibility = Visibility.Hidden;
                        t_lv4ob4.Visibility = Visibility.Hidden;
                        t_lv4ob5.Visibility = Visibility.Hidden;
                        break;
                    }
                case 5:
                    {
                        g_lv5ob1.Visibility = Visibility.Hidden;
                        g_lv5ob2.Visibility = Visibility.Hidden;
                        g_lv5ob3.Visibility = Visibility.Hidden;
                        g_lv5ob4.Visibility = Visibility.Hidden;
                        g_lv5ob5.Visibility = Visibility.Hidden;
                        t_lv5ob1.Visibility = Visibility.Hidden;
                        t_lv5ob2.Visibility = Visibility.Hidden;
                        t_lv5ob3.Visibility = Visibility.Hidden;
                        t_lv5ob4.Visibility = Visibility.Hidden;
                        t_lv5ob5.Visibility = Visibility.Hidden;
                        break;
                    }
                case 6:
                    {
                        g_lv6ob1.Visibility = Visibility.Hidden;
                        g_lv6ob2.Visibility = Visibility.Hidden;
                        g_lv6ob3.Visibility = Visibility.Hidden;
                        g_lv6ob4.Visibility = Visibility.Hidden;
                        g_lv6ob5.Visibility = Visibility.Hidden;
                        t_lv6ob1.Visibility = Visibility.Hidden;
                        t_lv6ob2.Visibility = Visibility.Hidden;
                        t_lv6ob3.Visibility = Visibility.Hidden;
                        t_lv6ob4.Visibility = Visibility.Hidden;
                        t_lv6ob5.Visibility = Visibility.Hidden;
                        break;
                    }
                case 7:
                    {
                        g_lv7ob1.Visibility = Visibility.Hidden;
                        g_lv7ob2.Visibility = Visibility.Hidden;
                        g_lv7ob3.Visibility = Visibility.Hidden;
                        g_lv7ob4.Visibility = Visibility.Hidden;
                        g_lv7ob5.Visibility = Visibility.Hidden;
                        t_lv7ob1.Visibility = Visibility.Hidden;
                        t_lv7ob2.Visibility = Visibility.Hidden;
                        t_lv7ob3.Visibility = Visibility.Hidden;
                        t_lv7ob4.Visibility = Visibility.Hidden;
                        t_lv7ob5.Visibility = Visibility.Hidden;
                        break;
                    }
                case 8:
                    {
                        g_lv8ob1.Visibility = Visibility.Hidden;
                        g_lv8ob2.Visibility = Visibility.Hidden;
                        g_lv8ob3.Visibility = Visibility.Hidden;
                        g_lv8ob4.Visibility = Visibility.Hidden;
                        g_lv8ob5.Visibility = Visibility.Hidden;
                        t_lv8ob1.Visibility = Visibility.Hidden;
                        t_lv8ob2.Visibility = Visibility.Hidden;
                        t_lv8ob3.Visibility = Visibility.Hidden;
                        t_lv8ob4.Visibility = Visibility.Hidden;
                        t_lv8ob5.Visibility = Visibility.Hidden;
                        break;
                    }
            }
        }
        private void changeLanguage(byte a)
        {
            switch (a)
            {
                case 1 :
                    {
                        txtComplete = " RÉNGSÉ";
                        txtLevel = "TINGKAT ";
                        txtStart = " DIKAWITAN";
                        tblCaraMaen.Text = "Cara Ulin";
                        tblKaluar.Text = "Kaluar";
                        tblPamekar.Text = "Pamekar";
                        tblNgawitan.Text = "Ngawitan";
                        tblPangaturan.Text = "Pangaturan";
                        tblUih.Text = "Uihan";
                        tbTimer.Text = txtTime = "Waktos";
                        ScoreTextBlock.Text = txtScore = "Peunteun";
                        AddedTimerItf.Text = "Waktos + ";
                        FScoreGameOver.Text = "Peunteun";
                        FinalText.Text = "Kaulinan Réngsé";
                        FinalScore.Text = "Peunteun";
                        tbHP_hand.Text = "Paké leungeun Anjeun pikeun milih sarta cocog gambar kalawan téks Sunda.";
                        tbHP_heart.Text = "Dina awal kaulinan anjeun dibikeun tilu nyawa kudu dijaga nepi ka ahir game.";
                        tbHP_break.Text = "Ulah dugi ka ngajalankeun kaluar kahirupan, kusabab lamun ngajalankeun kaluar maneh kudu ngulang kaulinan ti mimiti.";
                        tbHP_timer.Text = "Anjeun ogé dibéré waktos dina ngaréngsékeun tiap tahapan-Na. Unggal waktos Anjeun buka nepi panggung, anjeun dibéré waktos bonus. Lamun waktu ngalir kaluar, kaulinan réngsé.";
                        break;
                    }
                case 2:
                    {
                        txtComplete = " SELESAI";
                        txtLevel = "LEVEL ";
                        txtStart = " DIMULAI";
                        tblCaraMaen.Text = "Cara Main";
                        tblKaluar.Text = "Keluar";
                        tblPamekar.Text = "Pengembang";
                        tblNgawitan.Text = "Mulai Permainan";
                        tblPangaturan.Text = "Pengaturan";
                        tblUih.Text = "Kembali";
                        tbTimer.Text = txtTime = "Waktu";
                        ScoreTextBlock.Text = txtScore = "Nilai";
                        AddedTimerItf.Text = "Waktu + ";
                        FScoreGameOver.Text = "Nilai";
                        FinalText.Text = "Permainan Selesai";
                        FinalScore.Text = "Nilai";
                        tbHP_hand.Text = "Gunakan Kedua tangan kamu untuk memilih menu dan mencocokkan gambar dengan teks sunda.";
                        tbHP_heart.Text = "Pada awal permainan kamu diberikan 3 nyawa yang harus dipertahankan sampai akhir permainan.";
                        tbHP_break.Text = "Jangan sampai nyawa kamu habis, karena jika habis kamu harus mengulang permainan dari awal.";
                        tbHP_timer.Text = "Kamu juga diberikan waktu dalam menyelesaikan setiap stage-nya. Setiap kamu naik stage, kamu diberi bonus waktu. Jika waktu habis, permainan berakhir.";
                        break;
                    }
                case 3:
                    {
                        txtComplete = " COMPLETED";
                        txtLevel = "LEVEL ";
                        txtStart = " START";
                        tblCaraMaen.Text = "How to Play";
                        tblKaluar.Text = "Exit";
                        tblPamekar.Text = "Developer";
                        tblNgawitan.Text = "Start Game";
                        tblPangaturan.Text = "Settings";
                        tblUih.Text = "Back";
                        tbTimer.Text = txtTime = "Time";
                        ScoreTextBlock.Text = txtScore = "Score";
                        AddedTimerItf.Text = "Time + ";
                        FScoreGameOver.Text = "Score";
                        FinalText.Text = "Game Finished";
                        FinalScore.Text = "Score";
                        tbHP_hand.Text = "Use both hand to choose menus and to match pictures with sunda words.";
                        tbHP_heart.Text = "At the, player given 3 lives that must be saved until the end of the game.";
                        tbHP_break.Text = "When your lives reaches zero, you must restart the game from the beginning.";
                        tbHP_timer.Text = "There's also times given on every stage. Time bonus on every completed stage, but if the times running out, the game ends.";
                        break;
                    }
            }
        }
    }
}
