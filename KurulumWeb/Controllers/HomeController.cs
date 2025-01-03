using KurulumWeb.Helper;
using KurulumWeb.Models;
using OfficeOpenXml;
using SNMPDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace KurulumWeb.Controllers
{
    public class HomeController : BaseController
    {
        private MethodAzure a = new MethodAzure();
        private ZTP_TestEntities ctx = new ZTP_TestEntities();

        [HttpPost]
        public JsonResult AddUser(string Username, string Password, string Type, string Companyname, string CompanyDomain)
        {
            string sessionCompanyName = Session["Company_Name"]?.ToString(); // Oturumdan şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == sessionCompanyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            try
            {
                string creator = Session["UserName"]?.ToString() ?? "System";
                string createDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                bool isAdmin = Type?.ToLower() == "admin";

                // Şirket adı kontrolü
                if (string.IsNullOrWhiteSpace(Companyname))
                {
                    return Json(new { success = false, message = "Company name is required." });
                }

                if (IsPasswordSecure(Password))
                {
                    UserLoginModel45 newUser = new UserLoginModel45
                    {
                        Username = Username,
                        Password = Password,
                        IsAdmin = isAdmin,
                        Creator = creator,
                        CreateDate = createDateTime,
                        Status = "1",
                        LocationStatus = "0"
                    };

                    using (var ctx = new SNMPDB.ZTP_TestEntities())
                    {
                        if (companyId == 2)
                        {
                            // Eğer ID 2 ise yeni şirket ekle
                            var companyEntity = new SNMPDB.Company
                            {
                                Company_Name = Companyname,
                                Company_Domain = CompanyDomain
                            };
                            ctx.Company.Add(companyEntity);
                            ctx.SaveChanges();
                            newUser.Company_ID = companyEntity.Company_ID;
                        }
                        else
                        {
                            // ID 2 değilse oturumdan alınan şirket bilgilerini kullan
                            var existingCompany = ctx.Company.FirstOrDefault(c => c.Company_Name == sessionCompanyName);
                            if (existingCompany == null)
                            {
                                return Json(new { success = false, message = "Company not found." });
                            }
                            newUser.Company_ID = existingCompany.Company_ID;
                            newUser.Company_Name = existingCompany.Company_Name;
                            newUser.Company_Domain = existingCompany.Company_Domain;
                        }

                        SNMPDB.UserLogin45 userLoginEntity = newUser.ToUserLogin();
                        ctx.UserLogin45.Add(userLoginEntity);
                        ctx.SaveChanges();

                        return Json(new { success = true, message = "User added successfully." });
                    }
                }
                return Json(new { success = false, message = Resources.Resources.L338 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding a user: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteData(string GMS_No)
        {
            try
            {
                var dataToDelete = ctx.Basic_Setup.FirstOrDefault(s => s.GMS_No == GMS_No);

                if (dataToDelete != null)
                {
                    dataToDelete.Status = 0; // Silme yerine durumu 0 olarak işaretle
                    ctx.SaveChanges();

                    return Json(new { success = true, message = Resources.Resources.L331 });
                }
                else
                {
                    return Json(new { success = false, message = "No Data." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteUser(int User_ID)
        {
            try
            {
                // User_ID'ye göre veritabanındaki kullanıcıyı bulun ve durumunu 0 olarak güncelleyin
                var user = ctx.UserLogin45.SingleOrDefault(u => u.User_ID == User_ID);
                if (user != null)
                {
                    user.Status = "0"; // Veya kullanıcıyı veritabanından tamamen kaldırabilirsiniz.
                    ctx.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Deploy(List<AllModel> selectedData)
        {
            // Şirket bilgisi alınır
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (selectedData == null || selectedData.Count == 0)
            {
                return Json(new { success = false, message = "Seçili veri bulunamadı." });
            }

            foreach (var item in selectedData)
            {
                // Loglama işlemi
                Debug.WriteLine($"GMS_No: {item.Gsm_No}, APN_Name: {item.SeriNo}");

                var kurulumViewModel = new KurulumViewModel
                {
                    SelectedGsmNoID = item.GMS_No,
                    SelectedSeriNoID = item.Ricon_SN,
                };

                var result = Setup(kurulumViewModel);
                if (!string.IsNullOrEmpty(ViewBag.StartOKMessage))
                {
                    return Json(new
                    {
                        success = true,
                        startOKMessage = ViewBag.StartOKMessage,
                        infoTitle = ViewBag.InfoTitle,
                        result = ViewBag.Sonuc
                    });
                }
                if (!string.IsNullOrEmpty(ViewBag.CaseMessageYesNo))
                {
                    return Json(new
                    {
                        success = true,
                        infoTitle = ViewBag.InfoTitle,
                        caseMessageYesNo = ViewBag.CaseMessageYesNo,
                        result = ViewBag.Sonuc
                    });
                }
            }

            return Json(new { success = true, message = "Veriler başarıyla gönderildi." });
        }

        [HttpPost]
        public ActionResult Deploy2(List<AllModel> selectedData)
        {
            if (selectedData != null && selectedData.Count > 0)
            {
                // Çoklu seçim için KurulumViewModel listesini hazırlıyoruz
                var kurulumViewModel = new KurulumViewModel
                {
                    SelectedSeriNoIDs = selectedData.Select(x => x.Ricon_SN).ToList(),
                    SelectedGsmNoIDs = selectedData.Select(x => x.GMS_No).ToList(),
                    SelectedLocationIDs = selectedData.Where(x => !string.IsNullOrEmpty(x.SiteName))
                                                       .Select(x => x.SiteName).ToList()
                };

                // Setup2 metodunu çağırıyoruz
                var result2 = Setup2(kurulumViewModel);

                if (!string.IsNullOrEmpty(ViewBag.CaseMessageYesNo2))
                {
                    return Json(new
                    {
                        success = true,
                        infoTitle = ViewBag.InfoTitle,
                        caseMessageYesNo2 = ViewBag.CaseMessageYesNo2,
                        result2 = ViewBag.Sonuc
                    });
                }
            }

            // Seçim yapılmadıysa ya da işlem tamamlandıysa başarı mesajı dönüyoruz
            return Json(new { success = true, message = "Veriler başarıyla gönderildi." });
        }

        public JsonResult DeviceInstallationData()
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();

                // Kullanıcının Company_ID'sine göre verileri filtrele
                var query = ctx.Install
                    .Where((i => i.Company_ID == companyId || companyId == 2)) // Company_ID 2 olanlar tüm verileri görecek
                    .OrderByDescending(x => x.Install_ID)
                    .ToList();

                return Json(query, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeviceSetupData(int draw, int start, int length, int page, int pageSize, string search)
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();
            var skip = (page - 1) * pageSize;

            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();

                // Kullanıcının Company_ID'sine göre verileri filtrele
                var query = ctx.Basic_Setup
                    .Where(u => u.Status == 1 &&
                        (companyId == 2 || u.Company_ID == companyId) && // Company_ID 2 olanlar tüm verileri görecek
                        (search == null ||
                        u.GMS_No.ToLower().Contains(search) ||
                        u.APN_Name.ToLower().Contains(search) ||
                        (u.DHCP_Status != null && u.DHCP_Status.Value.ToString().ToLower().Contains(search) ||
                        u.DHCP_Start.ToLower().Contains(search.ToLower()) ||
                        (u.DHCP_User != null && u.DHCP_User.ToString().ToLower().Contains(search)) ||
                        u.LAN_IP.ToLower().Contains(search) ||
                        u.LAN_Subnet.ToLower().Contains(search) ||
                        u.Serial_no.ToLower().Contains(search) ||
                        (u.Datetime != null && u.Datetime.Value.ToString().ToLower().Contains(search)))))
                    .OrderByDescending(x => x.ID)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                // Toplam kayıt sayısını ve filtrelenmiş kayıt sayısını al
                var totalRecords = ctx.Basic_Setup.Count(u => u.Status == 1 && (companyId == 2 || u.Company_ID == companyId)); // Kullanıcının görme yetkisine göre toplam kayıt sayısı
                var filteredRecords = totalRecords; // Filtreleme yapılmadığı için toplam kayıt sayısıyla aynı

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords, // Toplam kayıt sayısı
                    recordsFiltered = filteredRecords, // Filtrelenmiş kayıt sayısı
                    data = query
                });
            }

            return Json(null);
        }

        public ActionResult DownloadExcel()
        {
            var fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/InstallData.xlsx"));
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InstallData.xlsx");
        }

        public ActionResult EditUser(UserViewModel usermodel)
        {
            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();
                DateTime islemSaati = DateTime.Now;

                // Kullanıcıyı veri tabanından al
                var userdata = ctx.UserLogin45.FirstOrDefault(u => u.User_ID == usermodel.User_ID);
                if (userdata != null)
                {
                    // Kullanıcının özelliklerini güncelle
                    userdata.Username = usermodel.Username;
                    userdata.Password = usermodel.Password;
                    userdata.IsAdmin = usermodel.IsAdmin;
                    userdata.Creator = kullanici;
                    userdata.Create_DateTime = islemSaati;

                    // Değişiklikleri veri tabanına kaydet
                    ctx.SaveChanges();

                    TempData["SuccessMessage"] = "User information has been successfully updated.";
                    return RedirectToAction("ManageAccounts");
                }
            }

            return RedirectToAction("Login"); // Kullanıcıyı başka bir sayfaya yönlendir
        }

        [HttpPost]
        public ActionResult ExportToExcel()
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            try
            {
                // Kullanıcının Company_ID'sine göre verileri çekin
                List<Install> installData = ctx.Install
                    .Where((i => i.Company_ID == companyId || companyId == 2)) // Company_ID 2 olanlar tüm verileri görecek
                    .OrderByDescending(x => x.Install_ID)
                    .ToList();

                // Excel veri modeli oluşturun
                ExcelDataModel excelModel = new ExcelDataModel
                {
                    Headers = new List<string>
                    {
                        "Install_ID", "Ricon_SN", "GSM_No", "Site_Name", "WAN_ip", "Operator", "Username", "Company_ID", "Date_Time"
                    },
                    Rows = installData.Select(install => new List<string>
                    {
                        install.Install_ID.ToString(),
                        install.Ricon_SN,
                        install.GSM_No,
                        install.Site_Name,
                        install.WAN_ip,
                        install.Username,
                        install.Company_ID.ToString(),
                        install.Date_Time.ToString()
                    }).ToList()
                };

                // Yeni bir ExcelPackage oluşturun
                using (var package = new ExcelPackage())
                {
                    var workbook = package.Workbook;
                    var worksheet = workbook.Worksheets.Add("InstallData");

                    // Sütun başlıklarını ekleyin
                    for (int i = 0; i < excelModel.Headers.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = excelModel.Headers[i];
                    }

                    // Veri satırlarını ekleyin
                    for (int i = 0; i < excelModel.Rows.Count; i++)
                    {
                        for (int j = 0; j < excelModel.Rows[i].Count; j++)
                        {
                            worksheet.Cells[i + 2, j + 1].Value = excelModel.Rows[i][j];
                        }
                    }

                    // Dosyayı sunucuda oluşturun
                    byte[] excelBytes = package.GetAsByteArray();
                    System.IO.File.WriteAllBytes(Server.MapPath("~/Content/InstallData.xlsx"), excelBytes);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GetConfirm()
        {
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            string kullanici = Session["UserName"]?.ToString();
            string company_id = companyId.ToString();
            string secilen_deviceType = Session["DeviceType"]?.ToString();

            string secilen_serino = Session["SelectedSeriNoID"]?.ToString();
            string secilen_gsm = Session["SelectedGsmNoID"]?.ToString();

            var oldlan = ctx.Basic_Setup.FirstOrDefault(x => x.GMS_No == secilen_gsm);
            string lanold = oldlan?.LAN_IP;
            Install install_query = new Install();
            bool deger = false;

            // Seri numarasına göre kayıtları sil
            var serinoRecords = ctx.Install.Where(x => x.Ricon_SN == secilen_serino).ToList();
            ctx.Install.RemoveRange(serinoRecords);
            ctx.SaveChanges();

            // GSM numarasına göre kayıtları sil
            var gsmRecords = ctx.Install.Where(x => x.GSM_No == secilen_gsm).ToList();
            ctx.Install.RemoveRange(gsmRecords);
            ctx.SaveChanges();

            deger = a.ConfigAzure(secilen_gsm, secilen_deviceType, secilen_serino, ViewBag); // Seçilen GSM no için Config mesajları gönderiliyor.

            if (deger) // Config metodundan dönen değer true ise
            {
                int lastindex = 1;
                var maxInstallID = ctx.Install.Max(p => (int?)p.Install_ID);

                // Eğer maxInstallID null değilse, en büyük değeri kullan
                if (maxInstallID.HasValue)
                {
                    lastindex = maxInstallID.Value;
                }

                if (!ctx.Install.Any(x => x.Ricon_SN == secilen_serino || x.GSM_No == secilen_gsm))
                {
                    install_query.Username = kullanici;
                    install_query.Ricon_SN = secilen_serino;
                    install_query.GSM_No = secilen_gsm;
                    install_query.Date_Time = DateTime.Now;
                    install_query.Company_ID = Convert.ToInt32(company_id);
                    install_query.Install_ID = (lastindex + 1);

                    ctx.Install.Add(install_query);
                    ctx.SaveChanges();

                    if (oldlan != null)
                    {
                        oldlan.Serial_no = secilen_serino;
                        oldlan.LAN_Eski = lanold; // Seri numarasını ata
                        oldlan.Datetime = DateTime.Now;
                        oldlan.UserName = kullanici;
                        ctx.SaveChanges(); // Değişiklikleri kaydet
                    }
                }
                var basicSetupRecord = ctx.Basic_Setup.FirstOrDefault(x => x.GMS_No == secilen_gsm);
                if (basicSetupRecord != null)
                {
                    basicSetupRecord.Flag = 1; // Başarılı durum için Flag'ı 1 yap
                    ctx.SaveChanges(); // Değişiklikleri kaydet
                }
                // Sonuç mesajını oluştur
                string sonucMesaj = "<b>SerialNumber:</b> " + secilen_serino;

                // Lokasyon bilgisi kontrolü
                if (!string.IsNullOrEmpty(Session["SelectedLocationID"]?.ToString()))
                {
                    sonucMesaj += "<br /><b>Location:</b> " + Session["SelectedLocationID"]; // Lokasyon varsa ekle
                }

                sonucMesaj += "<br /><b>Gsm No:</b> " + secilen_gsm + "<br /><br />" + CultureHelper.GetResourceKey("L339") + "<br /><br /><br />" + CultureHelper.GetResourceKey("L124");

                Session["Sonuc"] = sonucMesaj;

                ViewBag.Sonuc = sonucMesaj;
                return Json(new
                { success = true, result = ViewBag.Sonuc });
            }
            else
            {
                var basicSetupRecord = ctx.Basic_Setup.FirstOrDefault(x => x.GMS_No == secilen_gsm);
                if (basicSetupRecord != null)
                {
                    basicSetupRecord.Flag = 0; // Başarılı durum için Flag'ı 1 yap
                    ctx.SaveChanges(); // Değişiklikleri kaydet
                }

                Session["Sonuc"] = CultureHelper.GetResourceKey("L125");
                ViewBag.Sonuc = CultureHelper.GetResourceKey("L125");

                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult GetConfirm2()
        {
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            string kullanici = Session["UserName"]?.ToString();
            string company_id = companyId.ToString();
            string secilen_deviceType = Session["DeviceType"]?.ToString();

            var selectedSeriNos = TempData["SelectedSeriNos"] as List<string>;
            var selectedGsmNos = TempData["SelectedGsmNos"] as List<string>;

            var errorMessages = new List<string>();
            var resultMessages = new List<string>(); // Sonuç mesajlarını toplamak için liste

            if (selectedSeriNos != null && selectedGsmNos != null)
            {
                var dataToSend = new List<dynamic>();

                for (int i = 0; i < selectedSeriNos.Count; i++)
                {
                    string serino = selectedSeriNos[i];
                    string gsmNo = selectedGsmNos[i];

                    var oldlan = ctx.Data.FirstOrDefault(x => x.GMS_No == gsmNo);
                    string lanold = oldlan?.LAN_IP;

                    dataToSend.Add(new
                    {
                        GSM_No = gsmNo,
                        DeviceType = secilen_deviceType,
                        SerialNumber = serino,
                        UserName = kullanici,
                        CompanyID = company_id,
                        Location = Session["SelectedLocationID"]?.ToString()
                    });
                }

                var result = a.ConfigAzure2(dataToSend, ViewBag);

                for (int i = 0; i < selectedSeriNos.Count; i++)
                {
                    string serino = selectedSeriNos[i];
                    string gsmNo = selectedGsmNos[i];
                    var oldlan = ctx.Basic_Setup.FirstOrDefault(x => x.GMS_No == gsmNo);
                    string lanold = oldlan?.LAN_IP;

                    // Sözlükten her seri numarasının durumunu kontrol et
                    if (result.ContainsKey(serino))
                    {
                        bool isSuccess = result[serino];
                        Console.WriteLine($"Seri Numarası: {serino}, Durum: {isSuccess}");

                        if (isSuccess) // Eğer işlem başarılıysa
                        {
                            int lastindex = 1;
                            var maxInstallID = ctx.Install.Max(p => (int?)p.Install_ID);

                            if (maxInstallID.HasValue)
                            {
                                lastindex = maxInstallID.Value;
                            }

                            if (!ctx.Install.Any(x => x.Ricon_SN == serino || x.GSM_No == gsmNo))
                            {
                                Install install_query = new Install
                                {
                                    Username = kullanici,
                                    Ricon_SN = serino,
                                    GSM_No = gsmNo,
                                    Date_Time = DateTime.Now,
                                    Company_ID = Convert.ToInt32(company_id),
                                    Install_ID = (lastindex + 1)
                                };

                                ctx.Install.Add(install_query);
                                ctx.SaveChanges(); // Değişiklikleri kaydet

                                if (oldlan != null)
                                {
                                    oldlan.Serial_no = serino;
                                    oldlan.LAN_Eski = lanold;
                                    oldlan.Datetime = DateTime.Now;
                                    oldlan.UserName = kullanici;
                                    ctx.SaveChanges();
                                }
                            }

                            var basicSetupRecord = ctx.Basic_Setup.FirstOrDefault(x => x.GMS_No == gsmNo);
                            if (basicSetupRecord != null)
                            {
                                basicSetupRecord.Flag = 1; // Başarılı durum için Flag'ı 1 yap
                                ctx.SaveChanges();
                            }

                            string sonucMesaj = $"<b>SerialNumber:</b> {serino}";
                            if (!string.IsNullOrEmpty(Session["SelectedLocationID"]?.ToString()))
                            {
                                sonucMesaj += $"<br /><b>Location:</b> {Session["SelectedLocationID"]}";
                            }

                            sonucMesaj += $"<br /><b>Gsm No:</b> {gsmNo}<br /><br />" + CultureHelper.GetResourceKey("L339") + "<br /><br /><br />" + CultureHelper.GetResourceKey("L124");

                            resultMessages.Add(sonucMesaj); // Başarılı mesajları ekle
                        }
                        else
                        {
                            var basicSetupRecord = ctx.Basic_Setup.FirstOrDefault(x => x.GMS_No == gsmNo);
                            if (basicSetupRecord != null)
                            {
                                basicSetupRecord.Flag = 0;
                                ctx.SaveChanges();
                            }

                            errorMessages.Add($"Seri Numarası {serino} için sonuç bulunamadı.");
                        }
                    }
                    else
                    {
                        errorMessages.Add($"Seri Numarası {serino} için sonuç bulunamadı.");
                    }
                }
            }
            // Sonuçları döndürme
            if (errorMessages.Count > 0)
            {
                Session["Sonuc"] = string.Join("<br />", errorMessages);
                ViewBag.Sonuc = string.Join("<br />", errorMessages);
                return Json(new { success = false, result = ViewBag.Sonuc });
            }
            else
            {
                Session["Sonuc"] = string.Join("<br />", resultMessages);
                ViewBag.Sonuc = string.Join("<br />", resultMessages);
                return Json(new { success = true, result = ViewBag.Sonuc });
            }
        }

        public JsonResult GetDeviceSetupData(int draw, int start, int length, string search)
        {
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();

                // Şirket ID'sine göre filtre ekle
                var query = ctx.Basic_Setup
                    .Where(u => u.Status == 1 &&
                        (companyId == 2 || u.Company_ID == companyId) && // Şirket ID 2 ise tüm verileri gösterir
                        (string.IsNullOrEmpty(search) ||
                        u.GMS_No.ToLower().Contains(search.ToLower()) ||
                        u.Serial_no.ToLower().Contains(search.ToLower()) ||
                        (u.Datetime != null && u.Datetime.Value.ToString().ToLower().Contains(search.ToLower())) ||
                        (u.Flag == 1 && Resources.Resources.L412.ToLower().Contains(search.ToLower())) || // "Başarılı" durumu
                        (u.Flag == 0 && Resources.Resources.L413.ToLower().Contains(search.ToLower()))) // "Başarısız" durumu
                    )
                    .OrderByDescending(x => x.ID)
                    .Skip(start)
                    .Take(length)
                    .ToList();

                int recordsTotal = ctx.Basic_Setup.Count(u => u.Status == 1 && (companyId == 2 || u.Company_ID == companyId)); // Toplam kayıt sayısı
                int recordsFiltered = query.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = query.Select(u => new
                    {
                        u.GMS_No,
                        u.Serial_no,
                        Datetime = u.Datetime != null ? u.Datetime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        Status = u.Flag == 1 ? Resources.Resources.L412 : Resources.Resources.L413
                    }).ToList()
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(null);
        }

        [HttpGet]
        public ActionResult GetFilteredGSMNOList(string selectedValue)
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (Session["UserName"] != null)
            {
                string userName = Session["UserName"].ToString();

                if (userName != null)
                {
                    // Kullanıcının Company_ID'sine göre uygun GSM No'larını al
                    var allGSM = ctx.GsmNumber
                        .Where(x => (companyId == 2 || x.Company_ID == companyId) && x.Status == 1) // Company_ID 2 ise tüm GSM numaralarını al, aksi halde sadece kendi company'sini al
                        .Select(a => new SelectListItem
                        {
                            Text = a.GSM_No ?? "",
                            Value = a.GSM_No ?? ""
                        })
                        .ToList();

                    // Eğer bir selectedValue varsa, seçilen değeri filtrele
                    if (!string.IsNullOrEmpty(selectedValue))
                    {
                        allGSM = allGSM.Where(x => x.Text.Contains(selectedValue)).ToList();
                    }

                    return Json(allGSM, JsonRequestBehavior.AllowGet);
                }
            }

            // Oturum yoksa veya kullanıcı adı null ise veya filtreleme başarısız olursa null döndür
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetFilteredLocationList(string selectedValue)
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            // Kullanıcının Company_ID'sine göre uygun Location'ları al
            var allLocation = ctx.Sites
                .Where(x => (companyId == 2 || x.Company_ID == companyId) && x.Status == 1) // Company_ID 2 ise tüm Location'ları al, aksi halde sadece kendi company'sini al
                .Select(a => new SelectListItem
                {
                    Text = a.Site_Name,
                    Value = a.Site_Name
                })
                .ToList();

            // Eğer bir selectedValue varsa, seçilen değeri filtrele
            if (!string.IsNullOrEmpty(selectedValue))
            {
                selectedValue = selectedValue.ToLower(); // Küçük harf yap
                allLocation = allLocation.Where(x => x.Text.ToLower().Contains(selectedValue)).ToList();
            }

            return Json(allLocation, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetFilteredSeriNoList(string selectedValue)
        {
            // Şirket adını oturumdan al ve şirket ID'sini getir
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (companyId == 0)
            {
                return Json(new { success = false, message = "Şirket bilgisi eksik." }, JsonRequestBehavior.AllowGet);
            }

            // Device ve Registered_Router tablolarından ilgili verileri getir
            var allSerino = (from d in ctx.Device
                             join r in ctx.Registered_Router on d.Ricon_SN equals r.Ricon_SN
                             where d.Status == 1 // Durumu aktif olan cihazları al
                             select new
                             {
                                 Ricon_SN = d.Ricon_SN,
                                 Device_Type = d.Device_Type,
                                 Company_ID = d.Company_ID // Şirket ID'sini de al
                             }).ToList();

            // Eğer companyId 2 ise tüm seri numaralarını al, değilse kendi şirketine ait olanları al
            if (companyId != 2)
            {
                allSerino = allSerino
                    .Where(x => x.Company_ID == companyId) // Kendi şirketine ait olanları filtrele
                    .ToList();
            }

            // Seçilen değere göre filtreleme
            if (!string.IsNullOrEmpty(selectedValue))
            {
                selectedValue = selectedValue.ToLower(); // Küçük harfe çevir
                allSerino = allSerino
                    .Where(x => x.Ricon_SN.ToLower().Contains(selectedValue))
                    .ToList();
            }

            // SelectListItem olarak sonuç listesi oluştur
            var result = allSerino.Select(x => new
            {
                Text = x.Ricon_SN,
                Value = x.Ricon_SN,
                Device_Type = x.Device_Type
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetGsmNoBySeriNo(string seriNo)
        {
            // Şirket adını oturumdan al ve şirket ID'sini getir
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (companyId == 0)
            {
                return Json(new { success = false, message = "Şirket bilgisi eksik." }, JsonRequestBehavior.AllowGet);
            }

            // Veritabanından seçilen seri numarasına göre şirket ve GSM numarasını al
            var gsmNo = ctx.Registered_Router
                           .Where(d => d.Ricon_SN == seriNo && (companyId == 2 || d.Company_Name == companyName)) // Şirket ID 2 ise tüm verileri al, değilse kendi şirketine ait verileri al
                           .Select(d => d.GSM_No)
                           .FirstOrDefault();

            return Json(new { gsmNo = gsmNo }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInstallRatio()
        {
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (Session["UserName"] != null)
            {
                // Cihaz sayısını al
                int registeredCount = companyId == 2 ? ctx.Registered_Router.Count() : ctx.Registered_Router.Count(x => x.Company_Name == companyName);

                // Basic setup sayısını al
                int basicCount = companyId == 2 ? ctx.Basic_Setup.Count() : ctx.Basic_Setup.Count(x => x.Company_ID == companyId);

                // Install sayısını al
                int installCount = companyId == 2 ? ctx.Install.Count() : ctx.Install.Count(x => x.Company_ID == companyId);

                // Oran hesaplaması
                var ratio = registeredCount > 0 ? (installCount * 100) / registeredCount : 0; // Divide by zero kontrolü

                // Sonucu JSON formatında döndür
                var data = new
                {
                    RegisteredCount = registeredCount,
                    BasicCount = basicCount,
                    InstallCount = installCount,
                    Ratio = ratio
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetOperatorDetails(string gsmNo)
        {
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault(); // İlk eşleşen kaydı al, yoksa varsayılan (0) döner

            try
            {
                // Eğer companyId 2 ise, tüm verileri çek
                var data = ctx.Install
                    .Where(x => x.GSM_No == gsmNo && (x.Company_ID == companyId || companyId == 2))
                    .Select(x => new
                    {
                        Ricon_SN = x.Ricon_SN,
                        GSM_No = x.GSM_No,
                        Site_Name = x.Site_Name,
                        Username = x.Username,
                        Date_Time = x.Date_Time.ToString()
                    })
                    .ToList();

                if (data.Any())
                {
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "No data found." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSiteData(string sitename)
        {
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (Session["UserName"] != null)
            {
                var siteData = ctx.Sites.FirstOrDefault(x => x.Site_Name == sitename && x.Status == 1);
                if (siteData != null && (companyId == 2 || siteData.Company_ID == companyId)) // Kullanıcının Company_ID'sine göre kontrol
                {
                    var data = new
                    {
                        Site_Name = siteData.Site_Name,
                        Company_ID = siteData.Company_ID,
                    };

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ManageAccounts()
        {
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            // Oturum kontrolü yapılır, eğer kullanıcı oturum açmışsa devam edilir.
            if (Session["UserName"] != null && Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
            {
                if (companyId == 2)
                {
                    var userlist = ctx.UserLogin45.Where(u => u.Status == "1").ToList();

                    var viewModel = new UserViewModel45
                    {
                        Users = userlist
                    };
                    return View(viewModel);
                }
                else
                {
                    var userlist = ctx.UserLogin45.Where(u => u.Status == "1" && u.Company_ID == companyId.ToString()).ToList();

                    var viewModel = new UserViewModel45
                    {
                        Users = userlist
                    };
                    return View(viewModel);
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public ActionResult RegisteredRouter()
        {
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (Session["UserName"] != null)
            {
                string userName = Session["UserName"].ToString();
                var login = ctx.UserLogin45.FirstOrDefault(x => x.Username == userName);
                if (login != null)
                {
                    var viewModel = new RegisteredDataModel
                    {
                        GSM_No = "",
                        Ricon_SN = "",
                        Company_Name = " ",
                        Site_Name = "",
                    };
                    var registeredData = ctx.Registered_Router
                        .Where(rd => companyId == 2 || rd.Company_Name == companyName)
                        .ToList();

                    return View(viewModel);
                }
            }
            // "Edit" sayfasına yönlendir.
            return RedirectToAction("RegisteredRouter", "Home");
        }

        [HttpPost]
        public ActionResult RegisteredRouter(RegisteredDataModel model)
        {
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(model.GSM_No))
            {
                // Dropdown değeri seçilmemişse, aynı sayfaya yönlendir.
                return RedirectToAction("RegisteredRouter");
            }

            // ModelState üzerinde hataları kontrol etmek için
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();

            if (ModelState.IsValid)
            {
                var existingData = ctx.Registered_Router
                    .Where(rd => rd.GSM_No == model.GSM_No && (companyId == 2 || rd.Company_Name == companyName)) // Kendi verilerini kontrol et
                    .FirstOrDefault();

                if (existingData != null)
                {
                    // Gerekli işlemleri yapın (örneğin, güncelleme veya kayıt)
                }

                return RedirectToAction("RegisteredRouter");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Ricon()
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (Session["UserName"] != null)
            {
                string userName = Session["UserName"].ToString();
                var login = ctx.UserLogin45.FirstOrDefault(x => x.Username == userName);
                if (login != null)
                {
                    var viewModel = new RegisteredDataModel
                    {
                        GSM_No = "",
                        Ricon_SN = "",
                        Company_Name = " ",
                        Site_Name = "",
                    };
                    var registeredData = ctx.Registered_Router
                        .Where(rd => companyId == 2 || rd.Company_Name == companyName) // Tüm veriler veya kendi verileri
                        .ToList();

                    return View(viewModel);
                }
            }
            return RedirectToAction("RegisteredRouter", "Home");
        }

        [HttpPost]
        public ActionResult Ricon(RegisteredDataModel model)
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(model.GSM_No))
            {
                return RedirectToAction("RegisteredRouter");
            }

            // ModelState üzerinde hataları kontrol etmek için
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();

            if (ModelState.IsValid)
            {
                var existingData = ctx.Registered_Router
                    .Where(rd => rd.GSM_No == model.GSM_No && (companyId == 2 || rd.Company_Name == companyName)) // Kendi verilerini kontrol et
                    .FirstOrDefault();

                if (existingData != null)
                {
                    // Gerekli işlemleri yapın (örneğin, güncelleme veya kayıt)
                }

                return RedirectToAction("RegisteredRouter");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult SaveAutoconfData(Basic_Setup data)
        {
            if (Session["UserName"] != null)
            {
                var kullanici = Session["UserName"].ToString();
                var islemSaati = DateTime.Now;

                var mpData = ctx.Basic_Setup.FirstOrDefault(x => x.ID == data.ID);
                if (mpData != null)
                {
                    mpData.GMS_No = data.GMS_No;
                    mpData.APN_Name = data.APN_Name;
                    mpData.DHCP_Status = data.DHCP_Status;

                    if (data.DHCP_Status == 1)
                    {
                        mpData.DHCP_Start = data.DHCP_Start ?? string.Empty;
                        mpData.DHCP_User = data.DHCP_User ?? string.Empty;
                    }
                    else
                    {
                        mpData.DHCP_Start = string.Empty;
                        mpData.DHCP_User = string.Empty;
                    }

                    mpData.LAN_IP = data.LAN_IP;
                    mpData.LAN_Subnet = data.LAN_Subnet;
                    mpData.Serial_no = data.Serial_no;

                    ctx.SaveChanges();
                    return Json(new { success = true, message = Resources.Resources.L332 });
                }
            }
            return RedirectToAction("Login");
        }

        [HttpPost]
        public ActionResult SaveRegister(string GsmNo, string SeriNo, string Company_Name, string Lokasyons)
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(GsmNo) || string.IsNullOrEmpty(SeriNo) || string.IsNullOrEmpty(Company_Name))
            {
                return Json(new { success = false, message = "GSM No, Seri No ve Şirket Adı alanları zorunludur." });
            }
            try
            {
                using (var context = new ZTP_TestEntities())
                {
                    // GSM numarasını kontrol et
                    var existingGsm = context.GsmNumber.FirstOrDefault(g => g.GSM_No == GsmNo);
                    var existingCompany = context.Company.FirstOrDefault(c => c.Company_Name.ToLower() == Company_Name.ToLower());

                    if (existingCompany == null)
                    {
                        // Yeni Company kaydı oluştur
                        var companyEntry = new Company
                        {
                            Company_Name = Company_Name
                        };

                        context.Company.Add(companyEntry);
                        context.SaveChanges(); // İlk kaydı kaydet
                        companyId = companyEntry.Company_ID; // Yeni oluşturulan ID'yi al
                    }
                    else
                    {
                        companyId = existingCompany.Company_ID; // Mevcut kaydın ID'sini al
                    }

                    if (existingGsm != null)
                    {
                        // Eğer aynı GSM numarası varsa ve Company farklıysa sil
                        if (existingGsm.Company_ID != companyId)
                        {
                            context.GsmNumber.Remove(existingGsm); // Eski kaydı sil
                            context.SaveChanges(); // Değişiklikleri kaydet
                        }
                    }

                    // Yeni GSM kaydını ekle
                    var gsmEntry = new GsmNumber
                    {
                        GSM_No = GsmNo,
                        Company_ID = companyId,
                        Status = 1 // Yeni Company_ID'yi ekle
                    };
                    context.GsmNumber.Add(gsmEntry);

                    // Device tablosuna ekleme
                    var deviceEntry = new Device
                    {
                        Ricon_SN = SeriNo,
                        Device_Type = GetDeviceType(SeriNo).ToUpper(), // Device_Type değerini belirle
                        Company_ID = companyId, // Company_ID'yi ekle
                        Status = 1
                    };
                    context.Device.Add(deviceEntry);

                    // Sites tablosuna ekleme
                    var existingSite = context.Sites
                        .FirstOrDefault(s => s.Company_ID == companyId && s.Site_Name.Equals(Lokasyons, StringComparison.OrdinalIgnoreCase));

                    if (existingSite == null)
                    {
                        if (Lokasyons != null)
                        {
                            // Lokasyon yoksa yeni ekle
                            var siteEntry = new Site
                            {
                                Company_ID = companyId,
                                Site_Name = Lokasyons,
                                Status = 1
                            };
                            context.Sites.Add(siteEntry);
                        }
                    }

                    // Registered_Router tablosuna ekleme
                    var newEntry = new Registered_Router
                    {
                        GSM_No = GsmNo,
                        Ricon_SN = SeriNo,
                        Company_Name = Company_Name,
                        Site_Name = Lokasyons
                    };
                    context.Registered_Router.Add(newEntry);

                    // Veritabanına kaydet
                    context.SaveChanges();
                }
                return Json(new { success = true, message = "Data saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SaveSetupData(string DHCP_Starts, string LAN_IPs, string Lan_Subnets, string remarks, string APN_Name, byte DhcpStatuss, string DhcpUsers, string selectedSeriNo)
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();
            string kullanici = Session["UserName"]?.ToString();

            if (companyId == 0 || kullanici == null)
            {
                return Json(new { success = false, message = "Kullanıcı oturumu veya şirket bilgisi eksik." });
            }

            try
            {
                // Seçilen seri numarasının Device_Type değerini al
                var deviceType = ctx.Device
                    .Where(d => d.Ricon_SN == selectedSeriNo && d.Company_ID == companyId)
                    .Select(d => d.Device_Type)
                    .FirstOrDefault();

                string existingLanIp = deviceType == "M" ? "192.168.8.1" : null;

                // Giriş verilerinin geçerliliğini kontrol et
                if (IsValidInput1(LAN_IPs, Lan_Subnets, APN_Name))
                {
                    UpdateOrAddData1(DHCP_Starts, LAN_IPs, existingLanIp, Lan_Subnets, APN_Name, DhcpStatuss, DhcpUsers, remarks, selectedSeriNo, kullanici);
                    return Json(new { success = true, message = Resources.Resources.L332 });
                }
                else
                {
                    return Json(new { success = false, message = "Geçersiz Veri." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult Setup()
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();
            string kullanici = Session["UserName"]?.ToString();

            if (companyId == 0 || kullanici == null)
            {
                return RedirectToAction("Login", "Account"); // Oturum açma sayfasına yönlendir
            }

            // Kullanıcı login kontrolü ve şirket doğrulama
            var login = ctx.UserLogin45.FirstOrDefault(x => x.Username == kullanici && x.Company_ID == companyId.ToString());
            if (login == null)
            {
                return RedirectToAction("Setup", "Home");
            }

            // GsmNo seçeneklerini sırala ve operatöre göre filtrele
            var gsmno = ctx.Basic_Setup
                .Where(x => x.ID == x.ID && x.Company_ID == companyId)
                .OrderBy(x => x.GMS_No)
                .Select(a => new SelectListItem
                {
                    Text = a.GMS_No,
                    Value = a.GMS_No
                }).ToList();

            ViewData["GMS_No"] = new SelectList(gsmno, "Value", "Text");

            // DHCP durum seçenekleri
            var dhcpStatusList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Disable", Value = "0" },
                new SelectListItem { Text = "Enable", Value = "1" }
            };
            ViewData["DHCP_Status"] = new SelectList(dhcpStatusList, "Value", "Text");

            // Seri numaralarını filtrele ve ViewBag'e ata
            var serialNumbers = ctx.Device
                .Where(d => d.Company_ID == companyId)
                .Select(d => new SelectListItem
                {
                    Text = d.Ricon_SN,
                    Value = d.Ricon_SN
                }).ToList();

            ViewBag.SeriNo = new SelectList(serialNumbers, "Value", "Text");

            // Cihazın Device_Type'ını kontrol et
            var deviceType = ctx.Device
                .Where(d => d.Company_ID == companyId && d.Device_Type == "M")
                .Select(d => d.Device_Type)
                .FirstOrDefault() ?? string.Empty;

            // Model oluştur
            var viewModel = new AllModel
            {
                Ricon_SN = "",
                ApnName = "",
                Lanip = "",
                LanSubnet = "",
                DHCPStatus = dhcpStatusList.ToString(),
                DHCPStart = "",
                DHCPUser = "",
                remark1 = "",
                Device_Type = deviceType
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Setup(KurulumViewModel model)
        {
            // Şirket bilgisi alınır
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            if (Session["UserName"] == null || Session["IsAdmin"] == null || !(bool)Session["IsAdmin"])
            {
                // Kullanıcı giriş yapmamışsa ya da admin değilse giriş sayfasına yönlendirilir
                return RedirectToAction("Login", "Login");
            }

            ViewBag.LocationStatus = Session["LocationStatus"];

            try
            {
                if (string.IsNullOrEmpty(model.SelectedSeriNoID) || string.IsNullOrEmpty(model.SelectedGsmNoID))
                {
                    // Eksik veriler için hata mesajları
                    ViewBag.LocationID = model.SelectedLocationID == null ? CultureHelper.GetResourceKey("L119") : null;
                    ViewBag.SeriNoID = model.SelectedSeriNoID == null ? CultureHelper.GetResourceKey("L120") : null;
                    ViewBag.GsmNoID = model.SelectedGsmNoID == null ? CultureHelper.GetResourceKey("L315") : null;
                    ViewBag.Sonuc = CultureHelper.GetResourceKey("L122");
                    return View(new KurulumViewModel());
                }

                // Seçilen cihaz bilgileri session'a alınır
                Session["SelectedSeriNoID"] = model.SelectedSeriNoID;
                Session["SelectedLocationID"] = model.SelectedLocationID;
                Session["SelectedGsmNoID"] = model.SelectedGsmNoID;

                var existingDevice = ctx.Device.FirstOrDefault(x => x.Ricon_SN == model.SelectedSeriNoID);
                Session["DeviceType"] = existingDevice?.Device_Type;

                var gsmRecords = ctx.Install.Where(x => x.GSM_No == model.SelectedGsmNoID).ToList();
                var serialNumberRecords = ctx.Install.Where(x => x.Ricon_SN == model.SelectedSeriNoID).ToList();
                var commonRecords = ctx.Install
                    .Where(x => x.GSM_No == model.SelectedGsmNoID && x.Ricon_SN == model.SelectedSeriNoID)
                    .ToList();

                // Kayıtların durumuna göre kullanıcıya bilgi mesajları döndürülür
                if (gsmRecords.Count > 0 && serialNumberRecords.Count > 0)
                {
                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                    ViewBag.CaseMessageYesNo = CultureHelper.GetResourceKey("L210");
                }
                else if (gsmRecords.Count > 0 || serialNumberRecords.Count > 0)
                {
                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                    ViewBag.CaseMessageYesNo = CultureHelper.GetResourceKey("L210");
                }
                else
                {
                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                    ViewBag.StartOKMessage = CultureHelper.GetResourceKey("L239");
                }

                return View(new KurulumViewModel());
            }
            catch (Exception)
            {
                ViewBag.Sonuc = CultureHelper.GetResourceKey("L234");
                var emptyModel = new KurulumViewModel
                {
                    Ricon_SN = "",
                    Site_Name = "",
                    Gsm_Number = ""
                };
                return View(emptyModel);
            }
        }

        [HttpPost]
        public ActionResult Setup2(KurulumViewModel model)
        {
            ViewBag.LocationStatus = Session["LocationStatus"];
            try
            {
                // Kullanıcı girişi kontrolü
                if (Session["UserName"] != null && (bool?)Session["IsAdmin"] == true)
                {
                    string kullanici = Session["UserName"].ToString();
                    if (!string.IsNullOrEmpty(kullanici))
                    {
                        var selectedSeriNos = new List<string>();
                        var selectedGsmNos = new List<string>();

                        // İki listeyi tek döngü ile işle
                        for (int i = 0; i < model.SelectedSeriNoIDs.Count; i++)
                        {
                            string selectedSeriNo = model.SelectedSeriNoIDs[i];
                            string selectedGsmNo = model.SelectedGsmNoIDs[i];

                            selectedSeriNos.Add(selectedSeriNo);
                            selectedGsmNos.Add(selectedGsmNo);

                            var query_ricon_sr = ctx.Install.FirstOrDefault(x => x.Ricon_SN == selectedSeriNo);
                            var device = ctx.Device.FirstOrDefault(x => x.Ricon_SN == selectedSeriNo);

                            if (device != null)
                            {
                                Session["DeviceType"] = device.Device_Type;
                            }

                            var commonRecords = ctx.Install
                                .Where(x => x.GSM_No == selectedGsmNo && x.Ricon_SN == selectedSeriNo).ToList();

                            if (commonRecords.Count > 0 || commonRecords.Count == 0)
                            {
                                ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                ViewBag.CaseMessageYesNo2 = CultureHelper.GetResourceKey("L372");
                            }
                        }

                        // Seçilen seri numaralarını ve GSM numaralarını TempData ile taşıyoruz
                        TempData["SelectedSeriNos"] = selectedSeriNos;
                        TempData["SelectedGsmNos"] = selectedGsmNos;

                        return RedirectToAction("GetConfirm2");
                    }
                    else
                    {
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception)
            {
                ViewBag.Sonuc = CultureHelper.GetResourceKey("L234");
                return View(new KurulumViewModel());
            }
        }

        [HttpPost]
        public ActionResult StartOk()
        {
            string loklok = Session["SelectedLocationID"]?.ToString();
            string companyName = Session["Company_Name"]?.ToString();
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();

            string kullanici = Session["UserName"]?.ToString();
            string company_id = companyId.ToString();
            string secilen_deviceType = Session["DeviceType"]?.ToString();

            string serino = Session["SelectedSeriNoID"]?.ToString();
            string secilen_gsm = Session["SelectedGsmNoID"]?.ToString();

            var control_gsm = ctx.Install.FirstOrDefault(x => x.GSM_No == secilen_gsm);
            var oldlan = ctx.Data.FirstOrDefault(x => x.GMS_No == secilen_gsm);
            string lanold = oldlan?.LAN_IP;
            Install install_query = new Install();

            bool deger = a.ConfigAzure(secilen_gsm, secilen_deviceType, serino, ViewBag);

            if (deger)
            {
                int lastindex = 1;
                var maxInstallID = ctx.Install.Max(p => (int?)p.Install_ID);

                if (maxInstallID.HasValue)
                {
                    lastindex = maxInstallID.Value;
                }

                install_query.Username = kullanici;
                install_query.Ricon_SN = serino;
                install_query.GSM_No = secilen_gsm;
                install_query.Date_Time = DateTime.Now;
                install_query.Company_ID = Convert.ToInt32(company_id);
                install_query.Install_ID = (lastindex + 1);

                ctx.Install.Add(install_query);
                ctx.SaveChanges();

                if (oldlan != null)
                {
                    oldlan.Serial_no = serino;
                    oldlan.LAN_Eski = lanold;
                    oldlan.Datetime = DateTime.Now;
                    oldlan.UserName = kullanici;
                    oldlan.Lokasyon = loklok;
                    ctx.SaveChanges();
                }

                var basicSetupRecord = ctx.Basic_Setup.FirstOrDefault(x => x.GMS_No == secilen_gsm);
                if (basicSetupRecord != null)
                {
                    basicSetupRecord.Flag = 1; // Başarılı durum için Flag'ı 1 yap
                    ctx.SaveChanges(); // Değişiklikleri kaydet
                }

                // Sonuç mesajı oluşturma
                string sonucMesaj = "<b>SerialNumber:</b> " + serino;

                // Lokasyon kontrolü ve ekleme
                if (!string.IsNullOrEmpty(loklok))
                {
                    sonucMesaj += "<br /><b>Location:</b> " + loklok; // Lokasyon varsa seri numarasından sonra ekle
                }

                sonucMesaj += "<br /><b>Gsm No:</b> " + secilen_gsm + "<br /><br />" + CultureHelper.GetResourceKey("L339") + "<br /><br /><br />" + CultureHelper.GetResourceKey("L124");

                Session["Sonuc"] = sonucMesaj;
                ViewBag.Sonuc = sonucMesaj;

                return Json(new { success = true, result = sonucMesaj });
            }
            else
            {
                var basicSetupRecord = ctx.Basic_Setup.FirstOrDefault(x => x.GMS_No == secilen_gsm);
                if (basicSetupRecord != null)
                {
                    basicSetupRecord.Flag = 0; // Başarılı durum için Flag'ı 1 yap
                    ctx.SaveChanges(); // Değişiklikleri kaydet
                }

                Session["Sonuc"] = CultureHelper.GetResourceKey("L125");
                ViewBag.Sonuc = CultureHelper.GetResourceKey("L125");
                return Json(false);
            }
        }

        [HttpGet]
        public ActionResult TaskStatus()
        {
            return View();
        }

        private List<Basic_Setup> GetAllDevices()
        {
            using (var context = new ZTP_TestEntities())
            {
                return context.Basic_Setup.ToList(); // DeviceSetups tablonuzun adı
            }
        }

        // Seri numarasına göre Device_Type değerini belirleyen yardımcı metod
        private string GetDeviceType(string serialNo)
        {
            if (serialNo.Contains("MQ"))
            {
                return "M"; // MQ için M
            }
            else if (serialNo.Contains("XL"))
            {
                return "XL"; // XL için XL
            }
            else if (serialNo.Contains("L"))
            {
                return "XL"; // XL için XL
            }

            return null; // Geçersiz bir durum için null döndür
        }

        private bool IsPasswordSecure(string password)
        {
            if (password.Length < 8)
            {
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                return false;
            }

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
            {
                return false;
            }

            return true;
        }

        private bool IsValidInput1(string lanIps, string lanSubnets, string apnName)
        {
            return !string.IsNullOrWhiteSpace(lanIps) &&
                   !string.IsNullOrWhiteSpace(lanSubnets) && !string.IsNullOrWhiteSpace(apnName);
        }

        private void UpdateOrAddData1(string dhcpStarts, string lanIps, string existingLanIp, string lanSubnets, string apnName, byte dhcpStatuss, string dhcpUsers, string remarks, string selectedSerino, string kullanici)
        {
            string companyName = Session["Company_Name"]?.ToString(); // Şirket ismini al
            var companyId = ctx.Company
                .Where(x => x.Company_Name == companyName)
                .Select(x => x.Company_ID)
                .FirstOrDefault();
            // Basic_Setup tablosuna veri ekleme veya güncelleme işlemleri
            var setupData = ctx.Registered_Router.FirstOrDefault(x => x.Ricon_SN == selectedSerino);
            string oldGsm = setupData.GSM_No;
            var gsmdel = ctx.Basic_Setup.FirstOrDefault(x => x.Serial_no == selectedSerino);

            if (gsmdel != null)
            {
                // Eşleşen kayıt bulundu, bu kaydı sil
                ctx.Basic_Setup.Remove(gsmdel);
            }

            // Her durumda yeni kayıt oluşturma işlemi
            var newAutoconf = new Basic_Setup
            {
                GMS_No = oldGsm, // Eski GSM numarasını kullan
                APN_Name = apnName,
                NAT = 1,
                DHCP_Status = dhcpStatuss,
                Status = 1,
                DHCP_Start = (dhcpStatuss == 1) ? dhcpStarts : "NULL",
                DHCP_User = (dhcpStatuss == 1) ? dhcpUsers.ToString() : "NULL",
                LAN_Eski = existingLanIp,
                LAN_IP = lanIps,
                LAN_Subnet = lanSubnets,
                Serial_no = selectedSerino,
                Datetime = System.DateTime.Now,
                UserName = kullanici,
                Flag = 0,
                remark = remarks,
                Company_ID = companyId,
            };

            ctx.Basic_Setup.Add(newAutoconf);
            ctx.SaveChanges();
        }
    }
}