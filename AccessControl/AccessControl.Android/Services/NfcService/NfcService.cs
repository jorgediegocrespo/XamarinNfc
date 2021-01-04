using System;
using System.Diagnostics;
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
        private bool isWriting;
        private bool isCleaning;

        private Tag currentTag;
        private Intent currentItent;

        public NfcService()
        {
            nfcAdapter = NfcAdapter.GetDefaultAdapter(ActivityProvider.CurrentActivity);
            instance = this;
        }

        public event EventHandler<NfcTagInfo> OnNfcTagRead;
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

        public void Write()
        {
            isWriting = true;
            ManageCurrentOperation();
        }

        public void Clean()
        {
            isCleaning = true;
            ManageCurrentOperation();
        }

        public void StopDiscovering() => isDiscovering = false;
        public void StopReading() => isReading = false;
        public void StopWriting() => isWriting = false;
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

                //if (nfcAdapter == null)
                //    return;

                //Intent intent = new Intent(ActivityProvider.CurrentActivity, ActivityProvider.CurrentActivity.GetType()).AddFlags(ActivityFlags.SingleTop);
                //PendingIntent pendingIntent = PendingIntent.GetActivity(ActivityProvider.CurrentActivity, 0, intent, 0);

                //IntentFilter ndefFilter = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
                //ndefFilter.AddDataType("*/*");

                //IntentFilter tagFilter = new IntentFilter(NfcAdapter.ActionTagDiscovered);
                //tagFilter.AddCategory(Intent.CategoryDefault);

                //IntentFilter[] filters = new IntentFilter[] { ndefFilter, tagFilter };

                //nfcAdapter.EnableForegroundDispatch(ActivityProvider.CurrentActivity, pendingIntent, filters, null);

                //isReading = true;
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

            NfcTagInfo nTag = GetNfcTagInfo(currentTag);
            if (isDiscovering)
            {
                OnNfcTagDiscovered?.Invoke(this, null);
                currentItent = null;
            }
            if (isReading)
            {
                OnNfcTagRead?.Invoke(this, nTag);
                currentItent = null;
            }
            

            //if (isWriting)
            //{
            //    OnTagDiscovered?.Invoke(nTag, _isFormatting);
            //}
            //else
            //{
            //    // Read mode
            //    OnMessageReceived?.Invoke(nTag);
            //}
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
    }
}
