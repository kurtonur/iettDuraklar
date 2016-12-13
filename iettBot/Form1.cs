using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSoup;
using NSoup.Nodes;
using System.Threading;
using System.Xml;

namespace iettBot
{
    public partial class Form1 : Form
    {
        int i = 1;
        int durum = 1;
        List<string> dk;
        public Form1()
        {
            InitializeComponent();

            dk = new List<string>();
            hatlar();
            progressBar1.Maximum = i;
            progressBar1.Minimum = 0;
            
        }

        private void hatlar()
        {
            IConnection connection = NSoupClient.Connect("http://www.iett.gov.tr/tr/main/hatlar");
            connection.Timeout(30000);
            Document document = connection.Get();
            foreach (Element Hat in document.Select("h4.DetailLi_name"))
            {
                int index = Hat.Text().IndexOf(' ');
                string hatNo = Hat.Text().Substring(0, index).Trim();
                string hatAdi = Hat.Text().Substring(index).Trim();
                tmhatlar.Items.Add(i + "*" + hatNo + "*" + hatAdi);
                i++;
            }
        }
        
        private void duraklar()
        {

            XmlTextWriter yaz = new XmlTextWriter("Duraklar.xml", System.Text.UTF8Encoding.UTF8);
            yaz.Formatting = Formatting.Indented;
            yaz.WriteStartDocument();
            yaz.WriteStartElement("hepsi");

            foreach (string item in tmhatlar.Items)
            {
            string[] ayir = item.Split('*');

            IConnection connection;

                connection = NSoupClient.Connect("http://www.iett.gov.tr/tr/main/hatlar/" + ayir[1]);

            connection.Timeout(600000);

            Document document = connection.Get();
            
            yaz.WriteStartElement("hatid");
            yaz.WriteAttributeString("h",ayir[1]);
            foreach (Element yon in document.Select("div.LineMapList ol"))
            {
                int i = 1;
                if (yon.Select("li").Count > 0)
                {
                    yaz.WriteStartElement("yon");
                    string yn = "Geliş";
                    if (yon.Attr("data-station-direction") == "Going") yn = "Gidiş";
                    yaz.WriteAttributeString("y", yn);
                }
                foreach (Element Durak in yon.Select("li"))
                {


                    yaz.WriteStartElement("durak");
                    yaz.WriteAttributeString("durakno", i.ToString());
                    yaz.WriteAttributeString("lat", Durak.Attr("data-station-lat"));
                    yaz.WriteAttributeString("lng", Durak.Attr("data-station-lng"));
                    yaz.WriteAttributeString("isim", Durak.Attr("data-station-name"));

                    yaz.WriteEndElement();




                    i++;
                }
                 if (yon.Select("li").Count > 0)
                yaz.WriteEndElement();

            }
            yaz.WriteEndElement();
            durum++;
            }

            yaz.WriteEndElement();
            yaz.Close();
            MessageBox.Show("Duraklar XML kayit edildi.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(duraklar));
            thread.Start();

            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = durum;
            this.Text = durum.ToString();
            if (progressBar1.Value == progressBar1.Maximum)
            {
                timer1.Stop();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            

            try
            {
                XmlTextWriter yaz = new XmlTextWriter("Hatlar.xml", System.Text.UTF8Encoding.UTF8);
                yaz.Formatting = Formatting.Indented;
                yaz.WriteStartDocument();
                yaz.WriteStartElement("hepsi");
                foreach (string item in tmhatlar.Items)
                {
                    string[] ayir = item.Split('*');
                    yaz.WriteStartElement("hat");
                    yaz.WriteAttributeString("id", ayir[0]);
                    yaz.WriteAttributeString("numara", ayir[1]);
                    yaz.WriteAttributeString("isim", ayir[2]);
                    yaz.WriteEndElement();    
                }
                yaz.WriteEndElement();
                yaz.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                
                MessageBox.Show("Hatlar.xml Kayit Edildi.");
            }
        }
    }
}
