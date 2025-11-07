#pragma once
#include <iostream>
#include <vector>
#include <cstring>
#include <stdexcept>
#include <cstdint>

class clsEDP
{
public:
    static constexpr int MAX_SIZE = 65535;
    static constexpr int HEADER_SIZE = 6;

private:
    uint8_t m_nCommand = 0;
    uint8_t m_nParam = 0;
    int32_t m_nDataLength = 0;

    std::vector<uint8_t> m_abMessageData;
    std::vector<uint8_t> m_abMoreData;

public:
    // ===== Constructors =====

    // Constructor 1: parse from buffer
    explicit clsEDP(const std::vector<uint8_t>& buffer)
    {
        if (buffer.size() < HEADER_SIZE)
            throw std::invalid_argument("Buffer too small for EDP header.");

        size_t offset = 0;
        m_nCommand = buffer[offset++];
        m_nParam   = buffer[offset++];

        // Extract 4-byte int (little endian like BitConverter in C#)
        std::memcpy(&m_nDataLength, buffer.data() + offset, sizeof(int32_t));
        offset += sizeof(int32_t);

        // Ensure valid data length
        if (m_nDataLength < 0 || m_nDataLength > static_cast<int>(MAX_SIZE))
            throw std::runtime_error("Invalid data length field.");

        // Extract payload
        if (buffer.size() - HEADER_SIZE >= static_cast<size_t>(m_nDataLength))
            m_abMessageData.assign(buffer.begin() + HEADER_SIZE,
                                   buffer.begin() + HEADER_SIZE + m_nDataLength);

        // Extract leftover data if any
        size_t remaining = buffer.size() - HEADER_SIZE - m_nDataLength;
        if (remaining > 0)
            m_abMoreData.assign(buffer.end() - remaining, buffer.end());
    }

    // Constructor 2: create packet from fields
    clsEDP(uint8_t nCmd, uint8_t nParam, const std::vector<uint8_t>& abMsg)
        : m_nCommand(nCmd), m_nParam(nParam), m_abMessageData(abMsg)
    {
        m_nDataLength = static_cast<int32_t>(abMsg.size());
    }

    // ===== Getters =====
    uint8_t fnGetCommand() const { return m_nCommand; }
    uint8_t fnGetParam()   const { return m_nParam; }
    int32_t fnGetDataLength() const { return m_nDataLength; }
    const std::vector<uint8_t>& fnGetMessageData() const { return m_abMessageData; }
    const std::vector<uint8_t>& fnGetMoreData() const { return m_abMoreData; }

    // ===== Serialize to bytes =====
    std::vector<uint8_t> fnabGetBytes()
    {
        std::vector<uint8_t> buffer;
        buffer.reserve(HEADER_SIZE + m_abMessageData.size());

        buffer.push_back(m_nCommand);
        buffer.push_back(m_nParam);

        int32_t len = m_nDataLength;
        uint8_t lenBytes[4];
        std::memcpy(lenBytes, &len, sizeof(lenBytes));
        buffer.insert(buffer.end(), lenBytes, lenBytes + 4);

        buffer.insert(buffer.end(), m_abMessageData.begin(), m_abMessageData.end());
        return buffer;
    }

    // ===== Tuple-style message getter =====
    std::tuple<uint8_t, uint8_t, int32_t, std::vector<uint8_t>> fnGetMsg()
    {
        return { m_nCommand, m_nParam, m_nDataLength, m_abMessageData };
    }

    // ===== Static: read header only =====
    static std::tuple<uint8_t, uint8_t, int32_t> fnGetHeader(const std::vector<uint8_t>& buffer)
    {
        if (buffer.size() < HEADER_SIZE)
            throw std::invalid_argument("Buffer too small for header.");

        uint8_t nCommand = buffer[0];
        uint8_t nParam   = buffer[1];
        int32_t nLength;
        std::memcpy(&nLength, buffer.data() + 2, sizeof(int32_t));

        return { nCommand, nParam, nLength };
    }
};