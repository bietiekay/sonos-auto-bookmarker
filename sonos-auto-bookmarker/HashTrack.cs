using System;
using SONOSHttpAPI;
using System.Security.Cryptography;
using System.Text;

namespace sonosautobookmarker
{
	public static class HashTrack
	{
		public static byte[] GetHash(string inputString)
		{
			HashAlgorithm algorithm = MD5.Create();  // SHA1.Create()
			return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		public static string GetHashString(string inputString)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetHash(inputString))
				sb.Append(b.ToString("X2"));

			return sb.ToString();
		}

		public static String GenerateHashTrack (CurrentTrack incomingTrack)
		{
			if (incomingTrack != null)
				return GetHashString (incomingTrack.title + incomingTrack.album + incomingTrack.artist + Convert.ToString (incomingTrack.duration));
			else
				return null;
		}
	}
}

