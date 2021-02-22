using System;
using System.Linq;
using AccessControl.iOS.Services;
using AccessControl.Services;
using CoreFoundation;
using CoreNFC;
using Foundation;
using Xamarin.Forms;

[assembly: Dependency(typeof(NfcService))]
namespace AccessControl.iOS.Services
{
    public class NfcService : NFCNdefReaderSessionDelegate, INfcService
    {
        private static NfcService instance;

        private bool isDiscovering;
        private bool isReadingTag;
        private bool isWritingTagText;
        private bool isWritingTagUri;
        private bool isWritingTagMime;
        private bool isCleaningTag;

        private string textToWrite;
        private string uriToWrite;
        private string mimeToWrite;

        private NFCNdefReaderSession nfcSession;

        public event EventHandler OnNfcTagDiscovered;
        public event EventHandler<NfcTagInfo> OnNfcTagRead;
        public event EventHandler<bool> OnNfcTagTextWriten;
        public event EventHandler<bool> OnNfcTagUriWriten;
        public event EventHandler<bool> OnNfcTagMimeWriten;
        public event EventHandler<bool> OnNfcTagCleaned;
        public event EventHandler<bool> OnAppSelected;

        public NfcService()
        {
            instance = this;
        }

        public override void DidDetect(NFCNdefReaderSession session, NFCNdefMessage[] messages)
        {
            //if (isDiscovering)
            //    ManageDiscoverOperation();
            //if (isReadingTag)
            //    ManageReadOperation(session, messages);
            //if (isWritingTagText)
            //    OnNfcTagTextWriten?.Invoke(this, false);
            //if (isWritingTagUri)
            //    ManageWriteUriOperation();
            //if (isWritingTagMime)
            //    ManageWriteMimeOperation();
            //if (isCleaningTag)
            //    ManageCleanOperation();
        }

        public override void DidInvalidate(NFCNdefReaderSession session, NSError error)
        {
            //Error
            if (isReadingTag)
                OnNfcTagRead?.Invoke(this, null);
            else if (isWritingTagText)
                OnNfcTagTextWriten?.Invoke(this, false);
            else if (isWritingTagUri)
                OnNfcTagUriWriten?.Invoke(this, false);
            else if (isWritingTagMime)
                OnNfcTagMimeWriten?.Invoke(this, false);
            else if (isCleaningTag)
                OnNfcTagCleaned?.Invoke(this, false);
        }

        [Foundation.Export("readerSession:didDetectTags:")]
        public override void DidDetectTags(NFCNdefReaderSession session, INFCNdefTag[] tags)
        {
            if (isDiscovering)
                ManageDiscoverOperation();
            if (isReadingTag)
                ManageReadOperation(session, tags);
            if (isWritingTagText)
                ManageWriteTextOperation(session, tags);
        }

        private void BeginNfcSession()
        {
            nfcSession = new NFCNdefReaderSession(this, DispatchQueue.MainQueue, true)
            {
                AlertMessage = "Acerque tarjeta NFC"
            };
            nfcSession?.BeginSession();
        }

        public void StartDiscovering()
        {
            isDiscovering = true;
            BeginNfcSession();
        }

        public void ReadTag()
        {
            isReadingTag = true;
            BeginNfcSession();
        }

        public void WriteTagText(string text)
        {
            isWritingTagText = true;
            textToWrite = text;
            BeginNfcSession();
        }

        public void WriteTagUri(string uri)
        {
            throw new NotImplementedException();
        }

        public void WriteTagMime(string mime)
        {
            throw new NotImplementedException();
        }

        public void CleanTag()
        {
            throw new NotImplementedException();
        }

        public void StopDiscovering()
        {
            isDiscovering = false;
            nfcSession.InvalidateSession();
        }

        public void StopReadingTag()
        {
            isReadingTag = false;
            nfcSession.InvalidateSession();
        }

        public void StopWritingTagText()
        {
            isWritingTagText = false;
            nfcSession.InvalidateSession();
        }

        public void StopWritingTagUri()
        {
            isWritingTagUri = false;
            nfcSession.InvalidateSession();
        }

        public void StopWritingTagMime()
        {
            isWritingTagMime = false;
            nfcSession.InvalidateSession();
        }

        public void StopCleaningTag()
        {
            isCleaningTag = false;
            nfcSession.InvalidateSession();
        }

        private void ManageDiscoverOperation()
        {
            OnNfcTagDiscovered?.Invoke(this, null);
        }

        //private void ManageReadOperation(NFCNdefReaderSession session, NFCNdefMessage[] messages)
        //{
        //    if (messages?.Any() != true)
        //    {
        //        OnNfcTagRead?.Invoke(this, null);
        //        return;
        //    }

        //    NfcTagInfo tagInfo = new NfcTagInfo();
        //    OnNdefRed(messages.FirstOrDefault(), null);
        //}

        private void ManageReadOperation(NFCNdefReaderSession session, INFCNdefTag[] tags)
        {
            var nFCNdefTag = tags[0];
            session.ConnectToTag(nFCNdefTag, (error) => OnNfcTagTextWriten(this, false));
            nFCNdefTag.ReadNdef(OnNdefRed);
        }

        private void OnNdefRed(NFCNdefMessage message, NSError error)
        {
            if (message == null)
            {
                OnNfcTagRead?.Invoke(this, null);
                return;
            }
            NfcTagInfo tagInfo = new NfcTagInfo();
            tagInfo.Records = GetRecords(message.Records);

            OnNfcTagRead?.Invoke(this, tagInfo);
        }

        private NfcNdefRecord[] GetRecords(NFCNdefPayload[] records)
        {
            NfcNdefRecord[] results = new NfcNdefRecord[records.Length];
            for (var i = 0; i < records.Length; i++)
            {
                NFCNdefPayload record = records[i];
                var ndefRecord = new NfcNdefRecord
                {
                    TypeFormat = (NfcNdefTypeFormat)record.TypeNameFormat,
                    Uri = records[i].ToUri()?.ToString(),
                    MimeType = records[i].ToMimeType(),
                    Payload = record.Payload.ToByteArray()
                };
                results.SetValue(ndefRecord, i);
            }
            return results;
        }

        private void ManageWriteTextOperation(NFCNdefReaderSession session, INFCNdefTag[] tags)
        {
            var nFCNdefTag = tags[0];
            session.ConnectToTag(nFCNdefTag, (error) => OnNfcTagTextWriten(this, false));

            NFCNdefPayload payload = NFCNdefPayload.CreateWellKnownTypePayload(textToWrite, NSLocale.CurrentLocale);
            NFCNdefMessage nFCNdefMessage = new NFCNdefMessage(new NFCNdefPayload[] { payload });
            nFCNdefTag.WriteNdef(nFCNdefMessage, delegate
            {
                OnNfcTagTextWriten(this, true);
            });
        }














        public byte[] ReadByteBlock(int block, byte[] key, NfcKeyType nfcKeyType)
        {
            throw new NotImplementedException();
        }

        public void StartSelectingApp(byte[] app)
        {
            throw new NotImplementedException();
        }

        public void StopApduOperations()
        {
            throw new NotImplementedException();
        }

        public void StopByteBlockOperations()
        {
            throw new NotImplementedException();
        }

        public void StopSelectingApp()
        {
            throw new NotImplementedException();
        }

        public bool WriteByteBlock(int block, byte[] key, byte[] content, NfcKeyType nfcKeyType)
        {
            throw new NotImplementedException();
        }

        public bool WriteData(int file, byte[] data, int offset, int lenght)
        {
            throw new NotImplementedException();
        }

        public bool WriteRecord(int file, byte[] data, int length)
        {
            throw new NotImplementedException();
        }

        public bool CommitOperations()
        {
            throw new NotImplementedException();
        }

        public bool DecrementValue(int file, byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] FirstChallenge(byte keyNo)
        {
            throw new NotImplementedException();
        }

        public byte[] GetValue(int file)
        {
            throw new NotImplementedException();
        }

        public bool IncrementValue(int file, byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadData(int file, int offset, int lenght)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadRecords(int file, int firstRegister, int numberOfRegisters)
        {
            throw new NotImplementedException();
        }

        public byte[] SecondChallenge(byte[] value)
        {
            throw new NotImplementedException();
        }
    }
}
