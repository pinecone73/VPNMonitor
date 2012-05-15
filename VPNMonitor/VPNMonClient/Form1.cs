using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Security.Principal;
using System.Threading;

/* TODO:
 * -Delete forms files if not on the masterlist
 * -set timer to check at a certain time
 * -transmit VPN data to HQ
 * -setup client settings for user login, keepalive attempts
 * -icon down to system tray
 * -test for internet connectivity
 */

namespace VPNMonClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ServiceReference1.VPNMonServerClient obj = new ServiceReference1.VPNMonServerClient();

            try
            {
                string status = obj.keepAlive("001", "192.168.150.101");

                switch (status)
                {
                    case "1": // request sent and no further action needed
                        break;
                    case "2": // new files may require downloading

                        StreamReader fileList = new StreamReader(obj.getMasterList());
                        XDocument masterList = XDocument.Parse(fileList.ReadToEnd());
                        checkFiles(masterList);
                        break;
                    default:
                        break;
                }
                obj.Close();
            }
            catch
            {
                // client not connecting to server

            }
        }

        private void checkFiles(XDocument fileList)
        {
            // update user on what's going on
            richTextBox1.Text = "Update Requested By Server\n";
            var files = from f in fileList.Elements("MasterList").Elements("Directory")
                        select new XElement(f);
            string tmpMd5;
            string directoryName;
            string fileName;
            foreach (XElement el in files)
            {
                directoryName = el.Attribute("Name").Value;
                var directory = from d in el.Elements("File")
                                select new XElement(d);
                foreach (XElement eld in directory)
                {
                    fileName = eld.Attribute("Name").Value;
                    tmpMd5 = FileHash.FileHash.genHash(directoryName, fileName, true); 
                    if (tmpMd5 != eld.Element("MD5").Value)
                    {
                        ServiceReference1.VPNMonServerClient obj = new ServiceReference1.VPNMonServerClient();
                        richTextBox1.Text += fileName + " changed or missing\n";
                        string path = obj.getFileString(directoryName, fileName);
                        // check for existing folders
                        if (!Directory.Exists(@"c:\Forms\"))
                        {
                            Directory.CreateDirectory(@"c:\Forms\");
                        }

                        if (!Directory.Exists(@"c:\Forms\" + directoryName))
                        {
                            Directory.CreateDirectory(@"c:\Forms\" + directoryName);
                        }
                        File.Copy(path, @"c:\Forms\" + directoryName + @"\" + fileName, true);
                        obj.Close();
                        
                    }
                    else
                    {
                        //richTextBox1.Text += eld.Attribute("Name").Value + " file good\n";
                    }
                }
            }
            richTextBox1.Text += "Update completed!\n";
        }
    }
}
