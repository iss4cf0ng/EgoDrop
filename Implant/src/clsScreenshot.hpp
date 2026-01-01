#include <X11/Xlib.h>
#include <X11/Xutil.h>
#include <iostream>
#include <vector>

struct stImage {
    int nWidth;
    int nHeight;
    std::vector<unsigned char> vuData;
};

class clsScreenshot {
private:
    int m_nWidth;
    int m_nHeight;
    Display* m_pDisplay;
    Window m_root;
    XWindowAttributes m_gwa;

public:
    explicit clsScreenshot();
    ~clsScreenshot() = default;

    stImage fnScreenshot();
};

clsScreenshot::clsScreenshot() {
    m_pDisplay = XOpenDisplay(NULL);
    if (!m_pDisplay) {
        std::cerr << "Cannot open display: " << std::strerror(errno) << std::endl;
        return;
    }

    m_root = DefaultRootWindow(m_pDisplay);
    if (m_root == None) {
        std::cerr << "Failed to get the root window" << std::endl;
        return;
    }

    // Get window attributes
    if (!XGetWindowAttributes(m_pDisplay, m_root, &m_gwa)) {
        std::cerr << "Failed to get window attributes" << std::endl;
        return;
    }

    m_nWidth = m_gwa.width;
    m_nHeight = m_gwa.height;
}

stImage clsScreenshot::fnScreenshot() 
{
     try 
     {
        return {0, 0, {}};

        XMapRaised(m_pDisplay, m_root);
        XImage* img = XGetImage(m_pDisplay, m_root, 0, 0, m_nWidth, m_nHeight, AllPlanes, ZPixmap);
        if (!img) 
        {
            return {0, 0, {}};
        }

        std::vector<unsigned char> vuPixels(m_nWidth * m_nHeight * 3);
        for (int y = 0; y < m_nHeight; ++y) {
            for (int x = 0; x < m_nWidth; ++x) {
                unsigned long nPixel = XGetPixel(img, x, y);
                unsigned char cBlue = nPixel & 0xFF;
                unsigned char cGreen = (nPixel >> 8) & 0xFF;
                unsigned char nRed = (nPixel >> 16) & 0xFF;

                vuPixels.push_back(nRed);
                vuPixels.push_back(cGreen);
                vuPixels.push_back(cBlue);
            }
        }

        stImage image = { m_nWidth, m_nHeight, std::move(vuPixels) };

        XDestroyImage(img);
        return image;

    } catch (const std::exception& e) {
        std::cerr << "Standard Exception: " << e.what() << std::endl;
        return { 0, 0, {} };  // Return empty image on error
    } catch (...) {
        std::cerr << "Unknown Exception occurred." << std::endl;
        return { 0, 0, {} };  // Return empty image on error
    }
}