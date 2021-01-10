using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AccessControl.Services;
using Xamarin.Forms;

namespace AccessControl.Features
{
    public partial class CardManagementView
    {
        private readonly INfcService nfcService;
        private readonly IEncryptionService encryptionService;
        private bool isReading;

        public CardManagementView()
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

        private void pkType_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            enTickets.IsVisible = pkType.SelectedIndex == 0;
            dpEndDate.IsVisible = pkType.SelectedIndex == 1;
        }

        private void btnSave_Clicked(System.Object sender, System.EventArgs e)
        {
            isReading = false;
            nfcService.StartDiscovering();
        }

        private void btnReadInfo_Clicked(System.Object sender, System.EventArgs e)
        {
            isReading = true;
            nfcService.StartDiscovering();
        }

        private void NfcService_OnNfcTagDiscovered(object sender, EventArgs e)
        {
            nfcService.StopDiscovering();
            if (isReading)
                Read();
            else
                Save();
        }

        private void Read()
        {
            try
            {
                string data = ReadBlock(8);
                if (data == null)
                    return;
                enNif.Text = data;

                data = ReadBlock(9);
                if (data == null)
                    return;
                enName.Text = data;

                data = ReadBlock(10);
                if (data == null)
                    return;
                enSurname.Text = data;

                data = ReadBlock(12);
                if (data == null)
                    return;
                pkType.SelectedIndex = int.Parse(data);

                data = ReadBlock(13);
                if (data == null)
                    return;
                enTickets.Text = data;

                data = ReadBlock(14);
                if (data == null)
                    return;
                dpEndDate.Date = DateTime.Parse(data);

                DisplayAlert("Done", "Card configuration read", "Ok");
            }
            finally
            {
                nfcService.StopByteBlockOperations();
            }
        }

        private void Save()
        {
            try
            {
                if (!WriteBlock(8, enNif.Text))
                    return;
                if (!WriteBlock(9, enName.Text))
                    return;
                if (!WriteBlock(10, enSurname.Text))
                    return;
                if (!WriteBlock(12, pkType.SelectedIndex.ToString()))
                    return;
                if (!WriteBlock(13, enTickets.Text))
                    return;
                if (!WriteBlock(14, dpEndDate.Date.ToString("d")))
                    return;

                DisplayAlert("Done", "Card configured", "Ok");
            }
            finally
            {
                nfcService.StopByteBlockOperations();
            }
        }

        private bool WriteBlock(int block, string value)
        {
            byte[] cardKey = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            byte[] content = GetContent(value);
            byte[] encriptedContent = encryptionService.Encrypt(content);

            bool result = nfcService.WriteByteBlock(block, cardKey, encriptedContent, NfcKeyType.KeyB);
            if (!result)
                DisplayAlert("Error", "Error writing", "Ok");

            return result;
        }

        private byte[] GetContent(string source)
        {
            List<byte> content = Encoding.Default.GetBytes(source).ToList();
            for (int i = content.Count; i < 16; i++)
                content.Add(0x00);

            return content.ToArray();
        }

        private string ReadBlock(int block)
        {
            byte[] cardKey = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            byte[] encryptedResult = nfcService.ReadByteBlock(block, cardKey, NfcKeyType.KeyB);
            byte[] result = encryptionService.Decrypt(encryptedResult);

            if (result == null)
            {
                DisplayAlert("Error", "Error reading", "Ok");
                return null;
            }

            return Encoding.Default.GetString(result);
        }
    }
}
