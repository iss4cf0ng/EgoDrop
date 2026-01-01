#pragma once

#include <unistd.h>
#include <dlfcn.h>

class clsFdGuard
{
private:
    int m_fd;

public:
    explicit clsFdGuard(int fd = -1) : m_fd(fd)
    {

    }

    ~clsFdGuard()
    {
        if (m_fd >= 0)
        {
            close(m_fd);
        }
    }

    int get() const
    {
        return m_fd;
    }

    int release()
    {
        int t = m_fd;
        m_fd = -1;

        return t;
    }
};

class clsDlGuard
{
private:
    void* m_h;

public:
    explicit clsDlGuard(void* h = nullptr) : m_h(h)
    {

    }

    ~clsDlGuard()
    {
        if (m_h)
        {
            dlclose(m_h);
        }
    }

    void* get() const
    {
        return m_h;
    }

    void* release()
    {
        void* t = m_h;
        m_h = nullptr;

        return t;
    }
};