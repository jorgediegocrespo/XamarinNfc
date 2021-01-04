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
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            nfcService.OnNfcTagDiscovered += NfcService_OnNfcTagDiscovered;
            nfcService.OnNfcTagRead += NfcService_OnNfcTagRead;

            nfcService.StartDiscovering();
            lbState.Text = "Discovering...";
        }

        protected override void OnDisappearing()
        {
            nfcService.OnNfcTagDiscovered -= NfcService_OnNfcTagDiscovered;
            nfcService.OnNfcTagRead -= NfcService_OnNfcTagRead;

            nfcService.StopDiscovering();
            lbState.Text = "Waiting...";

            base.OnDisappearing();            
        }

        private void NfcService_OnNfcTagDiscovered(object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => lbState.Text = "NFC card discovered");
            btnRead.IsEnabled = true;
        }

        private void NfcService_OnNfcTagRead(object sender, NfcTagInfo e)
        {
            Device.BeginInvokeOnMainThread(() => lbState.Text = "NFC card read");
        }

        private void btnRead_Clicked(System.Object sender, System.EventArgs e)
        {
            nfcService.Read();
        }
    }
}
