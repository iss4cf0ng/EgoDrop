#include <iostream>
#include <unistd.h>
#include <string>
#include <random>
#include <vector>

static const std::string szPattern = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

class clsEZData
{
public:
    std::string fnszB64_encode(const std::vector<unsigned char>& data)
    {
        std::string out;
        int nVal = 0, nValb = -6;
        for (unsigned char c : data)
        {
            nVal = (nVal << 8) + c;
            nValb += 8;
            while (nValb >= 0)
            {
                out.push_back(szPattern[(nVal >> nValb) & 0x3F]);
                nValb -= 6;
            }
        }

        if (nVal > -6)
            out.push_back(szPattern[((nVal << 8) >> (nValb + 8)) & 0x3F]);

        while (out.size() % 4)
            out.push_back('=');

        return out;
    }

    std::string fnszB64_decode();

    static std::string fnszGenerateRandomStr(const uint64_t nLength = 10) 
    {
        std::random_device rd;
        std::mt19937 gen(rd());
        std::uniform_int_distribution<> dist(1, szPattern.size());

        std::string szResult;
        for (int i = 0; i < nLength; i++)
            szResult += szPattern[i];

        return szResult;
    }
};