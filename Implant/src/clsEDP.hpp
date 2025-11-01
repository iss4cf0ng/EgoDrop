#include <cstdint>
#include <cstring>
#include <vector>
#include <tuple>
#include <stdexcept>

class clsEDP
{
public:
    static constexpr int HEADER_SIZE = 6;

private:
    uint8_t _nCommand = 0;
    uint8_t _nParam = 0;
    uint32_t _nDataLength = 0;

    std::vector<uint8_t> _vuMessageData;
    std::vector<uint8_t> _vuMoreData;

public:
    //Getters
    uint8_t gtnCommand() const { return _nCommand; }
    uint8_t gtnParam() const { return _nParam; }
    int32_t gtnDataLength() const { return _nDataLength; }

    const std::vector<uint8_t>& gtvuMessageData() const { return _vuMessageData; }
    const std::vector<uint8_t>& gtvuMoreData() const { return _vuMoreData; }

    clsEDP(const std::vector<uint8_t>& vuBuffer)
    {
        if (vuBuffer.size() < HEADER_SIZE)
            return;

        try
        {
            _nCommand = vuBuffer[0];
            _nParam = vuBuffer[1];
            std::memcpy(&_nDataLength, &vuBuffer[2], sizeof(_nDataLength));

            if (vuBuffer.size() - HEADER_SIZE >= static_cast<size_t>(_nDataLength))
                _vuMessageData.assign(vuBuffer.begin() + HEADER_SIZE, vuBuffer.begin() + HEADER_SIZE + _nDataLength);

            if (vuBuffer.size() > HEADER_SIZE + _nDataLength)
                _vuMoreData.assign(vuBuffer.begin() + HEADER_SIZE + _nDataLength, vuBuffer.end());
        }
        catch(const std::exception& e)
        {
            
        }
        
    }

    clsEDP(uint8_t nCommand, uint8_t nParam, const std::vector<uint8_t>& vuMsg)
    : _nCommand(nCommand), _nParam(nParam), _vuMessageData(vuMsg), _nDataLength(static_cast<int32_t>(vuMsg.size()))
    {

    }

    std::vector<uint8_t> fnuvGetBytes() const
    {
        try
        {
            std::vector<uint8_t> vuBuffer;
            vuBuffer.reserve(HEADER_SIZE + _vuMessageData.size());

            vuBuffer.push_back(_nCommand);
            vuBuffer.push_back(_nParam);

            //Append _nDataLength as 4 bytes (little-endian).
            for (int i = 0; i < 4; ++i)
                vuBuffer.push_back(static_cast<uint8_t>((_nDataLength >> (8 * i)) & 0xFF));

            vuBuffer.insert(vuBuffer.end(), _vuMessageData.begin(), _vuMessageData.end());

            return vuBuffer;
        }
        catch(const std::exception& e)
        {
            return { };
        }
    }

    std::tuple<uint8_t, uint8_t, int32_t, std::vector<uint8_t>> fnGetMsg() const
    {
        return {
            _nCommand,
            _nParam,
            static_cast<int32_t>(_vuMessageData.size()),
            _vuMessageData
        };
    }

    static std::tuple<uint8_t, uint8_t, int32_t> fnGetHeader(const std::vector<uint8_t>& vuBuffer)
    {
        if (vuBuffer.size() < HEADER_SIZE)
            return { 0, 0, 0 };

        uint8_t nCmd = vuBuffer[0];
        uint8_t nParam = vuBuffer[1];
        uint8_t nLen = 0;

        std::memcpy(&nLen, &vuBuffer[2], sizeof(nLen));

        return { nCmd, nParam, nLen };
    }

};