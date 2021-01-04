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
            nfcService.Read();
        }

        private async void NfcService_OnNfcTagRead(object sender, NfcTagInfo e)
        {
            nfcService.StopReading();
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
            nfcService.WriteText("Hello NFC");
        }

        private void NfcService_OnNfcTagTextWriten(object sender, bool writen)
        {
            nfcService.StopWritingText();
            btnWriteText.IsEnabled = true;
            Device.BeginInvokeOnMainThread(() => lbState.Text = writen ? "NFC card writen" : "Error writing NFC card");
        }

        private void btnWriteCustom_Clicked(System.Object sender, System.EventArgs e)
        {
        }

        private void btnWriteUri_Clicked(System.Object sender, System.EventArgs e)
        {
        }
    }
}
