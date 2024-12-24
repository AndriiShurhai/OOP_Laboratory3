using System;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Laboratory3_1
{
    class ImageWorker
    {
        private List<string> files;
        public List<string> GetFiles()
        {
            Console.WriteLine("Enter a directory path or type D to use current directory path.");
            string directoryPath = Console.ReadLine();

            try
            {
                files = Directory.GetFiles(directoryPath).ToList();
                ValidateImages();
            }
            catch
            {
                files = Directory.GetFiles(Directory.GetCurrentDirectory()).ToList();
                ValidateImages();
            }

            Console.WriteLine($"Found {files.Count} files");
            return files;
        }

        public void ValidateImages()
        {
            Regex regexExtForImage = new Regex(@"\.(bmp|gif|tiff?|jpe?g|png)$", RegexOptions.IgnoreCase);

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                if (regexExtForImage.IsMatch(file))
                {
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine("Wrong file");
                    files.RemoveAt(i);
                }
            }
        }

        public void MirrorImage()
        {
            foreach (string file in files)
            {
                try
                {
                    Bitmap bitmap = new Bitmap(file);
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    string newFileName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + "-mirrored.gif");
                    bitmap.Save(newFileName, System.Drawing.Imaging.ImageFormat.Gif);

                    Console.WriteLine($"Processed and saved: {newFileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}