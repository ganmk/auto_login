using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Chrome;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;


namespace LoginTest
{
    class Program
    {

        static void Main(string[] args)
        {

            ChromeDriver driver = new ChromeDriver();


            driver.Navigate().GoToUrl("https://passport.cnblogs.com/user/signin");
            driver.Manage().Window.Maximize();//窗口最大化，便于脚本执行

            //设置超时等待(隐式等待)时间设置10秒
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10000);

            driver.FindElementByXPath("//*[@id=\"input1\"]").SendKeys("test02");
    
            driver.FindElementByXPath("//*[@id=\"input2\"]").SendKeys("gmk020330@");

            driver.FindElementByXPath("//*[@id=\"signin\"]").Click();
            Thread.Sleep(3000);
            driver.FindElementByXPath("//*[@id=\"captchaBox\"]/div/div[2]/div[1]/div[3]/span[1]").Click();
            Actions actions = new Actions(driver);
            Thread.Sleep(3000);
            driver.GetScreenshot().SaveAsFile("原始验证图.png");
            // /html/body/div[3]/div[2]/div[1]/div/div[1]/div[2]/div[2]
            driver.FindElementByXPath("/html/body/div[3]/div[2]/div[1]/div/div[1]/div[2]/div[2]").Click();
            //actions.DragAndDropToOffset(driver,leftShift,0).Build().Perform();//单击并在指定的元素上按下鼠标按钮,然后移动到指定位置
            Thread.Sleep(3000);
            driver.GetScreenshot().SaveAsFile("阴影验证图.png");

            //E:\LoginTest-master\LoginTest-master\LoginTest\bin\Debug
            Bitmap a = new Bitmap(@"E:\LoginTest-master\LoginTest-master\LoginTest\bin\Debug\原始验证图.png");
            Bitmap b = new Bitmap(@"E:\LoginTest-master\LoginTest-master\LoginTest\bin\Debug\阴影验证图.png");
            int c=  GetArgb(a, b);
            actions.DragAndDropToOffset(driver.FindElementByXPath("/html/body/div[3]/div[2]/div[1]/div/div[1]/div[2]/div[2]"),c-494, 0).Build().Perform();//移动滑块到阴影处


            List<Rectangle> Compare = ImageComparer.Compare(a, b);

            foreach (Rectangle item in Compare)
            {
                    Thread.Sleep(2000);
                    actions.DragAndDropToOffset(driver.FindElementByXPath("/html/body/div[3]/div[2]/div[1]/div/div[1]/div[2]/div[2]"), item.X+13 , 0).Build().Perform();//移动滑块到阴影处
            }
          
            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine(0);
            }

        }


        /// <summary>
        /// 比较两张图片的像素，确定阴影图片位置
        /// </summary>
        /// <param name="oldBmp"></param>
        /// <param name="newBmp"></param>
        /// <returns></returns>
        public static int GetArgb(Bitmap oldBmp, Bitmap newBmp)
        {
            bool a = true;
            bool b = true;
            //由于阴影图片四个角存在黑点(矩形1*1) 
            for (int i = 0; i < newBmp.Width; i++)
            {

                for (int j = 0; j < newBmp.Height; j++)
                {
                    if ((i >= 0 && i <= 1) && ((j >= 0 && j <= 1) || (j >= (newBmp.Height - 2) && j <= (newBmp.Height - 1))))
                    {
                        continue;
                    }
                    if ((i >= (newBmp.Width - 2) && i <= (newBmp.Width - 1)) && ((j >= 0 && j <= 1) || (j >= (newBmp.Height - 2) && j <= (newBmp.Height - 1))))
                    {
                        continue;
                    }

                    //获取该点的像素的RGB的颜色
                    Color oldColor = oldBmp.GetPixel(i, j);
                    Color newColor = newBmp.GetPixel(i, j);
                    if (Math.Abs(oldColor.R - newColor.R) > 60 || Math.Abs(oldColor.G - newColor.G) > 60 || Math.Abs(oldColor.B - newColor.B) > 60)
                    {
                        a = false;

                        if (!b)
                        {
                            return i;
                        }
                    }
                    else
                    {
                        if (!a)
                        {
                            b = false;
                        }

                    }


                }
            }
            return 0;
        }


        /// <summary>
        /// 图像比较.用于找出两副图片之间的差异位置
        /// </summary>
        public class ImageComparer
        {
            /// <summary>
            /// 图像颜色
            /// </summary>
            [StructLayout(LayoutKind.Explicit)]
            private struct ICColor
            {
                [FieldOffset(0)]
                public byte B;
                [FieldOffset(1)]
                public byte G;
                [FieldOffset(2)]
                public byte R;
            }

            /// <summary>
            /// 按20*20大小进行分块比较两个图像.
            /// </summary>
            /// <param name="bmp1"></param>
            /// <param name="bmp2"></param>
            /// <returns></returns>
            public static List<Rectangle> Compare(Bitmap bmp1, Bitmap bmp2)
            {
                return Compare(bmp1, bmp2, new Size(30, 30));
            }
            /// <summary>
            /// 比较两个图像
            /// </summary>
            /// <param name="bmp1"></param>
            /// <param name="bmp2"></param>
            /// <param name="block"></param>
            /// <returns></returns>
            public static List<Rectangle> Compare(Bitmap bmp1, Bitmap bmp2, Size block)
            {
                List<Rectangle> rects = new List<Rectangle>();
                PixelFormat pf = PixelFormat.Format24bppRgb;

                BitmapData bd1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, pf);
                BitmapData bd2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, pf);

                try
                {
                    unsafe
                    {
                        int w = 0, h = 0;

                        while (h < bd1.Height && h < bd2.Height)
                        {
                            byte* p1 = (byte*)bd1.Scan0 + h * bd1.Stride;
                            byte* p2 = (byte*)bd2.Scan0 + h * bd2.Stride;

                            w = 0;
                            while (w < bd1.Width && w < bd2.Width)
                            {
                                //按块大小进行扫描
                                for (int i = 0; i < block.Width; i++)
                                {
                                    int wi = w + i;
                                    if (wi >= bd1.Width || wi >= bd2.Width) break;

                                    for (int j = 0; j < block.Height; j++)
                                    {
                                        int hj = h + j;
                                        if (hj >= bd1.Height || hj >= bd2.Height) break;

                                        ICColor* pc1 = (ICColor*)(p1 + wi * 3 + bd1.Stride * j);
                                        ICColor* pc2 = (ICColor*)(p2 + wi * 3 + bd2.Stride * j);
                                        // if (Math.Abs(pc1->R - pc2->R)>10 || pc1->G != pc2->G || pc1->B != pc2->B)
                                        if (Math.Abs(pc1->R - pc2->R) > 100 || Math.Abs(pc1->G - pc2->G) > 100 || Math.Abs(pc1->B - pc2->B) > 100)
                                        {
                                            //当前块有某个象素点颜色值不相同.也就是有差异.

                                            int bw = Math.Min(block.Width, bd1.Width - w);
                                            int bh = Math.Min(block.Height, bd1.Height - h);
                                            if(rects.FindAll(a=>a.X<w).Count>0)
                                            {
                                                continue;

                                            }
                                            rects.Add(new Rectangle(w, h, bw, bh));

                                            goto E;
                                        }
                                    }
                                }
                            E:
                                w += block.Width;
                            }

                            h += block.Height;
                        }
                    }
                }
                finally
                {
                    bmp1.UnlockBits(bd1);
                    bmp2.UnlockBits(bd2);
                }

                return rects;
            }
        }


    }
}
