using System;

namespace AccessControl.Services
{
    public interface INfcService
    {
        void StartDiscovering();
        void Read();
        void WriteText(string text);
        void Clean();
        void StopDiscovering();
        void StopReading();
        void StopWritingText();
        void StopCleaning();

        event EventHandler OnNfcTagDiscovered;
        event EventHandler<NfcTagInfo> OnNfcTagRead;
        event EventHandler<bool> OnNfcTagTextWriten;
    }
}
