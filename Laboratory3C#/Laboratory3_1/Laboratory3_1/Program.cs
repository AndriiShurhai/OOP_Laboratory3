using System;

namespace Laboratory3_1
{
    class Program
    {
        static void Main(string[] args)
        {
            TextFileWorker reader = new TextFileWorker();
            int startFileIndexing = 10;
            int endFileIndexing = 29;
            reader.DeleteAllFiles(startFileIndexing, endFileIndexing);

            SetupTxtFiles(reader, startFileIndexing, endFileIndexing);

            var validProducts = new List<int>();

            validProducts = reader.ProcessTxtFiles(startFileIndexing, endFileIndexing);

            Console.WriteLine("Successful products:");
            for (int i = 0; i < validProducts.Count; i++)
            {
                Console.WriteLine(validProducts[i]);
            }

            int average;

            try
            {
                average = validProducts.Sum() / validProducts.Count;
                Console.WriteLine($"Average: {average}");
            }
            catch(DivideByZeroException ex) 
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Do you want delete all .txt files? y/n");
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "y":
                    reader.DeleteAllFiles(startFileIndexing, endFileIndexing);
                    break;

                case "n":
                    break;

                default:
                    break;


            }

            static void SetupTxtFiles(TextFileWorker reader, int start, int end)
            {
                for (int i = start; i <= end; i++)
                {
                    string name = Convert.ToString(i);
                    reader.CreateFile(i + ".txt", "initial file");
                }

                string directory = Environment.CurrentDirectory;
                reader.PopulateTxtFiles(directory, start, end);
            }
        }
    }
}