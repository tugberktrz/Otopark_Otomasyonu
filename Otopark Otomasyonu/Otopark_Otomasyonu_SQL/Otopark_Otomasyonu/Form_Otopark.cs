using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data;
using MySql.Data.MySqlClient;
using Otopark_Otomasyonu.Models;

namespace Otopark_Otomasyonu
{
    public partial class Form_Otopark : Form
    {
        public Form_Otopark()
        {
            InitializeComponent();
        }
        string myConnectionString = "Server=localhost;Database=otopark;uid=root;pwd=2469";
      

        MySqlConnection connection;
        MySqlConnection conn;
        MySqlDataAdapter adapter;
        MySqlCommand command;
        MySqlDataReader reader;
        DataTable table;
        


        void listele() 
        {
            connection = new MySqlConnection(myConnectionString);  
            connection.Open();
            adapter = new MySqlDataAdapter("select * from parkyerleri where durum = 'Boş'", connection); 
            table = new DataTable();
            adapter.Fill(table);

            comboYer.DataSource = table; 
            comboYer.ValueMember = "id";
            comboYer.DisplayMember = "id";
            connection.Close();

            connection = new MySqlConnection(myConnectionString);  
            connection.Open();
 
            adapter = new MySqlDataAdapter("select * from parkyerleri,arac where parkyerleri.durum = 'Dolu' and arac.park_edilen_yer=parkyerleri.id", connection);
            table = new DataTable();
            adapter.Fill(table);

            comboCikan.DataSource = table;
            comboCikan.ValueMember = "id";
            comboCikan.DisplayMember = "arac_plaka";
            connection.Close();
        }

        void yenile() 
        {
            foreach (Control item in panel3.Controls) 
            {
                if (item is Button) 
                {
                    if (item.Name.ToString()!= "button_programi_kapat") 
                    {
                        item.BackColor = Color.DarkSlateGray; 
                        item.ForeColor = Color.DarkGray; 
                        item.Text = item.Name.ToString(); 
                    }
                }
            }
            conn = new MySqlConnection(myConnectionString); 
            conn.Open();
            command = new MySqlCommand("select * from arac", conn); 
            reader = command.ExecuteReader(); 
            while (reader.Read())
            {
                foreach (Control item in panel3.Controls) 
                {
                    if (item is Button)
                    {
                        if (reader["park_edilen_yer"].ToString() == item.Name.ToString()) 
                        {
                            item.BackColor = Color.Maroon; 
                            item.ForeColor = Color.FloralWhite;
                            item.Text = reader["arac_plaka"].ToString();
                        }
                    }
                }
            }
            conn.Close();
        }
        private void Form_Otopark_Load(object sender, EventArgs e)
        {
            listele(); 
            yenile();
            foreach (Control item in panel3.Controls) 
            {
                if (item is Button)
                {
                    if (item.Name.ToString() != "button_programi_kapat") 
                    {
                        item.Click += butonlara_tıklanınca; 
                    }
                }
            }
        }

        private void butonlara_tıklanınca(object sender, EventArgs e) 
        {
            Button b = sender as Button; 
            if (b.BackColor==Color.DarkSlateGray) 
            {
                for (int i = 0; i < comboYer.Items.Count; i++) 
                {
                    comboYer.SelectedIndex = i;
                    if (b.Name.ToString() == comboYer.SelectedValue.ToString()) 
                    {
                        comboYer.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < comboCikan.Items.Count; i++)
                {
                    comboCikan.SelectedIndex = i;
                    if (b.Name.ToString() == comboCikan.SelectedValue.ToString())
                    {
                        comboCikan.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void button_aracgirisi_Click(object sender, EventArgs e)
        {
            if (txtPlaka.Text != "" || txtSahip.Text != "") 
            {
                try
                {
                    conn = new MySqlConnection(myConnectionString);
                    conn.Open();
                    command = new MySqlCommand("insert into arac(arac_plaka, arac_sahibi, arac_telefon,arac_giris_saati, park_edilen_yer)values('" + txtPlaka.Text.Trim() + "','" + txtSahip.Text.Trim() + "','"+txtTelefon.Text.Trim()+"','" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + "','" + comboYer.SelectedValue.ToString() + "')", conn);
                    command.ExecuteNonQuery();
                    conn.Close();
                    conn = new MySqlConnection(myConnectionString);
                    conn.Open();
                    command = new MySqlCommand("update parkyerleri set durum='Dolu' where id='" + comboYer.SelectedValue.ToString() +"'", conn);
                    command.ExecuteNonQuery();
                    conn.Close();

                 }
                  catch (Exception ex)
                  {
                      MessageBox.Show("Hata oluştu" + ex);
                  }
                txtSahip.Text = "";
                txtPlaka.Text = "";
                yenile();
                listele();
            }
            else MessageBox.Show("Boş alan bırakmayınız. ");

            listele();
            yenile();
        }

        private void button_arac_cikisi_Click(object sender, EventArgs e)
        {
            conn = new MySqlConnection(myConnectionString);
            conn.Open();
            command = new MySqlCommand("delete from arac where arac_plaka='" + comboCikan.Text + "'", conn);
            command.ExecuteNonQuery();
            conn.Close();
            conn = new MySqlConnection(myConnectionString);
            conn.Open();
            command = new MySqlCommand("update parkyerleri set durum='Boş' where id='" + comboCikan.SelectedValue.ToString() + "'", conn);
            command.ExecuteNonQuery();
            conn.Close();
            yenile();
            listele();

        }

        public void comboBox_Cikan_Arac_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSahip.Text = txtTelefon.Text = null;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();
            command = new MySqlCommand("select * from arac where arac_plaka='" + comboCikan.Text + "'", conn);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                label5.Text = "Araç Giriş Saati \n" + reader["arac_giris_saati"].ToString();
                txtTelefon.Text = reader["arac_telefon"].ToString();
                txtSahip.Text = reader["arac_sahibi"].ToString();
            }
            conn.Close();
        }

        private void btnSms_Click(object sender, EventArgs e)
        {
            SmsSender(txtTelefon.Text, txtSahip.Text, label5.Text);
        }
        public string XMLPOST(string PostAddress, string xmlData)
        {
            try
            {
                var res = "";
                byte[] bytes = Encoding.UTF8.GetBytes(xmlData);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PostAddress);
                request.Method = "POST";
                request.ContentLength = bytes.Length;
                request.ContentType = "text/xml";
                request.Timeout = 300000000;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        string message = String.Format(
                        "POST failed. Received HTTP {0}",
                        response.StatusCode);
                        throw new ApplicationException(message);
                    }
                    Stream responseStream = response.GetResponseStream();
                    using (StreamReader rdr = new StreamReader(responseStream))
                    {
                        res = rdr.ReadToEnd();
                    }
                    return res;
                }
            }
            catch
            {
                return "-1";
            }
        }

        public void SmsSender(string TelNo, string ad, string zaman)
        {
            String testXml = "<request>";
            testXml += "<authentication>";
            testXml += "<username></username>";
            testXml += "<password></password>";
            testXml += "</authentication>";
            testXml += "<order>";
            testXml += "<sender></sender>";
            testXml += "<sendDateTime></sendDateTime>";
            testXml += "<message>";
            testXml += $"<text>" + $@"Sayın {ad}, aracınız {zaman}'den beri otoparkımızda bulunmaktadır." + "</text>";
            testXml += "<receipents>";
            testXml += $"<number>{TelNo}</number>";
            testXml += "</receipents>";
            testXml += "</message>";
            testXml += "</order>";
            testXml += "</request>";
            this.XMLPOST("http://api.iletimerkezi.com/v1/send-sms", testXml);
            //" + $@"Sayın {reader["arac_sahibi"].ToString()}, aracınız {reader["arac_giris_saati"].ToString()}'den beri otoparkımızda bulunmaktadır." + "
        }

    }
}

