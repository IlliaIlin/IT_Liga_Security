using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace It_Liga_Security
{
    public partial class Login_Form : Form
    {
        public Login_Form()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            try
            {
                Application.Exit();
                Reg_Informat _Ri = new Reg_Informat();
                _Ri.Connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void Podkl_Serv_DestroyButton_Click(object sender, EventArgs e)
        {
            try
            {
                Reg_Informat _RI = new Reg_Informat();
                Podkl_k_BD_Form P_BD = new Podkl_k_BD_Form();
                Login_Form L_F = new Login_Form();
                _RI.Connection.Close();
                _RI.Register_Destroy();
                bunifuElipse1.Dispose();
                this.Hide();
                P_BD.Show();
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void PodklButton_Click(object sender, EventArgs e)
        {
            switch ((LoginTextBox.Text == "") | (PasswordTextBox.Text == "") )
            {
                case (true):
                    LoginTextBox.LineIdleColor = Color.Red;
                    PasswordTextBox.LineIdleColor = Color.Red;                    
                    LoginTextBox.HintForeColor = Color.Red;
                    PasswordTextBox.HintForeColor = Color.Red;
                    PasswordTextBox.isPassword = false;                    
                    break;
                case (false):
                    Con_Baze _CB = new Con_Baze();
                    General_Form G_F = new General_Form();
                    _CB.Auntentification(LoginTextBox.Text, PasswordTextBox.Text);
                    if (Program.SYACCSS == 1)
                    {
                        G_F.Show();
                        this.Hide();
                    }
                    else
                    {
                        MetroFramework.MetroMessageBox.Show(this, "Отсутствует доступ. Или введенные логин или пароль неверны!", "Авторизация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                 break;
                           
                   
            }
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

        private void PasswordTextBox_OnValueChanged(object sender, EventArgs e)
        {
            if (PasswordTextBox.Text == "")
            {
                PasswordTextBox.LineIdleColor = Color.Red;
                PasswordTextBox.ForeColor = Color.Red;
            }
            else
            {
                PasswordTextBox.LineIdleColor = Color.Green;
                PasswordTextBox.ForeColor = Color.Green;
            }
        }

        private void LoginTextBox_Load(object sender, EventArgs e)
        {
            LoginTextBox.HintText = "Введите логин";
        }

        private void PasswordTextBox_Load(object sender, EventArgs e)
        {
            PasswordTextBox.HintText = "Введите пароль";
        }

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            PasswordTextBox.isPassword = true;
        }

        private void PasswordTextBox_OnValueChanged_1(object sender, EventArgs e)
        {
            if (PasswordTextBox.Text == "")
            {
                PasswordTextBox.LineIdleColor = Color.Red;
                PasswordTextBox.ForeColor = Color.Red;
            }
            else
            {
                PasswordTextBox.LineIdleColor = Color.Green;
                PasswordTextBox.ForeColor = Color.Green;
            }
        }

        private void PasswordTextBox_Load_1(object sender, EventArgs e)
        {
            PasswordTextBox.HintText = "Введите пароль";
        }

        private void PasswordTextBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            PasswordTextBox.isPassword = true;
        }

        private void Login_Form_Load(object sender, EventArgs e)
        {

        }
    }
}
