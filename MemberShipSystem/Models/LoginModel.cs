using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Login.Models
{
    public class LoginModel
    {
    }

    /// <summary>
    /// 登入參數
    /// </summary>
    public class DoLogin
    {
        public string UserID { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// 登入訊息回傳
    /// </summary>
    public class DoLoginOut
    {
        public string ErrMsg { get; set; }
        public string ResultMsg { get; set; }
        public string Token { get; set; }
        public bool Status { get; set; }
    }
    public class AccountList
    {
        public int SerialNO { get; set; }
        public string UserName { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
    }
}