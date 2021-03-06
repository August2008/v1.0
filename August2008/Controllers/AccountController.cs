﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using August2008.Common.Interfaces;
using August2008.Model;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using August2008.Filters;
using August2008.Models;
using August2008;
using August2008.Common;
using System.Globalization;
using AutoMapper;
using August2008.Helpers;

namespace August2008.Controllers
{
    /// <summary>
    /// Orchestrates account related actions such as User Accouts, Logins, Authorization, etc.
    /// </summary>
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMetadataRepository _metadataRepository;

        /// <summary>
        /// Initializes controller with an instance of repository interface.
        /// </summary>
        public AccountController(IAccountRepository accountRepository, IMetadataRepository metadataRepository)
        {
            _accountRepository = accountRepository;
            _metadataRepository = metadataRepository;
        }
        /// <summary>
        /// Renders Login.cshtml view with logon options.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        /// <summary>
        /// Renders ExternalLoginsPartial.cshtml view with enabled OAuth provider options.
        /// </summary>
        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            // called from Login.cshtml
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("ExternalLoginsPartial", OAuthWebSecurity.RegisteredClientData);
        }
        /// <summary>
        /// Performs OAuth authentication with user-selected provider.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }
        /// <summary>
        /// Handles external OAuth provider's redirection callback.
        /// </summary>
        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            var result = OAuthWebSecurity.VerifyAuthentication();            
            ViewBag.ReturnUrl = returnUrl;            
            if (result.IsSuccessful)
            {
                var user = result.ToUser();
                int? userId;
                bool isOAuthUser;
                if (_accountRepository.TryGetUserRegistered(user.Email, result.Provider, out userId, out isOAuthUser)) 
                {
                    if (!isOAuthUser)
                    {
                        user.OAuth.UserId = userId;
                        _accountRepository.CreateOAuthUser(user.OAuth);
                    }
                    user = _accountRepository.GetUser(userId.Value);                     
                }
                else
                {
                    user = _accountRepository.CreateUser(user);
                    SiteHelper.SendEmail(ReplyEmail,
                        user.Email,
                        Resources.Global.Strings.EmailSubject,
                        Resources.Account.Strings.WelcomeRegistrationMessage);
                }
                Response.Cookies.Add(user.ToAuthCookie());
                return isOAuthUser ? RedirectToLocal(returnUrl) : View("ExternalLoginConfirmation", user.ToRegisterUser());
            }
            return RedirectToAction("LoginFailure");
        }
        /// <summary>
        /// Allows users to associtate their OpenID with local account.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLoginConfirmation(RegisterUser model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                _accountRepository.UpdateUser(model.ToUser());
                return RedirectToLocal(returnUrl);
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }
        [AllowAnonymous]
        public ActionResult LoginFailure()
        {
            return View();
        }
        private class ExternalLoginResult : ActionResult
        {
            // used by ExternalLogin action above
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            private string Provider { get; set; }
            private string ReturnUrl { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }
        /// <summary>
        /// Logs off user by removing a forms authentication ticket.
        /// </summary>
        [HttpPost]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
        /// <summary>
        /// Renders UserManagementPartial with initial information.
        /// </summary>
        [HttpGet]
        [NoCache]
        [Authorize2(Roles = "Admin")]
        public ActionResult ManageUsers()
        {
            var users = _accountRepository.SearchUsers();
            var model = new UserSearchModel
                {
                    Users = new List<UserModel>(),
                    AvailableRoles = _metadataRepository.GetRoles()
                };
            Mapper.Map(users, model.Users);
            return PartialView("ManageUsersPartial", model);
        }
        [HttpPost]
        [NoCache]
        [Authorize2(Roles = "Admin")]
        public ActionResult SearchUsers(string name)
        {
            var users = _accountRepository.SearchUsers(name);
            var model = new List<UserModel>();
            Mapper.Map(users, model);
            return PartialView("UserListPartial", model);
        }
        [HttpPost]
        [NoCache]
        [Authorize2(Roles = "Admin")]
        public ActionResult UserRoles(int userId)  
        {
            return GetUserRoles(userId);
        }
        [HttpPost]
        [NoCache]
        [Authorize2(Roles = "Admin")]
        public ActionResult AssignRoles(UserRoleModel model)
        {
            if (ModelState.IsValid)
            {
                var roles = _accountRepository.GetUserRoles(model.UserId);
                var assigned = (from r in roles
                                where !model.PostedRoles.IsNull() && model.PostedRoles.Contains(r.RoleId) && !r.UserId.HasValue
                                select r.RoleId).ToArray();
                var revoked = (from r in roles
                               where (model.PostedRoles.IsNull() || !model.PostedRoles.Contains(r.RoleId)) && r.UserId.HasValue
                               select r.RoleId).ToArray();
                if (!assigned.IsNullOrEmpty())
                {
                    _accountRepository.AssignUserToRoles(model.UserId, assigned);
                }
                if (!revoked.IsNullOrEmpty())
                {
                    _accountRepository.RevokeUserFromRoles(model.UserId, revoked);
                }
            }
            return GetUserRoles(model.UserId);
        }
        private ActionResult GetUserRoles(int userId)
        {
            var roles = _accountRepository.GetUserRoles(userId);
            var model = new UserRoleModel
                {
                    UserId = userId,
                    Roles = new List<RoleModel>()
                };
            Mapper.Map(roles, model.Roles);
            return PartialView("UserRolesPartial", model);
        }
    }
}