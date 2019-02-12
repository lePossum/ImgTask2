using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageReadCS;

namespace ImgTask2
{
    class Program
    {
        static ColorFloatImage img_expansion(ColorFloatImage inp_img, String mode, int radius)
        {
            ColorFloatImage out_img = new ColorFloatImage(inp_img.Width + 2 * radius, inp_img.Height + 2 * radius);
            for (int y = radius; y < out_img.Height - radius; y++) //centre part pf image
                for (int x = 0; x < out_img.Width; x++)
                    if (mode == "rep")//replicate
                        if (x < radius)
                            out_img[x, y] = inp_img[0, y - radius];
                        else if (x >= radius + inp_img.Width)
                            out_img[x, y] = inp_img[inp_img.Width - 1, y - radius];
                        else
                            out_img[x, y] = inp_img[x - radius, y - radius];
                    else if (mode == "odd")//odd
                        if (x < radius)
                            out_img[x, y] = 2 * inp_img[0, y - radius] + (-1) * inp_img[radius - x - 1, y - radius];
                        else if (x >= radius + inp_img.Width)
                            out_img[x, y] = 2 * inp_img[inp_img.Width - 1, y - radius] + (-1) * inp_img[radius + 2 * inp_img.Width - x - 1, y - radius];
                        else
                            out_img[x, y] = inp_img[x - radius, y - radius];
                    else if (mode == "even")//even
                        if (x < radius)
                            out_img[x, y] = inp_img[radius - x - 1, y - radius];
                        else if (x >= radius + inp_img.Width)
                            out_img[x, y] = inp_img[radius + 2 * inp_img.Width - x - 1, y - radius];
                        else
                            out_img[x, y] = inp_img[x - radius, y - radius];
            for (int y = 0; y < radius; y++) //upper part of image
                for (int x = 0; x < out_img.Width; x++)
                    if (mode == "rep")//replicate
                        out_img[x, y] = out_img[x, radius];
                    else if (mode == "odd") // odd
                        out_img[x, y] = 2 * out_img[x, radius] + (-1) * out_img[x, 2 * radius - y - 1];
                    else if (mode == "even") // even
                        out_img[x, y] = out_img[x, 2 * radius - y - 1];
            for (int y = inp_img.Height + radius; y < out_img.Height; y++) //lower part of image
                for (int x = 0; x < out_img.Width; x++)
                    if (mode == "rep")//replicate
                        out_img[x, y] = out_img[x, out_img.Height - radius - 1];
                    else if (mode == "odd")//odd
                        out_img[x, y] = 2 * out_img[x, out_img.Height - radius - 1] + (-1) * out_img[x, 2 * (out_img.Height - radius) - y - 1];
                    else if (mode == "even")//even
                        out_img[x, y] = out_img[x, 2 * (out_img.Height - radius) - y - 1];
            return out_img;
        }

        static ColorFloatImage sobel(ColorFloatImage image, String mode, String axis)
        {
            int x_flag = 0, y_flag = 0;
            if (axis == "x")
            {
                x_flag = 1;
            }
            else if (axis == "y")
            {
                y_flag = 1;
            }
            else
            {
                Console.WriteLine("Wrong axis in Sobel filter");
                return image;
            }

            if (mode != "rep" && mode != "odd" && mode != "even")
            {
                Console.WriteLine("Wrong edge mode");
                return image;
            }

            int rad = 1;
            ColorFloatImage test_image = img_expansion(image, mode, rad);
            ColorFloatImage out_img = new ColorFloatImage(image.Width, image.Height);
            for (int y = rad; y < out_img.Height + rad; y++)
            {
                for (int x = rad; x < out_img.Width + rad; x++)
                {
                    out_img[x - rad, y - rad] = x_flag * ((-1) * (test_image[x - 1, y - 1] + test_image[x + 1, y - 1]) + (-2) * test_image[x, y - 1] +
                        test_image[x - 1, y + 1] + test_image[x + 1, y + 1] + 2 * test_image[x, y + 1]) +
                        y_flag * ((-1) * (test_image[x - 1, y - 1] + test_image[x - 1, y + 1]) + (-2) * test_image[x - 1, y] +
                        test_image[x + 1, y - 1] + test_image[x + 1, y + 1] + 2 * test_image[x + 1, y]) + 128;
                }
            }

            return out_img;
        }

        static ColorFloatImage gauss(ColorFloatImage image, String mode, float sigma)
        {
            if (mode != "rep" && mode != "odd" && mode != "even")
            {
                Console.WriteLine("Wrong edge mode");
                return image;
            }
            if (sigma == 0)
            {
                Console.WriteLine("Sigma must differ from 0");
                return image;
            }
            float sigma2 = 2 * sigma * sigma;
            int window_rad = (int)Math.Round(3 * sigma);
            float[,] window = new float[window_rad * 2 + 1, window_rad * 2 + 1];

            float final_sum = 0;
            for (int i = 0; i <= window_rad; i++)
                for (int j = 0; j <= window_rad; j++)
                {
                    double xyz = Math.Exp((-i * i - j * j) / sigma2);
                    window[i + window_rad, j + window_rad] = (float)xyz;
                    window[window_rad + i, window_rad - j] = window[i + window_rad, j + window_rad];
                    window[window_rad - i, window_rad + j] = window[i + window_rad, j + window_rad];
                    window[window_rad - i, window_rad - j] = window[i + window_rad, j + window_rad];
                }
            for (int i = 0; i <= window_rad * 2; i++)
            {
                for (int j = 0; j <= window_rad * 2; j++)
                {
                    final_sum += window[i, j];
                }
            }
            ColorFloatImage test_image = img_expansion(image, mode, window_rad);
            ColorFloatImage out_img = new ColorFloatImage(image.Width, image.Height);

            for (int y = window_rad; y < out_img.Height + window_rad; y++)
                for (int x = window_rad; x < out_img.Width + window_rad; x++)
                {

                    for (int k = -window_rad; k <= window_rad; k++)
                        for (int n = -window_rad; n <= window_rad; n++)
                            out_img[x - window_rad, y - window_rad] +=
                                test_image[x + k, y + n] * window[window_rad + k, window_rad + n] / final_sum;


                }
            return out_img;
        }

        static double MSE(GrayscaleFloatImage img1, GrayscaleFloatImage img2)
        {
            double dif = 0;
            int height = Math.Min(img1.Height, img2.Height);
            int width = Math.Min(img1.Width, img2.Width);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    dif += Math.Pow(img1[x, y] - img2[x, y], 2);
            return Math.Sqrt(dif);
        }

        static double PSNR(GrayscaleFloatImage img1, GrayscaleFloatImage img2)
        {
            int S = 255;
            return 10*Math.Log10(S*S/MSE(img1, img2));
        }

        static double SSIM_block(GrayscaleFloatImage img1, GrayscaleFloatImage img2,
                                 int x0, int x1, int y0, int y1) 
        {
            double x_avg = 0, y_avg = 0;
            double height = y1 - y0;
            double width = x1 - x0;
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                {
                    x_avg += img1[x, y];
                    y_avg += img2[x, y];
                }
            x_avg /= height * width;
            y_avg /= height * width;

            double sigma2_x = 0, sigma2_y = 0;
            double sigma_xy = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    sigma2_x += Math.Pow(img1[x, y] - x_avg, 2);
                    sigma2_y += Math.Pow(img2[x, y] - y_avg, 2);
                    sigma_xy += (img1[x, y] - x_avg) * (img2[x, y] - y_avg);
                }
            sigma2_x /= height * width;
            sigma2_y /= height * width;
            sigma_xy /= height * width;

            double c1 = 0, c2 = 0;

            return (2 * x_avg * y_avg + c1) * (2 * sigma_xy + c2) /
                   ((x_avg * x_avg + y_avg * y_avg + c1) * (sigma2_x + sigma2_y + c2));
        }

        static double SSIM(GrayscaleFloatImage img1, GrayscaleFloatImage img2)
        {
            return SSIM_block(img1, img2, 0, img1.Width, 0, img1.Height);
        }

        static double MSSIM(GrayscaleFloatImage img1, GrayscaleFloatImage img2)
        {
            const int BLOCK_SIZE = 8;
            int x_amount = img1.Width / BLOCK_SIZE;
            int y_amount = img1.Height / BLOCK_SIZE;
            int REG_COEF = 0;
            if (img1.Width % BLOCK_SIZE == 0 || img2.Height % BLOCK_SIZE == 0)
                REG_COEF = 1;
            double common_SSIM = 0;
            int counter = 0;
            for (int y = 0; y < y_amount - REG_COEF; y++)
                for (int x = 0; x < x_amount - REG_COEF; x++)
                {
                    common_SSIM += SSIM_block(img1, img2, x * BLOCK_SIZE, (x + 1) * BLOCK_SIZE,
                                                          y * BLOCK_SIZE, (y + 1) * BLOCK_SIZE);
                    counter++;
                }
            return common_SSIM / ((x_amount - REG_COEF) * (y_amount - REG_COEF));
        }

        static ColorFloatImage gradient(ColorFloatImage image, String mode)
        {
            if (mode != "rep" && mode != "odd" && mode != "even")
            {
                Console.WriteLine("Wrong edge mode");
                return image;
            }

            ColorFloatImage temp_image = img_expansion(image, mode, 1);

            ColorFloatImage temp_image_x = new ColorFloatImage(image.Width, image.Height);
            ColorFloatImage temp_image_y = new ColorFloatImage(image.Width, image.Height);

            for (int y = 0; y < temp_image_x.Height; y++)
                for (int x = 0; x < temp_image_x.Width; x++)
                    temp_image_x[x, y] = temp_image[x + 1, y] + (-1) * temp_image[x, y];
            for (int y = 0; y < temp_image_y.Height; y++)
                for (int x = 0; x < temp_image_y.Width; x++)
                    temp_image_y[x, y] = temp_image[x, y + 1] + (-1) * temp_image[x, y];

            ColorFloatImage out_img = new ColorFloatImage(image.Width, image.Height);

            float max_color_r = 0;
            float max_color_g = 0;
            float max_color_b = 0;

            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    double r_x = temp_image_x[x, y].r, r_y = temp_image_y[x, y].r;
                    double g_x = temp_image_x[x, y].g, g_y = temp_image_y[x, y].g;
                    double b_x = temp_image_x[x, y].b, b_y = temp_image_y[x, y].b;
                    ColorFloatPixel temp_pixel;
                    temp_pixel.r = (float)(Math.Sqrt(r_x * r_x + r_y * r_y));
                    if (temp_pixel.r > max_color_r) max_color_r = temp_pixel.r;
                    temp_pixel.g = (float)(Math.Sqrt(g_x * g_x + g_y * g_y));
                    if (temp_pixel.g > max_color_g) max_color_g = temp_pixel.g;
                    temp_pixel.b = (float)(Math.Sqrt(b_x * b_x + b_y * b_y));
                    if (temp_pixel.r > max_color_b) max_color_b = temp_pixel.b;
                    temp_pixel.a = 0;
                    out_img[x, y] = temp_pixel;
                }
            //contrast increasing block
            double multiplier_r = 255 / max_color_r;
            double multiplier_g = 255 / max_color_g;
            double multiplier_b = 255 / max_color_b;
            float mul = 1;
            if ((multiplier_r <= multiplier_g) && (multiplier_r <= multiplier_b))
                mul = (float)multiplier_r;
            else if (multiplier_g <= multiplier_r && multiplier_g <= multiplier_b)
                mul = (float)multiplier_g;
            else if (multiplier_b <= multiplier_r && multiplier_b <= multiplier_g)
                mul = (float)multiplier_b;
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    out_img[x, y] = out_img[x, y] * mul;

            return out_img;
        }

        static GrayscaleFloatImage dir(ColorFloatImage image)
        {
            string mode = "even"; 
            ColorFloatImage temp_image = img_expansion(image, mode, 1);
            
            ColorFloatImage temp_image_x = new ColorFloatImage(image.Width, image.Height);
            ColorFloatImage temp_image_y = new ColorFloatImage(image.Width, image.Height);

            for (int y = 1; y < temp_image_x.Height + 1; y++)
                for (int x = 1; x < temp_image_x.Width + 1; x++)
                    temp_image_x[x - 1, y - 1] = temp_image[x + 1, y] + (-1) * temp_image[x, y];
            for (int y = 1; y < temp_image_y.Height + 1; y++)
                for (int x = 1; x < temp_image_y.Width + 1; x++)
                    temp_image_y[x - 1, y - 1] = temp_image[x, y + 1] + (-1) * temp_image[x, y];

            GrayscaleFloatImage out_img = new GrayscaleFloatImage(image.Width, image.Height);
            GrayscaleFloatImage x_grad = temp_image_x.ToGrayscaleFloatImage();
            GrayscaleFloatImage y_grad = temp_image_y.ToGrayscaleFloatImage();
            for (int y = 0; y < y_grad.Height; y++)
                for (int x = 0; x < x_grad.Width; x++) {
                    if (x_grad[x, y] == 0)
                    {
                        out_img[x, y] = 0;
                        continue;
                    }
                    double theta = Math.Atan2(y_grad[x, y], x_grad[x, y]) * (180 / Math.PI);
                    if (theta <= 22.5 && theta > -22.5 || theta <= -157.5 && theta > 157.5)
                        out_img[x, y] = 64; // ->
                    else if (theta <= 67.5 && theta > 22.5 || theta >= -157.5 && theta < -112.5)
                        out_img[x, y] = 192; // /
                    else if (theta > 67.5 && theta <= 112.5 || theta >= -112.5 && theta < -67.5)
                        out_img[x, y] = 128; // ^
                    else if (theta > 112.5 && theta <= 157.5 || theta >= -67.5 && theta < -22.5)
                        out_img[x, y] = 255; // \
                }
            return out_img;
        }
        
        static GrayscaleFloatImage nonmax(ColorFloatImage image)
        {
            int offset = 2;
            string mode = "even";

            ColorFloatImage temp_image = img_expansion(image, mode, offset);
            temp_image = gradient(image, mode);
            temp_image = img_expansion(temp_image, mode, offset);

            GrayscaleFloatImage gray_grad = temp_image.ToGrayscaleFloatImage();
            GrayscaleFloatImage dir_img = dir(image);
            GrayscaleFloatImage out_img = new GrayscaleFloatImage(image.Width, image.Height);
            float max_value = 0;

            for (int y = 0; y < out_img.Height; y++)
                for (int x = 0; x < out_img.Width; x++)
                {
                    float M = gray_grad[x + offset, y + offset];
                    if (M > max_value)
                        max_value = M;
                    switch (dir_img[x, y])
                    {
                        case 0: // o
                            break;
                        case 64: // ->
                            if (M < gray_grad[x + offset + 1, y + offset] ||
                                M < gray_grad[x + offset - 1, y + offset])
                                out_img[x, y] = 0;
                            else
                                out_img[x, y] = M;
                            break;
                        case 128: // ^
                            if (M < gray_grad[x + offset, y + offset + 1] ||
                                M < gray_grad[x + offset, y + offset - 1])
                                out_img[x, y] = 0;
                            else
                                out_img[x, y] = M;
                            break;
                        case 192: // /
                            if (M < gray_grad[x + offset + 1, y + offset + 1] ||
                                M < gray_grad[x + offset - 1, y + offset - 1])
                                out_img[x, y] = 0;
                            else
                                out_img[x, y] = M;
                            break;
                        case 255: // \
                            if (M < gray_grad[x + offset - 1, y + offset + 1] ||
                                M < gray_grad[x + offset + 1, y + offset - 1])
                                out_img[x, y] = 0;
                            else
                                out_img[x, y] = M;
                            break;
                    }
                }
            float mult = 255 / max_value;
            for (int y = 0; y < out_img.Height; y++)
                for (int x = 0; x < out_img.Width; x++)
                    out_img[x, y] *= mult;
            return out_img;
        }

        static GrayscaleFloatImage canny(ColorFloatImage image, int thr_high, int thr_low)
        {
            int MAX = 255;
            GrayscaleFloatImage temp_img = nonmax(image);

            float max_value = 0;
            for (int y = 0; y < temp_img.Height; y++)
                for (int x = 0; x < temp_img.Width; x++)
                    if (temp_img[x, y] > max_value)
                        max_value = temp_img[x, y];

            float high_value = max_value * thr_high / 1000;
            float low_value = max_value * thr_low / 1000;
            GrayscaleFloatImage out_img = new GrayscaleFloatImage(image.Width, image.Height);
            bool[,] visited_pixels = new bool[image.Width, image.Height];

            for (int y = 0; y < temp_img.Height; y++)
                for (int x = 0; x < temp_img.Width; x++)
                    if (temp_img[x, y] < low_value)
                    {
                        visited_pixels[x, y] = true;
                        out_img[x, y] = 0;
                    }
                    else if (temp_img[x, y] > high_value)
                    {
                        visited_pixels[x, y] = true;
                        out_img[x, y] = MAX;
                    }
            
            GrayscaleFloatImage dir_img = dir(image);
            List<Tuple<int, int>> pixels_list = new List<Tuple<int, int>>();
            for (int coord_y = 0; coord_y < temp_img.Height; coord_y++)
                for (int coord_x = 0; coord_x < temp_img.Width; coord_x++)
                {
                    bool flag = true;
                    int x1 = coord_x, x2 = coord_x;
                    int y1 = coord_y, y2 = coord_y;
                    while (flag)
                    {
                        if (!visited_pixels[x1, y1] || !visited_pixels[x2, y2])
                        {
                            visited_pixels[x1, y1] = true;
                            visited_pixels[x2, y2] = true;
                            Tuple<int, int> a = new Tuple<int, int>(x1, y1);
                            Tuple<int, int> b = new Tuple<int, int>(x2, y2);
                            pixels_list.Add(a);
                            pixels_list.Add(b);
                            switch (dir_img[x1, y1])
                            {
                                case 0: // o
                                    break;
                                case 64: // ->
                                    if (x1 < temp_img.Width - 1)
                                        x1++;
                                    break;
                                case 128: // ^
                                    if (y1 < temp_img.Height - 1)
                                        y1++;
                                    break;
                                case 192: // /
                                    if (x1 < temp_img.Width - 1)
                                        x1++;
                                    if (y1 < temp_img.Height - 1)
                                        y1++;
                                    break;
                                case 255: // \
                                    if (x1 > 0)
                                        x1--;
                                    if (y1 < temp_img.Height - 1)
                                        y1++;
                                    break;
                            }
                            switch (dir_img[x2, y2])
                            {
                                case 0: // o
                                    break;
                                case 64: // ->
                                    if (x2 > 0)
                                        x2--;
                                    break;
                                case 128: // ^
                                    if (y2 > 0)
                                        y2--;
                                    break;
                                case 192: // /
                                    if (x2 > 0)
                                        x2--;
                                    if (y2 > 0)
                                        y2--;
                                    break;
                                case 255: // \
                                    if (x2 < temp_img.Width - 1)
                                        x2++;
                                    if (y2 > 0)
                                        y2--;
                                    break;
                            }
                        }
                        else
                        {
                            flag = false;
                            visited_pixels[x1, y1] = true;
                            visited_pixels[x2, y2] = true;
                            if (out_img[x1, y1] == MAX || out_img[x2, y2] == MAX)
                            {
                                foreach (var a in pixels_list)
                                    out_img[a.Item1, a.Item2] = MAX;
                                pixels_list = new List<Tuple<int, int>>();
                            }
                            else
                            {
                                foreach (var a in pixels_list)
                                    out_img[a.Item1, a.Item2] = 0;
                                pixels_list = new List<Tuple<int, int>>();
                            }
                        }
                    }
                }
                
            return out_img;
        }

        static void Main(string[] args)
        {
            GrayscaleFloatImage output_image = null;
            if (args[0] == "mse")
            {
                GrayscaleFloatImage image = ImageIO.FileToGrayscaleFloatImage(args[1]);
                GrayscaleFloatImage image1 = ImageIO.FileToGrayscaleFloatImage(args[2]);
                Console.WriteLine(MSE(image, image1));
            }
            else if (args[0] == "psnr")
            {
                GrayscaleFloatImage image = ImageIO.FileToGrayscaleFloatImage(args[1]);
                GrayscaleFloatImage image1 = ImageIO.FileToGrayscaleFloatImage(args[2]);
                Console.WriteLine(PSNR(image, image1));
            }
            else if (args[0] == "ssim")
            {
                GrayscaleFloatImage image = ImageIO.FileToGrayscaleFloatImage(args[1]);
                GrayscaleFloatImage image1 = ImageIO.FileToGrayscaleFloatImage(args[2]);
                Console.WriteLine(SSIM(image, image1));
            }
            else if (args[0] == "mssim")
            {
                GrayscaleFloatImage image = ImageIO.FileToGrayscaleFloatImage(args[1]);
                GrayscaleFloatImage image1 = ImageIO.FileToGrayscaleFloatImage(args[2]);
                Console.WriteLine(MSSIM(image, image1));
            }
            else if (args[0] == "dir")
            {
                ColorFloatImage image = ImageIO.FileToColorFloatImage(args[2]);
                image = gauss(image, "even", (float)Convert.ToDouble(args[1]));
                output_image = dir(image);
                ImageIO.ImageToFile(output_image, args[3]);
            }
            else if (args[0] == "nonmax")
            {
                ColorFloatImage image = ImageIO.FileToColorFloatImage(args[2]);
                image = gauss(image, "even", (float)Convert.ToDouble(args[1]));
                output_image = nonmax(image);
                ImageIO.ImageToFile(output_image, args[3]);
            }
            else if (args[0] == "canny")
            {
                ColorFloatImage image = ImageIO.FileToColorFloatImage(args[4]);
                image = gauss(image, "even", (float)Convert.ToDouble(args[1]));
                output_image = canny(image, Convert.ToInt32(args[2]), Convert.ToInt32(args[3]));
                ImageIO.ImageToFile(output_image, args[5]);
            }
        }
    }
}
