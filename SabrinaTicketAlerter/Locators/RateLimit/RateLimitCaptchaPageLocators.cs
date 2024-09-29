using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabrinaTicketAlerter.Locators.RateLimit
{
    public class RateLimitCaptchaPageLocators : BaseLocators
    {
        public override By PageLocator => CaptchaButton;

        public By CaptchaButton => By.XPath("//div[@class=\"geetest_btn\"]");

        public By CaptchaCanvas => By.XPath("//div[@class=\"geetest_slicebg geetest_absolute\"]/canvas");

        public By CaptchaSlider => By.XPath("//div[@class=\"geetest_slider_button\"]");
    }
}
