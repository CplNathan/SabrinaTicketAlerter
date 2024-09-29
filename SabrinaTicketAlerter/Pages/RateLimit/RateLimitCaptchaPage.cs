using OpenCvSharp;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SabrinaTicketAlerter.Locators.RateLimit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
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

        public async Task TestAction()
            => await ActionAsyncImplementation(CancellationToken.None);

        protected override async Task ActionAsyncImplementation(CancellationToken token)
        {
            driver.FindElement(Locators.CaptchaButton).Click();

            Waiter.Until(x => x.FindElement(Locators.CaptchaCanvas) != null || !IsCurrentPage);

            if (!IsCurrentPage)
            {
                return;
            }

            var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutToken.Token);

            var solvedCaptcha = false;
            while (!linkedToken.IsCancellationRequested && !solvedCaptcha && IsCurrentPage)
            {
                solvedCaptcha = await Task.Run(async () =>
                {
                    var jsDriver = (IJavaScriptExecutor)driver;
                    var canvasAndSlider = driver.FindElements(Locators.CaptchaCanvas);
                    var canvasJs = "return arguments[0].getContext('2d').getImageData(0, 0, arguments[0].width, arguments[0].height).data";
                    var canvasSizeJs = "return [arguments[0].width, arguments[0].height]";

                    var canvasData = (jsDriver.ExecuteScript(canvasJs, canvasAndSlider.First()) as IReadOnlyCollection<object>)?.Select(x => Convert.ToByte(x));
                    var sliderData = (jsDriver.ExecuteScript(canvasJs, canvasAndSlider.Last()) as IReadOnlyCollection<object>)?.Select(x => Convert.ToByte(x));

                    var canvasSize = (jsDriver.ExecuteScript(canvasSizeJs, canvasAndSlider.First()) as IReadOnlyCollection<object>)?.Select(x => Convert.ToInt32(x));

                    ArgumentNullException.ThrowIfNull(canvasData);
                    ArgumentNullException.ThrowIfNull(sliderData);
                    ArgumentNullException.ThrowIfNull(canvasSize);

                    using var canvasMat = new Mat(new Size(canvasSize.ElementAt(0), canvasSize.ElementAt(1)), MatType.CV_8UC4); // Mat.FromArray(canvasData.ToArray());
                    using var sliderMat = new Mat(new Size(canvasSize.ElementAt(0), canvasSize.ElementAt(1)), MatType.CV_8UC4); // Mat.FromArray(sliderData.ToArray());

                    var length = canvasSize.ElementAt(0) * canvasSize.ElementAt(1) * 4;
                    Marshal.Copy(canvasData.ToArray(), 0, canvasMat.Data, length);
                    Marshal.Copy(sliderData.ToArray(), 0, sliderMat.Data, length);

                    using var canvasEdges = canvasMat
                        .CvtColor(ColorConversionCodes.RGB2GRAY)
                        .Canny(100, 100)
                        .CvtColor(ColorConversionCodes.GRAY2RGB);

                    using var sliderGray = sliderMat
                        .CvtColor(ColorConversionCodes.RGB2GRAY);

                    var sliderBounds = sliderGray.FindNonZero().BoundingRect();
                    using var sliderEdges = sliderGray[sliderBounds]
                        .Canny(100, 100)
                        .CvtColor(ColorConversionCodes.GRAY2RGB);

                    canvasEdges
                        .MatchTemplate(sliderEdges, TemplateMatchModes.CCoeffNormed)
                        .MinMaxLoc(out Point _, out Point maxVal);

                    var startPoint = maxVal; // this is start pos?
                    var endPoint = new Point(maxVal.X + sliderEdges.Width, maxVal.Y + sliderEdges.Height);
                    var foundRectangle = Rect.FromLTRB(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                    var rectangleCenter = new Point(foundRectangle.X + foundRectangle.Width / 2, foundRectangle.Y + foundRectangle.Height / 2);

                    var dragAction = new Actions(driver);

                    var sliderButton = driver.FindElement(Locators.CaptchaSlider);
                    dragAction
                        .DragAndDropToOffset(sliderButton, foundRectangle.X - Random.Shared.Next(0, (int)(foundRectangle.Width * 0.025)), 0)
                        .Perform();

                    await Task.Delay(TimeSpan.FromSeconds(2.5));

                    return !IsCurrentPage;
                }, linkedToken.Token);
            }

            if (linkedToken.IsCancellationRequested || !solvedCaptcha)
            {
                throw new TaskCanceledException();
            }
        }

        protected override ValueTask<object> GetDataAsyncImplementation()
        {
            throw new NotImplementedException();
        }
    }
}
