using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UpdateInfo.Models
{
    public class UpdateInfoModel
    {
    }

    /// <summary>
    /// 更新參數
    /// </summary>
    public class DoUpdateInfo
    {
        public string OldPWD { get; set; }
        public string NewPWD { get; set; }
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