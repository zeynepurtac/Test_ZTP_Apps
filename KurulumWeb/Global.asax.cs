﻿using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace KurulumWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
