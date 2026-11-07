using System.Security.Cryptography;
using System.Text;

namespace HybridCryptographyAlgorithm.Algorithms.Original
{
    class AesCryptor : IDisposable
    {
        public readonly byte[] Key;
        public readonly byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80, 10, 20, 30, 40, 50, 60, 70, 80 };
        private readonly Aes _AESCryptor;
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;

        public AesCryptor(byte[] key)
        {
            if (key.Length != 16)
            {
                throw new ArgumentException("Invalid key length. Key must be 128 bits.");
            }

            Key = key;

            _AESCryptor = Aes.Create();
            _AESCryptor.BlockSize = 128; // Default block size
            _AESCryptor.KeySize = 128; // Can be changed based on key size
            _AESCryptor.Key = Key;
            _AESCryptor.IV = IV;
            _AESCryptor.Padding = PaddingMode.PKCS7;
            _AESCryptor.Mode = CipherMode.CBC;

            _encryptor = _AESCryptor.CreateEncryptor();
            _decryptor = _AESCryptor.CreateDecryptor();
        }

        public byte[] Encrypt(byte[] PlainBlock)
        {
            byte[] CipherBlock = _encryptor.TransformFinalBlock(PlainBlock, 0, PlainBlock.Length);
            return CipherBlock;
        }

        public byte[] Decrypt(byte[] CipherBlock)
        {
            byte[] PlainBlock = _decryptor.TransformFinalBlock(CipherBlock, 0, CipherBlock.Length);
            return PlainBlock;
        }

        public void Dispose()
        {
            _encryptor?.Dispose();
            _decryptor?.Dispose();
            _AESCryptor?.Dispose();
        }
    }
}
