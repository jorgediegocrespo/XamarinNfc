using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccessControl.Services;
using Xamarin.Forms;

namespace AccessControl.Features
{
    public partial class AccessControlView
    {
        private readonly INfcService nfcService;
        private readonly IEncryptionService encryptionService;
        private bool initiated = false;

        public AccessControlView()
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
            {
                nfcService.StartDiscovering();
                initiated = true;
            }
        }

        private async void NfcService_OnNfcTagDiscovered(object sender, EventArgs e)
        {
            nfcService.StopDiscovering();
            try
            {
                string data = ReadBlock(8);
                if (data == null)
                    return;
                lbNif.Text = data;

                data = ReadBlock(9);
                if (data == null)
                    return;
                lbName.Text = data;

                data = ReadBlock(10);
                if (data == null)
                    return;
                lbSurname.Text = data;

                data = ReadBlock(12);
                if (data == null)
                    return;
                int type = int.Parse(data);

                data = ReadBlock(13);
                if (data == null)
                    return;
                int tickets = int.Parse(data);

                data = ReadBlock(14);
                if (data == null)
                    return;
                DateTime tillDate = DateTime.Parse(data);

                if (IsValidAccess(type, tickets, tillDate) &&
                    UpdateCardInfo(type, tickets))
                {
                    lbStatus.TextColor = Color.Green;
                    lbStatus.Text = "Access granted";
                }
                else
                {
                    lbStatus.TextColor = Color.Red;
                    lbStatus.Text = "Access denied";
                }

                await Task.Delay(3000);
            }
            finally
            {
                lbNif.Text = string.Empty;
                lbName.Text = string.Empty;
                lbSurname.Text = string.Empty;
                lbStatus.TextColor = Color.Black;
                lbStatus.Text = "Bring your card closer to the reader to access";

                nfcService.StopByteBlockOperations();
                nfcService.StartDiscovering();
            }
        }

        private bool IsValidAccess(int type, int tickets, DateTime tillDate)
        {
            if (type == 0)
                return tickets > 0;
            else
                return tillDate > DateTime.Today;
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

        private bool UpdateCardInfo(int type, int tickets)
        {
            if (type != 0)
                return true;

            tickets--;
            byte[] cardKey = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            byte[] content = GetContent(tickets.ToString());
            byte[] encriptedContent = encryptionService.Encrypt(content);

            bool result = nfcService.WriteByteBlock(13, cardKey, encriptedContent, NfcKeyType.KeyB);
            if (!result)
                DisplayAlert("Error", "Error updating card", "Ok");

            return result;
        }

        private byte[] GetContent(string source)
        {
            List<byte> content = Encoding.Default.GetBytes(source).ToList();
            for (int i = content.Count; i < 16; i++)
                content.Add(0x00);

            return content.ToArray();
        }
    }
}
