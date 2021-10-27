using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Gafware.Modules.Reservations
{
	public class RijndaelSimple
	{
		public RijndaelSimple()
		{
		}

		public static string Decrypt(string cipherText, string passPhrase, string saltValue, string hashAlgorithm, int passwordIterations, string initVector, int keySize)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(initVector);
			byte[] numArray = Encoding.ASCII.GetBytes(saltValue);
			byte[] numArray1 = Convert.FromBase64String(cipherText);
			byte[] bytes1 = (new PasswordDeriveBytes(passPhrase, numArray, hashAlgorithm, passwordIterations)).GetBytes(keySize / 8);
			ICryptoTransform cryptoTransform = (new RijndaelManaged()
			{
				Mode = CipherMode.CBC
			}).CreateDecryptor(bytes1, bytes);
			MemoryStream memoryStream = new MemoryStream(numArray1);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
			byte[] numArray2 = new byte[(int)numArray1.Length];
			int num = cryptoStream.Read(numArray2, 0, (int)numArray2.Length);
			memoryStream.Close();
			cryptoStream.Close();
			return Encoding.UTF8.GetString(numArray2, 0, num);
		}

		public static string Encrypt(string plainText, string passPhrase, string saltValue, string hashAlgorithm, int passwordIterations, string initVector, int keySize)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(initVector);
			byte[] numArray = Encoding.ASCII.GetBytes(saltValue);
			byte[] bytes1 = Encoding.UTF8.GetBytes(plainText);
			byte[] numArray1 = (new PasswordDeriveBytes(passPhrase, numArray, hashAlgorithm, passwordIterations)).GetBytes(keySize / 8);
			ICryptoTransform cryptoTransform = (new RijndaelManaged()
			{
				Mode = CipherMode.CBC
			}).CreateEncryptor(numArray1, bytes);
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
			cryptoStream.Write(bytes1, 0, (int)bytes1.Length);
			cryptoStream.FlushFinalBlock();
			byte[] array = memoryStream.ToArray();
			memoryStream.Close();
			cryptoStream.Close();
			return Convert.ToBase64String(array);
		}
	}
}