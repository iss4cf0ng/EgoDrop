#include <vector>
#include <unistd.h>
#include <iostream>

#include <openssl/rsa.h>
#include <openssl/pem.h>
#include <openssl/aes.h>
#include <openssl/rand.h>

class clsCrypto
{
private:
    uint32_t m_uiRSAKeyLength;
    RSA* m_rsaPublic;

public:
    unsigned char m_aesKey[32]; //256 bits
    unsigned char m_aesIV[16]; //128 bits
    std::vector<unsigned char> m_vuRSAPublicKey;

public:
    clsCrypto()
    {

    }
    clsCrypto(uint32_t uiRSAKeyLength, std::vector<unsigned char> vuRSAPublicKey, bool bCreateAES = true)
    {
        m_uiRSAKeyLength = uiRSAKeyLength;
        m_vuRSAPublicKey = vuRSAPublicKey;

        const unsigned char* p = vuRSAPublicKey.data();
        m_rsaPublic = d2i_RSAPublicKey(NULL, &p, uiRSAKeyLength);

        if (bCreateAES)
            fnCreateAESKey(true);
    }

    ~clsCrypto()
    {

    }

    void fnCreateAESKey(bool bSaveInClass = true)
    {
        RAND_bytes(m_aesKey, sizeof(m_aesKey));
        RAND_bytes(m_aesIV, sizeof(m_aesIV));
    }

    std::vector<unsigned char> fnvuRSAEncrypt(RSA* rsaPublic, const unsigned char* ucInput, size_t length)
    {
        int nKeySize = RSA_size(rsaPublic);
        std::vector<unsigned char> cipher(nKeySize);

        int ret = RSA_public_encrypt(length, ucInput, cipher.data(), rsaPublic, RSA_PKCS1_PADDING);
        if (ret == -1)
            throw std::runtime_error("RSA encryption failed.");

        cipher.resize(ret);

        return cipher;
    }

    std::vector<unsigned char> fnvuRSADecrypt(RSA* rsaPrivate, const unsigned char* ucInput, size_t length)
    {
        int nKeySize = RSA_size(rsaPrivate);
        std::vector<unsigned char> plain(nKeySize);

        int ret = RSA_private_decrypt(length, ucInput, plain.data(), rsaPrivate, RSA_PKCS1_PADDING);
        if (ret == -1)
            throw std::runtime_error("RSA decryption failed.");

        plain.resize(ret);

        return plain;
    }

    std::vector<unsigned char> fnvuAESEncrypt(const unsigned char* ucPlainText, int nPlainTextLength)
    {
        EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
        if (!ctx)
            throw std::runtime_error("AES encrypt: ctx error.");

        if (EVP_EncryptInit_ex(ctx, EVP_aes_256_cbc(), NULL, m_aesKey, m_aesIV) != 1)
            throw std::runtime_error("AES encrypt: init failed.");

        std::vector<unsigned char> cipher(nPlainTextLength + EVP_MAX_BLOCK_LENGTH);
        
        int nLength, nCipherLength = 0;
        if (EVP_EncryptUpdate(ctx, cipher.data(), &nLength, ucPlainText, nPlainTextLength) != 1)
            throw std::runtime_error("AES encrypt: update failed.");
        
        nCipherLength = nLength;

        if (EVP_EncryptFinal_ex(ctx, cipher.data() + nLength, &nLength) != 1)
            throw std::runtime_error("AES encrypt: final failed.");

        nCipherLength += nLength;

        cipher.resize(nCipherLength);
        EVP_CIPHER_CTX_free(ctx);

        return cipher;
    }

    std::vector<unsigned char> fnvuAESDecrypt(const unsigned char* ucCipherText, int nCipherTextLength)
    {
        EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
        if (!ctx)
            throw std::runtime_error("AES decrypt: ctx error.");

        if (EVP_DecryptInit_ex(ctx, EVP_aes_256_cbc(), NULL, m_aesKey, m_aesIV) != 1)
            throw std::runtime_error("AES decrypt: init failed.");

        std::vector<unsigned char> vuPlainText(nCipherTextLength);

        int nLength, nPlainTextLength = 0;
        if (EVP_DecryptUpdate(ctx, vuPlainText.data(), &nLength, ucCipherText, nCipherTextLength) != 1)
            throw std::runtime_error("AES decrypt: update failed.");

        nPlainTextLength = nLength;

        if (EVP_DecryptFinal_ex(ctx, vuPlainText.data() + nLength, &nLength) != 1)
            throw std::runtime_error("AES decrypt: final failed.");
        
        nPlainTextLength += nLength;

        vuPlainText.resize(nPlainTextLength);
        EVP_CIPHER_CTX_free(ctx);

        return vuPlainText;
    }
};