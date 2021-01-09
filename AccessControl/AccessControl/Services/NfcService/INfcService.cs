using System;

namespace AccessControl.Services
{
    public interface INfcService
    {
        void StartDiscovering();
        void ReadTag();
        void WriteTagText(string text);
        void WriteTagUri(string uri);
        void WriteTagMime(string mime);
        void CleanTag();

        void StopDiscovering();
        void StopReadingTag();
        void StopWritingTagText();
        void StopWritingTagUri();
        void StopWritingTagMime();
        void StopCleaningTag();

        byte[] ReadByteBlock(int block, byte[] key, NfcKeyType nfcKeyType);
        bool WriteByteBlock(int block, byte[] key, byte[] content, NfcKeyType nfcKeyType);
        void StopByteBlockOperations();

        event EventHandler OnNfcTagDiscovered;
        event EventHandler<NfcTagInfo> OnNfcTagRead;
        event EventHandler<bool> OnNfcTagTextWriten;
        event EventHandler<bool> OnNfcTagUriWriten;
        event EventHandler<bool> OnNfcTagMimeWriten;
        event EventHandler<bool> OnNfcTagCleaned;
    }
}
