using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using OPCAutomation;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ZXing;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        int ma_sp;
        string sensor = "false";
        int rst_sensor = 0;
        //int gaptruot = 0;
        int cam = 0;
        int mode = 0;
        Boolean sta = false;
        int counter = 0;
        Boolean dang_gap = false;
        double lechdc = 0;
        double Xlech = -62;
        double Ylech = -25;
        double X2_X1 = 0;
        double Y2_Y1 = 0;
        int R, B, G;
        string Zhome = "-192";                    // độ cao Z tại (0,0,0)
        string Zgapvat = "-242";
        string Zchogap = "-224";
        string Znangvat = "-212";
        double ho1 = -12, ho2 = -18, ho3 = -19;   // số sethome trên tia portal  // vị trí chạm công tắc hành trình
        string TR1 = "-4.01", TR2 = "49.85", TR3 = "17.78";   // góc theta thả vật màu đỏ của 3 động cơ step
        string TB1 = "54.84", TB2 = "32.05", TB3 = "-10.97";   // góc theta thả vật màu blue của 3 động cơ step
        string TG1 = "19.53", TG2 = "31.81", TG3 = "-7.73";    // góc theta thả vật màu green của 3 động cơ step
        //Coordinates of destined box
        string T1, T2, T3;
        #region OPC...
        public static int item = 0;
        public OPCAutomation.OPCServer AnOPCServer;
        public OPCAutomation.OPCServer ObjOPCServer;
        public OPCAutomation.OPCGroups ConnectedServerGroup;
        public OPCAutomation.OPCGroup ConnectedGroup;
        public string Groupname;

        int ItemCount;
        Array OPCItemIDs = Array.CreateInstance(typeof(string), 28);

        Array ItemServerHandles = Array.CreateInstance(typeof(Int32), 28);
        Array ItemServerErrors = Array.CreateInstance(typeof(Int32), 28);
        Array ClientHandles = Array.CreateInstance(typeof(Int32), 28);
        Array RequestedDataTypes = Array.CreateInstance(typeof(Int16), 28);
        Array AccessPaths = Array.CreateInstance(typeof(string), 28);
        Array WriteItems = Array.CreateInstance(typeof(object), 28);
        /// </summary>    
        #endregion 
        #region THÔNG SỐ ROBOT...
        // robot geometry
        System.Double ee = 70;// end effector       // chiều dài cạnh bệ dưới
        System.Double ff = 260;// base              // chiều dài cạnh bệ dưới
        System.Double re = 235; //arm//220          //    chiều dài thanh CACBON
        System.Double rf = 80;                      //    chiều dài cánh step
        // trigonometric constants
        const System.Double sqrt3 = 1.732;
        const System.Double pi = 3.14159;
        const System.Double sin120 = 0.86603;
        const System.Double cos120 = -0.5;
        const System.Double tan60 = 1.73205;
        const System.Double sin30 = 0.5;
        const System.Double tan30 = 0.57735;
        #endregion
        #region THÔNG SỐ XLA...  
        VideoCapture capture;           // khai báo capture kiểu video
        bool Pause = false, colour = false;
        string x1, y1, td, x3, y3, Colour1;
        Boolean am = true;
        Boolean td_am = true;
        Boolean ghilaivalue = false;
        int[] y0 = new int[900];
        int[] x0 = new int[900];

        int[] y2 = new int[900];
        int[] x2 = new int[900];

        int[] y4 = new int[900];
        int[] x4 = new int[900];

        int[] y6 = new int[900];
        int[] x6 = new int[900];

        int Hmax, Hmin, Smax, Smin, Vmax, Vmin, hinh = 4, Colour, X, Y;
        int MIN = 150 * 150;
        const double MAX = 307200 / 1.5;
        bool Configcolour = false, h = false;        //h là biến trạng thái tùy chọn màu bất kì
        //auto
        bool a = true;   // cho phép cánh tay gắp
        int dem, t;
        #endregion
        public Form1()
        {
            InitializeComponent();


        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (am == true)
            {
                Initial_timer.Start();
                //loi khong co ngo vao khi auto
                x3 = 0.ToString();
                y3 = 0.ToString();
                txtX.Text = x3;
                txtY.Text = y3;
            }
            btnAUTO.Enabled = false;
            btnManual.Enabled = false;
            groupManual.Enabled = false;
            groupParameter.Enabled = false;
            groupauto22.Enabled = false;
            groupAUTO1.Enabled = false;
            groupBox8.Visible = true;

            X2_X1 = Convert.ToDouble(txbsetX2.Text) / Convert.ToDouble(txbsetX1.Text);
            Y2_Y1 = Convert.ToDouble(txbsetY2.Text) / Convert.ToDouble(txbsetY1.Text);
            Xlech = Convert.ToDouble(txbsetXlech.Text);
            Ylech = Convert.ToDouble(txbsetYlech.Text);
            timer1.Start();
        }
        private void Initial_timer_Tick(object sender, EventArgs e)
        {

            if (a == true && Y > 55 && Y < 200 && am == true && sta == true)     // a = true khi thả xong vật, nếu phát hiện có vật thì chạy timer 10 thực hiện chuỗi lệnh gắp vật
            {
                a = false;
                counter = 0;
                timer10.Interval = 50;
                timer10.Start();           // timer thực hiện chuỗi lệnh gắp vật
            }

            //if(Y == 390 && am == true && sta == true)
            //{
            //    gaptruot++;
            //    if (gaptruot>=8){ gaptruot = 0;}
            //}
            if (h == true)        // khi bật trạng thái tùy chọn màu
            {
                groupAUTO3.Enabled = false;
                textBox8.Text = hScrollBar1.Value.ToString();
                textBox9.Text = hScrollBar2.Value.ToString();
                textBox10.Text = hScrollBar3.Value.ToString();
                textBox11.Text = hScrollBar4.Value.ToString();
                textBox12.Text = hScrollBar5.Value.ToString();
                textBox13.Text = hScrollBar6.Value.ToString();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)    // đọc tốc độ băng tải 
        {
            if (td_am == true) { txbtdbangtai.Text = txbtocdoauto.Text; lechdc = (Convert.ToDouble(txbtdbangtai.Text) * 1.2); }
        }
        #region TÍNH TOÁN ĐỘNG HỌC (GIẢI THUẬT ĐỘNG HỌC)...
        private void XYZtoTheta()        // toạ độ vật trong hệ robot suy ra góc quay.
        {
            double X1 = Convert.ToDouble(txtX.Text);
            double Y1 = Convert.ToDouble(txtY.Text);
            double Z1 = Convert.ToDouble(txtZ.Text);
            double T1 = 0;
            double T2 = 0;
            double T3 = 0;
            double YJ1 = 0, ZJ1 = 0, YJ2 = 0, ZJ2 = 0, YJ3 = 0, ZJ3 = 0;
            int Status2 = delta_calcInverse(X1, Y1, Z1, ref T1, ref T2, ref T3, ref YJ1, ref ZJ1, ref YJ2, ref ZJ2, ref YJ3, ref ZJ3);

            if (Status2 == 0)          // status2 = 0 thì tồn tại góc theta , = -1 thì không tồn tại.
            {
                txtSendMsg.Text = "OK";
                txbTheta1.Text = Convert.ToString(T1);
                txbTheta2.Text = Convert.ToString(T2);
                txbTheta3.Text = Convert.ToString(T3);
                if (am == false)
                {
                    txbYj1.Text = Convert.ToString(YJ1);
                    txbZj1.Text = Convert.ToString(ZJ1);
                    txbYj2.Text = Convert.ToString(YJ2);
                    txbZj2.Text = Convert.ToString(ZJ2);
                    txbYj3.Text = Convert.ToString(YJ3);
                    txbZj3.Text = Convert.ToString(ZJ3);
                }
            }
            else
            {
                txtSendMsg.Text = "non-existing position";
                txbTheta1.Text = " Ø ";
                txbTheta2.Text = " Ø ";
                txbTheta3.Text = " Ø ";
            }
        }
        //Tinh goc theta1 (mp YZ)
        int delta_calcAngleYZ(double x0, double y0, double z0, ref double theta, ref double YJ, ref double ZJ)
        {
            double y1 = -0.5 * 0.57735 * ff; // f/2 * tg 30
            double y3 = y0 - 0.5 * 0.57735 * ee;        // e/2 * tg30
            double a = (x0 * x0 + y3 * y3 + z0 * z0 + rf * rf - re * re - y1 * y1) / (2 * z0);
            double b = (y1 - y3) / z0;

            // discriminant
            double d = -(a + b * y1) * (a + b * y1) + rf * (b * b * rf + rf);
            if (d < 0) return -1; // non-existing point
            // suy ra tọa độ J(0,ỵ,zj)
            double yj = (y1 - a * b - Math.Sqrt(d)) / (b * b + 1); // choosing outer point
            double zj = a + b * yj;
            theta = Math.Round((180.0 * Math.Atan(-zj / (y1 - yj)) / pi + ((yj > y1) ? 180.0 : 0.0)), 2);
            YJ = Math.Round(yj, 2);
            ZJ = Math.Round(zj, 2);
            return 0;
        }
        // inverse kinematics: (x0, y0, z0) -> (theta1, theta2, theta3)
        // return: 0=OK, -1= vi tri ko ton tai
        int delta_calcInverse(double x0, double y0, double z0, ref double theta1, ref double theta2, ref double theta3, ref double YJ1, ref double ZJ1, ref double YJ2, ref double ZJ2, ref double YJ3, ref double ZJ3)
        {
            theta1 = theta2 = theta3 = 0;
            int status = delta_calcAngleYZ(x0, y0, z0, ref theta1, ref YJ1, ref ZJ1);
            if (status == 0) status = delta_calcAngleYZ(x0 * cos120 + y0 * sin120, y0 * cos120 - x0 * sin120, z0, ref theta2, ref YJ2, ref ZJ2);  // xoay goc toa do +120 deg
            if (status == 0) status = delta_calcAngleYZ(x0 * cos120 - y0 * sin120, y0 * cos120 + x0 * sin120, z0, ref theta3, ref YJ3, ref ZJ3);  // xoay goc toa do -120 deg
            return status;
        }
        #endregion

        #region XỬ LÝ ẢNH...
        private void btn_numbercamera_Click(object sender, EventArgs e)         // nút chọn camera số mấy ?
        {
            cam = Convert.ToInt16(txb_cameranumber.Text);
        }
        private void btn_connectcamera_Click(object sender, EventArgs e)        // nút conect camera
        {
            capture = new VideoCapture(cam);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 640);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);
            Mat m = new Mat();          // khai báo biến m thuộc kiểu matrix (matran)
            capture.Read(m);           // đọc video theo từng khung hình và lưu vào m
            lbTrangThai.Text = "CONNECTED";
        }
        private async void btn_opencamera_Click(object sender, EventArgs e)             // hàm không đồng bộ vì có thời gian trễ wait
        {
            if (capture == null) { return; }
            try
            {
                while (!Pause)
                {
                    Mat m = new Mat();
                    capture.Read(m);
                    lbTrangThai.Text = "CAMERA ON";
                    if (!m.IsEmpty)              // if có khung hình
                    {
                        if (!Configcolour)   // đang ở chế độ chọn sẵn màu
                        {
                            if (mode == 0)
                            {
                                contourss(m.ToImage<Bgr, byte>());
                                SAPXEP();
                            }

                            else if (mode == 1)
                            {
                                contoursss(m.ToImage<Bgr, byte>());
                                SAPXEPs();
                            }



                        }
                        if (Configcolour)
                        {
                            contour2(m.ToImage<Bgr, byte>());
                        }
                        double fps = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);   // đọc giá trị fps từ video (số khung hình/s)
                        await Task.Delay(500 / Convert.ToInt32(fps));    // delay thời gian 1 khung hình
                    }
                    else { break; }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void btn_stopcamera_Click(object sender, EventArgs e)
        {
            Pause = !Pause;
            if (Pause == true) { btn_stopcamera.BackColor = Color.DarkGray; }
            else { btn_stopcamera.BackColor = Color.LightGray; }
        }
        private void btn_configcolour_Click(object sender, EventArgs e)
        {
            Configcolour = true;
            panel1.Visible = true;
            txb_color.Visible = false;
            txb_modecolor.Visible = false;
            colour = true;
            a = false;   // khi nhấn config thì coi như không phát hiện vật nữa, chờ chỉnh màu xong mới cho a = true để phân loại tiếp
            Hmax = 255;
            Hmin = 0;
            Smax = 255;
            Smin = 0;
            Vmax = 255;
            Vmin = 0;
        }
        private void btn_BackToDefault_Click(object sender, EventArgs e)
        {
            Configcolour = false;
            h = false;
            a = true;
            panel1.Visible = false;
            txb_color.Visible = true;
            txb_modecolor.Visible = true;
        }
        private void btn_tracking_Click(object sender, EventArgs e)
        {
            h = true;
            a = true;
            textBox8.Text = hScrollBar1.Value.ToString();
            textBox9.Text = hScrollBar2.Value.ToString();
            textBox10.Text = hScrollBar3.Value.ToString();
            textBox11.Text = hScrollBar4.Value.ToString();
            textBox12.Text = hScrollBar5.Value.ToString();
            textBox13.Text = hScrollBar6.Value.ToString();
        }
        private void capnhat()
        {
            Hmax = hScrollBar2.Value;
            Hmin = hScrollBar1.Value;
            Smax = hScrollBar4.Value;
            Smin = hScrollBar3.Value;
            Vmax = hScrollBar6.Value;
            Vmin = hScrollBar5.Value;
        }
        private void contour2(Image<Bgr, byte> img)  //chương trình xla tùy chọn . khai báo biến img là ảnh Bgr
        {
            y0[0] = 0;
            //chuyen sang HSV
            Image<Hsv, byte> HSV = new Image<Hsv, byte>(img.Width, img.Height);
            CvInvoke.CvtColor(img, HSV, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
            if (colour == true) { capnhat(); }
            Hsv Limitduoi = new Hsv(Hmin, Smin, Vmin);
            Hsv Limittren = new Hsv(Hmax, Smax, Vmax);
            Image<Gray, byte> temp = HSV.InRange(Limitduoi, Limittren);
            //co dan
            var element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(8, 8), new Point(-1, -1));
            //var element1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            temp = temp.Erode(2);
            // CvInvoke.Erode(temp, temp, element1, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0, 0, 0));
            CvInvoke.Dilate(temp, temp, element, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0, 0, 0));
            //contours           
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat n = new Mat();
            CvInvoke.FindContours(temp, contours, n, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            for (int i = 0; i < contours.Size; i++)
            {
                double pemir = CvInvoke.ArcLength(contours[i], true);
                VectorOfPoint approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(contours[i], approx, 0.03 * pemir, true);
                //lay trong tam
                var moments = CvInvoke.Moments(contours[i]);
                double area = moments.M00;
                if (area > MIN && area < MAX && ((approx.Size == hinh) | (approx.Size >= 5 && hinh == 20)))
                {
                    int x = (int)(moments.M10 / moments.M00);
                    int y = (int)(moments.M01 / moments.M00);
                    if (x != 0)
                    {
                        if (y < 200) { y0[i] = y; x0[i] = x; }
                        x1 = x.ToString("D2");
                        y1 = y.ToString("D2");
                        td = x1 + "," + y1;
                    }
                    CvInvoke.DrawContours(img, contours, i, new MCvScalar(0, 0, 255), 3);
                    CvInvoke.Circle(img, new Point(x, y), 2, new MCvScalar(0, 0, 255), 2);
                    CvInvoke.PutText(img, td, new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                }
            }
            //sap xep theo chieu den truoc
            for (int l = 0; l < 10; l++)
            {
                for (int k = l + 1; k < 10; k++)
                {
                    if (y0[l] < y0[k])
                    {
                        int tam;
                        tam = x0[l];
                        x0[l] = x0[k];
                        x0[k] = tam;
                        int tam2;
                        tam2 = y0[l];
                        y0[l] = y0[k];
                        y0[k] = tam2;
                    }
                }
            }
            X = x0[0];
            Y = y0[0];
            CvInvoke.Line(img, new Point(0, 270), new Point(640, 270), new Bgr(Color.Black).MCvScalar, 2);
            CvInvoke.Line(img, new Point(90, 0), new Point(90, 480), new Bgr(Color.DarkGray).MCvScalar, 200);
            ///dịch sang tọa độ phần cứng
            if (Y != 0 && h == true && X > 230 && Y < 200)
            {
                //x3 = Math.Round(((-243.000/**0.998*/ + X) * 0.23684 * 3 / 1.91)).ToString();
                //y3 = Math.Round((-(2.000/**0.995*/ + (Y + 173)) * 0.23684 * 3 / 1.95)).ToString();

                x3 = Math.Round(X * X2_X1 + Xlech, 2).ToString();
                y3 = Math.Round(-Y * Y2_Y1 - Ylech - lechdc, 2).ToString();

                txbXPresent.Text = X.ToString();
                txbYPresent.Text = Y.ToString();
            }
            if (am == true && dang_gap == false)
            {
                txtX.Text = x3;
                txtY.Text = y3;
                txbXX.Text = x3;
                TXBYY.Text = y3;
                //XYZtoTheta();
            }
            //hien thi
            pictureBox2.Image = temp.Bitmap;
            if (h == true)
            {
                pictureBox1.Image = img.Bitmap;
            }
        }

        private void contoursss(Image<Bgr, byte> img)
        {
            y2[0] = y4[0] = y6[0] = 0;
            // chuyen sang che do xam
            Image<Gray, byte> imput_gray = new Image<Gray, byte>(img.Size);
            CvInvoke.CvtColor(img, imput_gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

            // tinh toan gradient theo huong x,y
            Image<Gray, byte> grad_x1 = new Image<Gray, byte>(img.Size);
            Image<Gray, byte> grad_y1 = new Image<Gray, byte>(img.Size);
            Image<Gray, byte> grad_all = new Image<Gray, byte>(img.Size);
            CvInvoke.Sobel(imput_gray, grad_x1, Emgu.CV.CvEnum.DepthType.Default, 0, 1, 3);
            CvInvoke.Sobel(imput_gray, grad_y1, Emgu.CV.CvEnum.DepthType.Default, 1, 0, 3);
            CvInvoke.Add(grad_x1, grad_y1, grad_all, null);

            // ĐỘ mờ
            grad_all = grad_all.SmoothGaussian(9);

            // Binarization
            CvInvoke.Threshold(grad_all, grad_all, 100, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

            // Loại bỏ các vết nứt
            Mat oMat1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(15, 15), new System.Drawing.Point(-1, -1));
            CvInvoke.MorphologyEx(grad_all, grad_all, Emgu.CV.CvEnum.MorphOp.Close, oMat1, new System.Drawing.Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(255, 0, 0, 255));

            // mở rộng và ăn mòn
            Mat oMat2 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(5, 5), new System.Drawing.Point(-1, -1));
            CvInvoke.Erode(grad_all, grad_all, oMat2, new System.Drawing.Point(-1, -1), 4, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(255, 0, 0, 255));
            CvInvoke.Dilate(grad_all, grad_all, oMat2, new System.Drawing.Point(-1, -1), 4, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(255, 0, 0, 255));

            //Find contours, draw contours
            #region Find triangles and rectangles

            List<Triangle2DF> triangleList = new List<Triangle2DF>();
            List<RotatedRect> boxList = new List<RotatedRect>();   //a box is a rotated rectangle

            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(grad_all, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    using (VectorOfPoint approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        double dArea = CvInvoke.ContourArea(approxContour, false);
                        if (dArea > 10000)
                        {
                            if (approxContour.Size == 3)
                            {
                                System.Drawing.Point[] pts = approxContour.ToArray();
                                triangleList.Add(new Triangle2DF(pts[0], pts[1], pts[2]));
                            }

                            else if (approxContour.Size == 4)
                            {
                                #region determine if all the angles in the contour are within [80, 100] degree
                                bool isRectangle = true;
                                System.Drawing.Point[] pts = approxContour.ToArray();
                                LineSegment2D[] edges = Emgu.CV.PointCollection.PolyLine(pts, true);

                                for (int j = 0; j < edges.Length; j++)
                                {
                                    double angle = Math.Abs(
                                        edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                    if (angle < 80 || angle > 100)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }
                                #endregion

                                if (isRectangle)
                                {
                                    CvInvoke.DrawContours(img, contours, i, new MCvScalar(0, 0, 255), 3);

                                    //lay trong tam
                                    var moments = CvInvoke.Moments(contours[i]);
                                    double area = moments.M00;        // diện tích hình 


                                    int x = (int)(moments.M10 / moments.M00);
                                    int y = (int)(moments.M01 / moments.M00);
                                    if (x != 0 && x > 200)
                                    {
                                        if (y < 200) { y2[i] = y; x2[i] = x; }
                                        x1 = x.ToString("D2");
                                        y1 = y.ToString("D2");
                                        td = x1 + "," + y1;
                                    }
                                    //sap xep lai theo chieu den noi truoc
                                    for (int l = 0; l < 10; l++)
                                    {
                                        for (int k = l + 1; k < 10; k++)
                                        {
                                            if (y2[l] < y2[k])
                                            {
                                                int tam;
                                                tam = x2[l];
                                                x2[l] = x2[k];
                                                x2[k] = tam;

                                                int tam2;
                                                tam2 = y2[l];
                                                y2[l] = y2[k];
                                                y2[k] = tam2;
                                            }
                                        }
                                    }

                                }

                            }
                        }
                    }
                }
            }

            #endregion
            pictureBox1.Image = img.Bitmap;
        }

        private void contourss(Image<Bgr, byte> img)   /// ct xla theo thông số cài sẵn (đỏ, lam, lục)
        {
            y2[0] = y4[0] = y6[0] = 0;
            //chuyen sang HSV
            Image<Hsv, byte> HSV = new Image<Hsv, byte>(img.Width, img.Height);    // khai báo và tạo biến HSV thuộc không gian màu HSV với thông số rộng, cao như hình cũ
            CvInvoke.CvtColor(img, HSV, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);    // chuyển img thuộc không gian màu bgr sang HSV và lưu vào biến HSV

            ////RED
            //Hsv Limitduoi = new Hsv(0, 178, 143); // (0,178,143)
            //Hsv Limittren = new Hsv(89, 255, 255); //(255,239,255)
            ////BLUE
            //Hsv Limitduoi1 = new Hsv(90, 190, 115);
            //Hsv Limittren1 = new Hsv(255, 255, 255);
            ////GREEN
            //Hsv Limitduoi2 = new Hsv(38, 109, 41);
            //Hsv Limittren2 = new Hsv(94, 255, 255);
            //Image<Gray, byte> temp = HSV.InRange(Limitduoi, Limittren);
            //Image<Gray, byte> temp1 = HSV.InRange(Limitduoi1, Limittren1);
            //Image<Gray, byte> temp2 = HSV.InRange(Limitduoi2, Limittren2);

            //RED
            Hsv Limitduoi = new Hsv(0, 100, 135);
            Hsv Limittren = new Hsv(29, 255, 255);
            //Hsv Limitduoii = new Hsv(0, 171, 88); 
            //Hsv Limittrenn = new Hsv(30, 255, 255); 
            //BLUE
            Hsv Limitduoi1 = new Hsv(104, 171, 64);
            Hsv Limittren1 = new Hsv(150, 255, 255);
            //GREEN
            Hsv Limitduoi2 = new Hsv(40, 122, 48);
            Hsv Limittren2 = new Hsv(92, 255, 255);

            Image<Gray, byte> temp = HSV.InRange(Limitduoi, Limittren);// img màu RED + HSV.InRange(Limitduoii, Limittrenn);  //Hình ảnh được chuyển sang HSV bây giờ chỉ giữ lại màu với giá trị trong khoảng () (nghĩa là đã được AND với mask rồi)
            Image<Gray, byte> temp1 = HSV.InRange(Limitduoi1, Limittren1); // img màu BLUE
            Image<Gray, byte> temp2 = HSV.InRange(Limitduoi2, Limittren2);  // img màu GREEN

            //co dãn
            var element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(8, 8), new Point(-1, -1));    // tạo 1 phần tử cấu trúc có kích thước và hình dạng được chỉnh định với ma trận (8,8) và (-1,-1) là điểm trung tâm
            temp = temp.Erode(2); temp1 = temp1.Erode(2); temp2 = temp2.Erode(2);   // co ảnh lại 2 lần
            CvInvoke.Dilate(temp, temp, element, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0, 0, 0));   //  giãn nở temp ra 2 lần (trở về như cũ) và lưu vào temp lại
            CvInvoke.Dilate(temp1, temp1, element, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0, 0, 0));
            CvInvoke.Dilate(temp2, temp2, element, new Point(-1, -1), 2, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(0, 0, 0));

            #region for RED         
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();    // khởi tạo biến đường bao
            Mat n = new Mat();
            CvInvoke.FindContours(temp, contours, n, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);   // tìm đường bao và trả về số lượng đường bao, lưu các đường bao tìm được vào biến contours dưới dạng mỗi đường bao là 1 vector điểm
            for (int i = 0; i < contours.Size; i++)     // kiểm tra xem có bao nhiêu đường bao
            {
                double pemir = CvInvoke.ArcLength(contours[i], true);     // tính độ dài đường bao
                VectorOfPoint approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(contours[i], approx, 0.03 * pemir, true);   // tính gần đúng số số điểm của đường bao và lưu vào approx.size (approx.size = 3 thì là hình tam giác, = 4 là hình chữ nhật). (tỉ lệ khoảng cách mà điểm đó có thể giữ lại nếu >= 0.03x độ dài tổng)
                //lay trong tam
                var moments = CvInvoke.Moments(contours[i]);
                double area = moments.M00;        // diện tích hình 
                textBox15.Text = Convert.ToString(approx.Size);
                if (area > MIN && area < MAX && ((approx.Size == hinh) | (approx.Size >= 5 && hinh == 20)))  // hinh = 4 : hình vuông, = 3 : tam giác, hình = 20: hình tròn
                {
                    int x = (int)(moments.M10 / moments.M00);
                    int y = (int)(moments.M01 / moments.M00);
                    if (x != 0 && x > 200)
                    {
                        if (y < 200) { y2[i] = y; x2[i] = x; }
                        x1 = x.ToString("D2");
                        y1 = y.ToString("D2");
                        td = x1 + "," + y1;
                    }
                    CvInvoke.DrawContours(img, contours, i, new MCvScalar(0, 0, 255), 3);    // vẽ từng đường bao tương ứng từng i
                    CvInvoke.Circle(img, new Point(x, y), 2, new MCvScalar(0, 0, 255), 2);
                    CvInvoke.PutText(img, td, new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                    CvInvoke.PutText(img, "RED", new Point(x + 15, y - 15), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                }
            }
            //sap xep lai theo chieu den noi truoc
            for (int l = 0; l < 10; l++)
            {
                for (int k = l + 1; k < 10; k++)
                {
                    if (y2[l] < y2[k])
                    {
                        int tam;
                        tam = x2[l];
                        x2[l] = x2[k];
                        x2[k] = tam;

                        int tam2;
                        tam2 = y2[l];
                        y2[l] = y2[k];
                        y2[k] = tam2;
                    }
                }
            }
            #endregion

            #region for BLUE
            VectorOfVectorOfPoint contours3 = new VectorOfVectorOfPoint();
            Mat n3 = new Mat();
            CvInvoke.FindContours(temp1, contours3, n3, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            for (int i = 0; i < contours3.Size; i++)
            {
                double pemir = CvInvoke.ArcLength(contours3[i], true);
                VectorOfPoint approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(contours3[i], approx, 0.03 * pemir, true);
                //lay trong tam
                var moments3 = CvInvoke.Moments(contours3[i]);
                double area = moments3.M00;
                if (area > MIN && area < MAX && ((approx.Size == hinh) | (approx.Size >= 5 && hinh == 20)))
                {
                    int x = (int)(moments3.M10 / moments3.M00);
                    int y = (int)(moments3.M01 / moments3.M00);
                    if (x != 0 && x > 200)
                    {
                        if (y < 200) { y4[i] = y; x4[i] = x; }
                        x1 = x.ToString("D2");
                        y1 = y.ToString("D2");
                        td = x1 + "," + y1;
                    }
                    CvInvoke.DrawContours(img, contours3, i, new MCvScalar(255, 0, 0), 3);
                    CvInvoke.Circle(img, new Point(x, y), 2, new MCvScalar(255, 0, 0), 2);
                    CvInvoke.PutText(img, td, new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(255, 0, 0), 2);
                    CvInvoke.PutText(img, "BLUE", new Point(x + 15, y - 15), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(255, 0, 0), 2);
                }
            }
            //sap xep lai theo chieu den noi truoc
            for (int l = 0; l < 10; l++)
            {
                for (int k = l + 1; k < 10; k++)
                {
                    if (y4[l] < y4[k])
                    {
                        int tam;
                        tam = x4[l];
                        x4[l] = x4[k];
                        x4[k] = tam;
                        int tam2;
                        tam2 = y4[l];
                        y4[l] = y4[k];
                        y4[k] = tam2;
                    }
                }
            }
            #endregion

            #region for GREEN
            VectorOfVectorOfPoint contours2 = new VectorOfVectorOfPoint();
            Mat n2 = new Mat();
            CvInvoke.FindContours(temp2, contours2, n2, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            for (int i = 0; i < contours2.Size; i++)
            {
                double pemir = CvInvoke.ArcLength(contours2[i], true);
                VectorOfPoint approx = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(contours2[i], approx, 0.03 * pemir, true);
                //lay trong tam
                var moments2 = CvInvoke.Moments(contours2[i]);
                double area = moments2.M00;
                if (area > MIN && area < MAX && ((approx.Size == hinh) | (approx.Size >= 5 && hinh == 20)))
                {
                    int x = (int)(moments2.M10 / moments2.M00);
                    int y = (int)(moments2.M01 / moments2.M00);
                    if (x != 0 && x > 200)
                    {
                        if (y < 200) { y6[i] = y; x6[i] = x; }
                        x1 = x.ToString("D2");
                        y1 = y.ToString("D2");
                        td = x1 + "," + y1;
                    }
                    CvInvoke.DrawContours(img, contours2, i, new MCvScalar(0, 255, 0), 3);
                    CvInvoke.Circle(img, new Point(x, y), 2, new MCvScalar(0, 255, 0), 2);
                    CvInvoke.PutText(img, td, new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 255, 0), 2);
                    CvInvoke.PutText(img, "GREEN", new Point(x + 15, y - 15), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 255, 0), 2);

                }
            }
            //sap xep lai theo chieu đến nơi truoc
            for (int l = 0; l < 10; l++)
            {
                for (int k = l + 1; k < 10; k++)
                {
                    if (y6[l] < y6[k])
                    {
                        int tam;
                        tam = x6[l];
                        x6[l] = x6[k];
                        x6[k] = tam;

                        int tam2;
                        tam2 = y6[l];
                        y6[l] = y6[k];
                        y6[k] = tam2;
                    }
                }
            }
            CvInvoke.Line(img, new Point(0, 270), new Point(640, 270), new Bgr(Color.Black).MCvScalar, 2);
            CvInvoke.Line(img, new Point(90, 0), new Point(90, 480), new Bgr(Color.DarkGray).MCvScalar, 200);
            #endregion
            //hien thi
            pictureBox1.Image = img.Bitmap;
            if (y2[0] == 0 && y4[0] == 0 && y6[0] == 0)
            {
                txb_color.Text = "Colour of The Nearest Object:   ";
                txb_modecolor.Text = "MODE COLOUR:   ";
            }
            else
            {
                txb_color.Text = "Colour of The Nearest Object: " + Colour1;
                txb_modecolor.Text = "Mode Colour: " + Colour.ToString();
            }
        }
        private void SAPXEP()
        {
            if (y2[0] > y4[0])
            {
                X = x2[0];
                Y = y2[0];
                Colour = 1;
                Colour1 = "RED";
            }
            else
            {
                X = x4[0];
                Y = y4[0];
                Colour = 2;
                Colour1 = "BLUE";
            }
            if (y6[0] > Y)
            {
                X = x6[0];
                Y = y6[0];
                Colour = 3;
                Colour1 = "GREEN";
            }



            if (Y != 0 && X > 210 && Y < 200)
            {
                //x3 = Math.Round(((-243.000/**0.998*/ + X) * 0.23684 * 3 / 1.91)).ToString();
                //y3 = Math.Round((-(2.000/**0.995*/ + (Y + 173)) * 0.23684 * 3 / 1.95)).ToString();
                //double xcam = Math.Abs(0.0001 * (X - 320));// textBox2.Text = Convert.ToString(xcam);
                //double ycam = Math.Abs(0.0001 * (Y - 240)); //textBox14.Text = Convert.ToString(ycam);
                x3 = Math.Round(X * X2_X1 + 0 + Xlech, 2).ToString();     // Xlech Ylech là lệch tâm của cam so với robot
                y3 = Math.Round(-Y * Y2_Y1 - 0 - Ylech - lechdc, 2).ToString();     // 

                txbXPresent.Text = X.ToString();
                txbYPresent.Text = Y.ToString();
                //set Box
                if (dang_gap == false)     // gắp thả vật xong thì = false 
                {
                    if (Colour == 1) { T1 = TR1; T2 = TR2; T3 = TR3; }
                    if (Colour == 2) { T1 = TB1; T2 = TB2; T3 = TB3; }
                    if (Colour == 3) { T1 = TG1; T2 = TG2; T3 = TG3; }
                }
            }
            if (am == true && dang_gap == false)   // am == true là ở chế độ auto
            {
                txtX.Text = x3;       // chế độ manual, toạ độ vật cần gắp X,Y trong hệ robot
                txtY.Text = y3;
                txbXX.Text = x3;      // chế độ auto, X y coordinates trong hệ robot
                TXBYY.Text = y3;
                //XYZtoTheta();
            }
        }

        private void SAPXEPs()
        {

            X = x2[0];
            Y = y2[0];


            if (Y != 0 && X > 210 && Y < 200)
            {
                //x3 = Math.Round(((-243.000/**0.998*/ + X) * 0.23684 * 3 / 1.91)).ToString();
                //y3 = Math.Round((-(2.000/**0.995*/ + (Y + 173)) * 0.23684 * 3 / 1.95)).ToString();
                //double xcam = Math.Abs(0.0001 * (X - 320));// textBox2.Text = Convert.ToString(xcam);
                //double ycam = Math.Abs(0.0001 * (Y - 240)); //textBox14.Text = Convert.ToString(ycam);
                x3 = Math.Round(X * X2_X1 + 0 + Xlech, 2).ToString();     // Xlech Ylech là lệch tâm của cam so với robot
                y3 = Math.Round(-Y * Y2_Y1 - 0 - Ylech, 2).ToString();     // 

                txbXPresent.Text = X.ToString();
                txbYPresent.Text = Y.ToString();
                //set Box
                if (am == true && dang_gap == false)     // gắp thả vật xong thì = false 
                {
                    ma_sp = get2(txtQRCode.Text);
                    //  Loai_QR.Text = ma_sp.ToString();

                    if (ma_sp == 14) { T1 = TR1; T2 = TR2; T3 = TR3; Loai_QR.Text = "1"; }
                    if (ma_sp == 15) { T1 = TB1; T2 = TB2; T3 = TB3; Loai_QR.Text = "2"; }
                    if (ma_sp == 16) { T1 = TG1; T2 = TG2; T3 = TG3; Loai_QR.Text = "3"; }
                }
            }
            if (am == true && dang_gap == false)   // am == true là ở chế độ auto
            {

                txtX.Text = x3;       // chế độ manual, toạ độ vật cần gắp X,Y trong hệ robot
                txtY.Text = y3;
                txbXX.Text = x3;      // chế độ auto, X y coordinates trong hệ robot
                TXBYY.Text = y3;
                //XYZtoTheta();

            }
        }
        int get2(string a) // chương trình con lấy 2 số đầu của mã sản phẩm 
        {
            try
            {
                string bienA = a.ToCharArray()[0].ToString() + a.ToCharArray()[1].ToString();
                return Convert.ToInt32(bienA);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        #endregion

        #region KHỞI TẠO OPC...
        //Connect device
        private void btnConnect_Click(object sender, EventArgs e)
        {
            //khởi tạo opc server
            try
            {
                string IOServer = "Kepware.KEPServerEX.V6";
                string IOGroup = "OPCGroup1";

                ObjOPCServer = new OPCAutomation.OPCServer();
                ObjOPCServer.Connect(IOServer, "");
                ConnectedGroup = ObjOPCServer.OPCGroups.Add(IOGroup);
                ConnectedGroup.DataChange += new DIOPCGroupEvent_DataChangeEventHandler(ObjOPCGroup_DataChange);
                ConnectedGroup.UpdateRate = 1000;
                ConnectedGroup.IsSubscribed = ConnectedGroup.IsActive;

                ItemCount = 24;
                OPCItemIDs.SetValue("Channel2.Device1.theta1", 1);
                ClientHandles.SetValue(1, 1);
                OPCItemIDs.SetValue("Channel2.Device1.theta2", 2);
                ClientHandles.SetValue(2, 2);
                OPCItemIDs.SetValue("Channel2.Device1.theta3", 3);
                ClientHandles.SetValue(3, 3);
                OPCItemIDs.SetValue("Channel2.Device1.enable_move", 4);
                ClientHandles.SetValue(4, 4);
                OPCItemIDs.SetValue("Channel2.Device1.mode", 5);
                ClientHandles.SetValue(5, 5);
                OPCItemIDs.SetValue("Channel2.Device1.enable_axis", 6);
                ClientHandles.SetValue(6, 6);
                OPCItemIDs.SetValue("Channel2.Device1.resethome", 7);
                ClientHandles.SetValue(7, 7);

                OPCItemIDs.SetValue("Channel2.Device1.forward_t1", 8);
                ClientHandles.SetValue(8, 8);
                OPCItemIDs.SetValue("Channel2.Device1.backward_t1", 9);
                ClientHandles.SetValue(9, 9);
                OPCItemIDs.SetValue("Channel2.Device1.forward_t2", 10);
                ClientHandles.SetValue(10, 10);
                OPCItemIDs.SetValue("Channel2.Device1.backward_t2", 11);
                ClientHandles.SetValue(11, 11);
                OPCItemIDs.SetValue("Channel2.Device1.forward_t3", 12);
                ClientHandles.SetValue(12, 12);
                OPCItemIDs.SetValue("Channel2.Device1.backward_t3", 13);
                ClientHandles.SetValue(13, 13);
                OPCItemIDs.SetValue("Channel2.Device1.gripper", 14);
                ClientHandles.SetValue(14, 14);

                OPCItemIDs.SetValue("Channel2.Device1.theta1_tt", 15);
                ClientHandles.SetValue(15, 15);
                OPCItemIDs.SetValue("Channel2.Device1.theta2_tt", 16);
                ClientHandles.SetValue(16, 16);
                OPCItemIDs.SetValue("Channel2.Device1.theta3_tt", 17);
                ClientHandles.SetValue(17, 17);

                OPCItemIDs.SetValue("Channel2.Device1.tocdo_bangtai", 18);
                ClientHandles.SetValue(18, 18);

                OPCItemIDs.SetValue("Channel2.Device1.number_red", 19);
                ClientHandles.SetValue(19, 19);
                OPCItemIDs.SetValue("Channel2.Device1.number_blue", 20);
                ClientHandles.SetValue(20, 20);
                OPCItemIDs.SetValue("Channel2.Device1.number_green", 21);
                ClientHandles.SetValue(21, 21);

                OPCItemIDs.SetValue("Channel2.Device1.Sensor", 22);
                ClientHandles.SetValue(22, 22);

                OPCItemIDs.SetValue("Channel2.Device1.rst_sensor", 23);
                ClientHandles.SetValue(23, 23);

                OPCItemIDs.SetValue("Channel2.Device1.MODE_QR", 24);
                ClientHandles.SetValue(24, 24);

                ConnectedGroup.OPCItems.DefaultIsActive = true;
                ConnectedGroup.OPCItems.AddItems(ItemCount, ref OPCItemIDs, ref ClientHandles, out ItemServerHandles, out ItemServerErrors, RequestedDataTypes, AccessPaths);
                btnConnect.BackColor = Color.Green;
                ghilaivalue = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            ObjOPCServer.Disconnect();
            btnConnect.BackColor = Color.DarkGray;
        }
        #endregion

        #region PICK PLACE... CÁC NÚT NHẤN XÁC ĐỊNH VỊ TRÍ
        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                WriteItems.SetValue("1", 6);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
                sta = true;
                btnStart.BackColor = Color.Green;
                btnAUTO.Enabled = true;   // khi nhấn nút start thì hiện nổi nút auto và manual, không không nhấn start thì hiển thị mờ 2 nút đó .
                btnManual.Enabled = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                sta = false;
                WriteItems.SetValue("0", 6);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
                btnStart.BackColor = Color.Gray;
                btnAUTO.Enabled = false;
                btnManual.Enabled = false;
                groupManual.Enabled = false;
                groupParameter.Enabled = false;
                groupauto22.Enabled = false;
                groupAUTO1.Enabled = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void btnhoming_Click(object sender, EventArgs e)
        {
            txtX.Text = "0";
            txtY.Text = "0";
            txtZ.Text = Zhome;
            txbTheta1.Text = "0";
            txbTheta2.Text = "0";
            txbTheta3.Text = "0";
            try
            {
                WriteItems.SetValue("0", 1);
                WriteItems.SetValue("0", 2);
                WriteItems.SetValue("0", 3);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            Enable_move();
            Disable_move();
        }
        private void btUpdate_Click(object sender, EventArgs e)
        {
            txtX.Text = "0";
            txtY.Text = "0";
            txtZ.Text = Zhome;
        }
        private void btAUTO_Click(object sender, EventArgs e)
        {
            am = true;
            btnAUTO.BackColor = Color.Green;
            btnManual.BackColor = Color.DarkGray;
            txtQRCode.Enabled = true;
            groupManual.Enabled = false;
            groupParameter.Enabled = false;
            groupauto22.Enabled = true;
            groupAUTO1.Enabled = true;
            vevitricu();
        }
        private void btsend_Click(object sender, EventArgs e)
        {
            if (Convert.ToDouble(txbTheta1.Text) >= ho1 && Convert.ToDouble(txbTheta2.Text) >= ho2 && Convert.ToDouble(txbTheta3.Text) >= ho3)
            {
                try
                {
                    WriteItems.SetValue(txbTheta1.Text, 1);
                    WriteItems.SetValue(txbTheta2.Text, 2);
                    WriteItems.SetValue(txbTheta3.Text, 3);
                    ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                Enable_move();
                Disable_move();
            }
        }
        void Enable_move()
        {
            try
            {
                WriteItems.SetValue("0", 4);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        void Disable_move()
        {
            try
            {
                WriteItems.SetValue("1", 4);

                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void ObjOPCGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            try
            {
                int solanvalue = 0;
                for (int i = 1; i <= NumItems; i++)
                {
                    if (am == false)
                    {
                        if (Convert.ToDouble(ClientHandles.GetValue(i)) == 15)
                        { txbbb1.Text = Convert.ToString(Math.Round(Convert.ToDouble(ItemValues.GetValue(i).ToString()), 2)); }
                        if (Convert.ToDouble(ClientHandles.GetValue(i)) == 16)
                        { txbbb2.Text = Convert.ToString(Math.Round(Convert.ToDouble(ItemValues.GetValue(i).ToString()), 2)); }
                        if (Convert.ToDouble(ClientHandles.GetValue(i)) == 17)
                        { txbbb3.Text = Convert.ToString(Math.Round(Convert.ToDouble(ItemValues.GetValue(i).ToString()), 2)); }
                    }
                    if (Convert.ToDouble(ClientHandles.GetValue(i)) == 18)
                    { txbtocdoauto.Text = Convert.ToString(Math.Round((Convert.ToDouble(ItemValues.GetValue(i).ToString()) * 3.1416 * 35.5) / 60, 2)); }

                    // đọc giá trị sensor 
                    if (Convert.ToDouble(ClientHandles.GetValue(i)) == 22)
                    {
                        //   sensor = Convert.ToInt16(ItemValues.GetValue(i).ToString());

                        sensor = ItemValues.GetValue(i).ToString();
                        sensor_value.Text = sensor;
                    }

                    if (ghilaivalue == true)      // đọc về và lưu lại số lượng RED, GREEN, BLUE khi lần đầu mở winform (Trường hợp lỡ tắt winfom nếu mở lại thì nó vẫn còn đọc về lưu lại đc) 
                    {
                        if (Convert.ToDouble(ClientHandles.GetValue(i)) == 19)
                        { R = Convert.ToInt16(ItemValues.GetValue(i).ToString()); textBoxred.Text = R.ToString(); solanvalue++; }
                        if (Convert.ToDouble(ClientHandles.GetValue(i)) == 20)
                        { B = Convert.ToInt16(ItemValues.GetValue(i).ToString()); textBoxblue.Text = B.ToString(); solanvalue++; }
                        if (Convert.ToDouble(ClientHandles.GetValue(i)) == 21)
                        { G = Convert.ToInt16(ItemValues.GetValue(i).ToString()); textBoxgreen.Text = G.ToString(); solanvalue++; }
                    }
                    if (solanvalue >= 3) { ghilaivalue = false; }
                }


            }
            catch (Exception ex) { }
        }
        #endregion

        #region CHẾ ĐỘ MANUAL...
        private void btnManual_Click(object sender, EventArgs e)
        {
            am = false;
            btnAUTO.BackColor = Color.DarkGray;
            btnManual.BackColor = Color.Green;
            groupManual.Enabled = true;
            groupParameter.Enabled = true;
            groupauto22.Enabled = false;
            groupAUTO1.Enabled = false;
        }
        private void btn_dichraTheta_Click(object sender, EventArgs e)
        {
            XYZtoTheta();
        }
        private void right_press(object sender, MouseEventArgs e)    // nhấn chuột
        {
            dem = 1;
            txtX.Text = Convert.ToString(int.Parse(txtX.Text) + 2);
            XYZtoTheta();
            if (Convert.ToDouble(txbTheta1.Text) < ho1 | Convert.ToDouble(txbTheta2.Text) < ho2 | Convert.ToDouble(txbTheta3.Text) < ho3 | txtSendMsg.Text == "non-existing position")
            {
                txtX.Text = Convert.ToString(int.Parse(txtX.Text) - 2);
                XYZtoTheta();
            }
            run();    // gửi giá trị góc quay theta về OPC và tạo xung để step quay
            timer_press.Start();    // nếu còn nhấn chuột thì cho timer_press chạy để khi đè thì dịch tự động, thả chuột thì stop timer
        }
        private void left_press(object sender, MouseEventArgs e)
        {
            dem = 2;
            txtX.Text = Convert.ToString(int.Parse(txtX.Text) - 2);
            XYZtoTheta();
            if (Convert.ToDouble(txbTheta1.Text) < ho1 | Convert.ToDouble(txbTheta2.Text) < ho2 | Convert.ToDouble(txbTheta3.Text) < ho3 | txtSendMsg.Text == "non-existing position")
            {
                txtX.Text = Convert.ToString(int.Parse(txtX.Text) + 2);
                XYZtoTheta();
            }
            run();
            timer_press.Start();
        }
        private void up_press(object sender, MouseEventArgs e)
        {
            dem = 3;
            txtY.Text = Convert.ToString(int.Parse(txtY.Text) + 2);
            XYZtoTheta();
            if (Convert.ToDouble(txbTheta1.Text) < ho1 | Convert.ToDouble(txbTheta2.Text) < ho2 | Convert.ToDouble(txbTheta3.Text) < ho3 | txtSendMsg.Text == "non-existing position")
            {
                txtY.Text = Convert.ToString(int.Parse(txtY.Text) - 2);
                XYZtoTheta();
            }
            run();
            timer_press.Start();
        }
        private void down_press(object sender, MouseEventArgs e)
        {
            dem = 4;
            txtY.Text = Convert.ToString(int.Parse(txtY.Text) - 2);
            XYZtoTheta();
            if (Convert.ToDouble(txbTheta1.Text) < ho1 | Convert.ToDouble(txbTheta2.Text) < ho2 | Convert.ToDouble(txbTheta3.Text) < ho3 | txtSendMsg.Text == "non-existing position")
            {
                txtY.Text = Convert.ToString(int.Parse(txtY.Text) + 2);
                XYZtoTheta();
            }
            run();
            timer_press.Start();
        }
        private void len_press(object sender, MouseEventArgs e)
        {
            dem = 5;
            txtZ.Text = Convert.ToString(int.Parse(txtZ.Text) + 2);
            XYZtoTheta();
            if (Convert.ToDouble(txbTheta1.Text) < ho1 | Convert.ToDouble(txbTheta2.Text) < ho2 | Convert.ToDouble(txbTheta3.Text) < ho3 | txtSendMsg.Text == "non-existing position")
            {
                txtZ.Text = Convert.ToString(int.Parse(txtZ.Text) - 2);
                XYZtoTheta();
            }
            run();
            timer_press.Start();
        }

        private void xuong_press(object sender, MouseEventArgs e)
        {
            dem = 6;
            txtZ.Text = Convert.ToString(int.Parse(txtZ.Text) - 2);
            XYZtoTheta();
            if (Convert.ToDouble(txbTheta1.Text) < ho1 | Convert.ToDouble(txbTheta2.Text) < ho2 | Convert.ToDouble(txbTheta3.Text) < ho3 | txtSendMsg.Text == "non-existing position")
            {
                txtZ.Text = Convert.ToString(int.Parse(txtZ.Text) + 2);
                XYZtoTheta();
            }
            run();
            timer_press.Start();
        }

        private void btnreset(object sender, EventArgs e)
        {
            txtX.Text = "0";
            txtY.Text = "0";
            txtZ.Text = Zhome;
            txbTheta1.Text = Convert.ToString(ho1);
            txbTheta2.Text = Convert.ToString(ho2);
            txbTheta3.Text = Convert.ToString(ho3);
        }
        private void resetvalue(object sender, EventArgs e)    // ghi giá trị value lại sau khi reset
        {
            R = B = G = 0;
            textBoxred.Text = R.ToString();
            textBoxblue.Text = B.ToString();
            textBoxgreen.Text = G.ToString();
            try
            {
                WriteItems.SetValue(R, 19);
                WriteItems.SetValue(B, 20);
                WriteItems.SetValue(G, 21);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void btnreset_mousedown(object sender, MouseEventArgs e)    // gửi giá trị reset cho OPC gửi về PLC lúc nhấn 
        {
            try
            {
                WriteItems.SetValue("1", 7);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }


        private void btnreset_mouseup(object sender, MouseEventArgs e)        // gửi giá trị reset cho OPC gửi về PLC lúc nhả 
        {
            try
            {
                WriteItems.SetValue("0", 7);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void btnGripDown(object sender, MouseEventArgs e)
        {
            try
            {
                WriteItems.SetValue("1", 14);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void btnGripUp(object sender, MouseEventArgs e)
        {
            try
            {
                WriteItems.SetValue("0", 14);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void release(object sender, MouseEventArgs e)
        {
            timer_press.Stop();
        }
        private void timer_press_Tick(object sender, EventArgs e)
        {
            if (Convert.ToDouble(txbTheta1.Text) > ho1 && Convert.ToDouble(txbTheta2.Text) > ho2 && Convert.ToDouble(txbTheta3.Text) > ho3)   // khi chưa đụng công tắc hành trình
            {
                switch (dem)
                {
                    case 1:
                        txtX.Text = Convert.ToString(int.Parse(txtX.Text) + 2);
                        XYZtoTheta();
                        run();
                        break;
                    case 2:
                        txtX.Text = Convert.ToString(int.Parse(txtX.Text) - 2);
                        XYZtoTheta();
                        run();
                        break;
                    case 3:
                        txtY.Text = Convert.ToString(int.Parse(txtY.Text) + 2);
                        XYZtoTheta();
                        run();
                        break;
                    case 4:
                        txtY.Text = Convert.ToString(int.Parse(txtY.Text) - 2);
                        XYZtoTheta();
                        run();
                        break;
                    case 5:
                        txtZ.Text = Convert.ToString(int.Parse(txtZ.Text) + 2);
                        XYZtoTheta();
                        run();
                        break;
                    case 6:
                        txtZ.Text = Convert.ToString(int.Parse(txtZ.Text) - 2);
                        XYZtoTheta();
                        run();
                        break;
                }
            }
        }
        private void run()
        {
            try
            {
                WriteItems.SetValue(txbTheta1.Text, 1);
                WriteItems.SetValue(txbTheta2.Text, 2);
                WriteItems.SetValue(txbTheta3.Text, 3);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            Enable_move();
            Disable_move();
        }
        #endregion 
        private void btnnhaptdbt_auto_Click(object sender, EventArgs e)
        {
            td_am = true;
            timer1.Start();
        }
        private void btnnhaptdbt_manual_Click(object sender, EventArgs e)
        {
            td_am = false;
            timer1.Stop();
            txbtdbangtai.Text = txbnhaptdbangtai.Text;
            lechdc = (Convert.ToDouble(txbtdbangtai.Text) * 1.2);
        }
        Int16 zz = 12;
        private void timer10_Tick(object sender, EventArgs e)
        {
            if (mode == 0)
            {
                if (counter == 0) { dang_gap = true; toichovat(); zz = 20; }
                if (counter == 10) { gapvat(); }
                if (counter == 10 + 3) { nhacvatlen(); if (T1 == TG1) { zz = 17; } if (T1 == TR1) { zz = 16; } }
                if (counter == 10 + 3 + 5) { toichothavat(); }
                if (counter == 10 + 3 + 5 + zz) { thavat(); }
                if (counter == 10 + 3 + 5 + zz + 6) { CloseValve(); }   // thả vật xong rồi nhé           
                if (counter == 10 + 3 + 5 + zz + 6 + 1) { vevitricu(); }
                if (counter == 10 + 3 + 5 + zz + 6 + 1 + 4) { dang_gap = false; a = true; timer10.Stop(); counter = 0; set_bit_rst_sensor(); rst_bit_rst_sensor(); }
                counter++;
            }
            else if (mode == 1)
            {

                if (counter == 0) { }
                if (counter == 25) { dang_gap = true; toichovat(); zz = 12; }
                if (counter == 35) { gapvat(); }
                if (counter == 35 + 3) { nhacvatlen(); if (T1 == TG1) { zz = 9; } if (T1 == TR1) { zz = 8; } }
                if (counter == 35 + 3 + 10) { toichothavat(); }
                if (counter == 35 + 3 + 10 + zz) { thavat(); }
                if (counter == 35 + 3 + 10 + zz + 6) { CloseValve(); }   // thả vật xong rồi nhé           
                if (counter == 35 + 3 + 10 + zz + 6 + 1) { vevitricu(); }
                if (counter == 35 + 3 + 10 + zz + 6 + 1 + 4) { dang_gap = false; a = true; timer10.Stop(); counter = 0; set_bit_rst_sensor(); rst_bit_rst_sensor(); }
                counter++;

            }

        }


        void set_bit_rst_sensor()
        {
            try
            {
                WriteItems.SetValue("1", 23);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        void rst_bit_rst_sensor()
        {
            try
            {
                WriteItems.SetValue("0", 23);

                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void toichovat()
        {
            txtZ.Text = Zchogap;   // z = -224
            XYZtoTheta();
            try
            {
                WriteItems.SetValue(txbTheta1.Text, 1);
                WriteItems.SetValue(txbTheta2.Text, 2);
                WriteItems.SetValue(txbTheta3.Text, 3);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            Enable_move();
            Disable_move();
        }
        private void gapvat()
        {
            txtZ.Text = Zgapvat;
            XYZtoTheta();
            try
            {
                WriteItems.SetValue(txbTheta1.Text, 1);
                WriteItems.SetValue(txbTheta2.Text, 2);
                WriteItems.SetValue(txbTheta3.Text, 3);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            Enable_move();
            Disable_move();
        }
        private void nhacvatlen()
        {
            double zn = Convert.ToDouble(Znangvat);
            txtZ.Text = Convert.ToString(zn);
            XYZtoTheta();
            while (Convert.ToDouble(txbTheta1.Text) < ho1 | Convert.ToDouble(txbTheta2.Text) < ho2 | Convert.ToDouble(txbTheta3.Text) < ho3)
            {
                zn--;
                txtZ.Text = Convert.ToString(zn);
                XYZtoTheta();
            }
            //Znangvat = "-212";
            textBox17.Text = txtZ.Text;
            try
            {
                WriteItems.SetValue(txbTheta1.Text, 1);
                WriteItems.SetValue(txbTheta2.Text, 2);
                WriteItems.SetValue(txbTheta3.Text, 3);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            Enable_move();
            Disable_move();
        }
        private void toichothavat()
        {
            try
            {
                WriteItems.SetValue(T1, 1);        // tới ô thả vật
                WriteItems.SetValue(T2, 2);
                WriteItems.SetValue(T3, 3);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            Enable_move();
            Disable_move();
        }
        private void thavat()
        {
            OpenValve();                  //thả vật
            if (T1 == TR1)
            {
                R = R + 1;
                textBoxred.Text = R.ToString();
            }
            if (T1 == TB1)
            {
                B = B + 1;
                textBoxblue.Text = B.ToString();
            }
            if (T1 == TG1)
            {
                G = G + 1;
                textBoxgreen.Text = G.ToString();
            }
            try
            {
                WriteItems.SetValue(R, 19);
                WriteItems.SetValue(B, 20);
                WriteItems.SetValue(G, 21);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }

        }
        private void thavatxong()
        {
            CloseValve();
        }
        private void vevitricu()
        {
            try
            {
                WriteItems.SetValue("0.74", 1);
                WriteItems.SetValue("10.86", 2);
                WriteItems.SetValue("-9.91", 3);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            Enable_move();
            Disable_move();
        }
        private void OpenValve()
        {
            try
            {
                WriteItems.SetValue("1", 14);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void CloseValve()
        {
            try
            {
                WriteItems.SetValue("0", 14);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        #region CHỈNH GÓC
        private void forward_t1(object sender, MouseEventArgs e)
        {
            t = 1;
            try
            {
                WriteItems.SetValue("1", 8);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void backward_t1(object sender, MouseEventArgs e)
        {

            t = 2;
            try
            {

                WriteItems.SetValue("1", 9);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void forward_t2(object sender, MouseEventArgs e)
        {
            t = 3;
            try
            {
                WriteItems.SetValue("1", 10);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void backward_t2(object sender, MouseEventArgs e)
        {
            t = 4;
            try
            {
                WriteItems.SetValue("1", 11);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void forward_t3(object sender, MouseEventArgs e)
        {
            t = 5;
            try
            {
                WriteItems.SetValue("1", 12);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void backward_t3(object sender, MouseEventArgs e)
        {
            t = 6;
            try
            {
                WriteItems.SetValue("1", 13);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void release_theta(object sender, MouseEventArgs e)
        {
            switch (t)
            {
                case 1:
                    try
                    {
                        WriteItems.SetValue("0", 8);
                        ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    break;
                case 2:
                    try
                    {
                        WriteItems.SetValue("0", 9);
                        ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    break;
                case 3:
                    try
                    {
                        WriteItems.SetValue("0", 10);
                        ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    break;
                case 4:
                    try
                    {
                        WriteItems.SetValue("0", 11);
                        ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    break;
                case 5:
                    try
                    {
                        WriteItems.SetValue("0", 12);
                        ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    break;
                case 6:
                    try
                    {
                        WriteItems.SetValue("0", 13);
                        ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    break;
            }
        }
        private void radioBtn1_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Stop();
            mode = 0;
            hinh = 4;
            mode_0();
        }
        private void radioBtn2_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Stop();
            mode = 0;
            hinh = 3;
            mode_0();
        }
        private void radioBtn3_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Stop();
            mode = 0;
            hinh = 20;
            mode_0();
        }



        private void hScrollBar7_Scroll(object sender, ScrollEventArgs e)
        {
            //MIN = hScrollBar7.Value * hScrollBar7.Value;
        }

        private void txb_color_TextChanged(object sender, EventArgs e)
        {

        }

        private void txbXX_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtX_TextChanged(object sender, EventArgs e)
        {

        }

        private void txbXPresent_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupAUTO2_Enter(object sender, EventArgs e)
        {

        }

        private void groupAUTO1_Enter(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txbsetZgapvat_TextChanged(object sender, EventArgs e)
        {

        }

        private void txb_cameranumber_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            mode = 1;
            timer2.Start();
            mode_1();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                BarcodeReader barcodereader = new BarcodeReader();
                Result result = barcodereader.Decode((Bitmap)pictureBox1.Image);

                if (result != null)
                {
                    txtQRCode.Text = result.ToString();
                    ma_sp = get2(txtQRCode.Text);


                }
            }
        }

        void mode_0()
        {
            try
            {
                WriteItems.SetValue("0", 24);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        void mode_1()
        {
            try
            {
                WriteItems.SetValue("1", 24);
                ConnectedGroup.SyncWrite(ItemCount, ref ItemServerHandles, ref WriteItems, out ItemServerErrors);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void label50_Click(object sender, EventArgs e)
        {

        }

        private void label51_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (Username.Text == "DATN" && Password.Text == "123")
            {
                groupBox8.Visible = false;
            }

            if (Username.Text != "DATN")
            {
                Username.BackColor = Color.Red;
            }

            if (Password.Text != "123")
            {
                Password.BackColor = Color.Red;
            }
        }








        #region SETTING

        private void btnload1_Click(object sender, EventArgs e)
        {
            ee = Convert.ToDouble(txbsetee.Text);
            ff = Convert.ToDouble(txbsetff.Text);
            re = Convert.ToDouble(txbsetre.Text);
            rf = Convert.ToDouble(txbsetrf.Text);
        }

        private void btnload2_Click(object sender, EventArgs e)
        {
            X2_X1 = Convert.ToDouble(txbsetX2.Text) / Convert.ToDouble(txbsetX1.Text);
            Y2_Y1 = Convert.ToDouble(txbsetY2.Text) / Convert.ToDouble(txbsetY1.Text);
            Xlech = Convert.ToDouble(txbsetXlech.Text);
            Ylech = Convert.ToDouble(txbsetYlech.Text);
        }
        private void btnload3_Click(object sender, EventArgs e)
        {
            Zgapvat = txbsetZgapvat.Text;
            Znangvat = txbsetZnangvat.Text;
        }
        private void btnload4_Click(object sender, EventArgs e)
        {
            MIN = Convert.ToInt32(txbsetMIN.Text);
            //MAX = Convert.ToInt32(txbsetMAX.Text);
        }
        #endregion
        private void btn_Len_Click(object sender, EventArgs e) { }
        #endregion
        #region
        private void fgg_Enter(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void tabPage3_Click(object sender, EventArgs e) { }
        private void textBox6_TextChanged(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label19_Click(object sender, EventArgs e) { }
        private void button3_Click(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void btnRight_Click(object sender, EventArgs e) { }
        private void groupBox7_Enter(object sender, EventArgs e) { }
        private void txbYj1_TextChanged(object sender, EventArgs e) { }
        private void btnFor1_Click(object sender, EventArgs e) { }
        private void btnBack1_Click(object sender, EventArgs e) { }
        private void btnFor2_Click(object sender, EventArgs e) { }
        private void btnFor3_Click(object sender, EventArgs e) { }
        private void btnBack2_Click(object sender, EventArgs e) { }
        private void btnBack3_Click(object sender, EventArgs e) { }
        private void label13_Click(object sender, EventArgs e) { }
        private void btn_Xuong_Click(object sender, EventArgs e) { }
        private void lbTrangThai_Click(object sender, EventArgs e) { }
        private void tabPage2_Click(object sender, EventArgs e) { }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
        #endregion

    }

}
