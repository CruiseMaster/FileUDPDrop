using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPInteraction
{
    public class DirectoryInstance
    {
        public DirectoryInstance(string path)
        {
            Path = path;
        }

        public string Path { get; set; }
        public List<FileInstance> FileList { get; private set; }
        public event EventHandler<DirectoryInstance> DirectoryExaminatedNotification;
        public event EventHandler<int> FilesExaminationNumberNotification;

        public void ExamineDirectory()
        {
            FileList.Clear();

            DirectoryInfo dirInfo = new DirectoryInfo(Path);

            if (!dirInfo.Exists)
            {
                FileList = null;
                return;
            }

            FillFileList(dirInfo);
            CutBasePath(dirInfo);

            DirectoryExaminatedNotification?.Invoke(this, this);
        }

        private void CutBasePath(DirectoryInfo dirInfo)
        {
            //List<FileInstance> newFileList = new List<FileInstance>(FileList.Count);

            Parallel.ForEach(FileList, (file) =>
                {
                    file.FilePath = file.FilePath.Remove(0, dirInfo.Parent.FullName.Length);
                });
        }

        private void FillFileList(DirectoryInfo dirInfo)
        {
            FileInfo[] allFiles = dirInfo.GetFiles("*", SearchOption.AllDirectories);

            int filesExaminated = 0;

            Parallel.ForEach(allFiles, (nfo) =>
            {
                bool examinated = false;

                do
                {
                    try
                    {
                        FileInstance instance = new FileInstance(nfo.Name, nfo.Length);
                        instance.ComputeHash();
                        FileList.Add(instance);
                        examinated = true;
                        FilesExaminationNumberNotification?.Invoke(this, filesExaminated);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                } while (!examinated);

                filesExaminated++;
            });
        }
    }
}
