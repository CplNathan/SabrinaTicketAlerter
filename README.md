# Sabrina Ticket Alerter
I made this tool to help secure some resale tickets for the upcoming Sabrina Carpenter tour.
This tool is not used nor intended to be used for scalping/resale of tickets.

## How to use
* Change the webhookUrl on line 20 on App.cs to your own webhook.
* Change the PagePath on line 24 in ArtistPage.cs

This program isn't really designed to be tidy or re-usable. I made it in just about a day, all the configuration is hardcoded, subject to ticket master changing their website structure you may also need to update the locators for each page, these can be CSS, XPath, or any valid query supported by Selenium.

## How it works
I'm using an undetected chrome driver in combination with web automation/testing tools to simulate real user behaviour, the tool will continually poll the ticketmaster website checking for new tickets in desired locations and artists, if a captcha page is detected it will attempt to solve it (only slider captchas supported) otherwise it will kill the browser instance and reload the tool, attempting to connect to a new proxy if needed. If tickets are found, the results get posted in a Discord channel using webhooks.

This works best if you can set the chrome user profile to be your personal user profile, and typically it is best to not use a proxy as IP trust factor & active profile trust factor/browsing history etc are all factors in displaying challenge pages.

![image](https://github.com/user-attachments/assets/8897eb2a-def3-4f44-b38e-e19db9e80b16)
