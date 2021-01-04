namespace AccessControl.Services
{
    public class NfcTagInfo
    {
		public NfcTagInfo()
		{
			IsSupported = true;
		}

		public NfcTagInfo(byte[] identifier, bool isNdef = true)
		{
			Identifier = identifier;
			SerialNumber = NfcUtils.ByteArrayToHexString(identifier);
			IsSupported = isNdef;
		}

		public byte[] Identifier { get; }
		public string SerialNumber { get; }
		public bool IsWritable { get; set; }
		public int Capacity { get; set; }
		public NfcNdefRecord[] Records { get; set; }
		public bool IsEmpty => Records == null || Records.Length == 0 || Records[0] == null || Records[0].TypeFormat == NfcNdefTypeFormat.Empty;
		public bool IsSupported { get; private set; }
	}
}
