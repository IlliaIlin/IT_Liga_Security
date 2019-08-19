using System;
using System.Windows.Forms;
using System.Data;
using System.Security.Principal;
using System.Diagnostics;
using System.ComponentModel;
using NLog;

namespace It_Liga_Security
{
    static class Program
    {
        public static int RG_MA;
        public static int RG_SA;
        public static int RG_TSA;
        public static int UID;
        public static int SYACCSS;
        public static int ADMINACCSS;
        public static bool Value;
        public static DataTable Ustroistva_Select;
        public static DataTable Sotr_Select;
        private static Logger logger = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ///Логи программы
                logger.Trace("trace message");
                logger.Debug("debug message");
                logger.Info("info message");
                logger.Warn("warn message");
                logger.Error("error message");
                logger.Fatal("fatal message");
            ///Логи программы

            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            if (hasAdministrativeRight == false)
            {
                ProcessStartInfo processInfo = new ProcessStartInfo(); //создаем новый процесс 
                processInfo.Verb = "runas"; //в данном случае указываем, что процесс должен быть запущен с правами администратора 
                processInfo.FileName = Application.ExecutablePath; //указываем исполняемый файл (программу) для запуска 
                try
                {
                    Process.Start(processInfo); //пытаемся запустить процесс 
                }
                catch (Win32Exception)
                {
                    //Ничего не делаем, потому что пользователь, возможно, нажал кнопку "Нет" в ответ на вопрос о запуске программы в окне предупреждения UAC(для Windows 7);
                }
                Application.Exit(); //закрываем текущую копию программы (в любом случае, даже если пользователь отменил запуск с правами администратора в окне UAC) 
            }
            else //имеем права администратора, значит, стартуем
            {
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Podkl_k_BD_Form());
                }

            }
        }
    }
}
