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
using System.Diagnostics;

namespace It_Liga_Security
{
    public partial class Podkl_k_BD_Form : Form
    {
        private Con_Baze _CB;
        private Reg_Informat _RI;
        private bool checking = true;
        private bool get_server_list = true;

        public Podkl_k_BD_Form()
        {
            InitializeComponent();
        }
        

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            try
            {
                _CB = new Con_Baze();
                _CB.List_Dbs += _BUList_Dbs;
                Reg_Informat.DS = ListBase.Text;
                Reg_Informat.UN = LoginTextBox.Text;
                Reg_Informat.UP = PasswordTextBox.Text;
                Thread Th = new Thread(_CB.Get_Base_list);
                Th.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void _CB_Status(bool value)
        {
            try
            {                
                Action Act = () =>
                {
                    checking = true;
                    switch (value)
                    {
                        case (true):
                            get_server_list = false;
                            flatStatusBar1.Text = "Подключение установленно!";                            
                            ListServer.Enabled = true;
                            Login_Form L_F = new Login_Form();
                            this.Hide();
                            L_F.Show();
                            break;
                        case (false):
                            flatStatusBar1.Text = "Отсутствует подключение! Скоро оно появится, ожидайте :-)";
                            MetroFramework.MetroMessageBox.Show(this, "Ожидайте, подключение скоро появится!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            PoluchSpisokButton.Enabled = false;
                            _CB.List_Server += _BU_List_Server;
                            Thread Th1 = new Thread(_CB.Get_Server_List);
                            Th1.Start();
                            break;
                    }
                };
                Invoke(Act);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void _BU_List_Server(DataTable value)
        {
            try
            {
                Action Act = () =>
                {
                    get_server_list = false;
                    foreach (DataRow Row in value.Rows)
                    {
                        ListServer.Items.Add(Row[0] + "\\" + Row[1]);
                    }
                    ListServer.Enabled = true;

                    flatStatusBar1.Text = "Список серверов получен.";
                    MetroFramework.MetroMessageBox.Show(this, "Подключение установлено. Выберите нужный сервер. Введите логин и пароль от него.", "Успешное выполнение", MessageBoxButtons.OK, MessageBoxIcon.Question);
                };
                Invoke(Act);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void _BUList_Dbs(DataSet value)
        {
            try
            {
                Action Act = () =>
                {
                    ListBase.DataSource = value.Tables[0];
                    ListBase.DisplayMember = "name";
                    ListBase.Enabled = true;
                    PodklButton.Enabled = true;
                };
                Invoke(Act);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void bunifuFlatButton2_Click(object sender, EventArgs e)
        {            
            Application.Exit();
            Reg_Informat _RI = new Reg_Informat();
            _RI.Connection.Close();
        }

        private void PodklButton_Click(object sender, EventArgs e)
        {
            Login_Form L_F = new Login_Form();
            try
            {
                    _RI = new Reg_Informat();
                    _RI.Register_set(ListServer.Text, ListBase.Text, LoginTextBox.Text, PasswordTextBox.Text);
                    L_F.Show();
                    bunifuElipse1.Dispose();
                    this.Hide();
            }
            catch (Exception ex)
            {
                MetroFramework.MetroMessageBox.Show(this, ex.Message, "Системная ошибка", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void ListServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
               if (ListServer.SelectedIndex >= 0)
                {                    
                    LoginTextBox.Enabled = true;
                    LoginTextBox.HintText = "Введите логин от сервера";
                    PasswordTextBox.Enabled = true;        
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PasswordTextBox_OnValueChanged(object sender, EventArgs e)
        {
            
            try
            {               

                if ((LoginTextBox.Text == "") + PasswordTextBox.Text == "")
                {
                    PasswordTextBox.LineIdleColor = Color.Red;
                    PasswordTextBox.ForeColor = Color.Red;
                    PoluchSpisokButton.Enabled = false;
                }
                else
                {
                    PasswordTextBox.LineIdleColor = Color.Green;
                    PasswordTextBox.ForeColor = Color.Green;
                    PoluchSpisokButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PasswordTextBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (PasswordTextBox.Text == "")
            {
                PasswordTextBox.HintText = "Введите пароль от сервера";
                PasswordTextBox.isPassword = false;
            }
            else
            {
                PasswordTextBox.HintText = "";
                PasswordTextBox.isPassword = true;
            }
        }

        private void LoginTextBox_Load(object sender, EventArgs e)
        {
            LoginTextBox.HintText = "Введите логин от сервера";
        }

        private void PasswordTextBox_Load(object sender, EventArgs e)
        {
            PasswordTextBox.isPassword = false;
            PasswordTextBox.HintText = "Введите пароль от сервера";
        }

        private void LoginTextBox_OnValueChanged(object sender, EventArgs e)
        {
            if (LoginTextBox.Text == "")
            {
                LoginTextBox.LineIdleColor = Color.Red;
                LoginTextBox.ForeColor = Color.Red;
            }
            else
            {
                LoginTextBox.LineIdleColor = Color.Green;
                LoginTextBox.ForeColor = Color.Green;
            }
        }

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            PasswordTextBox.isPassword = true;
        }

        private void Podkl_k_BD_Form_Load(object sender, EventArgs e)
        {
            try
            {
                this.StartPosition = FormStartPosition.CenterScreen;
                _CB = new Con_Baze();
                PoluchSpisokButton.Enabled = false;
                ListServer.Enabled = false;
                _CB.Status += _CB_Status;
                flatStatusBar1.Text = "Проверка подключения";
                Thread Th1 = new Thread(_CB.Connection_State);
                Th1.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
