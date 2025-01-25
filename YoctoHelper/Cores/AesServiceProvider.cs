using System.IO;
using System.Security.Cryptography;

namespace YoctoHelper.Cores;

public class AesServiceProvider
{

    private ICryptoTransform encryptor { get; set; }

    private ICryptoTransform decryptor { get; set; }

    public AesServiceProvider(byte[] key, byte[] iv, int keySize = 128, int blockSize = 128, CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.PKCS7)
    {
        if ((ArrayUtils.IsNullOrEmpty(key)) || (key.Length != keySize / 8))
        {
            key = AesServiceProvider.GenerateKey(keySize);
        }
        if ((ArrayUtils.IsNullOrEmpty(iv)) || (iv.Length != 16))
        {
            iv = AesServiceProvider.GenerateIv();
        }
        using AesManaged aesManaged = new AesManaged()
        {
            Key = key,
            IV = iv,
            KeySize = keySize,
            BlockSize = blockSize,
            Mode = cipherMode,
            Padding = paddingMode
        };
        this.encryptor = aesManaged.CreateEncryptor(key, iv);
        this.decryptor = aesManaged.CreateDecryptor(key, iv);
    }

    public byte[] Encrypt(byte[] data)
    {
        return this.CryptoTransform(data, this.encryptor);
    }

    public byte[] Decrypt(byte[] data)
    {
        return this.CryptoTransform(data, this.decryptor);
    }

    private byte[] CryptoTransform(byte[] data, ICryptoTransform cryptoTransform)
    {
        if (ArrayUtils.IsNullOrEmpty(data))
        {
            return new byte[0];
        }
        using MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.Close();
        return memoryStream.ToArray();
    }

    public static byte[] GenerateKey(int keySize = 128)
    {
        if ((keySize != 128) && (keySize != 192) && (keySize != 256))
        {
            keySize = 128;
        }
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = keySize;
            aes.GenerateKey();
            return aes.Key;
        }
    }

    public static byte[] GenerateIv()
    {
        byte[] iv = new byte[16];
        using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
        {
            rngCsp.GetBytes(iv);
        }
        return iv;
    }

}
