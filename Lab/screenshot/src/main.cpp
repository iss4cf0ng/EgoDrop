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
        fprintf(stderr, "Error: Failed to create PNG info structure.\n");
        png_destroy_write_struct(&pPng, NULL);
        fclose(fp);

        return;
    }

    if (setjmp(png_jmpbuf(pPng)))
    {
        fprintf(stderr, "Error: Failed to set PNG jump buffer.\n");
        png_destroy_write_struct(&pPng, &pInfo);
        fclose(fp);

        return;
    }

    png_init_io(pPng, fp);
    png_set_IHDR(pPng, pInfo, image->width, image->height, 8, PNG_COLOR_TYPE_RGBA, PNG_INTERLACE_NONE, PNG_COMPRESSION_TYPE_DEFAULT, PNG_FILTER_TYPE_DEFAULT);
    
    png_write_info(pPng, pInfo);

    png_write_info(pPng, pInfo);
    png_bytep *pRow = (png_bytep *)malloc(image->height * sizeof(png_bytep));
    for (int y = 0; y < image->height; y++)
        pRow[y] = (png_bytep)(image->data + y * image->bytes_per_line);

    png_write_image(pPng, pRow);
    png_write_end(pPng, NULL);

    free(pRow);
    png_destroy_write_struct(&pPng, &pInfo);
    fclose(fp);
}

int main()
{
    Display *display = XOpenDisplay(NULL);
    if (!display)
    {
        printf("Failed to open display.\n");
        return 1;
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
        return 1;
    }

    fnSaveImgToPng(image, "screenshot.png");

    XDestroyImage(image);
    XCloseDisplay(display);

    return 0;
}