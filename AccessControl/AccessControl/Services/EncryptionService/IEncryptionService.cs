namespace AccessControl.Services
{
    public interface IEncryptionService
    {
        byte[] Encrypt(byte[] source);
        byte[] Decrypt(byte[] encodedText);
    }
}
