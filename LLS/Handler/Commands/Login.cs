#region Copyright
/*	
	 __      ________ _   _ _____ _____        
	 \ \    / /  ____| \ | |_   _|  __ \ /\    
	  \ \  / /| |__  |  \| | | | | |__) /  \   
	   \ \/ / |  __| | . ` | | | |  ___/ /\ \  
	    \  /  | |____| |\  |_| |_| |  / ____ \ 
	     \/   |______|_| \_|_____|_| /_/    \_\
                                        
	Copyright (c) 2017 - 2018 gitlab.com/Venipa
	Project: LLS
	User: Venipa .
	Filename: Login.cs
*/
#endregion 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LLS.Networking;
using LLS.Lib;
using LLS.Lib.Extensions;
using LLS.Database;
using LC = BCrypt.Net;
using LLS.Database.Extensions;
using System.Data.Entity.Migrations;

namespace LLS.Handler.Commands
{
    public class Login
    {
        #region AuthHandler
        [Command(RequestType.AUTH, typeof(AuthContext))]
        public static ResponseContext handleAuth(AuthContext auth, ClientHandler client)
        {
            Log.WriteLine(LogSeverity.Debug, "Auth Triggered! JSON: {0}", auth.ToJsonString());
            var db = new Database.Context();
            bool isAuthed = db.UserPrograms.Count(x => x.AuthKey == auth.Auth) > 0;
            client.IsAuthed = isAuthed;
            client.AuthKey = auth.Auth;
            if (isAuthed)
            {
                return new ResponseContext() { ResponseType = ResponseType.OK };
            }
            else
            {
                return new ResponseContext() { ResponseType = ResponseType.FAIL };
            }

        }
        #endregion
        #region LoginHandler
        [Command(RequestType.LOGIN, typeof(LoginContext))]
        public static ResponseContext handle(LoginContext login, ClientHandler client)
        {
            Log.WriteLine(LogSeverity.Debug, "Login Triggered! JSON: {0}", login.ToJsonString());
            var db = new Context();
            var user = db.Users.FirstOrDefault(x => x.email == login.Email);
            if (user == null) return new ResponseContext(ResponseType.USER_NOT_FOUND, 
                new MessageContext("User not Found!"));
            var licpo = db.Licenses.Where(v => v.UsedBy == user.id).FirstOrDefault(x => x.Program.AuthKey == client.AuthKey);
            user.UsedLicenses.ToList().ForEach(x =>
            {
                Log.WriteLine(LogSeverity.Debug, "License of (ID: {0}): Key({1}), Used({2})", user.id, x.key, x.IsUsed.ToString());
            });
            if (licpo == null) return new ResponseContext(ResponseType.LICENSE_NOT_FOUND,
                new MessageContext("User is not Authorized to use this Program."));
            var lic = licpo;
            
            if(!LC.BCrypt.Verify(login.Password, user.password))
            {
                return new ResponseContext(ResponseType.PASS_WRONG, 
                    new MessageContext("Wrong Password!"));
            }
            return new ResponseContext(ResponseType.OK, 
                new UserData()
            {
                IP = client.IPAddress.Address.ToString(),
                CreatedAt = user.CreatedAt,
                Email = user.email,
                License = new UserLicense()
                {
                    ExpiresAt = lic.UpdatedAt.AddTicks(lic.ExpiresIn.Ticks),
                    ExpiresInSeconds = (lic.UpdatedAt == lic.CreatedAt ? lic.ExpiresIn.Seconds : lic.UpdatedAt.AddTicks(lic.ExpiresIn.Ticks).Subtract(DateTime.Now).TotalSeconds.ToType<int>()),
                    Level = lic.Level,
                    IsLifetime = lic.IsLifetime
                },

            });
        }
        #endregion
        #region RegisterHandler
        [Command(RequestType.REGISTER, typeof(RegisterContext))]
        public static ResponseContext HandleRegister(RegisterContext register, ClientHandler client)
        {
            Log.WriteLine(LogSeverity.Debug, "Register Triggered! JSON: {0}", register.ToJsonString());
            var db = new Context();
            var lic = db.Licenses.FirstOrDefault(x => x.key == register.License && !x.IsUsed);
            if (lic == null)
            {
                return new ResponseContext(ResponseType.LICENSE_NOT_FOUND, new MessageContext("License not Found or its already used."));
            }
            if (db.Users.Count(x => x.email == register.Email && x.UsedLicenses.Count(y => y.key == register.License) > 0) > 0)
            {
                return new ResponseContext(ResponseType.USER_ALREADY_EXISTS, new MessageContext($"User {register.Username} already registered with this License."));
            }
            Users u = null;
            if (db.Users.Count(x => x.email == register.Email) <= 0)
            {
                u = new Users()
                {
                    email = register.Email,
                    password = LC.BCrypt.HashPassword(register.Password, LC.BCrypt.GenerateSalt(10)),
                    username = register.Username
                };
                u = db.Users.Add(u);
                db.SaveChanges();
            } else
            {
                u = db.Users.FirstOrDefault(x => x.email == register.Email);
                if(!LC.BCrypt.Verify(register.Password, u.password))
                {
                    return new ResponseContext(ResponseType.PASS_WRONG, new MessageContext("Invalid Password."));
                }
            }
            if (u == null) return new ResponseContext(ResponseType.USER_NOT_FOUND, new MessageContext("User not Found!"));
            Log.WriteLine(LogSeverity.Debug, "Updated License: Used ID>{0}", u.id);
            lic.IsUsed = true;
            lic.UsedBy = u.id;
            lic.UpdatedAt = DateTime.Now;
            db.Update(lic);
            db.SaveChanges();
            return new ResponseContext(ResponseType.OK);
        }
        #endregion

        #region ForgotPassword
        [Command(RequestType.FORGOTPASS_REQUEST, typeof(ForgotPasswordGetContext))]
        public static ResponseContext handleForgotPassword(ForgotPasswordGetContext forgot, ClientHandler client)
        {
            var db = new Context();
            var user = db.Users.FirstOrDefault(x => x.email == forgot.Email);
            if (user == null) return new ResponseContext(ResponseType.USER_NOT_FOUND, "User not Found!");
            var f = db.ForgotPassword.FirstOrDefault(x => x.ForUser == user.id && (x.CreatedAt - DateTime.Now).Minutes <= 30);
            if (f != null) return new ResponseContext(ResponseType.FORGOTPASS_LOCK, "You can only Reset your Password every 30 Minutes!");
            var fp = new ForgotPassword()
            {
                code = RandomString(20).ToUpper(),
                ForUser = user.id
            };
            fp = db.ForgotPassword.Add(fp);
            bool PushOK = db.SaveChanges() > 0;
            if(PushOK)
            {
                Mail m = new Mail(user.email);
                m.Send("Password Reset", $"Your Reset Code: {fp.code + Environment.NewLine}Regards Venipa");
            }
            return new ResponseContext(PushOK ? ResponseType.OK : ResponseType.FAIL);

        }
        [Command(RequestType.FORGOTPASS_VALIDATE, typeof(ForgotPasswordPostContext))]
        public ResponseContext handleForgotPasswordValidate(ForgotPasswordPostContext forgot, ClientHandler client)
        {
            var db = new Context();
            var user = db.Users.FirstOrDefault(x => x.email == forgot.Email);
            if (user == null) return new ResponseContext(ResponseType.USER_NOT_FOUND, "User not Found!");
            var fp = db.ForgotPassword.FirstOrDefault(x => x.code == forgot.ResetCode && !x.IsUsed);
            if (fp == null) return new ResponseContext(ResponseType.FORGOTPASS_CODE_NOT_FOUND, "Reset Code could not be Found in the Database!");
            if (fp.code != forgot.ResetCode) return new ResponseContext(ResponseType.FORGOTPASS_CODE_INVALID, "Invalid Reset Code used.");
            fp.IsUsed = true;
            fp.UpdatedAt = DateTime.Now;
            bool PushOK = db.SaveChanges() > 0;
            if(PushOK)
            {
                string pass = RandomString(8);
                user.password = LC.BCrypt.HashPassword(pass);
                PushOK = db.SaveChanges() > 0;
                if(PushOK)
                {
                    Mail m = new Mail(user.email);
                    m.Send("Password Reset", $"Your new Password: \"<b>{pass}</b>\" for User: {user.username}");
                } else
                {
                    return new ResponseContext(ResponseType.FAIL, "Could not update User Account!");
                }
            } else
            {
                return new ResponseContext(ResponseType.FAIL, "Could not update Forgot Password Entry!");
            }

            return new ResponseContext(ResponseType.OK);
        }
        #endregion

        #region HeartBeat
        [Command(RequestType.HEARTBEAT, typeof(HeartBeatContext))]
        public static ResponseContext HandleHeartBeat(HeartBeatContext hb, ClientHandler client)
        {
            return new ResponseContext(ResponseType.OK);
        }
        #endregion
        public static string RandomString(int length)
        {
            var random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            chars += chars.ToLower();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
