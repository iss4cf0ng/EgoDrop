#pragma once

#include <arpa/inet.h>
#include <errno.h>
#include <fcntl.h>
#include <netinet/in.h>
#include <string.h>
#include <sys/select.h>
#include <sys/socket.h>
#include <unistd.h>
#include <vector>
#include <atomic>
#include <memory>
#include <thread>
#include <chrono>

#include "clsHostScan.hpp"
#include "clsThreadPool.hpp"
#include "clsVictim.hpp"
#include "clsEZData.hpp"
#include "clsTools.hpp"

class clsTcpConnScan : public clsHostScan
{
private:
    std::shared_ptr<clsVictim> m_victim;

public:
    enum enTcpPingResult
    {
        ALIVE,
        TIMEOUT,
        UNREACHABLE,
        ERROR,
    };

public:
    clsTcpConnScan(std::shared_ptr<clsVictim> victim)
    {
        m_victim = victim;
    }

    void fnStart(std::vector<std::string>& lsIP, std::vector<int>& lnPort) override
    {

    }

    void fnStop() override
    {

    }

    enTcpPingResult fnTcpConnectPing(
        const std::string& szIP,
        int nPort,
        int nTimeout_ms,
        int& nLatency_ms
    )
    {
        nLatency_ms = -1;
        int nSock = socket(AF_INET, SOCK_STREAM, 0);
        if (nSock < 0)
            return enTcpPingResult::ERROR;

        int nFlags = fcntl(nSock, F_GETFL, 0);
        fcntl(nSock, F_SETFL, nFlags | O_NONBLOCK);

        sockaddr_in addr {};
        addr.sin_family = AF_INET;
        addr.sin_port = htons(nPort);

        if (inet_pton(AF_INET, szIP.c_str(), &addr.sin_addr) != 1)
        {
            close(nSock);
            return enTcpPingResult::ERROR;
        }

        auto start = std::chrono::steady_clock::now();
        int nRet = connect(nSock, (sockaddr *)&addr, sizeof(addr));

        if (nRet == 0)
        {
            auto end = std::chrono::steady_clock::now();
            nLatency_ms = std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count();
            close(nSock);

            return enTcpPingResult::ALIVE;
        }

        if (errno != EINPROGRESS) {
            // immediate failure
            if (errno == ECONNREFUSED) {
                close(nSock);
                return enTcpPingResult::ALIVE;
            }
            if (errno == ENETUNREACH || errno == EHOSTUNREACH) {
                close(nSock);
                return enTcpPingResult::UNREACHABLE;
            }

            close(nSock);

            return enTcpPingResult::ERROR;
        }

        fd_set wfds;
        FD_ZERO(&wfds);
        FD_SET(nSock, &wfds);

        timeval tv {};
        tv.tv_sec = nTimeout_ms / 1000;
        tv.tv_usec = (nTimeout_ms % 1000) * 1000;

        nRet = select(nSock + 1, nullptr, &wfds, nullptr, &tv);

        if (nRet == 0)
        {
            close(nSock);
            return enTcpPingResult::TIMEOUT;
        }

        if (nRet < 0)
        {
            close(nSock);
            return enTcpPingResult::ERROR;
        }

        int so_error = 0;
        socklen_t len = sizeof(so_error);
        getsockopt(nSock, SOL_SOCKET, so_error, &so_error, &len);

        auto end = std::chrono::steady_clock::now();
        nLatency_ms = std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count();

        close(nSock);

        if (so_error == 0 || so_error == ECONNREFUSED)
            return enTcpPingResult::ALIVE;

        if (so_error == ETIMEDOUT)
            return enTcpPingResult::TIMEOUT;

        if (so_error == ENETUNREACH || so_error == EHOSTUNREACH)
            return enTcpPingResult::UNREACHABLE;

        return enTcpPingResult::ERROR;
    }
};