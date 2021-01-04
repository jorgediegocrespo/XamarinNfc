using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace AccessControl.Features
{
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
        }

        private async void NfcTests_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new NfcTestsView());
        }

        private async void AccessControl_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new AccessControlView());
        }
    }
}
