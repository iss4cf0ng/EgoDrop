#include <X11/Xlib.h>
#include <X11/Xutil.h>
#include <png.h>
#include <iostream>
#include <cstdlib>

bool save_png(const char* filename, unsigned char* data, int width, int height) {
    FILE* fp = fopen(filename, "wb");
    if (!fp) {
        std::cerr << "Failed to open file for writing: " << filename << "\n";
        return false;
    }

    png_structp png_ptr = png_create_write_struct(PNG_LIBPNG_VER_STRING, nullptr, nullptr, nullptr);
    if (!png_ptr) {
        fclose(fp);
        std::cerr << "Failed to create PNG write struct\n";
        return false;
    }

    png_infop info_ptr = png_create_info_struct(png_ptr);
    if (!info_ptr) {
        png_destroy_write_struct(&png_ptr, nullptr);
        fclose(fp);
        std::cerr << "Failed to create PNG info struct\n";
        return false;
    }

    if (setjmp(png_jmpbuf(png_ptr))) {
        png_destroy_write_struct(&png_ptr, &info_ptr);
        fclose(fp);
        std::cerr << "Error during PNG creation\n";
        return false;
    }

    png_init_io(png_ptr, fp);
    png_set_IHDR(
        png_ptr, info_ptr, width, height,
        8, PNG_COLOR_TYPE_RGB, PNG_INTERLACE_NONE,
        PNG_COMPRESSION_TYPE_DEFAULT, PNG_FILTER_TYPE_DEFAULT
    );
    png_write_info(png_ptr, info_ptr);

    // Convert BGRA → RGB
    png_bytep row = (png_bytep) malloc(3 * width * sizeof(png_byte));
    for (int y = 0; y < height; ++y) {
        for (int x = 0; x < width; ++x) {
            unsigned char* pixel = data + (y * width + x) * 4;
            row[x * 3 + 0] = pixel[2]; // R
            row[x * 3 + 1] = pixel[1]; // G
            row[x * 3 + 2] = pixel[0]; // B
        }
        png_write_row(png_ptr, row);
    }

    free(row);
    png_write_end(png_ptr, nullptr);
    png_destroy_write_struct(&png_ptr, &info_ptr);
    fclose(fp);
    return true;
}

int main() {
    const char* display_name = std::getenv("DISPLAY");
    if (!display_name) {
        std::cerr << "No X11 desktop detected (DISPLAY not set)\n";
        return 1;
    }

    Display* display = XOpenDisplay(nullptr);
    if (!display) {
        std::cerr << "Failed to open X11 display\n";
        return 1;
    }

    int screen_num = DefaultScreen(display);
    Window root = RootWindow(display, screen_num);

    XWindowAttributes gwa;
    XGetWindowAttributes(display, root, &gwa);

    int width = gwa.width;
    int height = gwa.height;
    int depth = gwa.depth;

    XImage* image = XGetImage(display, root, 0, 0, width, height,
                            (1UL << depth) - 1, ZPixmap);

    if (!image) {
        std::cerr << "XGetImage failed (maybe Wayland or BadMatch)\n";
        XCloseDisplay(display);
        return 1;
    }
    bool ok = save_png("screenshot.png", (unsigned char*)image->data, width, height);
    if (ok)
        std::cout << "Screenshot saved to screenshot.png (" << width << "x" << height << ")\n";
    else
        std::cerr << "Failed to save PNG file\n";

    XDestroyImage(image);
    XCloseDisplay(display);
    return 0;
}
