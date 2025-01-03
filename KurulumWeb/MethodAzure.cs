
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KurulumWeb.Helper;
using KurulumWeb.Models;
using SMSApi.Api;
using SNMPDB;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using System;
using System.Dynamic;

namespace KurulumWeb
{
    public class MethodAzure
    {
        private ZTP_TestEntities ctx = new ZTP_TestEntities();

        public string selected_gsm { get; set; }
        public string selected_riconsn { get; set; }

        public bool ConfigAzure(string GSM, string secilen_deviceType, string selected_riconserino, dynamic ViewBag) //Configuration yapacak metot
        {
            try
            {
                var query = ctx.Basic_Setup.Where(x => x.GMS_No == GSM).FirstOrDefault();

                //var query = ctx.SIMCards3.Where(x => x.GSM_No1 == GSM || x.GSM_No2 == GSM).FirstOrDefault(); // Gelen GSM numarasına göre veri tabanından veri getir.

                if (query != null) //veri boş değilse
                {

                    var query_device = ctx.Basic_Setup.Where(x => x.Serial_no == selected_riconserino).FirstOrDefault(); //simcards tablosundan getirdiği kişinin Operatorüne göre device tablosundan deviceları  getiriyor

                    if (query_device != null)
                    {
                        string username = "subscriptions@riconmobile.com";
                        string password = "Hedef2023!";
                        string title = "";
                        string donus = ""; //jet sms metodunun bize gönderdiği değer
                        int denemeSayisi = 0;
                        //  msj göndermek için olan deneme sayısı

                        if (secilen_deviceType == "XL")   // L
                        {
                            if (query.DHCP_Status == 0) // veri tabanında DHCP_status 0 ise
                            {
                                do
                                {
                                    string msj1 = "Modem=" + query.APN_Name +";" + "wannat=" + query.NAT + ";" + "Dhcpserver=0;" + "Lan=" +
                                           query.LAN_IP + "," + query.LAN_Subnet + ";";
                                    bool isfast = false;
                                    if (denemeSayisi != 0)
                                    {
                                        isfast = true;
                                    }
                                    donus = sendMessageAzure(GSM, msj1, username, password, title, isfast);
                                    if (denemeSayisi == 3)
                                    {
                                        if (donus == "UNDELIVERED" || donus == "SENT")
                                        {
                                            string failed_sms = CultureHelper.GetResourceKey("L236");

                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
                                            ViewBag.SmsMessage = CultureHelper.GetResourceKey("L236");
                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
                                            HttpContext.Current.Session["SmsMessage"] = CultureHelper.GetResourceKey("L236");
                                            return false;
                                        }
                                    }
                                    Thread.Sleep(1000);
                                    denemeSayisi++;
                                }

                                while (donus != "DELIVERED" && denemeSayisi <= 3);
                                denemeSayisi = 0;
                                Thread.Sleep(2000);
                            }
                            else if (query.DHCP_Status == 1)
                            {
                                do
                                {
                                    string msj1 = "Modem=" + query.APN_Name +";" + "wannat=" + query.NAT + ";" + "Dhcpserver=1;" + "wan_dns=8.8.8.8 8.8.4.4;" + "Lan=" +
                                            query.LAN_IP + "," + query.LAN_Subnet + ";";
                                    bool isfast = false;
                                    if (denemeSayisi != 0)
                                    {
                                        isfast = true;
                                    }
                                    donus = sendMessageAzure(GSM, msj1, username, password, title, isfast);
                                    if (denemeSayisi == 3)
                                    {
                                        if (donus == "UNDELIVERED" || donus == "SENT")
                                        {
                                            string failed_sms = CultureHelper.GetResourceKey("L236");

                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
                                            ViewBag.SmsMessage = CultureHelper.GetResourceKey("L236");
                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
                                            HttpContext.Current.Session["SmsMessage"] = CultureHelper.GetResourceKey("L236");
                                            return false;
                                        }
                                    }
                                    Thread.Sleep(1000);
                                    denemeSayisi++;
                                }

                                while (donus != "DELIVERED" && denemeSayisi <= 3);
                                denemeSayisi = 0;
                                Thread.Sleep(2000);
                                do
                                {
                                    string msj2_1 = "dhcprange=" + query.DHCP_Start + "," + query.DHCP_User + ";";
                                    bool isfast = false;
                                    if (denemeSayisi != 0)
                                    {
                                        isfast = true;
                                    }
                                    donus = sendMessageAzure(GSM, msj2_1, username, password, title, isfast);
                                    if (denemeSayisi == 3)
                                    {
                                        if (donus == "UNDELIVERED" || donus == "SENT")
                                        {
                                            string failed_sms = CultureHelper.GetResourceKey("L236");

                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
                                            ViewBag.SmsMessage = CultureHelper.GetResourceKey("L236");
                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
                                            HttpContext.Current.Session["SmsMessage"] = CultureHelper.GetResourceKey("L236");
                                            return false;
                                        }
                                    }
                                    Thread.Sleep(500);
                                    denemeSayisi++;
                                }
                                while (donus != "DELIVERED" && denemeSayisi <= 3);
                                denemeSayisi = 0;
                                Thread.Sleep(2000);


                                do
                                {
                                    string msj5 = "commit;";
                                    bool isfast = false;
                                    if (denemeSayisi != 0)
                                    {
                                        isfast = true;
                                    }
                                    donus = sendMessageAzure(GSM, msj5, username, password, title, isfast);
                                    if (denemeSayisi == 3)
                                    {
                                        if (donus == "UNDELIVERED" || donus == "SENT")
                                        {
                                            string failed_sms = CultureHelper.GetResourceKey("L236");

                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
                                            ViewBag.SmsMessage = CultureHelper.GetResourceKey("L236");
                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
                                            HttpContext.Current.Session["SmsMessage"] = CultureHelper.GetResourceKey("L236");
                                            return false;
                                        }
                                    }
                                    Thread.Sleep(500);
                                    denemeSayisi++;
                                }
                                while (donus != "DELIVERED" && denemeSayisi <= 3);
                                denemeSayisi = 0;
                                Thread.Sleep(2000);

                                do
                                {
                                    string msj6 = "reboot;";
                                    bool isfast = false;
                                    if (denemeSayisi != 0)
                                    {
                                        isfast = true;
                                    }
                                    donus = sendMessageAzure(GSM, msj6, username, password, title, isfast);
                                    if (denemeSayisi == 3)
                                    {
                                        if (donus == "UNDELIVERED" || donus == "SENT")
                                        {
                                            string failed_sms = CultureHelper.GetResourceKey("L236");

                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
                                            ViewBag.SmsMessage = CultureHelper.GetResourceKey("L236");
                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
                                            HttpContext.Current.Session["SmsMessage"] = CultureHelper.GetResourceKey("L236");
                                            return false;
                                        }
                                    }
                                    Thread.Sleep(500);
                                    denemeSayisi++;
                                }
                                while (donus != "DELIVERED" && denemeSayisi <= 3);
                                denemeSayisi = 0;
                                Thread.Sleep(2000);

                                //query.Datetime = DateTime.Now;  //tarihi güncelle
                                query.Status = 1; //statusu 1 yap
                                ctx.SaveChanges(); //değişiklikleri kaydet

                            }
                        }
                        else if (secilen_deviceType == "M")  // M
                        {
                            do
                            {
                                if (query.NAT==0)
                                {
                                    string msj1 = "set cli-command"+"\r\n" +
                                    "int br0"+"\r\n"+
                                    "no ip ad " + query.LAN_Eski + "/" + query.LAN_Subnet+"\r\n"+
                                    "ip ad " + query.LAN_IP + "/" + query.LAN_Subnet+"\r\n"+
                                     "q"+"\r\n"+
                                    "ip nat shutdown"+"\r\n"+
                                    "int modem 0"+"\r\n"+
                                    "access-point-name "+query.APN_Name+"\r\n";


                                    bool isfast = false;
                                    if (denemeSayisi != 0)
                                    {
                                        isfast = true;
                                    }
                                    donus = sendMessageAzure(GSM, msj1, username, password, title, isfast);
                                    if (denemeSayisi == 3)
                                    {
                                        if (donus == "UNDELIVERED" || donus == "SENT")
                                        {
                                            string failed_sms = CultureHelper.GetResourceKey("L236");

                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
                                            ViewBag.SmsMessage = CultureHelper.GetResourceKey("L236");
                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
                                            HttpContext.Current.Session["SmsMessage"] = CultureHelper.GetResourceKey("L236");
                                            return false;
                                        }
                                    }
                                    Thread.Sleep(1000);
                                    denemeSayisi++;
                                }

                                else
                                {
                                    string msj1 = "set cli-command"+"\r\n" +
                                        "int br0"+"\r\n"+
                                        "no ip ad " + query.LAN_Eski + "/" + query.LAN_Subnet+"\r\n"+
                                        "ip ad " + query.LAN_IP + "/" + query.LAN_Subnet+"\r\n"+
                                         "q"+"\r\n"+
                                        "no ip nat shutdown"+"\r\n"+
                                        "int modem 0"+"\r\n"+
                                        "access-point-name "+query.APN_Name+"\r\n";
                                    bool isfast = false;
                                    if (denemeSayisi != 0)
                                    {
                                        isfast = true;
                                    }
                                    donus = sendMessageAzure(GSM, msj1, username, password, title, isfast);
                                    if (denemeSayisi == 3)
                                    {
                                        if (donus == "UNDELIVERED" || donus == "SENT")
                                        {
                                            string failed_sms = CultureHelper.GetResourceKey("L236");

                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
                                            ViewBag.SmsMessage = CultureHelper.GetResourceKey("L236");
                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
                                            HttpContext.Current.Session["SmsMessage"] = CultureHelper.GetResourceKey("L236");
                                            return false;
                                        }
                                    }
                                    Thread.Sleep(1000);
                                    denemeSayisi++;

                                }
                            } while (donus != "DELIVERED" && denemeSayisi <= 3);
                            denemeSayisi = 0;
                            Thread.Sleep(2000);

                            //query.Datetime = DateTime.Now;  //tarihi güncelle
                            query.Status = 1; //statusu 1 yap
                            ctx.SaveChanges(); //değişiklikleri kaydet

                        }
                    }


                }
                return true;
            }
            catch (System.Exception e)
            {
                //install_query.Date_Time = DateTime.Now;
                e.Message.ToString();
                return true;
            }
        }
        public Dictionary<string, bool> ConfigAzure2(IEnumerable<dynamic> dataToSend, dynamic ViewBag) // Çoklu data için metot
        {
            // Sonuçları tutmak için Dictionary oluştur
            Dictionary<string, bool> resultDictionary = new Dictionary<string, bool>();

            try
            {
                foreach (var dataItem in dataToSend)
                {
                    // Dinamik olarak öğeleri al
                    string GSM = dataItem.GSM_No; // GSM_No kullanıyoruz
                    string secilen_deviceType = dataItem.DeviceType;
                    string selected_riconserino = dataItem.SerialNumber;

                    // ConfigAzure metodunu çağır
                    bool result = ConfigAzure(GSM, secilen_deviceType, selected_riconserino, ViewBag);

                    // Sonuçları sakla
                    resultDictionary[selected_riconserino] = result;
                }
            }
            catch (System.Exception ex)
            {
                // Hata durumunda gerekli bilgiyi al
                ViewBag.ErrorMessage = ex.Message;
            }

            return resultDictionary; // Başarı durumunu döndür
        }

        public string sendMessageAzure(string GSM, string mesaj, string username, string password, string title, bool fast)
        {
            string m_result = "";

            try
            {
                SMSApi.Api.IClient client = new SMSApi.Api.ClientOAuth("MiQksF5buJOGtXvqCvOWpl3PuCDQYABA5C3oUv4f");

                var smsApi = new SMSApi.Api.SMSFactory(client, ProxyAddress.SmsApiCom);

                var result =
                    smsApi.ActionSend()
                        .SetText(mesaj)
                        .SetTo(GSM)
                        .SetSender("Ricon")
                        .SetFast(fast)
                        .Execute();

                string[] ids = new string[result.Count];

                for (int i = 0, l = 0; i < result.List.Count; i++)
                {
                    if (!result.List[i].isError())
                    {
                        if (!result.List[i].isFinal())
                        {
                            ids[l] = result.List[i].ID;
                            l++;
                        }
                    }
                }

                result =
                  smsApi.ActionGet()
                        .Ids(ids)
                        .Execute();
                int time_count = 0;
                while ((result.List[0].Status != "DELIVERED") && (result.List[0].Status != "UNDELIVERED") && time_count < 80)
                {
                    Thread.Sleep(1000);
                    time_count++;

                    result =
                    smsApi.ActionGet()
                       .Ids(ids)
                       .Execute();
                }

                m_result = result.List[0].Status;
            }
            catch (SMSApi.Api.ActionException e)
            {
                /**
                 * Action error
                 */
                System.Console.WriteLine(e.Message);
                return e.Message;
            }
            catch (SMSApi.Api.ClientException e)
            {
                /**
                 * Error codes (list available in smsapi docs). Example:
                 * 101 	Invalid authorization info
                 * 102 	Invalid username or password
                 * 103 	Insufficient credits on Your account
                 * 104 	No such template
                 * 105 	Wrong IP address (for IP filter turned on)
                 * 110	Action not allowed for your account
                 */
                System.Console.WriteLine(e.Message);
                return e.Message;
            }
            catch (SMSApi.Api.HostException e)
            {
                /*
                 * Server errors
                 * SMSApi.Api.HostException.E_JSON_DECODE - problem with parsing data
                 */
                System.Console.WriteLine(e.Message);
            }
            catch (SMSApi.Api.ProxyException e)
            {
                // communication problem between client and sever
                System.Console.WriteLine(e.Message);
                return e.Message;
            }
            return m_result;
        }
    }
}