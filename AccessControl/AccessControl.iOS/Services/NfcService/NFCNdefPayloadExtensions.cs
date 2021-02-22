using System;
using System.Text;
using CoreNFC;
using Foundation;

namespace AccessControl.iOS.Services
{
    internal static class UtilExtensions
    {
		public static string ToMimeType(this NFCNdefPayload payload)
		{
			switch (payload.TypeNameFormat)
			{
				case NFCTypeNameFormat.NFCWellKnown:
					if (payload.Type.ToString() == "T")
						return "text/plain";
					break;
				case NFCTypeNameFormat.Media:
					return payload.Type.ToString();
			}
			return null;
		}

		public static Uri ToUri(this NFCNdefPayload payload)
		{
			switch (payload.TypeNameFormat)
			{
				case NFCTypeNameFormat.NFCWellKnown:
					if (payload.Type.ToString() == "U")
					{
						var uri = payload.Payload.ParseWktUri();
						return uri;
					}
					break;
				case NFCTypeNameFormat.AbsoluteUri:
				case NFCTypeNameFormat.Media:
                    string content = Encoding.UTF8.GetString(payload.Payload.ToByteArray());
					if (Uri.TryCreate(content, UriKind.RelativeOrAbsolute, out var result))
						return result;

					break;
			}
			return null;
		}

		internal static byte[] ToByteArray(this NSData data)
		{
			var bytes = new byte[data.Length];
			if (data.Length > 0) System.Runtime.InteropServices.Marshal.Copy(data.Bytes, bytes, 0, Convert.ToInt32(data.Length));
			return bytes;
		}

		private static Uri ParseWktUri(this NSData data)
		{
			var payload = data.ToByteArray();

			if (payload.Length < 2)
				return null;

			var prefixIndex = payload[0] & 0xFF;
			if (prefixIndex < 0 || prefixIndex >= uriPrefixesMap.Length)
				return null;

			var prefix = uriPrefixesMap[prefixIndex];
			var suffix = Encoding.UTF8.GetString(CopyOfRange(payload, 1, payload.Length));

			if (Uri.TryCreate(prefix + suffix, UriKind.Absolute, out var result))
				return result;

			return null;
		}

		private static byte[] CopyOfRange(byte[] src, int start, int end)
		{
			var length = end - start;
			var dest = new byte[length];
			for (var i = 0; i < length; i++)
				dest[i] = src[start + i];
			return dest;
		}

		private static readonly string[] uriPrefixesMap = new string[] {
			"", // 0x00
            "http://www.", // 0x01
            "https://www.", // 0x02
            "http://", // 0x03
            "https://", // 0x04
            "tel:", // 0x05
            "mailto:", // 0x06
            "ftp://anonymous:anonymous@", // 0x07
            "ftp://ftp.", // 0x08
            "ftps://", // 0x09
            "sftp://", // 0x0A
            "smb://", // 0x0B
            "nfs://", // 0x0C
            "ftp://", // 0x0D
            "dav://", // 0x0E
            "news:", // 0x0F
            "telnet://", // 0x10
            "imap:", // 0x11
            "rtsp://", // 0x12
            "urn:", // 0x13
            "pop:", // 0x14
            "sip:", // 0x15
            "sips:", // 0x16
            "tftp:", // 0x17
            "btspp://", // 0x18
            "btl2cap://", // 0x19
            "btgoep://", // 0x1A
            "tcpobex://", // 0x1B
            "irdaobex://", // 0x1C
            "file://", // 0x1D
            "urn:epc:id:", // 0x1E
            "urn:epc:tag:", // 0x1F
            "urn:epc:pat:", // 0x20
            "urn:epc:raw:", // 0x21
            "urn:epc:", // 0x22
            "urn:nfc:", // 0x23
		};
	}
}
