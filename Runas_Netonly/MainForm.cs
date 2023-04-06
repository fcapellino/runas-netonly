using Microsoft.VisualBasic.FileIO;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Runas_Netonly
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void ButtonAddApplication_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialogApp.Filter = "EXE FILES (*.exe)|*.exe|BAT FILES (*.bat)|*.bat";
                openFileDialogApp.InitialDirectory = SpecialDirectories.ProgramFiles;

                if (openFileDialogApp.ShowDialog() == DialogResult.OK)
                {
                    var path = openFileDialogApp.FileName.ToUpper();
                    if (Properties.Settings.Default.APPLICATIONS.Contains(path))
                    {
                        throw new InvalidOperationException("APPLICATION IS ALREADY ON THE LIST!");
                    }

                    Properties.Settings.Default.APPLICATIONS.Add(path);
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message.ToUpper();
                MessageBox.Show(message, "RUNAS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                LoadDataGridViewApps();
            }
        }

        private void ButtonSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateCredentials())
                {
                    throw new InvalidOperationException("CREDENTIALS ARE REQUIRED!");
                }

                Properties.Settings.Default.DOMAIN = Helpers.Base64Encode(textBoxDomain.Text.Trim());
                Properties.Settings.Default.USER = Helpers.Base64Encode(textBoxUser.Text.Trim());
                Properties.Settings.Default.PASSWORD = Helpers.Base64Encode(textBoxPassword.Text.Trim());
                Properties.Settings.Default.Save();

                MessageBox.Show("SETTINGS SAVED!", "RUNAS", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message.ToUpper();
                MessageBox.Show(message, "RUNAS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonSeePassword_Click(object sender, EventArgs e)
        {
            textBoxPassword.UseSystemPasswordChar = !textBoxPassword.UseSystemPasswordChar;
        }

        private void DataGridViewApps_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex.Equals(DatagridColumns.EXECUTE))
                {
                    if (!ValidateCredentials())
                    {
                        throw new InvalidOperationException("CREDENTIALS ARE REQUIRED!");
                    }

                    var applicationPath = dataGridViewApps.Rows[e.RowIndex].Cells[DatagridColumns.PATH].Value.ToString().ToLower();
                    string command1 = "CD C:\\WINDOWS\\SYSTEM32\\";
                    string command2 = Regex.Replace($"runas /user:{textBoxDomain.Text.Trim()}\\{textBoxUser.Text.Trim()} /netonly \"{applicationPath}\"", "[+^%~()]", "{$0}");
                    string command3 = textBoxPassword.Text.Trim();

                    var newProcess = Process.Start("cmd.exe");
                    Thread.Sleep(990);
                    SendKeys.SendWait(command1);
                    Thread.Sleep(500);
                    SendKeys.SendWait("{ENTER}");
                    Thread.Sleep(500);
                    SendKeys.SendWait(command2);
                    Thread.Sleep(500);
                    SendKeys.SendWait("{ENTER}");
                    Thread.Sleep(990);
                    SendKeys.SendWait(command3);
                    Thread.Sleep(500);
                    SendKeys.SendWait("{ENTER}");
                    Thread.Sleep(500);
                    newProcess.Kill();
                }
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message.ToUpper();
                MessageBox.Show(message, "RUNAS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedRow()
        {
            try
            {
                var isRowSelected = dataGridViewApps.SelectedRows.Cast<DataGridViewRow>().Any();
                if (isRowSelected)
                {
                    var path = dataGridViewApps.SelectedRows[0].Cells[DatagridColumns.PATH].Value.ToString();
                    Properties.Settings.Default.APPLICATIONS.Remove(path);
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message.ToUpper();
                MessageBox.Show(message, "RUNAS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                LoadDataGridViewApps();
            }
        }

        private void LoadCredentials()
        {
            textBoxDomain.Text = Helpers.Base64Decode(Properties.Settings.Default.DOMAIN);
            textBoxUser.Text = Helpers.Base64Decode(Properties.Settings.Default.USER);
            textBoxPassword.Text = Helpers.Base64Decode(Properties.Settings.Default.PASSWORD);
        }

        private void LoadDataGridViewApps()
        {
            var applicationsList = Properties.Settings.Default.APPLICATIONS.Cast<string>().ToList();
            dataGridViewApps.Rows.Clear();

            if (applicationsList.Any())
            {
                applicationsList.ForEach(path =>
                {
                    var applicationName = FileVersionInfo.GetVersionInfo(path).FileDescription.ToUpper();
                    dataGridViewApps.Rows.Add(new string[] { applicationName, path });
                });
                dataGridViewApps.Sort(dataGridViewApps.Columns[DatagridColumns.APPLICATION], ListSortDirection.Ascending);
                dataGridViewApps.Rows[0].Selected = true;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadDataGridViewApps();
            LoadCredentials();
        }

        private void TextBoxCredential_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !(Char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == (char)Keys.Back);
        }

        private bool ValidateCredentials()
        {
            return !string.IsNullOrWhiteSpace(textBoxDomain.Text) && !string.IsNullOrWhiteSpace(textBoxUser.Text) && !string.IsNullOrWhiteSpace(textBoxPassword.Text);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete)
            {
                DeleteSelectedRow();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
