using System;

namespace AccessControl.Services
{
    public interface INfcService
    {
        void StartDiscovering();
        void Read();
        void Write();
        void Clean();
        void StopDiscovering();
        void StopReading();
        void StopWriting();
        void StopCleaning();

        event EventHandler<NfcTagInfo> OnNfcTagRead;
        event EventHandler OnNfcTagDiscovered;
    }
}
