using MihaZupan;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BruteForce
{
    public partial class Form1 : Form
    {
        private string file { get; set; }
        public string url { get; set; }
        public string paramName { get; set; }
        public string paramValue { get; set; }
        public string keyName { get; set; }
        public bool isJson { get; set; }

        public string ProxyIP { get; set; }
        public string ProxyPORT { get; set; }

        private char[] ar = {'a', 'b','c','d','e','f','g','h','j','k','l','m','n','ñ','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','Ñ','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            '1','2','3','4','5','6','7','8','9','0','=','/','¿','?',',','.',':',';','+','-','*','Ç','ç','!','¡','-','_','%','&','(',')',
            '$','€','@','"','\'','\\','|','º','ª','¬','{','}','<','>'};

        public Form1()
        {
            InitializeComponent();
        }

        private void btnAttack_Click(object sender, EventArgs e)
        {
            setParameters();

            if (comboBox1.SelectedIndex == 1)
            {
                passwordsGeneratedByFile();
            }
            else if(comboBox1.SelectedIndex == 0)
            {
                var nMin = this.Controls["nMin"] as NumericUpDown;
                var nMax = this.Controls["nMax"] as NumericUpDown;

                if (nMin != null && nMax != null)
                {
                    passwordsGeneratedByRange(int.Parse(nMin.Value.ToString()), int.Parse(nMax.Value.ToString()));
                }
            }
            else
            {
                MessageBox.Show("Selecciona un generador de contraseñas");
            }
        }

        private void passwordsGeneratedByFile()
        {
            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    sendRequest(line);
                }
            }
        }

        private void passwordsGeneratedByRange(int nMin, int nMax)
        {
            for (int i = nMin; i <= nMax; i++)
            {
                char[] pass = new char[i];
                recursion(ref pass, 0, i);
            }
        }

        private void recursion(ref char[] pass, int index, int maxLength)
        {
            for (int i = 0; i < ar.Length; i++)
            {
                pass[index] = ar[i];

                if (index < maxLength - 1)
                {
                    recursion(ref pass, index + 1, maxLength);
                }
                else
                {
                    sendRequest(String.Concat(pass));
                }
            }
        }

        private void sendRequest(string s)
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

            var json = "{ \"" + paramName + "\" : \"" + paramValue + "\",  " +
                " \"" + keyName + "\" : \"" + s + "\"}";

            if (isJson)
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var r = client.PostAsync(url, content).Result;
                var c = r.Content.ReadAsStringAsync().Result;
                addRowToDataGrid(r, c, json);
            }
            else
            {
                var content = new FormUrlEncodedContent(new[]
                {
                                new KeyValuePair<string, string>(paramName, paramValue),
                                new KeyValuePair<string, string>(keyName, s),
                            });

                var r = client.PostAsync(url, content).Result;
                var c = r.Content.ReadAsStringAsync().Result;
                addRowToDataGrid(r, c, json);
            }
        }

        private void addRowToDataGrid(HttpResponseMessage r, string content, string parametros)
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

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                file = openFileDialog.FileName;
                var txtFileName = this.Controls["txtFileName"] as Label;
                if(txtFileName != null) txtFileName.Text = file;
            }
        }

        private void setParameters()
        {
            if(this.txtUrl.Text.Equals("") || this.txtParamName.Text.Equals("") || this.txtParamValue.Text.Equals("") || this.txtKeyName.Text.Equals(""))
            {
                MessageBox.Show("Rellena bien los campos");
            }
            else
            {
                url = this.txtUrl.Text;
                paramName = this.txtParamName.Text;
                paramValue = this.txtParamValue.Text;
                keyName = this.txtKeyName.Text;
                isJson = this.checkIsJson.Checked;
                ProxyIP = this.txtProxyIp.Text;
                ProxyPORT = this.txtProxyPort.Text;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine(comboBox1.SelectedIndex);
            Console.WriteLine(comboBox1.SelectedItem);
            if(comboBox1.SelectedIndex == 0)
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
                    4,
                    0,
                    0,
                    0});
                nMin.Name = "nMin";
                nMin.Size = new Size(120, 22);
                nMin.TabIndex = 45;
                nMin.Value = new decimal(new int[] {
                    4,
                    0,
                    0,
                    0});
                // 
                // nMax
                // 
                nMax.Location = new Point(550, 650);
                nMax.Minimum = new decimal(new int[] {
                    4,
                    0,
                    0,
                    0});
                nMax.Name = "nMax";
                nMax.Size = new Size(120, 22);
                nMax.TabIndex = 44;
                nMax.Value = new decimal(new int[] {
                    4,
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
            else if(comboBox1.SelectedIndex == 1)
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
                btnSelectFile.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
                btnSelectFile.Location = new Point(500, 500);
                btnSelectFile.Name = "btnSelectFile";
                btnSelectFile.Size = new Size(126, 37);
                btnSelectFile.TabIndex = 26;
                btnSelectFile.Text = "Select file";
                btnSelectFile.UseVisualStyleBackColor = true;
                btnSelectFile.Click += new EventHandler(this.btnSelectFile_Click);
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
        }
    }
}
