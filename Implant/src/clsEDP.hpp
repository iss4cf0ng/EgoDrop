#pragma once
#include <cstdint>
#include <cstring>
#include <vector>
#include <tuple>
#include <stdexcept>
#include <iostream>

class clsEDP
{
public:
    static constexpr int HEADER_SIZE = 6;

private:
    uint8_t  _nCommand = 0;
    uint8_t  _nParam   = 0;
    int32_t  _nDataLength = 0;

    std::vector<uint8_t> _abMessageData;
    std::vector<uint8_t> _abMoreData;

public:
    // Getters
    uint8_t  m_nCommand() const { return _nCommand; }
    uint8_t  m_nParam()   const { return _nParam; }
    int32_t  m_nDataLength() const { return _nDataLength; }
    const std::vector<uint8_t>& m_abMessageData() const { return _abMessageData; }
    const std::vector<uint8_t>& m_abMoreData() const { return _abMoreData; }

    // Constructor-1 (parse from raw buffer)
    clsEDP(const std::vector<uint8_t>& abBuffer)
    {
        if (abBuffer.size() < HEADER_SIZE)
            return; // invalid buffer

        // read header
        _nCommand = abBuffer[0];
        _nParam   = abBuffer[1];
        std::memcpy(&_nDataLength, &abBuffer[2], sizeof(_nDataLength));

        // extract data payload
        if (abBuffer.size() >= HEADER_SIZE + static_cast<size_t>(_nDataLength))
        {
            _abMessageData.insert(
                _abMessageData.end(),
                abBuffer.begin() + HEADER_SIZE,
                abBuffer.begin() + HEADER_SIZE + _nDataLength
            );
        }

        // extract leftover (more data)
        if (abBuffer.size() > HEADER_SIZE + static_cast<size_t>(_nDataLength))
        {
            _abMoreData.insert(
                _abMoreData.end(),
                abBuffer.begin() + HEADER_SIZE + _nDataLength,
                abBuffer.end()
            );
        }
    }

    // Constructor-2 (build message from command, param, payload)
    clsEDP(uint8_t nCmd, uint8_t nParam, const std::vector<uint8_t>& abMsg)
        : _nCommand(nCmd),
          _nParam(nParam),
          _nDataLength(static_cast<int32_t>(abMsg.size())),
          _abMessageData(abMsg)
    {
    }

    // Serialize to byte array
    std::vector<uint8_t> fnabGetBytes() const
    {
        std::vector<uint8_t> abBuffer;
        abBuffer.reserve(HEADER_SIZE + _abMessageData.size());

        // write command + param
        abBuffer.push_back(_nCommand);
        abBuffer.push_back(_nParam);

        // write 4-byte data length (little endian)
        for (int i = 0; i < 4; ++i)
            abBuffer.push_back(static_cast<uint8_t>((_nDataLength >> (8 * i)) & 0xFF));

        // write message data
        abBuffer.insert(abBuffer.end(), _abMessageData.begin(), _abMessageData.end());

        return abBuffer;
    }

    // Extract message info (equivalent to fnGetMsg)
    std::tuple<uint8_t, uint8_t, int32_t, std::vector<uint8_t>> fnGetMsg() const
    {
        return { _nCommand, _nParam, _nDataLength, _abMessageData };
    }

    // Static: get header info only
    static std::tuple<uint8_t, uint8_t, int32_t> fnGetHeader(const std::vector<uint8_t>& abBuffer)
    {
        if (abBuffer.size() < HEADER_SIZE)
            return { 0, 0, 0 };

        uint8_t nCmd  = abBuffer[0];
        uint8_t nParam = abBuffer[1];
        int32_t nLen = 0;
        std::memcpy(&nLen, &abBuffer[2], sizeof(nLen));

        return { nCmd, nParam, nLen };
    }
};
