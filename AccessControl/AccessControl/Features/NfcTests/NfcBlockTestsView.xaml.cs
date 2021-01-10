using System;
using AccessControl.Services;
using Xamarin.Forms;

namespace AccessControl.Features
{
    public partial class NfcBlockTestsView
    {
        private readonly INfcService nfcService;
        private readonly IEncryptionService encryptionService;
        private bool initiated = false;

        public NfcBlockTestsView()
        {
            nfcService = DependencyService.Get<INfcService>();
            encryptionService = DependencyService.Get<IEncryptionService>();

            nfcService.OnNfcTagDiscovered += NfcService_OnNfcTagDiscovered;

            NavigationPage.SetHasBackButton(this, false);
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            nfcService.OnNfcTagDiscovered -= NfcService_OnNfcTagDiscovered;
            return base.OnBackButtonPressed();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!initiated)
                nfcService.StartDiscovering();
        }

        private void NfcService_OnNfcTagDiscovered(object sender, System.EventArgs e)
        {
            nfcService.StopDiscovering();
            Device.BeginInvokeOnMainThread(() => lbState.Text = "NFC card discovered");
        }

        private void btnReadBlock_Clicked(System.Object sender, System.EventArgs e)
        {
            byte[] cardKey = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            byte[] encryptedResult = nfcService.ReadByteBlock(16, cardKey, NfcKeyType.KeyB );

            byte[] result = encryptionService.Decrypt(encryptedResult);
            Device.BeginInvokeOnMainThread(() => lbState.Text = result == null ? "Error reading NFC block" : $"NFC block read {BitConverter.ToString(result)}");
        }

        private void btnWriteBlock_Clicked(System.Object sender, System.EventArgs e)
        {
            byte[] cardKey = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] content = new byte[] { 0x4A, 0x6F, 0x72, 0x67, 0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; //Jorge
            byte[] encriptedContent = encryptionService.Encrypt(content);

            bool result = nfcService.WriteByteBlock(16, cardKey, encriptedContent, NfcKeyType.KeyB);
            Device.BeginInvokeOnMainThread(() => lbState.Text = result ? "NFC block writen" : "Error writing NFC block");
        }

        private void btnResetBlock_Clicked(System.Object sender, System.EventArgs e)
        {
            byte[] cardKey = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            byte[] content = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] encriptedContent = encryptionService.Encrypt(content);

            bool result = nfcService.WriteByteBlock(16, cardKey, encriptedContent, NfcKeyType.KeyB);
            Device.BeginInvokeOnMainThread(() => lbState.Text = result ? "NFC block writen" : "Error writing NFC block");
        }

        private void btnStop_Clicked(System.Object sender, System.EventArgs e)
        {
            nfcService.StopByteBlockOperations();
            Device.BeginInvokeOnMainThread(() => lbState.Text = "Waiting...");
        }
    }
}
