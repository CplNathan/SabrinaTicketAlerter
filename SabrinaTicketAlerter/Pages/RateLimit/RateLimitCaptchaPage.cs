using OpenCvSharp;
using OpenQA.Selenium;
using SabrinaTicketAlerter.Locators.RateLimit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Pages.RateLimit
{
    public class RateLimitCaptchaPage(IWebDriver driver) : RateLimitBasePage<RateLimitCaptchaPageLocators, object>(driver)
    {
        public override bool IsCurrentPage
        {
            get
            {
                try
                {
                    return driver.FindElement(Locators.PageLocator) != null;
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            }
        }

        protected override string PagePath => string.Empty;

        public override void SignalSuccess()
        {
            // throw new NotImplementedException();

            // If we pass this no cooldown is required.
        }

        protected override async Task ActionAsyncImplementation()
        {
            driver.FindElement(Locators.CaptchaButton).Click();

            Waiter.Until(x => x.FindElement(Locators.CaptchaCanvas) != null || !IsCurrentPage);

            if (!IsCurrentPage)
            {
                return;
            }

            await Task.Run(() =>
            {
                var jsDriver = (IJavaScriptExecutor)driver;
                var canvasAndSlider = driver.FindElements(Locators.CaptchaCanvas);
                var canvasJs = "return arguments[0].getContext('2d').getImageData(0, 0, arguments[0].width, arguments[0].height).data";
                var test = jsDriver.ExecuteScript(canvasJs, canvasAndSlider.Last());

                var canvasData = (jsDriver.ExecuteScript(canvasJs, canvasAndSlider.First()) as IReadOnlyCollection<object>)?.Cast<Int64>().Select(x => Convert.ToByte(x));
                var sliderData = (jsDriver.ExecuteScript(canvasJs, canvasAndSlider.Last()) as IReadOnlyCollection<object>)?.Cast<Int64>().Select(x => Convert.ToByte(x));

                ArgumentNullException.ThrowIfNull(canvasData);
                ArgumentNullException.ThrowIfNull(sliderData);

                using var canvasMat = Mat.FromImageData(canvasData.ToArray());
                using var sliderMat = Mat.FromArray(sliderData.ToArray());

                using var canvasEdges = canvasMat
                    .CvtColor(ColorConversionCodes.RGB2GRAY)
                    .Canny(100, 100)
                    .CvtColor(ColorConversionCodes.GRAY2RGB);

                using var sliderEdges = sliderMat
                    .FindNonZero()
                    .CvtColor(ColorConversionCodes.RGB2GRAY)
                    .Canny(100, 100)
                    .CvtColor(ColorConversionCodes.GRAY2RGB);

                canvasEdges
                    .MatchTemplate(sliderEdges, TemplateMatchModes.CCoeffNormed)
                    .MinMaxLoc(out Point _, out Point maxVal);

                var topLeft = maxVal; // this is pos?
                var bottomRight = new Point(maxVal.X + sliderEdges.Width, maxVal.Y + sliderEdges.Height);

                canvasEdges.Rectangle(maxVal, bottomRight, new Scalar(255, 0, 0), 2);

                var result = Convert.ToBase64String(canvasEdges.ImEncode());
            });
        }

        protected override ValueTask<object> GetDataAsyncImplementation()
        {
            throw new NotImplementedException();
        }
    }
}
