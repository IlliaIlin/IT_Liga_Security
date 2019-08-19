using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Reflection;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text;
using NLog;

namespace It_Liga_Security
{

    class Con_Baze
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Reg_Informat _RI;
        Podkl_k_BD_Form P_BD = new Podkl_k_BD_Form();
        public event Action<bool> Status;
        public SqlCommand _CmdSql;
        public event Action<DataTable> List_Server;
        public event Action<DataSet> List_Dbs;
        public SqlConnection GDtBsLstCn;
        private string Pas_user_id = "select [dbo].[Sotr].[Id_Sotr] from [dbo].[Sotr] inner join [dbo].[Zan_Dolj_Sotr] on [dbo].[Sotr].[Dolj_Sotr_id]=[dbo].[Zan_Dolj_Sotr].[Id_Zan_Dolj_Sotr] where [Sotr].[Password_Sotr]='";
        string sy_access = "SELECT Sotr.System_access FROM Sotr INNER JOIN Zan_Dolj_Sotr ON Sotr.Dolj_Sotr_id = Zan_Dolj_Sotr.Id_Zan_Dolj_Sotr WHERE Sotr.Id_Sotr =";
        string admin_access = "SELECT Zan_Dolj_Sotr.admin_access FROM Sotr INNER JOIN Zan_Dolj_Sotr ON Sotr.Dolj_Sotr_id = Zan_Dolj_Sotr.Id_Zan_Dolj_Sotr WHERE Sotr.Id_Sotr =";
        

        public void Connection_State()
        {
            _RI = new Reg_Informat();
            _RI.Register_get();
            _RI.Connection.Close();
            try
            {
                _RI.Connection.Open();
                Status(true);                
                _RI.Connection.Close();
            }
            catch (Exception msg)
            {
                Status(false);
                logger.Debug(msg.ToString());
            }
        }


        public void Get_Server_List()//Получение данных о серверах
        {
            try
            {
                SqlDataSourceEnumerator ServersCheck = SqlDataSourceEnumerator.Instance;
                DataTable ServersTable = ServersCheck.GetDataSources();
                List_Server(ServersTable);
            }
            catch (Exception msg)
            {
                logger.Debug(msg.ToString());
            }
        }
        public void Get_Base_list()//Получение листа БД
        {
            P_BD.StartPosition = FormStartPosition.CenterScreen;
            
            _RI = new Reg_Informat();
            try
            {
                _RI.Set_Connection();
                GDtBsLstCn = new SqlConnection("Data Source = " + Reg_Informat.DS + "; Initial Catalog = master; Persist Security Info=True; User ID = " + Reg_Informat.UN + "; Password=\"" + Reg_Informat.UP + "\"");
                GDtBsLstCn.Open();
                SqlDataAdapter BsAdpt = new SqlDataAdapter("exec sp_helpdb", GDtBsLstCn);
                DataSet BDst = new DataSet();
                BsAdpt.Fill(BDst, "db");
                List_Dbs(BDst);
                _RI.Connection.Close();
            }
            catch (Exception msg)
            {
                MetroFramework.MetroMessageBox.Show(P_BD,"Данные введены неверно! Проверьте правильность ввода логина или пароля.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                logger.Debug(msg.ToString());
            }

        }
        public void Auntentification(string Login_User, string Pass_user)//Аутентификация пользователя
        {
            try
            {
                _RI = new Reg_Informat();
                _RI.Set_Connection();
                SqlCommand _CmdSql1 = new SqlCommand("select Id_Sotr from Sotr where Login_Sotr = '" + Login_User + "' and " +
                "Password_Sotr = '" + Pass_user + "'", _RI.Connection);
                _RI.Connection.Open();
                Program.UID = Convert.ToInt32(_CmdSql1.ExecuteScalar().ToString());
                switch (Program.UID)
                {
                    case (0):
                        MessageBox.Show("Данные введены не верно!");
                        break;
                    default:
                        _RI = new Reg_Informat();
                        _CmdSql1 = new SqlCommand(sy_access + Program.UID, _RI.Connection);
                        _RI.Set_Connection();
                        _RI.Connection.Open();
                        Program.SYACCSS = Convert.ToInt32(_CmdSql1.ExecuteScalar().ToString());
                        switch (Program.SYACCSS)
                        {
                            case (0):
                                MessageBox.Show("У вас нет прав доступа к системе!");
                                break;
                            case (1):
                                _RI = new Reg_Informat();
                                _RI.Set_Connection();
                                _RI.Connection.Open();
                                SqlCommand ADaccess = new SqlCommand(admin_access + Program.UID, _RI.Connection);
                                Program.ADMINACCSS = Convert.ToInt32(ADaccess.ExecuteScalar().ToString());
                                Program.Value = true;
                                _RI.Connection.Close();
                                break;
                        }
                        _RI.Connection.Close();
                        break;



                }
                _RI.Connection.Close();
            }
            catch (Exception msg)
            {
                logger.Debug(msg.ToString());
            }
        }

        //Хранимые процедуры и представления для таблицы Ustroistva
        public void Ustroistva_Select()//Представление
        {
            try
            {
                _RI = new Reg_Informat();
                _RI.Set_Connection();
                _RI.Connection.Open();
                SqlCommand Ustroistva_Select = new SqlCommand("SELECT Id_Ustroistva as [Идентификатор], MAC_Adress as [Строка подключения], Name_ustroistva as [Имя устройства], Prohod_name as [Расположение] FROM Ustroistva", _RI.Connection);
                SqlDataReader tableReader = Ustroistva_Select.ExecuteReader();
                DataTable table = new DataTable();
                table = new DataTable();
                table.Load(tableReader);
                Program.Ustroistva_Select = table;
                _RI.Connection.Close();
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        public void Ustroistva_Insert(string Mac_Adress, string Name_Ustroistva, string Name_Prohod)//Процедура вставки
        {
            try
            {
                _RI = new Reg_Informat();
                _RI.Set_Connection();
                _RI.Connection.Open();
                SqlCommand StrProc = new SqlCommand("Ustroistva_Insert", _RI.Connection);
                StrProc.CommandType = CommandType.StoredProcedure;
                StrProc.Parameters.AddWithValue("@MAC_Adress", Mac_Adress);
                StrProc.Parameters.AddWithValue("@Name_ustroistva", Name_Ustroistva);
                StrProc.Parameters.AddWithValue("@Prohod_name", Name_Prohod);
                StrProc.ExecuteNonQuery();
                _RI.Connection.Close();
            }
            catch (System.Exception msg)
            {
                System.Windows.Forms.MessageBox.Show(msg.Message);
                logger.Debug(msg.ToString());
            }
        }

        public void Ustroistva_Delete(int Id_Ustroistva)//Процедура удаления
        {

            try
            {
                _RI = new Reg_Informat();
                _RI.Set_Connection();
                _RI.Connection.Open();
                SqlCommand StrProc = new SqlCommand("Ustroistva_Delete", _RI.Connection);
                StrProc.CommandType = CommandType.StoredProcedure;
                StrProc.Parameters.AddWithValue("@Id_Ustroistva", Id_Ustroistva);
                StrProc.ExecuteNonQuery();
                _RI.Connection.Close();
            }
            catch (System.Exception msg)
            {
                logger.Debug(msg.ToString());
                System.Windows.Forms.MessageBox.Show(msg.Message);
            }
        }

        public void Ustroistva_Edit(int Id_Ustroistva, string Mac_Adress, string Name_Ustroistva, string Name_Prohod)//Процедура изменения
        {
            try
            {
                _RI = new Reg_Informat();
                _RI.Set_Connection();
                _RI.Connection.Open();
                SqlCommand StrProc = new SqlCommand("Ustroistva_Update", _RI.Connection);
                StrProc.CommandType = CommandType.StoredProcedure;
                StrProc.Parameters.AddWithValue("@Id_Ustroistva", Id_Ustroistva);
                StrProc.Parameters.AddWithValue("@MAC_Adress", Mac_Adress);
                StrProc.Parameters.AddWithValue("@Name_ustroistva", Name_Ustroistva);
                StrProc.Parameters.AddWithValue("@Prohod_name", Name_Prohod);
                StrProc.ExecuteNonQuery();
                _RI.Connection.Close();
            }
            catch (System.Exception msg)
            {
                logger.Debug(msg.ToString());
                System.Windows.Forms.MessageBox.Show(msg.Message);
            }
        }

        //Хранимые процедуры и представления для таблицы Ustroistva
        public void Sotr_Select()//Представление
        {
            try
            {
                _RI = new Reg_Informat();
                _RI.Set_Connection();
                _RI.Connection.Open();
                SqlCommand Sotr_Select = new SqlCommand("SELECT Id_Sotr as [Идентификатор], F_Sotr as [Фамилия], I_Sotr as [Илья] , O_Sotr as [Отчество], Date_rozd_Sotr as [Дата рождения], Num_telef_Sotr as [№ телефона], Foto_Sotr as [Фото сотрудника], Key_Sotr_id as [Ключ], Email_Sotr as [Почта сотрудника], Login_Sotr as [Логин сотрудника], Password_Sotr as [Пароль сотрудника], System_access as [Доступ к системе], Dolj_Sotr_id as [№ Должности], Dostup_Id as [№ Доступа] FROM Sotr", _RI.Connection);
                SqlDataReader tableReader = Sotr_Select.ExecuteReader();
                DataTable table = new DataTable();
                table = new DataTable();
                table.Load(tableReader);
                Program.Sotr_Select = table;
                _RI.Connection.Close();
            }
            catch (System.Exception msg)
            {
                logger.Debug(msg.ToString());
                System.Windows.Forms.MessageBox.Show(msg.Message);
            }
        }

        public void Sotr_Insert(string F_Sotr, string I_Sotr, string O_Sotr, string Date_rozd_Sotr, string Num_telef_Sotr, string Foto_Sotr, int Key_Sotr_id, string Email_Sotr, string Login_Sotr, string Password_Sotr, int System_access, int Dolj_Sotr_id, int Dostup_Id)//Процедура вставки
        {
            try
            {
                _RI = new Reg_Informat();
                _RI.Set_Connection();
                _RI.Connection.Open();
                SqlCommand StrProc = new SqlCommand("Sotr_Insert", _RI.Connection);
                StrProc.CommandType = CommandType.StoredProcedure;
                StrProc.Parameters.AddWithValue("@F_Sotr", F_Sotr);
                StrProc.Parameters.AddWithValue("@I_Sotr", I_Sotr);
                StrProc.Parameters.AddWithValue("@O_Sotr", O_Sotr);
                StrProc.Parameters.AddWithValue("@Date_rozd_Sotr", Date_rozd_Sotr);
                StrProc.Parameters.AddWithValue("@Num_telef_Sotr", Num_telef_Sotr);
                StrProc.Parameters.AddWithValue("@Foto_Sotr", Foto_Sotr);
                StrProc.Parameters.AddWithValue("@Key_Sotr_id", Key_Sotr_id);
                StrProc.Parameters.AddWithValue("@Email_Sotr", Email_Sotr);
                StrProc.Parameters.AddWithValue("@Login_Sotr", Login_Sotr);
                StrProc.Parameters.AddWithValue("@Password_Sotr", Password_Sotr);
                StrProc.Parameters.AddWithValue("@System_access", System_access);
                StrProc.Parameters.AddWithValue("@Dolj_Sotr_id", Dolj_Sotr_id);
                StrProc.Parameters.AddWithValue("@Dostup_Id", Dostup_Id);

                StrProc.ExecuteNonQuery();
                _RI.Connection.Close();
            }
            catch (System.Exception msg)
            {
                logger.Debug(msg.ToString());
                System.Windows.Forms.MessageBox.Show(msg.Message);
            }
        }

        public void Sotr_Delete(int Id_Sotr)//Процедура удаления
        {
            try
            {
                _RI = new Reg_Informat();
                _RI.Set_Connection();
                _RI.Connection.Open();
                SqlCommand StrProc = new SqlCommand("Sotr_Delete", _RI.Connection);
                StrProc.CommandType = CommandType.StoredProcedure;
                StrProc.Parameters.AddWithValue("@Id_Sotr", Id_Sotr);
                StrProc.ExecuteNonQuery();
                _RI.Connection.Close();
            }
            catch (System.Exception msg)
            {
                logger.Debug(msg.ToString());
                System.Windows.Forms.MessageBox.Show(msg.Message);
            }
        }

        public void Sotr_Edit(int Id_Sotr, string F_Sotr, string I_Sotr, string O_Sotr, string Date_rozd_Sotr, string Num_telef_Sotr, string Foto_Sotr, int Key_Sotr_id, string Email_Sotr, string Login_Sotr, string Password_Sotr, int System_access, int Dolj_Sotr_id, int Dostup_Id)//Процедура изменения
        {
            try
            {
                _RI = new Reg_Informat();
                _RI.Set_Connection();
                _RI.Connection.Open();
                SqlCommand StrProc = new SqlCommand("Ustroistva_Update", _RI.Connection);
                StrProc.CommandType = CommandType.StoredProcedure;
                StrProc.Parameters.AddWithValue("@Id_Sotr", Id_Sotr);
                StrProc.Parameters.AddWithValue("@F_Sotr", F_Sotr);
                StrProc.Parameters.AddWithValue("@I_Sotr", I_Sotr);
                StrProc.Parameters.AddWithValue("@O_Sotr", O_Sotr);
                StrProc.Parameters.AddWithValue("@Date_rozd_Sotr", Date_rozd_Sotr);
                StrProc.Parameters.AddWithValue("@Num_telef_Sotr", Num_telef_Sotr);
                StrProc.Parameters.AddWithValue("@Foto_Sotr", Foto_Sotr);
                StrProc.Parameters.AddWithValue("@Key_Sotr_id", Key_Sotr_id);
                StrProc.Parameters.AddWithValue("@Email_Sotr", Email_Sotr);
                StrProc.Parameters.AddWithValue("@Login_Sotr", Login_Sotr);
                StrProc.Parameters.AddWithValue("@Password_Sotr", Password_Sotr);
                StrProc.Parameters.AddWithValue("@System_access", System_access);
                StrProc.Parameters.AddWithValue("@Dolj_Sotr_id", Dolj_Sotr_id);
                StrProc.Parameters.AddWithValue("@Dostup_Id", Dostup_Id);
                StrProc.ExecuteNonQuery();
                _RI.Connection.Close();
            }
            catch (System.Exception msg)
            {
                logger.Debug(msg.ToString());
                System.Windows.Forms.MessageBox.Show(msg.Message);
            }
        }


        //Экспорт данных в разные форматы
        public void Export_Data_To_Word(DataGridView DGV, string filename) //Экспорт даты в Word 
        {
            try
            {

                if (DGV.Rows.Count != 0)
                {
                    int RowCount = DGV.Rows.Count;
                    int ColumnCount = DGV.Columns.Count;
                    Object[,] DataArray = new object[RowCount + 1, ColumnCount + 1];

                    //Добавления ячеек 
                    int r = 0;
                    for (int c = 0; c <= ColumnCount - 1; c++)
                    {
                        for (r = 0; r <= RowCount - 1; r++)
                        {
                            DataArray[r, c] = DGV.Rows[r].Cells[c].Value;
                        }
                    }

                    Microsoft.Office.Interop.Word.Document oDoc = new Microsoft.Office.Interop.Word.Document();
                    oDoc.Application.Visible = true;
                    //Ориентация страницы 
                    oDoc.PageSetup.Orientation = Microsoft.Office.Interop.Word.WdOrientation.wdOrientLandscape;
                    dynamic oRange = oDoc.Content.Application.Selection.Range;
                    string oTemp = "";
                    for (r = 0; r <= RowCount - 1; r++)
                    {
                        for (int c = 0; c <= ColumnCount - 1; c++)
                        {
                            oTemp = oTemp + DataArray[r, c] + "\t";

                        }
                    }
                    //Формат таблицы 
                    oRange.Text = oTemp;
                    object oMissing = Missing.Value;
                    object Separator = Microsoft.Office.Interop.Word.WdTableFieldSeparator.wdSeparateByTabs;
                    object ApplyBorders = true;
                    object AutoFit = true;
                    object AutoFitBehavior = Microsoft.Office.Interop.Word.WdAutoFitBehavior.wdAutoFitContent;

                    oRange.ConvertToTable(ref Separator, ref RowCount, ref ColumnCount,
                    Type.Missing, Type.Missing, ref ApplyBorders,
                    Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, ref AutoFit, ref AutoFitBehavior, Type.Missing);
                    oRange.Select();
                    oDoc.Application.Selection.Tables[1].Select();
                    oDoc.Application.Selection.Tables[1].Rows.AllowBreakAcrossPages = 0;
                    oDoc.Application.Selection.Tables[1].Rows.Alignment = 0;
                    oDoc.Application.Selection.Tables[1].Rows[1].Select();
                    oDoc.Application.Selection.InsertRowsAbove(1);
                    oDoc.Application.Selection.Tables[1].Rows[1].Select();
                    //Стиль текста ячейки 
                    oDoc.Application.Selection.Tables[1].Rows[1].Range.Bold = 1;
                    oDoc.Application.Selection.Tables[1].Rows[1].Range.Font.Name = "Times New Roman";
                    oDoc.Application.Selection.Tables[1].Rows[1].Range.Font.Size = 14;
                    //Добавление ячейки 
                    for (int c = 0; c <= ColumnCount - 1; c++)
                    {
                        oDoc.Application.Selection.Tables[1].Cell(1, c + 1).Range.Text = DGV.Columns[c].HeaderText;
                    }
                    //Стиль таблицы 
                    oDoc.Application.Selection.Tables[1].Rows[1].Select();
                    oDoc.Application.Selection.Cells.VerticalAlignment = Microsoft.Office.Interop.Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                    //Тест 
                    foreach (Microsoft.Office.Interop.Word.Section section in oDoc.Application.ActiveDocument.Sections)
                    {
                        Microsoft.Office.Interop.Word.Range headerRange = section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                        headerRange.Fields.Add(headerRange, Microsoft.Office.Interop.Word.WdFieldType.wdFieldPage);
                        headerRange.Text = "Экспортированные данные";
                        headerRange.Font.Size = 16;
                        headerRange.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    }
                    //Сохранение файла 
                    oDoc.SaveAs(filename, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing);
                }
            }
            catch (System.Exception msg)
            {
                logger.Debug(msg.ToString());
                System.Windows.Forms.MessageBox.Show(msg.Message);
            }
        }

        public void Export_Data_To_XLSX(DataGridView DGW, string filename) //Экспорт данных в формате XLSX 
        {
            Microsoft.Office.Interop.Excel._Application excel = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook workbook = excel.Workbooks.Add(Type.Missing);
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
            try
            {
                worksheet = workbook.ActiveSheet;
                worksheet.Name = "Вывод данных";
                int cellRowIndex = 1;
                int cellColumnIndex = 1;
                for (int i = 0; i < DGW.Rows.Count - 1; i++)
                {
                    for (int j = 0; j < DGW.Columns.Count; j++)
                    {
                        if (cellRowIndex == 1)
                        {
                            worksheet.Cells[cellRowIndex, cellColumnIndex] = DGW.Columns[j].HeaderText;
                        }
                        else
                        {
                            worksheet.Cells[cellRowIndex, cellColumnIndex] = DGW.Rows[i].Cells[j].Value.ToString();
                        }
                        cellColumnIndex++;
                    }
                    cellColumnIndex = 1;
                    cellRowIndex++;
                }

                //Сохранение документа 
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.FileName = filename;
                saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                saveDialog.FilterIndex = 2;
                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    workbook.SaveAs(saveDialog.FileName);
                    MessageBox.Show("Документ успешно сохранен");
                }
            }
            catch (System.Exception msg)
            {
                logger.Debug(msg.ToString());
                System.Windows.Forms.MessageBox.Show(msg.Message);
            }
            finally
            {
                excel.Quit();
                workbook = null;
                excel = null;
            }
        }

            public void exportgridtopdf(DataGridView DGW, string filename) //Экспорт данных в формате PDF 
            {
            string FONT_LOCATION = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.TTF");
            BaseFont BF = BaseFont.CreateFont(FONT_LOCATION, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            PdfPTable pdftable = new PdfPTable(DGW.Columns.Count);
            pdftable.DefaultCell.Padding = 3;
            pdftable.WidthPercentage = 100;
            pdftable.HorizontalAlignment = Element.ALIGN_LEFT;
            pdftable.DefaultCell.BorderWidth = 1;
            iTextSharp.text.Font text = new iTextSharp.text.Font(BF, 10, iTextSharp.text.Font.NORMAL);

            //Добавление текста 
            foreach (DataGridViewColumn column in DGW.Columns)
            {
                PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText, text));
                cell.BackgroundColor = new iTextSharp.text.Color(240, 240, 240);
                pdftable.AddCell(cell);
            }
            //Добавление ячейки 
            foreach (DataGridViewRow row in DGW.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    pdftable.AddCell(new Phrase(cell.Value.ToString(), text));
                }
            }
            var savefilledialoge = new SaveFileDialog();
            savefilledialoge.FileName = filename;
            savefilledialoge.DefaultExt = ".pdf";
            savefilledialoge.Filter = "PDF Documents (*.pdf)|*.pdf";
            if (savefilledialoge.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream stream = new FileStream(savefilledialoge.FileName, FileMode.Create))
                    {
                        Document pdfdoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                        PdfWriter.GetInstance(pdfdoc, stream);
                        pdfdoc.Open();
                        pdfdoc.Add(pdftable);
                        pdfdoc.Close();
                        stream.Close();
                    }
                }
                catch (System.Exception msg)
                {                 
                    logger.Debug(msg.ToString());
                    MessageBox.Show("Файл занят другим процессом");                
                }
            }
        }

    }
}
