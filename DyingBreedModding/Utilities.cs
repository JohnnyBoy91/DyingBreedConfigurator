using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using static JCDyingBreedConfigurator.Plugin.ModManager;

namespace JCDyingBreedConfigurator
{
    //helper functions
    internal class Utilities
    {
        private static readonly StringBuilder sb = new StringBuilder();
        public static string CombineStrings(params string[] strings)
        {
            sb.Clear();
            foreach (string s in strings)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }

        public static void TryMethod(Action method)
        {
            try
            {
                method();
            }
            catch (Exception ex)
            {
                Log(CombineStrings("Failure in ", method.Method.Name, ": ", ex.Message), 3);
            }
        }

        /// <summary>
        /// Write to text file
        /// </summary>
        public static void WriteConfig(string fileName, List<string> text)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            StreamWriter writer = new StreamWriter(fileName, true);
            for (int i = 0; i < text.Count; i++)
            {
                writer.Write('\n' + text[i]);
            }
            writer.Close();
        }

        public static void WriteConfig(string fileName, string text)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            StreamWriter writer = new StreamWriter(fileName, true);
            writer.Write(text);
            writer.Close();
        }

        public static void WriteJsonConfig(string filePath, object dataToSerialize)
        {
            var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            string jsonString = JsonSerializer.Serialize(dataToSerialize, options);
            WriteConfig(filePath, jsonString);
        }

        public static object ReadJsonConfig<T>(string filePath)
        {
            StreamReader reader = new StreamReader(filePath, true);
            string jsonString = reader.ReadToEnd();
            reader.Close();

            var optionsRead = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            T dataToDeserializeInto = JsonSerializer.Deserialize<T>(jsonString, optionsRead);
            return dataToDeserializeInto;
        }
    }
}
