using KurulumWeb.Helper;
using KurulumWeb.Models;
using SMSApi.Api.Response;
using SNMPDB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KurulumWeb.Controllers
{
    public class LoginController : BaseController
    {
        
        private ZTP_TestEntities ctx = new ZTP_TestEntities();

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            var query = ctx.UserLogin45.FirstOrDefault(x => x.Username == UserName && x.Password == Password && x.Status == "1"); //textboxlardan girilen değere göre Login tablosundan kullanıcı getir

            if (query != null) //kullanıcı var ise
            {
                Session["UserID"] = query.User_ID.ToString(); //Id sini Sessionda tut
                Session["UserName"] = query.Username.ToString(); //Username Sessionda tut
                Session["LocationStatus"] = query.LocationStatus.ToString();
                Session["Company_Name"] = query.Company_Name;
                Session["Company_ID"] = query.Company_ID;
                Session["Company_Domain"] = query.Company_Domain;
                Session["IsAdmin"] = query.IsAdmin;
                if (query.IsAdmin == true)
                {
                    return RedirectToAction("RegisteredRouter", "Home");
                }
                else
                {
                    return RedirectToAction("Ricon", "Home");
                }
            }
            else
            {
                // Kimlik doğrulama başarısız, hata mesajını göster
                ViewBag.LoginError = CultureHelper.GetResourceKey("L116");
                return View();
            }
        }

        public ActionResult SetCulture(string culture)
        {
            CultureHelper.setCulture(culture);
            return RedirectToAction("Login");
        }
    }
}