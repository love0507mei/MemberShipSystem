using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignUpModel.Models
{
    public class SignUpModel
    {
    }

    /// <summary>
    /// 註冊參數
    /// </summary>
    public class DoSingUp
    {
        public string UserName { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public string CheckPWD { get; set; }
    }

    /// <summary>
    /// 訊息回傳
    /// </summary>
    public class DoOut
    {
        public string ErrMsg { get; set; }
        public string ResultMsg { get; set; }
    }

    public class AccountList
    {
        public int SerialNO { get; set; }
        public string UserName { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
    }
}