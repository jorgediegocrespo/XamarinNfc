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

[assembly: Dependency(typeof(NfcService))]
namespace AccessControl.Droid.Services
{
    public class NfcService : INfcService
    {
        private static NfcService instance;
        private readonly NfcAdapter nfcAdapter;

        private bool isDiscovering;
        private bool isReading;
        private bool isWritingText;
        private bool isCleaning;

        private string textToWrite;

        private Tag currentTag;
        private Intent currentItent;

        public NfcService()
        {
            nfcAdapter = NfcAdapter.GetDefaultAdapter(ActivityProvider.CurrentActivity);
            instance = this;
        }

        public event EventHandler<NfcTagInfo> OnNfcTagRead;
        public event EventHandler<bool> OnNfcTagTextWriten;
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

        public void Read()
        {
            isReading = true;
            ManageCurrentOperation();
        }

        public void WriteText(string text)
        {
            textToWrite = text;
            isWritingText = true;
            ManageCurrentOperation();
        }

        public void Clean()
        {
            isCleaning = true;
            ManageCurrentOperation();
        }

        public void StopDiscovering() => isDiscovering = false;
        public void StopReading() => isReading = false;
        public void StopWritingText() => isWritingText = false;
        public void StopCleaning() => isCleaning = false;

        private void ManageCurrentOperation()
        {
            try
            {
                if (nfcAdapter == null)
                    return; //NFC not supported

                if (currentItent != null)
                {
                    ManageCurrentItent();
                    return;
                }

                IntentFilter tagDetected = new IntentFilter(NfcAdapter.ActionTagDiscovered);
                IntentFilter ndefDetected = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
                IntentFilter techDetected = new IntentFilter(NfcAdapter.ActionTechDiscovered);

                IntentFilter[] filters = new[] { tagDetected, ndefDetected, techDetected };
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
            currentItent = intent;
            ManageCurrentItent();
        }

        private void ManageCurrentItent()
        {
            if (currentItent == null)
                return;

            if (currentItent.Action != NfcAdapter.ActionTagDiscovered &&
                currentItent.Action != NfcAdapter.ActionNdefDiscovered &&
                currentItent.Action != NfcAdapter.ActionTechDiscovered)
                return;

            currentTag = currentItent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
            if (currentTag == null)
                return;

            if (isDiscovering)
                ManageDiscoverOperation();
            if (isReading)
                ManageReadOperation();
            if (isWritingText)
                ManageWriteTextOperation();
        }

        private void ManageDiscoverOperation()
        {
            OnNfcTagDiscovered?.Invoke(this, null);
            currentItent = null;
        }

        private void ManageReadOperation()
        {
            NfcTagInfo nTag = GetNfcTagInfo(currentTag);

            OnNfcTagRead?.Invoke(this, nTag);
            currentItent = null;
        }

        private void ManageWriteTextOperation()
        {
            Ndef ndef = null;
            bool writen = false;
            try
            {
                NfcNdefRecord record = new NfcNdefRecord
                {
                    TypeFormat = NfcNdefTypeFormat.WellKnown,
                    MimeType = "application/com.companyname.accesscontrol",
                    Payload = NfcUtils.EncodeToByteArray(textToWrite),
                };

                ndef = Ndef.Get(currentTag);
                if (!CheckWriteOperation(ndef, record))
                    return;

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
                    return;
                }
                                        
                ndef.WriteNdefMessage(message);
                writen = true;
            }
            catch (Android.Nfc.TagLostException tlex)
            {
                throw new Exception("Tag Lost Error: " + tlex.Message);
            }
            catch (Java.IO.IOException ioex)
            {
                throw new Exception("Tag IO Error: " + ioex.Message);
            }
            catch (Android.Nfc.FormatException fe)
            {
                throw new Exception("Tag Format Error: " + fe.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Tag Error:" + ex.Message);
            }
            finally
            {
                if (ndef?.IsConnected == true)
                    ndef.Close();

                currentTag = null;
                currentItent = null;
                OnNfcTagTextWriten?.Invoke(this, writen);
            }
        }

        private bool CheckWriteOperation(Ndef ndef, NfcNdefRecord record)
        {
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

        private NfcTagInfo GetNfcTagInfo(Tag tag, NdefMessage ndefMessage = null)
        {
            if (tag == null)
                return null;

            Ndef ndef = Ndef.Get(tag);
            NfcTagInfo nTag = new NfcTagInfo(tag.GetId(), ndef != null);

            if (ndef != null)
            {
                nTag.Capacity = ndef.MaxSize;
                nTag.IsWritable = ndef.IsWritable;

                if (ndefMessage == null)
                    ndefMessage = ndef.CachedNdefMessage;

                if (ndefMessage != null)
                {
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
                case NfcNdefTypeFormat.External:
                    ndefRecord = NdefRecord.CreateExternal(record.ExternalDomain, record.ExternalType, record.Payload);
                    break;
                case NfcNdefTypeFormat.Empty:
                    byte[] empty = Array.Empty<byte>();
                    ndefRecord = new NdefRecord(NdefRecord.TnfEmpty, empty, empty, empty);
                    break;
                case NfcNdefTypeFormat.Unknown:
                case NfcNdefTypeFormat.Unchanged:
                case NfcNdefTypeFormat.Reserved:
                default:
                    break;

            }
            return ndefRecord;
        }
    }
}
