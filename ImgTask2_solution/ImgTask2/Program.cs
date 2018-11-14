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

        static ColorFloatImage gradient(ColorFloatImage image, String mode, float sigma)
        {
            if (mode != "rep" && mode != "odd" && mode != "even")
            {
                Console.WriteLine("Wrong edge mode");
                return image;
            }

            ColorFloatImage temp_image = gauss(image, mode, sigma);
            temp_image = img_expansion(temp_image, mode, 1);

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

        static GrayscaleFloatImage dir(ColorFloatImage image, float sigma)
        {
            ColorFloatImage temp_image = gauss(image, "even", sigma);
            temp_image = img_expansion(temp_image, "even", 1);

            ColorFloatImage temp_image_x = new ColorFloatImage(image.Width, image.Height);
            ColorFloatImage temp_image_y = new ColorFloatImage(image.Width, image.Height);

            for (int y = 0; y < temp_image_x.Height; y++)
                for (int x = 0; x < temp_image_x.Width; x++)
                    temp_image_x[x, y] = temp_image[x + 1, y] + (-1) * temp_image[x, y];
            for (int y = 0; y < temp_image_y.Height; y++)
                for (int x = 0; x < temp_image_y.Width; x++)
                    temp_image_y[x, y] = temp_image[x, y + 1] + (-1) * temp_image[x, y];

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
                        out_img[x, y] = 64;
                    else if (theta <= 67.5 && theta > 22.5 || theta >= -157.5 && theta < -112.5)
                    {
                        out_img[x, y] = 255;
                    }
                    else if (theta > 67.5 && theta <= 112.5 || theta >= -112.5 && theta < -67.5)
                        out_img[x, y] = 128;
                    else if (theta > 112.5 && theta <= 157.5 || theta >= -67.5 && theta < -22.5)
                        out_img[x, y] = 192;
                }
            return out_img;
        }

        static void Main(string[] args)
        {
            GrayscaleFloatImage image1 = ImageIO.FileToGrayscaleFloatImage(args[0]);
            GrayscaleFloatImage image2 = ImageIO.FileToGrayscaleFloatImage(args[1]);
            ColorFloatImage image = ImageIO.FileToColorFloatImage(args[0]);
            GrayscaleFloatImage output_image = dir(image, (float)1.4);
            ImageIO.ImageToFile(output_image, args[1]);
            Console.WriteLine("Image transformed (or not)");
            Console.ReadKey();
        }
    }
}
