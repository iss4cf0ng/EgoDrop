#pragma once

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
    uint32_t m_uiRSAKeyLength = 0;
    RSA* m_rsaPublic = nullptr;

public:
    std::array<unsigned char, 32> m_aesKey; // 256-bit
    std::array<unsigned char, 16> m_aesIV;  // 128-bit
    std::vector<unsigned char> m_vuRSAPublicKey;

public:
    clsCrypto() = default;

    clsCrypto(const std::vector<unsigned char>& vuRSAPublicKey, bool bCreateAES = true) {
        m_vuRSAPublicKey = vuRSAPublicKey;

        const unsigned char* p = vuRSAPublicKey.data();
        m_rsaPublic = d2i_RSAPublicKey(NULL, &p, vuRSAPublicKey.size());
        if (!m_rsaPublic)
            throw std::runtime_error("Failed to parse RSA public key.");

        if (bCreateAES)
            fnCreateAESKey();
    }

    ~clsCrypto() {
        if (m_rsaPublic)
            RSA_free(m_rsaPublic);
    }

    // ===== AES key creation =====
    std::tuple<std::array<unsigned char, 32>, std::array<unsigned char, 16>> fnCreateAESKey() {
        if (RAND_bytes(m_aesKey.data(), m_aesKey.size()) != 1)
            throw std::runtime_error("RAND_bytes AES key failed.");
        if (RAND_bytes(m_aesIV.data(), m_aesIV.size()) != 1)
            throw std::runtime_error("RAND_bytes AES IV failed.");
        return { m_aesKey, m_aesIV };
    }

    // ===== RSA encryption =====
    std::vector<unsigned char> fnvuRSAEncrypt(std::string& szPlain)
    {
        std::vector<unsigned char> vuPlain(szPlain.begin(), szPlain.end());
        return fnvuRSAEncrypt(vuPlain.data(), vuPlain.size());
    }
    std::vector<unsigned char> fnvuRSAEncrypt(const unsigned char* ucPlain, size_t nLength) {
        if (!m_rsaPublic)
            throw std::runtime_error("RSA public key not initialized.");
        int nKeySize = RSA_size(m_rsaPublic);
        std::vector<unsigned char> cipher(nKeySize);
        int ret = RSA_public_encrypt(nLength, ucPlain, cipher.data(), m_rsaPublic, RSA_PKCS1_PADDING);
        if (ret == -1)
            throw std::runtime_error("RSA encryption failed.");
        cipher.resize(ret);
        return cipher;
    }

    // ===== AES encrypt =====
    std::vector<unsigned char> fnvuAESEncrypt(const unsigned char* ucPlainText, int nPlainTextLength) {
        EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
        if (!ctx)
            throw std::runtime_error("AES encrypt: ctx error.");

        if (EVP_EncryptInit_ex(ctx, EVP_aes_256_cbc(), NULL, m_aesKey.data(), m_aesIV.data()) != 1)
            throw std::runtime_error("AES encrypt: init failed.");

        std::vector<unsigned char> cipher(nPlainTextLength + EVP_MAX_BLOCK_LENGTH);
        int nLen = 0, nCipherLen = 0;

        if (EVP_EncryptUpdate(ctx, cipher.data(), &nLen, ucPlainText, nPlainTextLength) != 1)
            throw std::runtime_error("AES encrypt: update failed.");
        nCipherLen = nLen;

        if (EVP_EncryptFinal_ex(ctx, cipher.data() + nLen, &nLen) != 1)
            throw std::runtime_error("AES encrypt: final failed.");
        nCipherLen += nLen;

        cipher.resize(nCipherLen);
        EVP_CIPHER_CTX_free(ctx);
        return cipher;
    }

    std::vector<unsigned char> fnvuAESEncrypt(const std::string& szPlain) {
        return fnvuAESEncrypt(reinterpret_cast<const unsigned char*>(szPlain.data()), szPlain.size());
    }

    // ===== AES decrypt =====
    std::vector<unsigned char> fnvuAESDecrypt(std::vector<unsigned char> vuCipher)
    {
        return fnvuAESDecrypt(vuCipher.data(), vuCipher.size());
    }
    std::vector<unsigned char> fnvuAESDecrypt(const unsigned char* ucCipherText, int nCipherTextLength) {
        EVP_CIPHER_CTX* ctx = EVP_CIPHER_CTX_new();
        if (!ctx)
            throw std::runtime_error("AES decrypt: ctx error.");

        if (EVP_DecryptInit_ex(ctx, EVP_aes_256_cbc(), NULL, m_aesKey.data(), m_aesIV.data()) != 1)
            throw std::runtime_error("AES decrypt: init failed.");

        std::vector<unsigned char> plain(nCipherTextLength);
        int nLen = 0, nPlainLen = 0;

        if (EVP_DecryptUpdate(ctx, plain.data(), &nLen, ucCipherText, nCipherTextLength) != 1)
            throw std::runtime_error("AES decrypt: update failed.");
        nPlainLen = nLen;

        if (EVP_DecryptFinal_ex(ctx, plain.data() + nLen, &nLen) != 1)
            throw std::runtime_error("AES decrypt: final failed.");
        nPlainLen += nLen;

        plain.resize(nPlainLen);
        EVP_CIPHER_CTX_free(ctx);
        return plain;
    }
};