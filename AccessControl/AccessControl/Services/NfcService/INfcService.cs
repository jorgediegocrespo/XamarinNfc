using System;

namespace AccessControl.Services
{
    public interface INfcService
    {
        void StartDiscovering();
        void Read();
        void WriteText(string text);
        void WriteUri(string uri);
        void WriteMime(string mime);
        void Clean();
        void StopDiscovering();
        void StopReading();
        void StopWritingText();
        void StopWritingUri();
        void StopWritingMime();
        void StopCleaning();

        event EventHandler OnNfcTagDiscovered;
        event EventHandler<NfcTagInfo> OnNfcTagRead;
        event EventHandler<bool> OnNfcTagTextWriten;
        event EventHandler<bool> OnNfcTagUriWriten;
        event EventHandler<bool> OnNfcTagMimeWriten;
    }
}
