namespace Darklight.DataService
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;
	using System.Security.Cryptography;
	using System.Text;

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
					File.WriteAllText(path, JsonUtility.ToJson(Data));
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

					data = JsonUtility.FromJson<T>(File.ReadAllText(path));
				}

				return data;
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to load data due to: {e.Message} {e.StackTrace}");
				throw;
			}
		}

		private void WriteEncryptedData<T>(T data, FileStream stream)
		{
			using Aes aesProvider = Aes.Create();

			aesProvider.Key = Convert.FromBase64String(KEY);
			aesProvider.IV = Convert.FromBase64String(IV);

			using (ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor())
			using (CryptoStream cryptoStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Write))
			{
				byte[] bytes = Encoding.ASCII.GetBytes(JsonUtility.ToJson(data));
				cryptoStream.Write(bytes, 0, bytes.Length);
			}
		}

		private T ReadEncryptedData<T>(string path)
		{
			using (Aes aesProvider = Aes.Create())
			{
				aesProvider.Key = Convert.FromBase64String(KEY);
				aesProvider.IV = Convert.FromBase64String(IV);

				using (ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor())
				using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
				using (CryptoStream cryptoStream = new CryptoStream(fileStream, cryptoTransform, CryptoStreamMode.Read))
				using (StreamReader reader = new StreamReader(cryptoStream))
				{
					string result = reader.ReadToEnd();
					return JsonUtility.FromJson<T>(result);
				}
			}
		}
	}

}

