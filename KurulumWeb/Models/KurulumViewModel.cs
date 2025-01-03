using SNMPDB;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace KurulumWeb.Models
{
    public class AllModel
    {
        #region Public Properties

        public string APN_Name { get; set; }
        public string APN_Password { get; set; }
        public string APN_User { get; set; }
        public string ApnName { get; set; }
        public string ApnPassword { get; set; }
        public string ApnUser { get; set; }
        public byte Cloud_Enable { get; set; }
        public string CloudEnable { get; set; }
        public int Company_ID { get; set; }
        public int Companyid { get; set; }
        public string Device_Type { get; set; }
        public SelectList DeviceType { get; set; }
        public string DHCP_Start { get; set; }
        public byte DHCP_Status { get; set; }
        public int DHCP_User { get; set; }
        public string DHCPStart { get; set; }
        public string DHCPStatus { get; set; }
        public string DHCPUser { get; set; }
        public string GMS_No { get; set; }
        public string Gsm_No { get; set; }
        public SelectList gsmno { get; set; }
        public string Heart_tm { get; set; }
        public string Heart_tm1 { get; set; }
        public string HeartBeat { get; set; }
        public string HeartBeat1 { get; set; }
        public int ID { get; set; }
        public string Iface { get; set; }
        public string iface1 { get; set; }
        public string Il { get; set; }
        public string Il1 { get; set; }
        public string Ilce { get; set; }
        public string ilce1 { get; set; }
        public string LAN_IP { get; set; }
        public string Lan_Subnet { get; set; }
        public string Lanip { get; set; }
        public string LanSubnet { get; set; }
        public string Lokasyon { get; set; }
        public string Lokasyon1 { get; set; }
        public byte NAT { get; set; }
        public string Nat1 { get; set; }
        public string Remark { get; set; }
        public string remark1 { get; set; }
        public string Ricon_SN { get; set; }
        public string RiconSeri { get; set; }
        public string SelectedDeviceTypeID { get; set; }
        public string SelectedSeriNoID { get; set; }
        public string SelectedSiteName { get; set; }
        public SelectList SeriNo { get; set; }
        public string ServIp { get; set; }
        public string ServIp1 { get; set; }
        public string ServPort { get; set; }
        public string ServPort1 { get; set; }
        public int SId { get; set; }
        public SelectList site { get; set; }
        public string Site_Name { get; set; }
        public string SiteName { get; set; }
        public byte Status { get; set; }
        public string Status1 { get; set; }
        public string TextGsm { get; set; } // TextBoxFor için
        public string WAN_IP { get; set; }
        public string WanIp { get; set; }

        #endregion Public Properties
    }

    [Serializable]
    public class ConfigData
    {
        #region Public Properties

        public string CompanyID { get; set; }
        public string DeviceType { get; set; }
        public string GSM_No { get; set; }
        public string LAN_IP { get; set; }
        public string Location { get; set; }
        public string SerialNumber { get; set; }
        public string UserName { get; set; }

        #endregion Public Properties
    }

    public class DataItem
    {
        #region Public Properties

        public string GSM { get; set; }
        public string SecilenDeviceType { get; set; }
        public string SelectedRiconSerino { get; set; }

        #endregion Public Properties
    }

    public class DeviceDetails
    {
        #region Public Properties

        public string Details { get; set; }
        public string GSMNumber { get; set; }
        public string Operator { get; set; }

        #endregion Public Properties
    }

    // Excel veri modeli
    public class ExcelDataModel
    {
        #region Public Properties

        public List<string> Headers { get; set; }
        public List<List<string>> Rows { get; set; }

        #endregion Public Properties
    }

    public class InstallRatioViewModel
    {
        #region Public Properties

        public int InstallCount { get; set; }
        public double Ratio { get; set; }
        public int SimCardCount { get; set; }

        #endregion Public Properties
    }

    public class KurulumViewModel
    {
        #region Public Properties

        public string APN_Name { get; set; }

        public SelectList DeviceType { get; set; }

        public string DHCP_Start { get; set; }

        public byte DHCP_Status { get; set; }

        public int DHCP_User { get; set; }

        public string Gsm_Number { get; set; }

        public SelectList GsmNo { get; set; }

        public SelectList GsmNo1 { get; set; }

        public SelectList GsmNo2 { get; set; }

        public string LAN_IP { get; set; }

        public string Lan_Subnet { get; set; }

        public SelectList Location { get; set; }

        public string Lokasyon { get; set; }

        public string Remark { get; set; }

        public string Ricon_SN { get; set; }

        // Ekstra verileri tutacak bir özellik ekleyin
        public List<SelectedDataModel> SelectedData { get; set; }

        public string SelectedDeviceTypeID { get; set; }

        public string SelectedGsmNoID { get; set; }

        public List<string> SelectedGsmNoIDs { get; set; }

        public string SelectedLocationID { get; set; }

        public List<string> SelectedLocationIDs { get; set; }

        public string SelectedSeriNoID { get; set; }

        // Çoklu seçim için Liste türünde property'ler tanımlanıyor
        public List<string> SelectedSeriNoIDs { get; set; }

        // Konum ID'leri için
        public SelectList SeriNo { get; set; }

        public string Site_Name { get; set; }

        #endregion Public Properties
    }

    public class RegisteredDataModel
    {
        #region Public Properties

        public string Company_Name { get; set; }
        public string Companyname { get; set; }
        public string GSM_No { get; set; }
        public string Gsm_Number { get; set; }
        public int Id { get; set; } // Assuming there's an Id field
        public string Location { get; set; }
        public string Ricon_SN { get; set; }
        public string RiconSeriNo { get; set; }
        public string Site_Name { get; set; }

        #endregion Public Properties
    }

    public class SelectedDataModel
    {
        #region Public Properties

        public string APN_Name { get; set; }
        public DateTime Datetime { get; set; }
        public string DHCP_Start { get; set; }
        public string DHCP_Status { get; set; }
        public string DHCP_User { get; set; }
        public string GMS_No { get; set; }
        public string LAN_IP { get; set; }
        public string LAN_Subnet { get; set; }
        public string Remark { get; set; }
        public string Serial_no { get; set; }

        #endregion Public Properties
    }

    public class UploadResult
    {
        #region Public Properties

        public string ErrorMessage { get; set; }
        public List<int> ErrorRows { get; set; }
        public bool Success { get; set; }

        #endregion Public Properties
    }

    public class UserLoginModel45
    {
        #region Public Properties

        public int Company_ID { get; set; }
        public string Company_Name { get; set; }
        public string Company_Domain { get; set; }
        public string CreateDate { get; set; }
        public string Creator { get; set; }

        // Şifre (güvenlik nedenleriyle şifreler genellikle şifrelenmiş olarak saklanır)
        public bool IsAdmin { get; set; }

        public string LocationStatus { get; set; }
        public string Password { get; set; }

        // Şifre (güvenlik nedenleriyle şifreler genellikle şifrelenmiş olarak saklanır) Kullanıcı
        // tipi (örneğin, "User" veya "Admin") Oluşturan kullanıcı adı Kayıt oluşturulma tarihi ve saati
        public string Status { get; set; }

        public int User_ID { get; set; } // Kullanıcı kimliği (gerektiğinde kullanılabilir)
        public string Username { get; set; }

        #endregion Public Properties

        #region Public Methods

        // Kullanıcı adı İhtiyaç durumuna göre başka özellikler de eklenebilir
        public SNMPDB.UserLogin45 ToUserLogin()
        {
            return new SNMPDB.UserLogin45
            {
                Username = this.Username,
                Password = this.Password,
                Company_Name = this.Company_Name,
                Company_Domain = this.Company_Domain,
                Creator = this.Creator,
                IsAdmin = this.IsAdmin,
                Create_DateTime = DateTime.Now,
                Status = this.Status,
                Company_ID = this.Company_ID.ToString(),
                LocationStatus = this.LocationStatus,
                // Diğer alanları da atama yap
            };
        }

        #endregion Public Methods
    }

    public class UserViewModel
    {
        #region Public Properties

        public DateTime Create_DateTime { get; set; }
        public string Creator { get; set; }
        public bool IsAdmin { get; set; }
        public string Password { get; set; }
        public int User_ID { get; set; }
        public string Username { get; set; }
        public List<UserLogin> Users { get; set; }

        #endregion Public Properties
    }

    public class UserViewModel45
    {
        #region Public Properties

        public DateTime Create_DateTime { get; set; }
        public string Company_Name { get; set; }
        public string Company_Domain { get; set; }
        public string Creator { get; set; }
        public bool IsAdmin { get; set; }
        public string Password { get; set; }
        public int User_ID { get; set; }
        public string Username { get; set; }
        public List<UserLogin45> Users { get; set; }

        #endregion Public Properties
    }
}