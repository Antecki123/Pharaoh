using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace App.Helpers
{
    public class FileDataHandler<T> where T : class
    {
        private readonly string dataDirPath = Application.persistentDataPath;
        private readonly string dataFileName = string.Empty;
        private readonly bool useEncryption = false;
        private readonly string encryptionCodeWord = "WXHHIMBFHFTQBFH";

        public FileDataHandler(string dataFileName, bool useEncryption = false)
        {
            this.dataFileName = dataFileName;
            this.useEncryption = useEncryption;
        }

        public bool FileExist()
        {
            var fullPath = Path.Combine(dataDirPath, $"{dataFileName}");
            return File.Exists(fullPath);
        }

        public T Load()
        {
            var fullPath = Path.Combine(dataDirPath, dataFileName);
            T loadedData = null;

            if (File.Exists(fullPath))
            {
                try
                {
                    var dataToLoad = "";
                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                    {
                        using StreamReader reader = new StreamReader(stream);
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }

                    loadedData = JsonConvert.DeserializeObject<T>(dataToLoad);

                    if (useEncryption)
                    {
                        dataToLoad = EncryptDecrypt(dataToLoad);
                    }

                    loadedData = JsonConvert.DeserializeObject<T>(dataToLoad);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
                }
            }

            return loadedData;
        }

        public void Save(T data)
        {
            var fullPath = Path.Combine(dataDirPath, dataFileName);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                var dataToStore = JsonConvert.SerializeObject(data);

                if (useEncryption)
                {
                    dataToStore = EncryptDecrypt(dataToStore);
                }

                using FileStream stream = new FileStream(fullPath, FileMode.Create);
                using StreamWriter writer = new StreamWriter(stream);

                writer.Write(dataToStore);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
            }
        }

        private string EncryptDecrypt(string data)
        {
            var modifiedData = "";
            for (int i = 0; i < data.Length; i++)
            {
                modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
            }
            return modifiedData;
        }
    }
}