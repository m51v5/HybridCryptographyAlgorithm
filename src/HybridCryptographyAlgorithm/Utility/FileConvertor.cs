using System.IO;
using Microsoft.Win32;

namespace HybridCryptographyAlgorithm.Utility
{
    class FileConvertor
    {
        public static string TextFileToString(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);

            return fileContent;
        }

        public static string StringToTextFile(string content)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Save your file",
                DefaultExt = "txt" // default file extension if user doesn't specify one
            };

            // Show the SaveFileDialog and check if the user selected a file
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    // Write the content to the file
                    File.WriteAllText(filePath, content);
                    Console.WriteLine("File saved successfully.");
                    return filePath;
                }
                catch (Exception ex)
                {
                    // Handle any errors (e.g., file permission issues, path errors)
                    Console.WriteLine($"Error saving file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Save operation was cancelled.");
            }
            return string.Empty;
        }
    }
}
