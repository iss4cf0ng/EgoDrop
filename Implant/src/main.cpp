#include <iostream>
#include <unistd.h>
#include <cstring>
#include <string>
#include <stdio.h>
#include <stdlib.h>

#include <arpa/inet.h>

#include <openssl/rsa.h>
#include <openssl/pem.h>
#include <openssl/aes.h>
#include <openssl/rand.h>

///

#include "clsEDP.hpp"
#include "clsInfoSpyder.hpp"

char* g_szIP = "10.98.242.96";
uint16_t g_nPort = 5000;

const int g_nREAD_LENGTH = 65535;

void fnHandler(int sktSrv)
{
    char caBuffer[g_nREAD_LENGTH] = {0};
    int nRead = recv(sktSrv, caBuffer, sizeof(caBuffer), 0);
}

int main(int argc, char *argv[])
{
    if (argc == 3)
    {
        g_szIP = argv[1];
        g_nPort = atoi(argv[2]);
    }

    int sktSrv = socket(AF_INET, SOCK_STREAM, 0);
    sockaddr_in srvAddr {};
    srvAddr.sin_family = AF_INET;
    srvAddr.sin_port = htons(g_nPort);
    inet_pton(AF_INET, g_szIP, &srvAddr.sin_addr);

    if (connect(sktSrv, (struct sockaddr *)&srvAddr, sizeof(srvAddr)) < 0)
    {
        perror("connect");
        return 1;
    }
    else
    {
        fnHandler(sktSrv);
    }

    uint32_t uiKeyLength;
}