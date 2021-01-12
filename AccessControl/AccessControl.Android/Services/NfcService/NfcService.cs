using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AccessControl.Droid.Services;
using AccessControl.Services;
using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Xamarin.Forms;
using AccessControl.Extensions;

[assembly: Dependency(typeof(NfcService))]
namespace AccessControl.Droid.Services
{
    public class NfcService : INfcService
    {
        private static NfcService instance;
        private readonly NfcAdapter nfcAdapter;

        private bool isDiscovering;
        private bool isReadingTag;
        private bool isWritingTagText;
        private bool isWritingTagUri;
        private bool isWritingTagMime;
        private bool isCleaningTag;

        private string textToWrite;
        private string uriToWrite;
        private string mimeToWrite;

        private Tag currentTag;
        private Intent currentIntent;

        private MifareClassic currentMifareClassic;
        private byte[] innitialCardId;

        private IsoDep currentIsoDept;
        private byte[] appToSelect;
        private bool isSelectingApp;

        public NfcService()
        {
            //TODO 1. DISCOVERING CARD 3
            nfcAdapter = NfcAdapter.GetDefaultAdapter(ActivityProvider.CurrentActivity);
            instance = this;
        }

        public event EventHandler<NfcTagInfo> OnNfcTagRead;
        public event EventHandler<bool> OnNfcTagTextWriten;
        public event EventHandler<bool> OnNfcTagUriWriten;
        public event EventHandler<bool> OnNfcTagMimeWriten;
        public event EventHandler<bool> OnNfcTagCleaned;
        public event EventHandler OnNfcTagDiscovered;

        internal static void OnNewIntent(Intent intent)
        {
            instance?.ManageIntent(intent);
        }

        public void StartDiscovering()
        {
            isDiscovering = true;
            ManageCurrentOperation();            
        }

        public void ReadTag()
        {
            isReadingTag = true;
            ManageCurrentOperation();
        }

        public void WriteTagText(string text)
        {
            textToWrite = text;
            isWritingTagText = true;
            ManageCurrentOperation();
        }

        public void WriteTagUri(string uri)
        {
            uriToWrite = uri;
            isWritingTagUri = true;
            ManageCurrentOperation();
        }

        public void WriteTagMime(string mime)
        {
            mimeToWrite = mime;
            isWritingTagMime = true;
            ManageCurrentOperation();
        }

        public void CleanTag()
        {
            isCleaningTag = true;
            ManageCurrentOperation();
        }

        public void StopDiscovering() => isDiscovering = false;
        public void StopReadingTag() => isReadingTag = false;
        public void StopWritingTagText() => isWritingTagText = false;
        public void StopWritingTagUri() => isWritingTagUri = false;
        public void StopWritingTagMime() => isWritingTagMime = false;
        public void StopCleaningTag() => isCleaningTag = false;
        
        private void ManageCurrentOperation()
        {
            try
            {
                if (nfcAdapter == null)
                    return; //NFC not supported

                if (currentIntent != null)
                {
                    ManageCurrentItent();
                    return;
                }

                //TODO 1. DISCOVERING CARD 4
                IntentFilter tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
                IntentFilter ndefDetected = new IntentFilter(NfcAdapter.ActionNdefDiscovered);

                IntentFilter[] filters = new[] { tagDetected, ndefDetected };
                Intent intent = new Intent(ActivityProvider.CurrentActivity, ActivityProvider.CurrentActivity.GetType()).AddFlags(ActivityFlags.SingleTop);
                PendingIntent pendingIntent = PendingIntent.GetActivity(ActivityProvider.CurrentActivity, 0, intent, 0);

                nfcAdapter.EnableForegroundDispatch(ActivityProvider.CurrentActivity, pendingIntent, filters, null);
            }
            catch
            {
                Debug.WriteLine("Exception processing NFC operation");
            }
        }

        private void ManageIntent(Intent intent)
        {
            currentIntent = intent;
            ManageCurrentItent();
        }

        private void ManageCurrentItent()
        {
            if (currentIntent == null)
                return;

            if (currentIntent.Action != NfcAdapter.ActionTagDiscovered &&
                currentIntent.Action != NfcAdapter.ActionNdefDiscovered)
                return;

            currentTag = currentIntent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
            if (currentTag == null)
                return;

            if (isDiscovering)
                ManageDiscoverOperation();
            if (isReadingTag)
                ManageReadOperation();
            if (isWritingTagText)
                ManageWriteTextOperation();
            if (isWritingTagUri)
                ManageWriteUriOperation();
            if (isWritingTagMime)
                ManageWriteMimeOperation();
            if (isCleaningTag)
                ManageCleanOperation();
            if (isSelectingApp)                SelectApp();
        }

        private void ManageDiscoverOperation()
        {
            OnNfcTagDiscovered?.Invoke(this, null);
        }

        private void ManageReadOperation()
        {
            //TODO 2. READING CARD 5
            NfcTagInfo nTag = GetNfcTagInfo(currentTag);

            OnNfcTagRead?.Invoke(this, nTag);
            currentIntent = null;
        }

        private NfcTagInfo GetNfcTagInfo(Tag tag)
        {
            if (tag == null)
                return null;

            Ndef ndef = Ndef.Get(tag);
            NfcTagInfo nTag = new NfcTagInfo();

            if (ndef != null)
            {
                if (ndef.CachedNdefMessage != null)
                {
                    NdefMessage ndefMessage = ndef.CachedNdefMessage; //TODO 2. READING CARD 6 ndef.NdefMessage -> TagLost
                    var records = ndefMessage.GetRecords();
                    nTag.Records = GetRecords(records);
                }
            }

            return nTag;
        }

        private NfcNdefRecord[] GetRecords(NdefRecord[] records)
        {
            var results = new NfcNdefRecord[records.Length];
            for (var i = 0; i < records.Length; i++)
            {
                var ndefRecord = new NfcNdefRecord
                {
                    TypeFormat = (NfcNdefTypeFormat)records[i].Tnf,
                    Uri = records[i].ToUri()?.ToString(),
                    MimeType = records[i].ToMimeType(),
                    Payload = records[i].GetPayload()
                };
                results.SetValue(ndefRecord, i);
            }
            return results;
        }

        private void ManageWriteTextOperation()
        {
            NfcNdefRecord record = new NfcNdefRecord
            {
                TypeFormat = NfcNdefTypeFormat.WellKnown,
                Payload = NfcUtils.EncodeToByteArray(textToWrite),
            };

            OnNfcTagTextWriten?.Invoke(this, WriteNfcNdefRecord(record));
        }

        private void ManageWriteUriOperation()
        {
            NfcNdefRecord record = new NfcNdefRecord
            {
                TypeFormat = NfcNdefTypeFormat.Uri,
                Payload = NfcUtils.EncodeToByteArray(uriToWrite)
            };

            OnNfcTagUriWriten?.Invoke(this, WriteNfcNdefRecord(record));
        }

        private void ManageWriteMimeOperation()
        {
            NfcNdefRecord record = new NfcNdefRecord
            {
                TypeFormat = NfcNdefTypeFormat.Mime,
                Payload = NfcUtils.EncodeToByteArray(mimeToWrite)
            };

            OnNfcTagMimeWriten?.Invoke(this, WriteNfcNdefRecord(record));
        }

        private bool WriteNfcNdefRecord(NfcNdefRecord record)
        {
            //TODO 3. WRITING CARD 4
            Ndef ndef = null;
            try
            {
                ndef = Ndef.Get(currentTag);
                if (!CheckWriteOperation(ndef, record))
                    return false;

                ndef.Connect();

                NdefMessage message = null;
                List<NdefRecord> records = new List<NdefRecord>();
                if (GetAndroidNdefRecord(record) is NdefRecord ndefRecord)
                    records.Add(ndefRecord);

                if (records.Any())
                    message = new NdefMessage(records.ToArray());

                if (message == null)
                {
                    Debug.WriteLine("NFC tag can not be writen with null message");
                    return false;
                }

                ndef.WriteNdefMessage(message);
                return true;
            }
            catch (Android.Nfc.TagLostException tlex)
            {
                Debug.WriteLine($"Tag Lost Error: {tlex.Message}");
            }
            catch (Java.IO.IOException ioex)
            {
                Debug.WriteLine($"Tag IO Error: {ioex.Message}");
            }
            catch (Android.Nfc.FormatException fe)
            {
                Debug.WriteLine($"Tag Format Error: {fe.Message}");
            }
            catch
            {
                Debug.WriteLine($"Tag Error");
            }
            finally
            {
                if (ndef?.IsConnected == true)
                    ndef.Close();

                currentTag = null;
                currentIntent = null;
            }

            return false;
        }

        private void ManageCleanOperation()
        {
            //TODO 3. WRITING CARD 5
            Ndef ndef = null;
            NfcNdefRecord record = null;
            try
            {
                ndef = Ndef.Get(currentTag);
                if (!CheckWriteOperation(ndef, record))
                {
                    OnNfcTagCleaned?.Invoke(this, false);
                    return;
                }

                ndef.Connect();

                byte[] empty = Array.Empty<byte>();
                NdefMessage message = new NdefMessage(new NdefRecord[1] { new NdefRecord(NdefRecord.TnfEmpty, empty, empty, empty) });

                ndef.WriteNdefMessage(message);
                OnNfcTagCleaned?.Invoke(this, true);
                return;
            }
            catch (Android.Nfc.TagLostException tlex)
            {
                Debug.WriteLine($"Tag Lost Error: {tlex.Message}");
            }
            catch (Java.IO.IOException ioex)
            {
                Debug.WriteLine($"Tag IO Error: {ioex.Message}");
            }
            catch (Android.Nfc.FormatException fe)
            {
                Debug.WriteLine($"Tag Format Error: {fe.Message}");
            }
            catch
            {
                Debug.WriteLine($"Tag Error");
            }
            finally
            {
                if (ndef?.IsConnected == true)
                    ndef.Close();

                currentTag = null;
                currentIntent = null;
            }

            OnNfcTagCleaned?.Invoke(this, false);
        }

        private bool CheckWriteOperation(Ndef ndef, NfcNdefRecord record)
        {
            //TODO 3. WRITING CARD 6
            if (ndef == null)
                return false;

            if (!ndef.IsWritable)
            {
                Debug.WriteLine("NFC tag is readonly");
                return false;
            }

            if (ndef.MaxSize < NfcUtils.GetSize(new NfcNdefRecord[] { record }))
            {
                Debug.WriteLine("NFC tag size is less than the message to write");
                return false;
            }

            return true;
        }

        private NdefRecord GetAndroidNdefRecord(NfcNdefRecord record)
        {
            if (record == null)
                return null;

            NdefRecord ndefRecord = null;
            switch (record.TypeFormat)
            {
                case NfcNdefTypeFormat.WellKnown:
                    ndefRecord = NdefRecord.CreateTextRecord("en", Encoding.UTF8.GetString(record.Payload));
                    break;
                case NfcNdefTypeFormat.Mime:
                    ndefRecord = NdefRecord.CreateMime(record.MimeType, record.Payload);
                    break;
                case NfcNdefTypeFormat.Uri:
                    ndefRecord = NdefRecord.CreateUri(Encoding.UTF8.GetString(record.Payload));
                    break;
                case NfcNdefTypeFormat.Empty:
                    byte[] empty = Array.Empty<byte>();
                    ndefRecord = new NdefRecord(NdefRecord.TnfEmpty, empty, empty, empty);
                    break;
                case NfcNdefTypeFormat.External:
                case NfcNdefTypeFormat.Unknown:
                case NfcNdefTypeFormat.Unchanged:
                case NfcNdefTypeFormat.Reserved:
                default:
                    break;

            }
            return ndefRecord;
        }
        
        public byte[] ReadByteBlock(int block, byte[] key, NfcKeyType nfcKeyType)
        {
            //TODO 4. MIFARE OPERATIONS 2
            try
            {
                //No card found
                if (!LoadCurrentMifareClassic())                    return null;

                //Different card ids
                if (!CheckCardId())                    return null;                //No authenticated                if (!AuthenticateSector(block, nfcKeyType, key))                    return null;                byte[] data = currentMifareClassic.ReadBlock(block);
                return data;
            }            catch            {                return null;            }
        }

        public bool WriteByteBlock(int block, byte[] key, byte[] content, NfcKeyType nfcKeyType)
        {
            //TODO 4. MIFARE OPERATIONS 3
            try
            {
                if (content?.Length != 16)
                    return false;

                //No card found
                if (!LoadCurrentMifareClassic())
                    return false;

                //Different card ids
                if (!CheckCardId())
                    return false;

                //No authenticated
                if (!AuthenticateSector(block, nfcKeyType, key))
                    return false;

                currentMifareClassic.WriteBlock(block, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void StopByteBlockOperations()
        {
            innitialCardId = null;            currentMifareClassic?.Close();            currentMifareClassic?.Dispose();            currentMifareClassic = null;

            currentTag = null;
            currentIntent = null;
        }

        private bool LoadCurrentMifareClassic()        {            if (currentMifareClassic != null)
            {
                ConnectCurrentMifareClassic();
                return true;
            }

            if (currentTag == null)
                return false;

            currentMifareClassic = MifareClassic.Get(currentTag);
            if (currentMifareClassic == null)
                return false;

            ConnectCurrentMifareClassic();
            return true;        }

        private void ConnectCurrentMifareClassic()        {            if (!currentMifareClassic.IsConnected)                currentMifareClassic.Connect();        }

        private bool CheckCardId()        {            SetCardId();            if (currentTag == null)                return false;            byte[] currentCardId = currentTag.GetId();            if (innitialCardId == null || currentCardId == null)                return false;            if (BitConverter.ToString(innitialCardId) != BitConverter.ToString(currentCardId))                return false;            return true;        }

        private bool SetCardId()        {            if (innitialCardId != null)                return true;            if (currentTag == null)                return false;            innitialCardId = currentTag.GetId();            return true;        }

        private bool AuthenticateSector(int block, NfcKeyType nfcKeyType, byte[] key)        {            int sector = currentMifareClassic.BlockToSector(block);            switch (nfcKeyType)
            {
                case NfcKeyType.KeyB:
                    return key != null && currentMifareClassic.AuthenticateSectorWithKeyB(sector, key);
                case NfcKeyType.KeyA:
                default:
                    return key != null && currentMifareClassic.AuthenticateSectorWithKeyA(sector, key);
            }        }

        public void StartSelectingApp(byte[] app)
        {
            appToSelect = app;            isSelectingApp = true;            ManageCurrentItent();
        }

        private void SelectApp()        {            try
            {
                if (!LoadCurrentIsoDept())
                    OnAppSelected?.Invoke(this, false);

                List<byte> selectOperation = new List<byte> { 0x5A };                selectOperation.AddRange(appToSelect);                byte[] result = currentIsoDept.Transceive(selectOperation.ToArray());                if (result.Length == 1 && result[0] == 0x00)                    OnAppSelected?.Invoke(this, true);                else                    OnAppSelected?.Invoke(this, false);
            }            catch            {                OnAppSelected?.Invoke(this, false);            }        }

        public byte[] FirstChallenge(byte keyNo)
        {
            try
            {
                if (!LoadCurrentIsoDept())
                    return null;

                byte[] op = new byte[] { 0x0A, keyNo };                byte[] result = currentIsoDept.Transceive(new byte[] { 0x0A, keyNo });                if (result != null && result.Length > 0 && result[0] == 0xAF)                    return result.Skip(1).ToArray();                else                    return null;
            }
            catch
            {
                return null;
            }
        }

        public byte[] SecondChallenge(byte[] value)
        {
            try
            {
                if (!LoadCurrentIsoDept())
                    return null;

                List<byte> op = new List<byte> { 0xAF };                op.AddRange(value);                byte[] result = currentIsoDept.Transceive(op.ToArray());                if (result != null && result.Length == 9 && result[0] == 0x00)                    return result.Skip(1).ToArray();                else                    return null;
            }
            catch
            {
                return null;
            }
        }

        public byte[] GetValue(int file)
        {
            try
            {
                if (!LoadCurrentIsoDept())
                    return null;

                List<byte> readFileOp = new List<byte> { 0x6C };                string hexFile = file.ToString("X").PadLeft(2, '0');                readFileOp.AddRange(hexFile.StringToByteArray()); //Files

                byte[] result = currentIsoDept.Transceive(readFileOp.ToArray());                if (result.Length > 0 && result[0] == 0x00)                    return result.Skip(1).ToArray();                else                    return null;
            }
            catch
            {
                return null;
            }
        }

        public byte[] ReadData(int file, int offset, int lenght)
        {
            try
            {
                if (!LoadCurrentIsoDept())
                    return null;

                return Read(0xBD, file, offset, lenght);
            }
            catch
            {
                return null;
            }
        }

        public byte[] ReadRecords(int file, int firstRegister, int numberOfRegisters)
        {
            try
            {
                if (!LoadCurrentIsoDept())
                    return null;

                return Read(0xBB, file, firstRegister, numberOfRegisters);
            }
            catch
            {
                return null;
            }
        }

        public bool WriteData(int file, byte[] data, int offset, int lenght)
        {
            try
            {
                if (!LoadCurrentIsoDept())
                    return false;

                return Write(0x3D, data, file, offset, lenght);
            }
            catch
            {
                return false;
            }
        }

        public bool WriteRecord(int file, byte[] data, int length)
        {
            try
            {
                if (!LoadCurrentIsoDept())
                    return false;

                return Write(0x3B, data, file, 0, length);
            }
            catch
            {
                return false;
            }
        }

        public bool IncrementValue(int file, byte[] data)
        {
            try
            {
                if (!LoadCurrentIsoDept())
                    return false;

                List<byte> incrementOp = new List<byte> { 0x0C };                string fileHex = file.ToString("X").PadLeft(2, '0');                incrementOp.AddRange(fileHex.StringToByteArray()); //Files
                incrementOp.AddRange(data); //Data

                byte[] result = currentIsoDept.Transceive(incrementOp.ToArray());                if (result.Length == 1 && result[0] == 0x00)                    return true;                else                    return false;
            }
            catch
            {
                return false;
            }
        }

        public bool DecrementValue(int file, byte[] data)
        {
            try
            {
                if (!LoadCurrentIsoDept())
                    return false;

                List<byte> decrementOp = new List<byte> { 0xDC };                string fileHex = file.ToString("X").PadLeft(2, '0');                decrementOp.AddRange(fileHex.StringToByteArray()); //Files
                decrementOp.AddRange(data); //Data

                byte[] result = currentIsoDept.Transceive(decrementOp.ToArray());                if (result.Length == 1 && result[0] == 0x00)                    return true;                else                    return false;
            }
            catch
            {
                return false;
            }
        }

        public bool CommitOperations()
        {
            try
            {
                if (!LoadCurrentIsoDept())
                    return false;

                byte[] result = currentIsoDept.Transceive(new byte[] { 0xC7 });                if (result.Length == 1 && result[0] == 0x00)                    return true;                else                    return false;
            }
            catch
            {
                return false;
            }
        }

        public void StopSelectingApp() => isSelectingApp = false;

        public void StopApduOperations()
        {
            currentIsoDept?.Close();            currentIsoDept?.Dispose();            currentIsoDept = null;

            currentTag = null;
            currentIntent = null;
        }

        public event EventHandler<bool> OnAppSelected;

        private bool LoadCurrentIsoDept()        {            if (currentIsoDept != null)
            {
                ConnectCurrentIsoDept();                return true;
            }            if (currentTag == null)                return false;            currentIsoDept = IsoDep.Get(currentTag);            if (currentIsoDept == null)                return false;            ConnectCurrentIsoDept();            return true;        }

        private void ConnectCurrentIsoDept()        {            if (!currentIsoDept.IsConnected)                currentIsoDept.Connect();        }

        private byte[] Read(byte operation, int fileToRead, int offsetToRead, int lengthToRead)        {            List<byte> readFileOp = new List<byte> { operation };            string file = fileToRead.ToString("X").PadLeft(2, '0');            readFileOp.AddRange(file.StringToByteArray()); //File

            string offset = offsetToRead.ToString("X").PadLeft(6, '0');            readFileOp.AddRange(offset.GetReverseHex().StringToByteArray()); //Offset

            string lenght = lengthToRead.ToString("X").PadLeft(6, '0');            readFileOp.AddRange(lenght.GetReverseHex().StringToByteArray()); //Length

            bool continueReading = false;            List<byte> totalResult = new List<byte>();            do            {                byte[] result = currentIsoDept.Transceive(readFileOp.ToArray());                if (result.Length > 0 && result[0] == 0xAF)                {                    totalResult.AddRange(result.Skip(1));                    readFileOp = new List<byte> { 0xAF };                    continueReading = true;                }                else if (result.Length > 0 && result[0] == 0x00)                {                    totalResult.AddRange(result.Skip(1));                    continueReading = false;                }                else                {                    totalResult = null;                    continueReading = false;                }            }            while (continueReading);            return totalResult.ToArray();        }

        private bool Write(byte operation, byte[] dataToWrite, int fileToWrite, int offsetToWrite, int lengthToWrite)        {            try            {                bool finished = false;                int index = 0;                do                {                    byte[] result;                    if (index == 0)                        result = WriteFirstBlock(operation, dataToWrite, fileToWrite, offsetToWrite, lengthToWrite);                    else                        result = ContinueWritingBlock(index, dataToWrite);                    if (result.Length == 1 && result[0] == 0x00)                    {                        finished = true;                        return true;                    }                    else if (result.Length == 1 && result[0] == 0xAF)                    {                        finished = false;                    }                    else                    {                        finished = true;                        return false;                    }                    index++;                } while (!finished);                return false;            }            catch            {                return false;            }        }        private byte[] WriteFirstBlock(byte operation, byte[] dataToWrite, int fileToWrite, int offsetToWrite, int lengthToWrite)        {            int fileLength = dataToWrite.Length / 2;            List<byte> writeFileOp = new List<byte> { operation };            string file = fileToWrite.ToString("X").PadLeft(2, '0');            writeFileOp.AddRange(file.StringToByteArray()); //File

            string offset = offsetToWrite.ToString("X").PadLeft(6, '0');            writeFileOp.AddRange(offset.GetReverseHex().StringToByteArray()); //Offset

            int length = lengthToWrite == 0 ? dataToWrite.Length / 2 : lengthToWrite;            string hexLength = length.ToString("X").PadLeft(6, '0');            writeFileOp.AddRange(hexLength.GetReverseHex().StringToByteArray()); //Length

            if (fileLength > 52)            {                byte[] currentFileToWrite = new byte[52];                dataToWrite.ToList().CopyTo(0, currentFileToWrite, 0, 52);                writeFileOp.AddRange(currentFileToWrite); //Data
            }            else            {                writeFileOp.AddRange(dataToWrite); //Data
            }            byte[] result = currentIsoDept.Transceive(writeFileOp.ToArray());            return result;        }        private byte[] ContinueWritingBlock(int index, byte[] dataToWrite)        {            int fileLength = dataToWrite.Length / 2;            List<byte> writeFileOp = new List<byte> { 0xAF };            if (fileLength > 52 + (index * 59))            {                byte[] currentFileToWrite = new byte[59];                dataToWrite.ToList().CopyTo(52 + ((index - 1) * 59), currentFileToWrite, 0, 59);                writeFileOp.AddRange(currentFileToWrite); //Data
            }            else            {                int blockSize = 59 - (52 + (index * 59) - fileLength);                byte[] currentFileToWrite = new byte[blockSize];                dataToWrite.ToList().CopyTo(52 + ((index - 1) * 59), currentFileToWrite, 0, blockSize);                writeFileOp.AddRange(currentFileToWrite); //Data
            }            byte[] result = currentIsoDept.Transceive(writeFileOp.ToArray());            return result;        }
    }
}
