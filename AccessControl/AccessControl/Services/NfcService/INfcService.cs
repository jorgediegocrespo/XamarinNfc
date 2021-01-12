using System;

namespace AccessControl.Services
{
    public interface INfcService
    {
        void StartDiscovering(); //TODO 1. DISCOVERING CARD 1
        void ReadTag(); //TODO 2. READING CARD 1
        void WriteTagText(string text); //TODO 3. WRITING CARD 1
        void WriteTagUri(string uri);
        void WriteTagMime(string mime);
        void CleanTag();

        void StopDiscovering();


        void StopReadingTag(); //TODO 2. READING CARD 4
        void StopWritingTagText(); //TODO 3. WRITING CARD 3
        void StopWritingTagUri();
        void StopWritingTagMime();
        void StopCleaningTag();

        //TODO 4. MIFARE OPERATIONS 1
        byte[] ReadByteBlock(int block, byte[] key, NfcKeyType nfcKeyType);
        bool WriteByteBlock(int block, byte[] key, byte[] content, NfcKeyType nfcKeyType);
        void StopByteBlockOperations();

        event EventHandler OnNfcTagDiscovered; //TODO 1. DISCOVERING CARD 2
        event EventHandler<NfcTagInfo> OnNfcTagRead; //TODO 2. READING CARD 3
        event EventHandler<bool> OnNfcTagTextWriten; //TODO 3. WRITING CARD 2
        event EventHandler<bool> OnNfcTagUriWriten;
        event EventHandler<bool> OnNfcTagMimeWriten;
        event EventHandler<bool> OnNfcTagCleaned;
    }
}
