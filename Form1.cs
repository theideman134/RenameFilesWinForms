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
            string[] fileNames = Directory.GetFiles(dirText.Text);

            int i = 1; 

            foreach (string file in fileNames) 
            {

                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Exists)
                {
                    if(fileInfo.Extension == ".jpg" || fileInfo.Extension == ".mov" || fileInfo.Extension == ".mpg" || fileInfo.Extension == ".mp4")
                    {
                        
                         // Get the last write time (modified date) of the file
                        DateTime lastWriteTime = fileInfo.LastWriteTime;
                        string fileName = lastWriteTime.ToString( "yyyyMMdd_HHmmss");

                        string newFileName = fileInfo.DirectoryName + "\\" + fileName;
                        if (fileInfo.Extension == ".jpg")
                        {
                            newFileName = newFileName + ".jpg";
                        }
                        if (fileInfo.Extension == ".mov")
                        {
                            newFileName = newFileName + ".mov";
                        }
                        if (fileInfo.Extension == ".mpg")
                        {
                            newFileName = newFileName + ".mpg";
                        }
                        if (fileInfo.Extension == ".mp4")
                        {
                            newFileName = newFileName + ".mp4";
                        }
                        try
                        {
                            fileInfo.MoveTo(newFileName);
                        }
                        catch
                        {
                            fileInfo.MoveTo(newFileName);
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
    }
}
