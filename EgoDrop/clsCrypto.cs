using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;

namespace EgoDrop
{
    public class clsCrypto
    {
        public string m_szChallenge = string.Empty;

        public int m_nRSA_KeySize = 4096;
        public int m_nAES_KeySize = 256;
        public int m_nAES_BlockSize = 128;

        public (string szPublicKey, string szPrivateKey) m_RSAKeyPair { get; set; }
        public (byte[] abPublicKey, byte[] abPrivateKey) m_abRSAKeyPair { get; set; }
        public (byte[] abKey, byte[] abIV) m_abAESKey { get; set; }
        public (string szKey, string szIV) m_szAESKey { get { return (Convert.ToBase64String(m_abAESKey.abKey), Convert.ToBase64String(m_abAESKey.abIV)); } }

        public clsCrypto(bool bCreateKey = false)
        {
            if (bCreateKey)
            {
                fnCreateKey();
            }
        }

        public ((string szPublicKey, string szPrivateKey) RSA, (string szKey, string szIV) AES) fnCreateKey()
        {
            fnAESGenerateKey();
            fnCreateRSAKey();

            return (m_RSAKeyPair, m_szAESKey);
        }

        #region RSA

        public (string szPublicKey, string szPrivateKey) fnCreateRSAKey(bool bSaveInClass = true)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = m_nRSA_KeySize;

            string szPublicKey = rsa.ToXmlString(false);
            string szPrivateKey = rsa.ToXmlString(true);

            var keyPair = (szPublicKey, szPrivateKey);
            var abKeyPair = (rsa.ExportRSAPublicKey(), rsa.ExportRSAPrivateKey());

            if (bSaveInClass)
            {
                m_RSAKeyPair = keyPair;
                m_abRSAKeyPair = abKeyPair;
            }

            rsa.Dispose();

            return keyPair;
        }

        public byte[] fnabRSAEncrypt(string szPlain, string szPublicKey) => fnabRSAEncrypt(Encoding.UTF8.GetBytes(szPlain), szPublicKey);
        public byte[] fnabRSAEncrypt(byte[] abBuffer, string szPublicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = m_nRSA_KeySize;
            rsa.FromXmlString(szPublicKey);

            byte[] abCipher = rsa.Encrypt(abBuffer, false);

            rsa.Dispose();

            return abCipher;
        }

        public byte[] fnabRSADecrypt(byte[] abBuffer, byte[] abKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = m_nRSA_KeySize;
            rsa.ImportRSAPrivateKey(abKey, out _);

            byte[] abPlain = rsa.Decrypt(abBuffer, false);

            rsa.Dispose();

            return abPlain;
        }
        public byte[] fnabRSADecrypt(string szCipher) => fnabRSADecrypt(szCipher, m_RSAKeyPair.szPrivateKey);
        public byte[] fnabRSADecrypt(byte[] abBuffer) => fnabRSADecrypt(abBuffer, m_RSAKeyPair.szPrivateKey);
        public byte[] fnabRSADecrypt(string szCipher, string szPrivateKey) => fnabRSADecrypt(Encoding.UTF8.GetBytes(szCipher), szPrivateKey);
        public byte[] fnabRSADecrypt(byte[] abBuffer, string szPrivateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = m_nRSA_KeySize;
            rsa.FromXmlString(szPrivateKey);

            byte[] abPlain = rsa.Decrypt(abBuffer, false);

            rsa.Dispose();

            return abPlain;
        }

        #endregion

        #region AES

        public (byte[] abKey, byte[] abIV) fnAESGenerateKey(bool bSaveInClass = true)
        {
            byte[] abKey = { };
            byte[] abIV = { };

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = m_nAES_KeySize;

                aes.GenerateKey();
                aes.GenerateIV();

                abKey = aes.Key;
                abIV = aes.IV;
            }

            if (bSaveInClass)
                m_abAESKey = (abKey, abIV);

            return (abKey, abIV);
        }

        public void fnAesSetNewKeyIV(byte[] abKey, byte[] abIV)
        {
            m_abAESKey = (abKey, abIV);
        }

        public string fnszAESEncrypt(string szPlain) => Convert.ToBase64String(fnabAESEncrypt(szPlain));
        public byte[] fnabAESEncrypt(string szPlain) => fnabAESEncrypt(szPlain, m_abAESKey.abKey, m_abAESKey.abIV);
        public byte[] fnabAESEncrypt(string szPlain, byte[] abKey, byte[] abIV)
        {
            byte[] abCipher = { };
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = m_nAES_KeySize;
                aes.BlockSize = m_nAES_BlockSize;
                aes.Key = abKey;
                aes.IV = abIV;

                ICryptoTransform encryptor = aes.CreateEncryptor(abKey, abIV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(szPlain);
                        }

                        abCipher = ms.ToArray();
                    }
                }
            }

            return abCipher;
        }

        public string fnszAESDecrypt(byte[] abCipher) => fnszAESDecrypt(abCipher, m_abAESKey.abKey, m_abAESKey.abIV);
        public byte[] fnabAESDecrypt(byte[] abCipher, byte[] abKey, byte[] abIV) => Encoding.UTF8.GetBytes(fnszAESDecrypt(abCipher, abKey, abIV));
        public string fnszAESDecrypt(byte[] abCipher, byte[] abKey, byte[] abIV)
        {
            string szPlain = string.Empty;
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = m_nAES_KeySize;
                aes.BlockSize = m_nAES_BlockSize;
                aes.Key = abKey;
                aes.IV = abIV;

                ICryptoTransform decryptor = aes.CreateDecryptor(abKey, abIV);
                using (MemoryStream ms = new MemoryStream(abCipher))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            szPlain = sr.ReadToEnd();
                            return szPlain;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
