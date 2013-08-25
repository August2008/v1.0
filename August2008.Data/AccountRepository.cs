﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using August2008.Common;
using August2008.Model;
using August2008.Common.Interfaces;
using System.Diagnostics;

namespace August2008.Data
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ILogger Logger;

        public AccountRepository(ILogger logger)
        {
            Logger = logger;
        }
        public User GetUser(int userId)
        {
            var user = new User();
            using (var db = new DataAccess())
            {
                db.CreateStoredProcCommand("dbo.GetUserByUserId");
                db.AddInParameter("@UserId", DbType.Int32, userId);
                try
                {
                    db.ReadInto(user,
                                user.Profile,
                                user.Profile.Lang,
                                user.OAuth,
                                user.Roles);
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while getting user.", ex);
                    throw;
                }
            }
            return user;
        }
        public bool TryGetUserRegistered(string email, string provider, out int? userId, out bool isOAuthUser)
        {
            userId = null;
            using (var db = new DataAccess())
            {
                db.CreateStoredProcCommand("dbo.GetUserRegistered");
                db.AddInParameter("@Email", DbType.String, email);
                db.AddInParameter("@Provider", DbType.String, provider);
                db.AddOutParameter("@UserId", DbType.Int32);
                db.AddOutParameter("@IsOAuthUser", DbType.Boolean);
                try
                {
                    db.ExecuteNonQuery();
                    userId = db.GetParameterValue<int?>("@UserId");
                    isOAuthUser = db.GetParameterValue<bool>("@IsOAuthUser");
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while checking registered user.", ex);
                    throw;
                }
            }
            return userId.HasValue;
        }
        public User CreateUser(User user)
        {
            using (var tran = new DbTransactionManager())
            {
                tran.BeginTransaction();
                using (var db = new DataAccess(tran))
                {
                    try
                    {
                        db.CreateStoredProcCommand("dbo.CreateUser");
                        db.AddInParameter("@Email", DbType.String, user.Email);
                        db.AddInParameter("@DisplayName", DbType.String, user.DisplayName);
                        db.AddOutParameter("@UserId", DbType.Int32);
                        db.ExecuteNonQuery();
                        user.UserId = db.GetParameterValue<int>("@UserId");

                        user.OAuth.UserId = user.UserId;
                        user.OAuth = CreateOAuthUser(user.OAuth, tran);

                        db.CreateStoredProcCommand("dbo.CreateUserProfile");
                        db.AddInParameter("@UserId", DbType.String, user.UserId);
                        db.AddInParameter("@LanguageId", DbType.String, user.Profile.Lang.LanguageId);
                        db.AddInParameter("@Dob", DbType.String, user.Profile.Dob);
                        db.AddInParameter("@Nationality", DbType.String, user.Profile.Nationality);
                        db.AddOutParameter("@UserProfileId", DbType.Int32);
                        db.ExecuteNonQuery();
                        user.Profile.UserProfileId = db.GetParameterValue<int>("@UserProfileId");

                        user = GetUser(user.UserId);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Logger.Error("Error while creating user.", ex);
                        throw;
                    }
                    return user;
                }
            }
        }
        public OAuthUser CreateOAuthUser(OAuthUser user)
        {
            using (var tran = new DbTransactionManager())
            {
                try
                {
                    tran.BeginTransaction();
                    user = CreateOAuthUser(user, tran);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    Logger.Error("Error while creating OAuth user.", ex);
                    throw;
                }
            }
            return user;
        }
        private OAuthUser CreateOAuthUser(OAuthUser user, DbTransactionManager tran)
        {
            using (var db = new DataAccess(tran))
            {
                db.CreateStoredProcCommand("dbo.CreateOAuthUser");
                db.AddInParameter("@UserId", DbType.String, user.UserId);
                db.AddInParameter("@Email", DbType.String, user.Email);
                db.AddInParameter("@ProviderId", DbType.String, user.ProviderId);
                db.AddInParameter("@ProviderName", DbType.String, user.ProviderName);
                db.AddInParameter("@ProviderData", DbType.Xml, user.ProviderData.ToDbXml());
                db.AddOutParameter("@OAuthUserId", DbType.Int32);
                try
                {
                    db.ExecuteNonQuery();
                    user.OAuthUserId = db.GetParameterValue<int>("@OAuthUserId");
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return user;
        }
        public void UpdateUser(User user)
        {
            using (var tran = new DbTransactionManager())
            {
                tran.BeginTransaction();
                using (var db = new DataAccess(tran))
                {
                    db.CreateStoredProcCommand("dbo.UpdateUser");
                    db.AddInParameter("@UserId", DbType.Int32, user.UserId);
                    db.AddInParameter("@Email", DbType.String, user.Email);
                    db.AddInParameter("@DisplayName", DbType.String, user.DisplayName);
                    try
                    {
                        db.ExecuteNonQuery();

                        if (user.Profile != null)
                        {
                            UpdateUserProfile(user.Profile);
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Logger.Error("Error while updating user.", ex);
                        throw;
                    }
                }
            }
        }
        public void UpdateUserProfile(UserProfile profile)
        {
            using (var db = new DataAccess())
            {
                db.CreateStoredProcCommand("dbo.UpdateUserProfile");
                db.AddInParameter("@UserId", DbType.Int32, profile.UserId);
                db.AddInParameter("@LanguageId", DbType.String, profile.Lang.LanguageId);
                db.AddInParameter("@Dob", DbType.String, profile.Dob);
                db.AddInParameter("@Nationality", DbType.String, profile.Nationality);
                try
                {
                    db.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while updating user profile.", ex);
                    throw;
                }
            }
        }
        public void UpdateUserProfileAddress(int userId, Address address)
        {
            using (var db = new DataAccess())
            {
                db.CreateStoredProcCommand("dbo.UpdateUserProfileAddress");                
                db.AddInParameter("@UserId", DbType.Int32, userId);
                db.AddInParameter("@Street", DbType.String, address.Street);
                db.AddInParameter("@CityId", DbType.String, address.CityId);
                db.AddInParameter("@StateId", DbType.String, address.StateId);
                db.AddInParameter("@CountryId", DbType.String, address.CountryId);
                db.AddInParameter("@Latitude", DbType.Double, address.Latitude);
                db.AddInParameter("@Longitude", DbType.Double, address.Longitude);
                db.AddInParameter("@PostalCode", DbType.String, address.PostalCode);
                try
                {
                    db.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while updating user profile address.", ex);
                    throw;
                }
            }
        }
        public IEnumerable<User> GetUsers()
        {
            using (var db = new DataAccess())
            {
                try
                {
                    db.CreateStoredProcCommand("dbo.GetUsers");
                    var users = new List<User>();
                    db.ReadInto(users);
                    return users;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while getting user.", ex);
                    throw;
                }
            }
        }
        public IEnumerable<User> SearchUsers(string name = null)
        {
            using (var db = new DataAccess())
            {
                db.CreateStoredProcCommand("dbo.SearchUsers");
                db.AddInParameter("@StartsWith", DbType.String, name);
                try
                {
                    var users = new List<User>();
                    db.ReadInto(users);
                    return users;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while searching users.", ex);
                    throw;
                }
            }
        }
        public IEnumerable<Role> GetUserRoles(int userId)
        {
            using (var db = new DataAccess())
            {
                try
                {
                    db.CreateStoredProcCommand("dbo.GetUserRoles");
                    db.AddInParameter("@UserId", DbType.Int32, userId);
                    var roles = new List<Role>();
                    db.ReadInto(roles);
                    return roles;
                }
                catch (Exception ex)
                {
                    Logger.Error("Error while getting user roles.", ex);
                    throw;
                }
            }
        }
        public void AssignUserToRoles(int userId, IEnumerable<int> roles)
        {
            using (var tran = new DbTransactionManager())
            {
                tran.BeginTransaction();
                using (var db = new DataAccess(tran))
                {
                    try
                    {
                        db.CreateStoredProcCommand("dbo.AssignUserToRole");
                        foreach (var id in roles)
                        {
                            db.AddInParameter("@UserId", DbType.Int32, userId);
                            db.AddInParameter("@RoleId", DbType.Int32, id);
                            db.ExecuteNonQuery();
                            db.ResetCommand(false);
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Logger.Error("Error while assigning roles.", ex);
                        throw;
                    }
                }
            }
        }
        public void RevokeUserFromRoles(int userId, IEnumerable<int> roles)
        {
            using (var tran = new DbTransactionManager())
            {
                tran.BeginTransaction();
                using (var db = new DataAccess(tran))
                {
                    try
                    {
                        db.CreateStoredProcCommand("dbo.RevokeUserFromRole");
                        foreach (var id in roles)
                        {
                            db.AddInParameter("@UserId", DbType.Int32, userId);
                            db.AddInParameter("@RoleId", DbType.Int32, id);
                            db.ExecuteNonQuery();
                            db.ResetCommand(false);
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        Logger.Error("Error while revoking roles.", ex);
                        throw;
                    }
                }
            }
        }
    }
}
