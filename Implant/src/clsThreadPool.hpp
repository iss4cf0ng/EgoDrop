#include <condition_variable>
#include <functional>
#include <mutex>
#include <queue>
#include <thread>
#include <vector>

class clsThreadPool
{
private:
    std::vector<std::thread> m_workers;
    std::queue<std::function<void()>> m_jobs;
    std::mutex mtx;
    std::condition_variable m_cv;
    bool m_bStop;

public:
    clsThreadPool(size_t n) : m_bStop(false)
    {
        for (size_t i = 0; i < n; ++i)
        {
            m_workers.emplace_back([this]() 
            {
                while (true)
                {
                    std::function<void()> job;
                    {
                        std::unique_lock<std::mutex> lock(mtx);
                        m_cv.wait(lock, [&] 
                        {
                            return m_bStop || !m_jobs.empty();
                        });

                        if (m_bStop && m_jobs.empty())
                            return;

                        job = std::move(m_jobs.front());
                        m_jobs.pop();
                    }

                    job();
                }
            });
        }
    }

    ~clsThreadPool()
    {
        {
            std::lock_guard<std::mutex> lock(mtx);
            m_bStop = true;
        }

        m_cv.notify_all();
        for (auto& t : m_workers)
            t.join();
    }

    void fnEnqueue(std::function<void()> job)
    {
        {
            std::lock_guard<std::mutex> lock(mtx);
            m_jobs.push(std::move(job));
        }

        m_cv.notify_one();
    }
};