using System.Drawing.Imaging;
using System.Text;
using System.Web;

namespace RenameFilesWinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            /*
            string dateTakenStr = "2011:01:04 22:44:03\0".Replace("\0","");
            DateTime dateTime = DateTime.ParseExact(dateTakenStr, "yyyy:MM:dd HH:mm:ss", null);
            */

            string[] fileNames = Directory.GetFiles(dirText.Text);

            int i = 1; 

            foreach (string file in fileNames) 
            {

                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Exists && fileInfo.Extension != null)
                {
                    if(fileInfo.Extension.ToLower() == ".jpg" || fileInfo.Extension.ToLower() == ".jpeg" || fileInfo.Extension.ToLower() == ".mov" || fileInfo.Extension.ToLower() == ".mpg" || fileInfo.Extension.ToLower() == ".mp4" || fileInfo.Extension.ToLower() == ".png")
                    {
                        

                        // Get the last write time (modified date) of the file
                        //     DateTime lastWriteTime = fileInfo.LastWriteTime;
                        DateTime lastWriteTime = GetDateTaken(fileInfo.FullName);

                        string fileName = lastWriteTime.ToString("yyyyMMdd_HHmmss");

                        string newFileName = fileInfo.DirectoryName + "\\" + fileName;
                        string newFileNameExt = AddedFileExtention(fileInfo, newFileName);
                        try
                        {
                            fileInfo.MoveTo(newFileNameExt);
                        }
                        catch
                        {
                            try
                            {
                                newFileName = newFileName + "(" + i + ")";
                                newFileNameExt = AddedFileExtention(fileInfo, newFileName);
                                fileInfo.MoveTo(newFileNameExt);
                                i++;
                            }
                            catch
                            {
                                newFileName = newFileName + "(" + i + ")";
                                newFileNameExt = AddedFileExtention(fileInfo, newFileName);
                                fileInfo.MoveTo(newFileNameExt);
                                i++;
                            }
                        }

                    }
                    /*
                    // Get the last write time (modified date) of the file
                    DateTime lastWriteTime = fileInfo.LastWriteTime;
                    string fileName = lastWriteTime.ToString( "yyyyMMdd_HHmmss");
                    
                    string newFileName =  fileInfo.DirectoryName + "\\" + fileName + ".jpg";

                    fileInfo.MoveTo(newFileName);
                    */

                    // fileInfo.MoveTo(fileInfo.DirectoryName + "\\" + fileInfo.LastWriteTime.ToString("yyyyMMdd_HHmmss") + ".jpg");

                }
            }

        }

        private static string AddedFileExtention(FileInfo fileInfo, string fileName)
        {
            string newFileName = fileName;
            
            if (fileInfo.Extension.ToLower() == ".jpg" || fileInfo.Extension.ToLower() == ".jpeg")
            {
                newFileName = newFileName + ".jpg";
            }
            if (fileInfo.Extension.ToLower() == ".mov")
            {
                newFileName = newFileName + ".MOV";
            }
            if (fileInfo.Extension.ToLower() == ".mpg")
            {
                newFileName = newFileName + ".mpg";
            }
            if (fileInfo.Extension.ToLower() == ".mp4")
            {
                newFileName = newFileName + ".mp4";
            }
            if (fileInfo.Extension.ToLower() == ".png")
            {
                newFileName = newFileName + ".png";
            }
            return newFileName;
        }

        static DateTime GetDateTaken(string filePath)
        {

            if (filePath.ToLower().EndsWith(".jpg") || filePath.ToLower().EndsWith(".jpeg") || filePath.ToLower().EndsWith(".png"))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (Image image = Image.FromStream(fs, false, false))
                    {
                        foreach (PropertyItem propItem in image.PropertyItems)
                        {
                            if (propItem.Id == 0x9003) // PropertyTag DateTime
                            {
                                string dateTakenStr = Encoding.UTF8.GetString(propItem.Value);
                                dateTakenStr = dateTakenStr.Replace("\0", "");

                                return DateTime.ParseExact(dateTakenStr, "yyyy:MM:dd HH:mm:ss", null);
                            }
                        }
                    }
                }
            }
            // If date taken information is not found, return the last write time
            return File.GetLastWriteTime(filePath);
        }




    }
}
