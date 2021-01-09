namespace AccessControl.Features
{
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
        }

        private async void btnNfcTests_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new NfcTestsView());
        }

        private async void btnNfcBlockTests_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new NfcBlockTestsView());
        }

        private async void btnAccessControl_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new AccessControlView());
        }
    }
}
