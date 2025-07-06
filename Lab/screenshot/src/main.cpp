#include <iostream>

#include <X11/Xlib.h>
#include <X11/Xutil.h>
#include <png.h>

using namespace std;

void fnSaveImgToPng(XImage *image, const char *pFileName)
{
    FILE *fp = fopen(pFileName, "wb");
    if (!fp)
    {
        fprintf(stderr, "Error: Failed to open file %s for writing.\n", pFileName);
        return;
    }

    png_structp pPng = png_create_write_struct(PNG_LIBPNG_VER_STRING, NULL, NULL, NULL);
    if (!pPng)
    {
        fprintf(stderr, "Error: Failed to create PNG write structure.\n");
        fclose(fp);
        return;
    }

    png_infop pInfo = png_create_info_struct(pPng);
    if (!pInfo)
    {
        
        return;
    }
}

int main()
{
    Display *display = XOpenDisplay(NULL);
    if (!display)
    {
        printf("Failed to open display.\n");
        return;
    }

    Window root = DefaultRootWindow(display);
    XWindowAttributes gwa;
    XGetWindowAttributes(display, root, &gwa);
    int w = gwa.width, h = gwa.height;

    XImage *image = XGetImage(display, root, 0, 0, w, h, AllPlanes, ZPixmap);
    if (!image)
    {
        printf("Failed to get image from display.\n");
        XCloseDisplay(display);
        return;
    }
}