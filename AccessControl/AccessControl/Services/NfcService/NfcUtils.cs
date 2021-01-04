using System.Linq;
using System.Text;

namespace AccessControl.Services
{
    public class NfcUtils
    {
		public static int GetSize(NfcNdefRecord[] records)
		{
			var size = 0;
			if (records != null && records.Length > 0)
			{
				for (var i = 0; i < records.Length; i++)
				{
					if (records[i] != null)
						size += records[i].Payload.Length;
				}
			}
			return size;
		}

		public static string GetMessage(NfcNdefTypeFormat type, byte[] payload, string uri)
		{
			string message;
			if (!string.IsNullOrWhiteSpace(uri))
				message = uri;
			else
			{
				if (type == NfcNdefTypeFormat.WellKnown)
				{
					// NDEF_WELLKNOWN Text record
					var status = payload[0];
					var enc = status & 0x80;
					var languageCodeLength = status & 0x3F;
					if (enc == 0)
						message = Encoding.UTF8.GetString(payload, languageCodeLength + 1, payload.Length - languageCodeLength - 1);
					else
						message = Encoding.Unicode.GetString(payload, languageCodeLength + 1, payload.Length - languageCodeLength - 1);
				}
				else
				{
					// Other NDEF types
					message = Encoding.UTF8.GetString(payload, 0, payload.Length);
				}
			}
			return message;
		}

		public static byte[] EncodeToByteArray(string text) => Encoding.UTF8.GetBytes(text);

		public static string GetMessage(NfcNdefRecord record)
		{
			if (record == null)
				return string.Empty;

			return GetMessage(record.TypeFormat, record.Payload, record.Uri);
		}

		public static string ByteArrayToHexString(byte[] bytes, string separator = null)
		{
			return bytes == null ? string.Empty : string.Join(separator ?? string.Empty, bytes.Select(b => b.ToString("X2")));
		}

		public static bool IsWritingSupported()
		{
			return true;
		}
	}
}
