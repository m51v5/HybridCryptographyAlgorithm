using System.IO;
using System.Security.Cryptography;
using System.Text;

public class DesCryptor
{
    public readonly byte[] sKey;

    public DesCryptor(byte[] sKey)
    {
        this.sKey = sKey;
    }

    public byte[] Encrypt(byte[] str)
    {
        DES des = DES.Create();
        byte[] inputByteArray = str;
        des.Key = sKey;
        byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };
        des.IV = IV;
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(),
       CryptoStreamMode.Write);
        cs.Write(inputByteArray, 0, inputByteArray.Length);
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    public byte[] Decrypt(byte[] pToDecrypt)
    {
        DES des = DES.Create();
        byte[] inputByteArray = pToDecrypt;
        des.Key = sKey;
        byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };
        des.IV = IV;
        MemoryStream ms = new MemoryStream();
        CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(),
       CryptoStreamMode.Write);
        cs.Write(inputByteArray, 0, inputByteArray.Length);
        cs.FlushFinalBlock();
        return ms.ToArray();
    }
}
