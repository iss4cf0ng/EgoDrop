#pragma once

#include <iostream>
#include <unistd.h>
#include <string>
#include <random>
#include <vector>
#include <iterator>
#include <algorithm>
#include <sstream>

#include "clsTools.hpp"

static const std::string szPattern = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

class clsEZData
{
public:
    static const std::string fnStrToUpper(std::string& szInput)
    {
        std::transform(szInput.begin(), szInput.end(), szInput.begin(), [](unsigned char c) { return std::toupper(c); });
        return szInput;
    }
    static const std::string fnStrToLower(std::string& szInput)
    {
        std::transform(szInput.begin(), szInput.end(), szInput.begin(), [](unsigned char c) { return std::tolower(c); });
        return szInput;
    }

    static BUFFER fnStringToBuffer(const std::string& szData)
    {
        BUFFER buffer(szData.begin(), szData.end());
        return buffer;
    }

    // ======== Base64 Decode ========
    static std::vector<unsigned char> fnb64Decode(const std::string& input) {
        static const int DECODE_TABLE[256] = {
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 62,-1,-1,-1, 63,
            52,53,54,55,56,57,58,59,60,61,-1,-1,-1,-1,-1,-1,
            -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11,12,13,14,
            15,16,17,18,19,20,21,22,23,24,25,-1,-1,-1,-1,-1,
            -1,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,
            41,42,43,44,45,46,47,48,49,50,51,-1,-1,-1,-1,-1,
            // rest -1
        };
        std::vector<unsigned char> out;
        int val = 0, valb = -8;
        for (unsigned char c : input) {
            if (c == '=' || c == '\r' || c == '\n' || c == ' ')
                continue;
            int d = DECODE_TABLE[c];
            if (d == -1)
                throw std::invalid_argument("Invalid Base64 character");
            val = (val << 6) + d;
            valb += 6;
            if (valb >= 0) {
                out.push_back(static_cast<unsigned char>((val >> valb) & 0xFF));
                valb -= 8;
            }
        }
        return out;
    }

    // ======== Base64 Encode ========
    static std::string fnb64Encode(const std::vector<unsigned char>& data) {
        static const char ENC_TABLE[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        std::string out;
        int val = 0, valb = -6;
        for (unsigned char c : data) {
            val = (val << 8) + c;
            valb += 8;
            while (valb >= 0) {
                out.push_back(ENC_TABLE[(val >> valb) & 0x3F]);
                valb -= 6;
            }
        }
        if (valb > -6)
            out.push_back(ENC_TABLE[((val << 8) >> (valb + 8)) & 0x3F]);
        while (out.size() % 4)
            out.push_back('=');
        return out;
    }

    // ======== UTF-8 convenience wrappers ========
    static std::string fnb64DecodeUtf8(const std::vector<unsigned char>& vuMsg) {
        return fnb64DecodeUtf8(std::string(vuMsg.begin(), vuMsg.end()));
    }
    static std::string fnb64DecodeUtf8(const std::string& input) {
        std::vector<unsigned char> bytes = fnb64Decode(input);
        return std::string(bytes.begin(), bytes.end());
    }
    static std::string fnb64EncodeUtf8(const std::vector<unsigned char>& vuMsg) {
        return fnb64EncodeUtf8(std::string(vuMsg.begin(), vuMsg.end()));
    }
    static std::string fnb64EncodeUtf8(const std::string& utf8Text) {
        std::vector<unsigned char> bytes(utf8Text.begin(), utf8Text.end());
        return fnb64Encode(bytes);
    }

    // ======== Random string ========
    static std::string fnszGenerateRandomStr(const uint64_t nLength = 10) {
        std::random_device rd;
        std::mt19937 gen(rd());
        std::uniform_int_distribution<> dist(0, szPattern.size() - 1);
        std::string szResult;
        for (uint64_t i = 0; i < nLength; i++)
            szResult += szPattern[dist(gen)];
        return szResult;
    }

    static std::vector<std::string> fnvsSplit(const std::string& szInput, char cSplitter = '|')
    {
        std::vector<std::string> tokens;
        std::istringstream iss(szInput);
        std::string token;

        while (std::getline(iss, token, cSplitter))
            tokens.push_back(token);

        return tokens;
    }

    static std::string fnszJoin(const std::vector<std::string>& elements, const std::string& szSplitter = "|")
    {
        std::ostringstream oss;
        for (size_t i = 0; i < elements.size(); ++i)
        {
            if (i > 0)
                oss << szSplitter;
            
            oss << elements[i];
        }

        return oss.str();
    }

    static std::vector<std::string> fnvsB64ToVectorStringParser(const std::string& szInput)
    {
        std::vector<std::string> parts = fnvsSplit(szInput);
        std::vector<std::string> decoded;
        std::transform(parts.begin(), parts.end(), std::back_inserter(decoded), [](const std::string& s) { return fnb64DecodeUtf8(s); });

        return decoded;
    }
    
    static std::string fnszSendParser(const std::vector<std::string>& vsMsg, const std::string szSplitter = "|")
    {
        std::vector<std::string> encoded;
        std::transform(vsMsg.begin(), vsMsg.end(), std::back_inserter(encoded), [](const std::string& s) { return fnb64EncodeUtf8(s); });

        std::string szJoin = fnszJoin(encoded, szSplitter);

        return szJoin;
    }

    static std::string fnszSend2dParser(const std::vector<std::vector<std::string>>& vsMsg, const std::string& szSplitter = "|")
    {
        std::vector<std::string> x;
        for (auto& i : vsMsg)
        {
            std::string s = fnszSendParser(i, szSplitter);
            x.push_back(s);   
        }
        
        std::string ret = fnszSendParser(x, szSplitter);

        return ret;
    }
};