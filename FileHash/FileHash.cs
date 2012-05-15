using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Xml;

namespace FileHash
{
    /// <summary>
    /// This class is designed to perform all functions for filehashing and hash checking.
    /// This class will also generate an xml file of a "Master Files" list.  This will allow
    /// remote locations to compare the changes and retrieve files if needed.
    /// </summary>
    public static class FileHash
    {
        public static string xmlpath = @"StoreFiles.xml";
        public static string sharePath = @"\\192.168.0.126\f-drive\forms\BOJ\Acrobat\All\BOJ\";
        private static string storePath = @"c:\Forms\";

        /// <summary>
        /// Generates an MD5 hash of a file given the directory and filename
        /// </summary>
        /// <param name="directory">Subdirectory of the Forms Folder</param>
        /// <param name="fileName">Filename of file in subfolder</param>
        /// <param name="store">True if is the remote location so it can use a different path</param>
        /// <returns>MD5 hash string as string</returns>
        public static string genHash(string directory, string fileName, bool store)
        {
            string path;
            if (store == true)
                path = storePath;
            else
                path = sharePath;

            string filePath = path + directory + @"\" + fileName;
            StringBuilder sb = new StringBuilder();
            if (File.Exists(filePath))
            {
                FileStream file = new FileStream(filePath, FileMode.Open);
                MD5 hashstring = new MD5CryptoServiceProvider();
                byte[] retVal = hashstring.ComputeHash(file);
                file.Close();

                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
            }
            else
            {
                sb.Append("");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Takes an MD5 hash from a remote file and compares it to the MD5 generated in the
        /// Master List
        /// </summary>
        /// <param name="directory">Subdirectory of the Forms Folder</param>
        /// <param name="fileName">Filename of file in subfolder</param>
        /// <param name="md5">MD5 hash string</param>
        /// <returns>True if MD5 hash strings match</returns>
        public static bool fileCheck(string directory, string fileName, string md5)
        {
            bool result = false;
            // check md5 of file
            var md5XmlQuery = from el in XDocument.Load(xmlpath).
                                Descendants("File").Where(e => (string)e.Attribute("Name") == fileName)
                                select el;
            string md5Check = "";
            foreach (XElement ele in md5XmlQuery)
            {
                md5Check += ele.Element("MD5").Value;
            }

            if (md5 == md5Check)
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Checks files in the master list and generates an xml file with the subdirectory, 
        /// file name and MD5 hash string
        /// </summary>
        /// <returns>true if created file was successful</returns>
        public static bool xmlGen()
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("MasterList"));
            List<string> directories = new List<string>();
            // compile directory info
            // first put together root directory
            
            List<XElement> files = new List<XElement>();
            DirectoryInfo di = new DirectoryInfo(sharePath);
            
            foreach (DirectoryInfo di1 in di.GetDirectories())
            {
                // get list of files
                DirectoryInfo dif = new DirectoryInfo(di1.FullName);
                foreach (FileInfo dif1 in dif.GetFiles("*.pdf"))
                {
                    files.Add(new XElement("File", new XAttribute("Name", dif1.Name),
                        new XElement("MD5", genHash(di1.Name, dif1.Name, false))));
                }

                doc.Element("MasterList").Add(new XElement("Directory",
                new XAttribute("Name", di1.Name), files));
                files.Clear();

                // now get subdirectories
                DirectoryInfo dsub = new DirectoryInfo(di1.FullName);
                foreach (DirectoryInfo dsub1 in dsub.GetDirectories())
                {
                    // get list of files
                    DirectoryInfo dsubf = new DirectoryInfo(dsub1.FullName);
                    foreach (FileInfo dsubf1 in dsubf.GetFiles("*.pdf"))
                    {
                        files.Add(new XElement("File", new XAttribute("Name", dsubf1.Name),
                            new XElement("MD5", genHash(di1.Name + @"\" + dsub1.Name, dsubf1.Name, false))));
                    }
                    doc.Element("MasterList").Add(new XElement("Directory", new XAttribute("Name", di1.Name + @"\" + dsub1.Name), files));
                    files.Clear();

                    // so ... spanish team excel has another subdirectory

                    // TODO maybe put this in a function
                    DirectoryInfo dsubsub = new DirectoryInfo(dsub1.FullName);
                    foreach (DirectoryInfo dsubsub1 in dsubsub.GetDirectories())
                    {
                        // get list of files
                        DirectoryInfo dsubsubf = new DirectoryInfo(dsubsub1.FullName);
                        foreach (FileInfo dsubsubf1 in dsubsubf.GetFiles("*.pdf"))
                        {
                            files.Add(new XElement("File", new XAttribute("Name", dsubsubf1.Name),
                                new XElement("MD5", genHash(di1.Name + @"\" + dsub1.Name + @"\" +
                                    dsubsub1.Name, dsubsubf1.Name, false))));
                        }
                        doc.Element("MasterList").Add(new XElement("Directory", new XAttribute("Name", di1.Name + @"\" +
                            dsub1.Name + @"\" + dsubsub1.Name), files));
                        files.Clear();
                    }
                }
            }
            doc.Save(xmlpath);
            
            return true;
        }
        
        /// <summary>
        /// Sends MasterFiles.xml as an XDocument variable
        /// </summary>
        public static XDocument getFileList()
        {
            return XDocument.Load(xmlpath);
        }
    }
}
