using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Darklight.Unity.Backend.Data
{
    public class JsonDataService : IDataService
    {
        private const string KEY = "Y7z5JYgoJTNT3z1hhaFpjLo1bWVfznE7w2vUKTeesz0=";
        private const string IV = "o7idq1HoqWq6BE6ahpoCIw==";

        public bool SaveData<T>(string RelativePath, T Data, bool Encrypted)
        {
            string path = Application.persistentDataPath + RelativePath;

            try
            {
                if (File.Exists(path))
                {
                    Debug.Log("Data exists. Deleting old file and writing a new one!");
                    File.Delete(path);
                }
                else
                {
                    Debug.Log("Creating file for the first time!");
                }


                using FileStream stream = File.Create(path);
                if (Encrypted)
                {
                    WriteEncryptedData(Data, stream);
                    Debug.Log("Writing Encrypted data");
                }
                else
                {
                    stream.Close();
                    File.WriteAllText(path, JsonConvert.SerializeObject(Data));
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to save data due to: {e.Message} {e.StackTrace}");
                return false;
            }
        }


        public T LoadData<T>(string RelativePath, bool Encrypted)
        {
            string path = Application.persistentDataPath + RelativePath;

            if (!File.Exists(path))
            {
                Debug.LogError($"Cannot load file at {path}. File does not exist!");
                throw new FileNotFoundException($"Path does not exist: {path}");
            }

            try
            {
                T data;
                if (Encrypted)
                {
                    data = ReadEncryptedData<T>(path);
                }
                else
                {
                    data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
                }

                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load data due to: {e.Message} {e.StackTrace}");
                throw;
            }
        }

        private void WriteEncryptedData<T>(T Data, FileStream Stream)
        {
            using Aes aesProvider = Aes.Create();

            aesProvider.Key = Convert.FromBase64String(KEY);
            aesProvider.IV = Convert.FromBase64String(IV);

            using ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor();
            using CryptoStream cryptoStream = new CryptoStream(Stream, cryptoTransform, CryptoStreamMode.Write);


            // uncomment to get an auto generated version
            //Debug.Log($"Init Vector: {Convert.ToBase64String(aesProvider.IV)}");
            //Debug.Log($"Key: {Convert.ToBase64String(aesProvider.Key)}");

            cryptoStream.Write(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(Data)));
        }

        private T ReadEncryptedData<T>(string Path)
        {
            byte[] fileBytes = File.ReadAllBytes(Path);
            using Aes aesProvider = Aes.Create();

            aesProvider.Key = Convert.FromBase64String(KEY);
            aesProvider.IV = Convert.FromBase64String(IV);

            using ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor(aesProvider.Key, aesProvider.IV);
            using MemoryStream decryptionStream = new MemoryStream(fileBytes);
            using CryptoStream cryptoStream = new CryptoStream(decryptionStream, cryptoTransform, CryptoStreamMode.Read);
            using StreamReader reader = new StreamReader(cryptoStream);

            string result = reader.ReadToEnd();

            Debug.Log($"Decrypt data result::  {result}");
            return JsonConvert.DeserializeObject<T>(result);
        }
    }

}

