#pragma once

#include <X11/Xlib.h>
#include <X11/Xutil.h>
#include <iostream>
#include <sstream>
#include <vector>

#include "clsTools.hpp"

std::ostringstream oss;

struct stImage
{
    int nWidth;
    int nHeight;
    std::vector<unsigned char> vuData;
};

class clsScreenshot
{
private:
    int m_nWidth;
    int m_nHeight;

    Display* m_pDisplay;
    Window m_root;
    XWindowAttributes m_gwa;

public:
    explicit clsScreenshot();
    ~clsScreenshot();

    stImage fnScreenshot();
};

clsScreenshot::clsScreenshot()
{
    oss.clear();
    clsTools eztools;
    Display* display = XOpenDisplay(NULL);

    if (!display)
    {
        oss << "Cannot open display: " << stderr;
        eztools.fnLogErr(oss.str());
        oss.clear();

        return;
    }

    Window root = DefaultRootWindow(display);
    XWindowAttributes gwa;
    XGetWindowAttributes(display, root, &gwa);

    m_nWidth = gwa.width;
    m_nHeight = gwa.height;

    m_pDisplay = display;
    m_root = root;
    m_gwa = gwa;
}

stImage clsScreenshot::fnScreenshot()
{
    XImage *img = XGetImage(m_pDisplay, m_root, 0, 0, m_nWidth, m_nHeight, AllPlanes, ZPixmap);
    if (!img)
    {
        throw std::runtime_error("Failed to get XImage.");
    }

    std::vector<unsigned char> vuPixels(m_nWidth * m_nHeight * 3);
    for (int y = 0; y < m_nHeight; ++y)
    {
        for (int x = 0; x < m_nWidth; ++x)
        {
            unsigned long nPixel = XGetPixel(img, x, y);
            unsigned char cBlue = nPixel & 0xFF;
            unsigned char cGreen = (nPixel >> 8) & 0xFF;
            unsigned char nRed = (nPixel >> 16) & 0xFF;

            vuPixels.push_back(nRed);
            vuPixels.push_back(cGreen);
            vuPixels.push_back(cBlue);
        }
    }

    stImage image = {
        m_nWidth,
        m_nHeight,
        std::move(vuPixels)
    };

    XDestroyImage(img);

    return image;
}

