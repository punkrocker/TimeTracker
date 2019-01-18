using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Model;
using TimeTrackerServer.App_Start;
using TimeTrackerServer.Models;

namespace TimeTrackerServer.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl, string message)
        {
            if (returnUrl == null || returnUrl.Contains("ChangePassword"))
                returnUrl = "/";
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Message = message;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 这不会计入到为执行帐户锁定而统计的登录失败次数中
            // 若要在多次输入错误密码的情况下触发帐户锁定，请更改为 shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, false, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    using (var db = new TimeTrackerEntities())
                    {
                        T_Sys_UserInfo user = db.T_Sys_UserInfo.Where(a => a.UseName.Equals(model.UserName)).FirstOrDefault();
                        if (user != null)
                        {
                            Session["UserId"] = user.UserID;
                            Session["UserName"] = user.UseName;
                        }
                    }
                    AuthenticationManager.SignIn(new AuthenticationProperties(), new ClaimsIdentity());
                    return RedirectToLocal(returnUrl);
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "无效的登录尝试。");
                    return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ChangePassword(string origin_password, string new_password, string re_new_password)
        {
            using (var db = new TimeTrackerEntities())
            {
                int userID = Convert.ToInt16(Session["UserId"]);
                T_Sys_UserInfo user = db.T_Sys_UserInfo.Where(a => a.UserID == userID).FirstOrDefault();
                if (user != null)
                {
                    if (AppUtils.SHA1Hash(origin_password).Equals(user.UserPwd))
                    {
                        if (new_password.Equals(re_new_password))
                        {
                            user.UserPwd = AppUtils.SHA1Hash(new_password);
                            db.T_Sys_UserInfo.Attach(user);
                            db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        return RedirectToAction("Login", new
                        {
                            message = 0
                        });
                    }
                    else
                    {
                        return RedirectToAction("Login", new
                        {
                            message = -1
                        });
                    }
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region 帮助程序
        // 用于在添加外部登录名时提供 XSRF 保护
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider
            {
                get; set;
            }
            public string RedirectUri
            {
                get; set;
            }
            public string UserId
            {
                get; set;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}