﻿using CoreHome.Admin.Services;
using CoreHome.Infrastructure.Services;
using CoreHome.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Diagnostics;

namespace CoreHome.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMemoryCache cache;
        private readonly NotifyService notifyService;
        private readonly SecurityService securityService;

        public HomeController(IMemoryCache cache, NotifyService notifyService, SecurityService securityService)
        {
            this.cache = cache;
            this.notifyService = notifyService;
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            //有管理员权限的话直接跳转的Overview验证访问令牌
            if (Request.Cookies.TryGetValue("accessToken", out _))
            {
                return Redirect("/Admin/Overview");
            }
            return View();
        }

        public IActionResult Login()
        {
            string cacheKey = Guid.NewGuid().ToString();
            Response.Cookies.Append("user", cacheKey, new CookieOptions()
            {
                Expires = DateTimeOffset.Now.AddHours(24)
            });

            //随机生成密码
            string password = Guid.NewGuid().ToString().Substring(0, 6);

            //记录密码并设置过期时间为一分钟
            cache.Set(cacheKey, password, DateTimeOffset.Now.AddMinutes(1));
            try
            {
                //发送密码到手机
                notifyService.PushNotify("CoreHome", "VerifyCode：" + password);
                return Content("验证码已经发送");
            }
            catch (Exception)
            {
                return Content("网络错误");
            }
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete("accessToken");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult VerifyPassword([FromForm]string pwd)
        {
            string cacheKey = null, password = null;
            try
            {
                cacheKey = Request.Cookies["user"];
                password = cache.Get(cacheKey).ToString();
            }
            catch (Exception) { }

            if (pwd == password && pwd != null && password != null)
            {
                //颁发访问令牌
                Response.Cookies.Append("accessToken", securityService.Encryptor(cacheKey), new CookieOptions()
                {
                    Expires = DateTimeOffset.Now.AddHours(10)
                });

                //重定向到仪表盘
                return Redirect("/Admin/Overview");
            }
            return Redirect("/Admin/Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
