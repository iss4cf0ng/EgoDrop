#pragma once

#include <iostream>
#include <string>

class clsTools
{
private:
    const char* RED    = "\033[31m";
    const char* GREEN  = "\033[32m";
    const char* BLUE   = "\033[34m";
    const char* RESET  = "\033[0m";

public:
    clsTools()
    {

    }
    ~clsTools()
    {

    }

    void fnLogOK(const std::string& szMsg)
    {
        std::cout << RED << "[+] " << RESET << szMsg << std::endl;
    }

    void fnLogInfo(const std::string& szMsg)
    {
        std::cout << BLUE << "[*] " << RESET << szMsg << std::endl;
    }

    void fnLogErr(const std::string& szMsg)
    {
        std::cout << RED << "[-] " << RESET << szMsg << std::endl;
    }

    std::vector<unsigned char> fnvuCombineBytes(
        const std::vector<unsigned char>& vuFirstBytes,
        const std::vector<unsigned char>& vuSecondBytes
    )
    {
        return fnvuCombineBytes(
            vuFirstBytes, 0, vuFirstBytes.size(),
            vuSecondBytes, 0, vuSecondBytes.size()
        );
    }

    std::vector<unsigned char> fnvuCombineBytes(
        const std::vector<unsigned char>& vuFirstBytes, 
        const size_t nFirstIndex,
        const size_t nFirstLength,
        const std::vector<unsigned char>& vuSecondBytes,
        const size_t nSecondIndex,
        const size_t nSecondLength
    )
    {
        std::vector<unsigned char> abBuffer(nFirstLength + nSecondLength);

        //First bytes
        std::copy(
            vuFirstBytes.begin() + nFirstIndex,
            vuFirstBytes.begin() + nFirstIndex + nFirstLength,
            std::back_inserter(abBuffer)
        );

        //Second bytes
        std::copy(
            vuSecondBytes.begin() + nSecondIndex,
            vuSecondBytes.begin() + nSecondIndex + nSecondLength,
            std::back_inserter(abBuffer)
        );

        return abBuffer;
    }
};