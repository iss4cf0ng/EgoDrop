#include "clsScreenshot.hpp"
#include "clsTools.hpp"

#include <stdio.h>
#include <stdlib.h>
#include <X11/Xlib.h>
#include <X11/Xutil.h>

void clsScreenshot::fnScreenshot()
{
    /*
    Display *display = XOpenDisplay(NULL);
    if (!display)
    {
        fnLogError("Failed to open display.");
        return;
    }

    Window root = DefaultRootWindow(display);
    XImage *image = XGetImage(display, root, 0, 0, 1920, 1080, AllPlanes, ZPixmap);
    if (!image)
    {
        fnLogError("Failed to get image from display.");
        XCloseDisplay(display);
        return;
    }

    */
}