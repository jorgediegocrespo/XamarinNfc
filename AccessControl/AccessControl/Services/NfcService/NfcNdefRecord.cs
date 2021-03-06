﻿namespace AccessControl.Services
{
    public class NfcNdefRecord
    {
		public NfcNdefTypeFormat TypeFormat { get; set; }
		public string MimeType { get; set; } = "text/plain";
		public byte[] Payload { get; set; }
		public string Uri { get; set; }
		public string Message => NfcUtils.GetMessage(TypeFormat, Payload, Uri);
	}
}
