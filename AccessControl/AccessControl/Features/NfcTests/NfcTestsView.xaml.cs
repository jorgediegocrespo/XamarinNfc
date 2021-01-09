using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccessControl.Services;
using Xamarin.Forms;

namespace AccessControl.Features
{
    public partial class NfcTestsView
    {
        private readonly INfcService nfcService;

        public NfcTestsView()
        {
            nfcService = DependencyService.Get<INfcService>();
            nfcService.OnNfcTagDiscovered += NfcService_OnNfcTagDiscovered;
            nfcService.OnNfcTagRead += NfcService_OnNfcTagRead;
            nfcService.OnNfcTagTextWriten += NfcService_OnNfcTagTextWriten;
            nfcService.OnNfcTagUriWriten += NfcService_OnNfcTagUriWriten;
            nfcService.OnNfcTagMimeWriten += NfcService_OnNfcTagMimeWriten;
            nfcService.OnNfcTagCleaned += NfcService_OnNfcTagCleaned;

            InitializeComponent();
        }

        private void btnDiscover_Clicked(System.Object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => lbState.Text = "Discovering...");
            btnDiscover.IsEnabled = false;
            nfcService.StartDiscovering();
        }
        
        private void NfcService_OnNfcTagDiscovered(object sender, System.EventArgs e)
        {
            nfcService.StopDiscovering();
            btnDiscover.IsEnabled = true;
            Device.BeginInvokeOnMainThread(() => lbState.Text = "NFC card discovered");
        }

        private void btnRead_Clicked(System.Object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => lbState.Text = "Reading...");
            btnRead.IsEnabled = false;
            nfcService.ReadTag();
        }

        private async void NfcService_OnNfcTagRead(object sender, NfcTagInfo e)
        {
            nfcService.StopReadingTag();
            btnRead.IsEnabled = true;
            Device.BeginInvokeOnMainThread(() => lbState.Text = "NFC card read");
            await ShowNfcMessage(e.Records.FirstOrDefault());
        }

        private async Task ShowNfcMessage(NfcNdefRecord record)
        {
            if (record == null)
                return;

            string message = $"Message: {record.Message}";
            message += Environment.NewLine;
            message += $"RawMessage: {Encoding.UTF8.GetString(record.Payload)}";
            message += Environment.NewLine;
            message += $"Type: {record.TypeFormat}";

            if (!string.IsNullOrWhiteSpace(record.MimeType))
            {
                message += Environment.NewLine;
                message += $"MimeType: {record.MimeType}";
            }

            await DisplayAlert("NFC card read", message, "Ok");
        }

        private void btnWriteText_Clicked(System.Object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => lbState.Text = "Writing text...");
            btnWriteText.IsEnabled = false;
            nfcService.WriteTagText("Hello NFC");
        }

        private void NfcService_OnNfcTagTextWriten(object sender, bool writen)
        {
            nfcService.StopWritingTagText();
            btnWriteText.IsEnabled = true;
            Device.BeginInvokeOnMainThread(() => lbState.Text = writen ? "Text writen on NFC card" : "Error writing text on NFC card");
        }

        private void btnWriteUri_Clicked(System.Object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => lbState.Text = "Writing uri...");
            btnWriteUri.IsEnabled = false;
            nfcService.WriteTagUri("https://jorgediegocrespo.wordpress.com/");
        }

        private void NfcService_OnNfcTagUriWriten(object sender, bool writen)
        {
            nfcService.StopWritingTagUri();
            btnWriteUri.IsEnabled = true;
            Device.BeginInvokeOnMainThread(() => lbState.Text = writen ? "Uri writen on NFC card" : "Error writing uri on NFC card");
        }

        private void btnWriteMime_Clicked(System.Object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => lbState.Text = "Writing mime...");
            btnWriteMime.IsEnabled = false;
            nfcService.WriteTagMime("Hello NFC");
        }

        private void NfcService_OnNfcTagMimeWriten(object sender, bool writen)
        {
            nfcService.StopWritingTagMime();
            btnWriteMime.IsEnabled = true;
            Device.BeginInvokeOnMainThread(() => lbState.Text = writen ? "MIME writen on NFC card" : "Error writing MIME on NFC card");
        }

        private void btnClean_Clicked(System.Object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => lbState.Text = "Cleaning NFC card...");
            btnClean.IsEnabled = false;
            nfcService.CleanTag();
        }

        private void NfcService_OnNfcTagCleaned(object sender, bool cleaned)
        {
            nfcService.StopCleaningTag();
            btnClean.IsEnabled = true;
            Device.BeginInvokeOnMainThread(() => lbState.Text = cleaned ? "NFC card cleaned" : "Error cleaning NFC card");
        }
    }
}
