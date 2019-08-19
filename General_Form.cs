using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using NLog;

namespace It_Liga_Security
{
    
    public partial class General_Form : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();//Логирование данных программы
        internal VideoCapture capture;// Переменные для распознавания лиц
        public CascadeClassifier oclface;
        internal Image<Bgr, Byte> currentFrame;
        internal Image<Gray, byte> grayface = null;
        internal IInputArray OclImage = null;//Для распознавания лиц
        Con_Baze _CB = new Con_Baze();

        public General_Form()
        {
            oclface = new CascadeClassifier (Application.StartupPath + "\\face.xml");//алгоритм распознавания
            InitializeComponent();
        }

        private void bunifuImageButton3_Click(object sender, EventArgs e)
        {
            if (tabGeneralControl.SelectedIndex == 2)
            {
                Application.Exit();
                stop_capture();//Прекрашение видео потока
            }
            else
            {
                Application.Exit();
            }
        }

        
        #region DirectShow List Video Devices
        //===================================
        internal static readonly Guid SystemDeviceEnum = new Guid(0x62BE5D10, 0x60EB, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);
        internal static readonly Guid VideoInputDevice = new Guid(0x860BB310, 0x5D01, 0x11D0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

        [ComImport, Guid("55272A00-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IPropertyBag
        {
            [PreserveSig]
            int Read(
                [In, MarshalAs(UnmanagedType.LPWStr)] string propertyName,
                [In, Out, MarshalAs(UnmanagedType.Struct)] ref object pVar,
                [In] IntPtr pErrorLog);
            [PreserveSig]
            int Write(
                [In, MarshalAs(UnmanagedType.LPWStr)] string propertyName,
                [In, MarshalAs(UnmanagedType.Struct)] ref object pVar);
        }

        [ComImport, Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICreateDevEnum
        {
            [PreserveSig]
            int CreateClassEnumerator([In] ref Guid type, [Out] out IEnumMoniker enumMoniker, [In] int flags);
        }

        private void ListVideoDevices()
        {
            Object bagObj = null;
            object comObj = null;
            ICreateDevEnum enumDev = null;
            IEnumMoniker enumMon = null;
            IMoniker[] moniker = new IMoniker[99];
            IPropertyBag bag = null;
            try
            {
                // Get the system device enumerator
                Type srvType = Type.GetTypeFromCLSID(SystemDeviceEnum);
                // create device enumerator
                comObj = Activator.CreateInstance(srvType);
                enumDev = (ICreateDevEnum)comObj;
                // Create an enumerator to find filters of specified category
                enumDev.CreateClassEnumerator(VideoInputDevice, out enumMon, 0);
                Guid bagId = typeof(IPropertyBag).GUID;
                while (enumMon.Next(1, moniker, IntPtr.Zero) == 0)
                {
                    // get property bag of the moniker
                    moniker[0].BindToStorage(null, null, ref bagId, out bagObj);
                    bag = (IPropertyBag)bagObj;
                    // read FriendlyName
                    object val = "";
                    bag.Read("FriendlyName", ref val, IntPtr.Zero);
                    //list in box
                    VideoDevicesBox.Items.Add((string)val);
                }

            }
            catch (Exception)
            {
            }
            finally
            {
                bag = null;
                if (bagObj != null)
                {
                    Marshal.ReleaseComObject(bagObj);
                    bagObj = null;
                }
                enumDev = null;
                enumMon = null;
                moniker = null;
            }
            if (VideoDevicesBox.Items.Count > 0) VideoDevicesBox.SelectedIndex = 0;
        }
        #endregion //List Video Devices
        //Блок кода для распознавания лица и подключения к устройствам

        private void VideoDevicesBox_SelectedIndexChanged(object sender, EventArgs e)//Подключение к usb-камерам 
        {
            if (!newlyloaded)
            {
                stop_capture();
                initialise_capture(VideoDevicesBox.SelectedIndex);
            }
        }

        bool newlyloaded = true;

        private void IP_Cam_DevicesBox_SelectedIndexChanged(object sender, EventArgs e)//Подключение к ip-камерам
        {
            if (!newiploaded)
            {
                stop_capture();
                initialise_capture(DevicesBox.SelectedIndex);
            }
        }

        bool newiploaded = true;

        private void WebCamButton_CheckedChanged(object sender)
        {           
           ListVideoDevices();
           initialise_capture(0);
           newlyloaded = false;      
        }

        internal void stop_capture()
        {
            grabstart = false;
            try
            {
                tvthread.Abort();
            }
            catch { }
            Thread.Sleep(1000);
            capture.Dispose();
        }

        internal bool grabstart = false;
        System.Threading.Thread tvthread;
        internal void initialise_capture(int camndex)
        {
            capture = new VideoCapture(camndex);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1280);
            capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 800);
            grabstart = true;
            tvthread = new System.Threading.Thread(runthreadedframe);
            tvthread.Priority = ThreadPriority.AboveNormal;
            tvthread.Start();
        }

        internal void runthreadedframe(object State)
        {
            Thread.Sleep(1000);
            timerthreadedframe = new System.Threading.Timer(voidthreadedframe, null, 0, Timeout.Infinite);
        }

        System.Threading.Timer timerthreadedframe;
        internal void voidthreadedframe(object State)//Зацикливание потока
        {
            xTvGrabber();
            if (grabstart) timerthreadedframe.Change(0, Timeout.Infinite); else timerthreadedframe.Change(Timeout.Infinite, Timeout.Infinite);
        }

        string name_face = "Unkown";
        Rectangle orig_area;

        internal void xTvGrabber()
        {
            try
            {
                GC.AddMemoryPressure(1028);
                currentFrame = capture.QueryFrame().ToImage<Bgr, Byte>().Resize(640, 400, Emgu.CV.CvEnum.Inter.Cubic);
                if (currentFrame != null)
                {
                    Rectangle[] faces = null;
                    OclImage = currentFrame.Convert<Gray, Byte>();
                    faces = oclface.DetectMultiScale(OclImage, 1.1, 10, new Size(default), new Size(default));


                    for (int i = 0; i < faces.Length; i++)
                    {
                        orig_area = new Rectangle(faces[i].X, faces[i].Y, faces[i].Width, faces[i].Height);
                        pictureBox3.Image = currentFrame.Copy(orig_area).ToBitmap();
                        currentFrame.Draw(orig_area, new Bgr(Color.Yellow), 3);
                        currentFrame.Draw(name_face + " ", new Point(orig_area.X - 1, orig_area.Y - 2), FontFace.HersheyPlain, 1.6, new Bgr(Color.DeepSkyBlue), 1, LineType.AntiAlias, false);
                        if (name_face == "Unkown")
                        {
                            currentFrame.Draw(orig_area, new Bgr(Color.Red), 3);
                        }
                        else
                        {
                            currentFrame.Draw(orig_area, new Bgr(Color.Green), 3);
                        }


                    }
                    pictureBox2.Image = currentFrame.ToBitmap();

                }
                GC.RemoveMemoryPressure(1028);
                GC.Collect();
            }
            catch (Exception msg)//Формирует лог при возникновении ошибки
            {
                logger.Debug(msg.ToString());
            }

        }


        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            if (leftMenu.Width == 50)
            {
                leftMenu.Visible = false;
                leftMenu.Width = 200;
                PanelAnimator.ShowSync(leftMenu);
                LogoAnimator.ShowSync(logo);
            }
            else
            {
                LogoAnimator.HideSync(logo);
                leftMenu.Visible = false;
                leftMenu.Width = 50;
                PanelAnimator.ShowSync(leftMenu);
            }
        }

        private void ConfigButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (UstroistvaButton.Visible == false) 
                {
                    UstroistvaButton.Visible = true;
                }
                else
                {
                    UstroistvaButton.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PropuskaButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (SotrButton.Visible == false)
                {
                    SotrButton.Visible = true;
                }
                else
                {
                    SotrButton.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void SizeGeneralFormButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void VisibleGeneralFormButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.WindowState = FormWindowState.Minimized;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LogoHomeButton_Click(object sender, EventArgs e)
        {
            tabGeneralControl.Visible = false;
            LogoAnimator.HideSync(logo);
            leftMenu.Visible = false;
            leftMenu.Width = 200;
            PanelAnimator.ShowSync(leftMenu);
        }

        private void ExportWordButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "Word Documents (*.docx)|*.docx";

            sfd.FileName = "Ustroistva.docx";

            if (sfd.ShowDialog() == DialogResult.OK)
            {

                _CB.Export_Data_To_Word(UstroistvaGrid, sfd.FileName);
            }
        }

        private void General_Form_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "it_Liga_SecurityDataSet.Ustroistva". При необходимости она может быть перемещена или удалена.
            this.ustroistvaTableAdapter.Fill(this.it_Liga_SecurityDataSet.Ustroistva);
            try
            {
                switch (Program.ADMINACCSS)
                {
                    case (0):
                        ConfigButton.Visible = false;
                        PropuskaButton.Visible = false;
                        MonitoringButton.Visible = false;
                        SobitiaButton.Visible = true;
                        LogoutButton.Visible = true;
                        break;
                    case (1):
                        ConfigButton.Visible = true;
                        PropuskaButton.Visible = true;
                        SobitiaButton.Visible = true;
                        LogoutButton.Visible = true;
                        break;
                }

               

                this.WindowState = FormWindowState.Maximized;
                _CB.Ustroistva_Select();
                UstroistvaGrid.DataSource = Program.Ustroistva_Select;
                UstroistvaGrid.Columns[0].Visible = false;
                UstroistvaGrid.Columns[1].Visible = true;
                UstroistvaGrid.Columns[2].Visible = true;
                UstroistvaGrid.Columns[3].Visible = true;

                _CB.Sotr_Select();
                UchetGrid.DataSource = Program.Sotr_Select;
                UchetGrid.Columns[0].Visible = false;
                UchetGrid.Columns[1].Visible = true;
                UchetGrid.Columns[2].Visible = true;
                UchetGrid.Columns[3].Visible = true;
                UchetGrid.Columns[4].Visible = false;
                UchetGrid.Columns[5].Visible = false;
                UchetGrid.Columns[6].Visible = false;
                UchetGrid.Columns[7].Visible = false;
                UchetGrid.Columns[8].Visible = true;
                UchetGrid.Columns[9].Visible = true;
                UchetGrid.Columns[10].Visible = true;
                UchetGrid.Columns[11].Visible = false;
                UchetGrid.Columns[12].Visible = false;
                UchetGrid.Columns[13].Visible = false;
            }
            catch(Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void UstroistvaGrid_Click(object sender, EventArgs e)
        {
            try
            {
                MAC_adressTextBox.Text = UstroistvaGrid.CurrentRow.Cells[1].Value.ToString();
                NameUstroistvaTextBox.Text = UstroistvaGrid.CurrentRow.Cells[2].Value.ToString();
                ProhodNameTextbox.Text = UstroistvaGrid.CurrentRow.Cells[3].Value.ToString();
            }
            catch (System.Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ExportExcelButton_Click(object sender, EventArgs e)
        {
            _CB.Export_Data_To_XLSX(UstroistvaGrid, "UstroistvaXLS");
        }

        private void ExportPdfButton_Click(object sender, EventArgs e)
        {
            _CB.exportgridtopdf(UstroistvaGrid, "UstroistvaSpisokPDF");
        }

        private void UchetZapButton_Click(object sender, EventArgs e)
        {
            tabGeneralControl.Show();
            tabGeneralControl.SelectedIndex = 1;
            LogoAnimator.HideSync(logo);
            leftMenu.Visible = false;
            leftMenu.Width = 50;
            PanelAnimator.ShowSync(leftMenu);
        }

        private void UstroistvaButton_Click_1(object sender, EventArgs e)
        {
            tabGeneralControl.Show();
            tabGeneralControl.SelectedIndex = 0;
            LogoAnimator.HideSync(logo);
            leftMenu.Visible = false;
            leftMenu.Width = 50;
            PanelAnimator.ShowSync(leftMenu);
        }

        private void LogoutButton_Click_1(object sender, EventArgs e)
        {
            Program.ADMINACCSS = 0;
            Program.SYACCSS = 0;
            Program.UID = 0;

            Login_Form L_F = new Login_Form();
            L_F.Show();
            this.Hide();
        }

       

        private void MonitoringButton_Click(object sender, EventArgs e)
        {
            tabGeneralControl.Show();
            tabGeneralControl.SelectedIndex = 2;
            LogoAnimator.HideSync(logo);
            leftMenu.Visible = false;
            leftMenu.Width = 50;
            PanelAnimator.ShowSync(leftMenu);
        }

        private void SobitiaButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (MonitoringButton.Visible == false)
                {
                    MonitoringButton.Visible = true;
                }
                else
                {
                    MonitoringButton.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveFaceButton_Click(object sender, EventArgs e)
        {
            name_face = NameFaceTextBox.Text;
        }

        private void AddButtonUstr_Click(object sender, EventArgs e)
        {
            try
            {
                if ((MAC_adressTextBox.Text == "") | (NameUstroistvaTextBox.Text == "") | (ProhodNameTextbox.Text == ""))
                {
                    MetroFramework.MetroMessageBox.Show(this, "Не все поля заполнены!", "Добавление устройства", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _CB.Ustroistva_Insert(MAC_adressTextBox.Text, NameUstroistvaTextBox.Text, ProhodNameTextbox.Text);
                    _CB.Ustroistva_Select();
                    UstroistvaGrid.DataSource = Program.Ustroistva_Select;
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void DeleteButtonUstr_Click(object sender, EventArgs e)
        {
            try
            {
                _CB.Ustroistva_Delete(Convert.ToInt32(UstroistvaGrid.CurrentRow.Cells[0].Value));
                _CB.Ustroistva_Select();
                UstroistvaGrid.DataSource = Program.Ustroistva_Select;
            }
            catch (System.Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void EditButtonUstr_Click(object sender, EventArgs e)
        {
            try
            {
                if ((MAC_adressTextBox.Text == "") | (NameUstroistvaTextBox.Text == "") | (ProhodNameTextbox.Text == ""))
                {
                    MetroFramework.MetroMessageBox.Show(this, "Не все поля заполнены!", "Добавление устройства", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _CB.Ustroistva_Edit(Convert.ToInt32(UstroistvaGrid.CurrentRow.Cells[0].Value), MAC_adressTextBox.Text, NameUstroistvaTextBox.Text, ProhodNameTextbox.Text);
                    _CB.Ustroistva_Select();
                    UstroistvaGrid.DataSource = Program.Ustroistva_Select;
                }
            }
            catch (System.Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LogNowButton_Click(object sender, EventArgs e)
        {
            LogTextBox.Text += string.Format("[{0:dd.MM.yyy -- HH:mm:ss.fff}]", DateTime.Now + "; " + name_face + "; " + VideoDevicesBox.SelectedItem);
        }

        private void flatGroupBox3_MouseClick(object sender, MouseEventArgs e)
        {
            LogTextBox.Text += string.Format("[{0:dd.MM.yyy -- HH:mm:ss.fff}]", DateTime.Now + "; " + name_face + "; " + VideoDevicesBox.SelectedItem);
        }
    }
}
