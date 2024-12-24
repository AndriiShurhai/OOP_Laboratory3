using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;

namespace Laboratory3_1
{
    class TextFileWorker
    {
        public string currentDirectory = Environment.CurrentDirectory;
        public void CreateFile(string name, string prompt)
        {
            string filePath = Path.Combine(currentDirectory, name);

            File.WriteAllText(filePath, prompt);
        }

        public void AddBadFile(string name, string file)
        {
            string filePath = Path.Combine(currentDirectory, name);

            try
            {
                File.AppendAllText(filePath, "\n" + file);
            }
            catch (FileNotFoundException)
            {
                File.WriteAllText(filePath, "\n" + file);
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string[]? ReadFile(string file)
        {
            string filePath = Path.Combine(currentDirectory, file);
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                return lines;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                AddBadFile("no_file.txt", file);
            }

            return null;

        }

        public void WriteFile(string file, string textToWrite)
        {
            string filePath = Path.Combine(currentDirectory, file);
            File.WriteAllText(filePath, textToWrite);
        }

        public void RemoveTxtFile(string file)
        {
            string filePath = Path.Combine(currentDirectory, file);

            try 
            {
                File.Delete(filePath);
            }
            catch(FileNotFoundException ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void PopulateTxtFiles(string directory, int start, int end)
        {
            Random random = new Random();
            for (int i = start; i <= end; i++)
            {
                string filePath = Path.Combine(directory, $"{i}.txt");

                int fileType = random.Next(4);

                switch (fileType)
                {
                    case 0: // Valid
                        WriteFile(filePath, $"{random.Next(1, 100)}\n{random.Next(1, 100)}");
                        break;
                    case 1: // Bad data
                        int badDataType = random.Next(3);
                        switch (badDataType)
                        {
                            case 0: // Not enough numbers
                                WriteFile(filePath, $"{random.Next(1, 100)}");
                                break;
                            case 1: // Nothing
                                WriteFile(filePath, "");
                                break;
                            case 2: // Something else
                                WriteFile(filePath, $"Nazar\n{1}\n{2}\n");
                                break;

                            default:
                                break;
                        }
                        break;

                    case 2: // Overflow
                        WriteFile(filePath, $"{Int64.MaxValue}");
                        break;

                    case 3: // NoFile
                        RemoveTxtFile($"{i}.txt");
                        break;

                }
            }
        }

        public void DeleteAllFiles(int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                string name = Convert.ToString(i);
                RemoveTxtFile(i + ".txt");
            }
            RemoveTxtFile("bad_data.txt");
            RemoveTxtFile("overflow.txt");
            RemoveTxtFile("no_file.txt");
        }

        public List<int> ProcessTxtFiles(int start, int end)
        {
            List<int> products = new List<int>();

            for (int i = start; i <= end; i++)
            {
                try
                {
                    string[] linesInFile = ReadFile($"{i}.txt");

                    try
                    {
                        Validate(linesInFile, $"{i}.txt");
                        int product = Convert.ToInt32(linesInFile[0]) * Convert.ToInt32(linesInFile[1]);
                        products.Add(product);
                        Console.WriteLine($"File {i}.txt processed successfully: Product = {product}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Validation error for file {i}.txt: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file {i}.txt: {ex.Message}");
                }
            }

            return products;
        }


        private void Validate(string[] lines, string file)
        {
            try
            {
                _ = Convert.ToInt32(lines[0]);
                _ = Convert.ToInt32(lines[1]);
            }
            catch (FormatException ex)
            {
                AddBadFile("bad_data.txt", file);
                Console.WriteLine($"Format error: {ex.Message}");
            }
            catch (OverflowException ex)
            {
                AddBadFile("overflow.txt", file);
                Console.WriteLine($"Overflow error: {ex.Message}");
            }
            catch (FileNotFoundException ex)
            {
                AddBadFile("no_file.txt", file);
                Console.WriteLine(ex.Message);
                Console.WriteLine("LOOOOOOOOOOOOOOOOOOOOOOOOL");
            }
            catch (Exception ex)
            {
                AddBadFile("bad_data.txt", file);
                Console.WriteLine($"Unexpected validation error: {ex.Message}");
            }
        }
    }
}
