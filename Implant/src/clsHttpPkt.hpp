#pragma once

#include <iostream>
#include <sstream>
#include <vector>
#include <string>

#include "clsTools.hpp"
#include "clsEZData.hpp"

class clsHttpPkt
{
public:
    enum enMethod
    {
        GET,
        POST,
        HEAD,
        PUT,
        DELETE,
    };
private:
    enMethod m_method;
    std::string m_szPath;
    std::string m_szHost;
    std::string m_szContentType;
    std::string m_szUA;

private:
    std::string fnEnumToString(enMethod method)
    {
        switch (method)
        {
            case enMethod::GET: return "GET";
            case enMethod::POST: return "POST";
            case enMethod::HEAD: return "HEAD";
            case enMethod::PUT: return "PUT";
            case enMethod::DELETE: return "DELETE";
            default: return "UNKNOWN";
        }
    }

public:
    struct stHttpResponse {
        std::string szHeader;
        std::string szBody;
    };

public:
    clsHttpPkt()
    {

    }

    clsHttpPkt(
        enMethod method,
        std::string& szPath,
        std::string& szHost,
        std::string& szContentType,
        std::string& szUA
    )
    {
        m_method = method;
        m_szPath = szPath;
        m_szHost = szHost;
        m_szContentType = szContentType;
        m_szUA = szUA;
    }

    ~clsHttpPkt() = default;

    BUFFER fnGetPacket(std::string& szBody)
    {
        return fnGetPacket(
            m_method,
            m_szPath,
            m_szHost,
            m_szContentType,
            m_szUA,
            szBody
        );
    }
    BUFFER fnGetPacket(
        enMethod method,
        const std::string& szPath,
        const std::string& szHost,
        const std::string& szContentType,
        const std::string& szUA,
        const std::string& szBody
    )
    {
        std::string szMethod = fnEnumToString(method);

        std::string szRequest = szMethod + " " + szPath + " HTTP/1.1\r\n";
        szRequest += "Host: " + szHost + "\r\n";
        szRequest += "User-Agent: " + szUA + "\r\n";
        szRequest += "Connection: keep-alive\r\n";

        if (method == enMethod::POST || method == enMethod::PUT)
        {
            szRequest += "Content-Length: " + std::to_string(szBody.size()) + "\r\n";
            szRequest += "Content-Type: " + szContentType + "\r\n";
            szRequest += "\r\n";
            
            szRequest += szBody;
        }
        else
        {
            szRequest += "\r\n";
        }

        BUFFER abBuffer = clsEZData::fnStringToBuffer(szRequest);

        return abBuffer;
    }

    static stHttpResponse fnParseHttpResponse(const std::string& response) {
        stHttpResponse result;

        const std::string delimiter = "\r\n\r\n";
        size_t pos = response.find(delimiter);

        if (pos != std::string::npos) {
            result.szHeader = response.substr(0, pos);
            result.szBody   = response.substr(pos + delimiter.size());
        } else {
            result.szHeader = response;
            result.szBody = "";
        }

        return result;
    }
};