using System.Text;
using HybridCryptographyAlgorithm.Algorithms.Original;

namespace HybridCryptographyAlgorithm.Algorithms
{
    class HybridCryptor
    {
        public readonly byte[] DesKey;
        public readonly byte[] AesKey;

        enum enCryptor
        {
            AesCryptor,
            DesCryptor
        }

        public HybridCryptor(byte[] desKey, byte[] aesKey)
        {
            AesKey = aesKey;
            DesKey = desKey;
        }

        // Blocks are joined with '.', which Base64 never contains.
        const char BlockSeparator = '.';
        const int BlockSize = 16;

        public string Encrypt(string plainText)
        {
            using AesCryptor aes = new AesCryptor(AesKey);
            DesCryptor des = new DesCryptor(DesKey);

            enCryptor CurrentCryptor = enCryptor.AesCryptor;
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            List<string> cipherBlocks = new List<string>();

            for (int i = 0; i < plainBytes.Length; i += BlockSize)
            {
                byte[] plainBlock = plainBytes[i..Math.Min(i + BlockSize, plainBytes.Length)];

                if (CurrentCryptor == enCryptor.AesCryptor)
                {
                    cipherBlocks.Add(Convert.ToBase64String(aes.Encrypt(plainBlock)));
                    CurrentCryptor = enCryptor.DesCryptor;
                }
                else
                {
                    cipherBlocks.Add(Convert.ToBase64String(des.Encrypt(plainBlock)));
                    CurrentCryptor = enCryptor.AesCryptor;
                }
            }
            return string.Join(BlockSeparator, cipherBlocks);
        }

        public string Decrypt(string cipherText)
        {
            using AesCryptor aes = new AesCryptor(AesKey);
            DesCryptor des = new DesCryptor(DesKey);

            enCryptor CurrentCryptor = enCryptor.AesCryptor;
            List<byte> plainBytes = new List<byte>();

            foreach (string block in cipherText.Trim().Split(BlockSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                byte[] cipherBlock = Convert.FromBase64String(block);

                if (CurrentCryptor == enCryptor.AesCryptor)
                {
                    plainBytes.AddRange(aes.Decrypt(cipherBlock));
                    CurrentCryptor = enCryptor.DesCryptor;
                }
                else
                {
                    plainBytes.AddRange(des.Decrypt(cipherBlock));
                    CurrentCryptor = enCryptor.AesCryptor;
                }
            }
            return Encoding.UTF8.GetString(plainBytes.ToArray());
        }
    }
}
