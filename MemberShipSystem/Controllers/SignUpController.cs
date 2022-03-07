using Dapper;
using SignUpModel.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SignUp.Controllers
{
    public class SignUpController : Controller
    {
		// GET: SignUp
		public ActionResult SignUp()
        {
            return View();
        }

        /// <summary>
        /// 執行註冊
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public ActionResult DoSingUp(DoSingUp datas)
        {
			DoOut outModel = new DoOut();

			if (string.IsNullOrEmpty(datas.UserID) || string.IsNullOrEmpty(datas.Password) || string.IsNullOrEmpty(datas.UserName))
			{
				outModel.ErrMsg = "請輸入必填資料";
			}
			else if (datas.Password != datas.CheckPWD)
			{
				outModel.ErrMsg = "密碼與確認密碼不相同";
			}
			else
			{
				SqlConnection Conn = null;
				try
				{
					// 資料庫連線
					string ConnStr = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ConnDB"].ConnectionString;
					Conn = new SqlConnection();
					Conn.ConnectionString = ConnStr;
					Conn.Open();

					// 使用參數化填值
					List<AccountList> AccountList = null;
					string strSql = "SELECT * FROM [dbo].[MemberInfo] WHERE [UserID] = @UserID";
					AccountList = Conn.Query<AccountList>(strSql, new { UserID = datas.UserID }).ToList();

					if (AccountList.Count > 0)
					{
						outModel.ErrMsg = "此帳號已存在";
					}
					else
					{
						byte[] aesBytes = data_encryption(System.Text.Encoding.UTF8.GetBytes(datas.Password));
						string NewPwd = Convert.ToBase64String(aesBytes);

						// 註冊資料新增至資料庫
						strSql = "INSERT INTO [dbo].[MemberInfo] (UserName,UserID,Password) VALUES (@UserName, @UserID, @Password);";
						//新增參數
						Conn.Execute(strSql, new { UserName = datas.UserName, UserID = datas.UserID, Password = NewPwd });

						outModel.ResultMsg = "註冊完成";
					}
				}
				catch (Exception ex)
				{
					throw ex;
				}
				finally
				{
					if (Conn != null)
					{
						//關閉資料庫連線
						Conn.Close();
						Conn.Dispose();
					}
				}
			}

			// 輸出json
			return Json(outModel);
		}

		//AES加密
		public byte[] data_encryption(byte[] dataBytes)
		{
			//宣告AES 256加密演算法
			AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
			SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
			byte[] key = sha256.ComputeHash(Convert.FromBase64String(WebConfigurationManager.AppSettings["key"]));
			byte[] iv = sha256.ComputeHash(Convert.FromBase64String(WebConfigurationManager.AppSettings["iv"]));
			byte[] xx = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				xx[i] = iv[i];
			}

			aes.Key = key;
			aes.IV = xx;

			byte[] encryptDataBytes;
			using (MemoryStream msData = new MemoryStream())
			using (CryptoStream cs = new CryptoStream(msData, aes.CreateEncryptor(), CryptoStreamMode.Write))
			{
				cs.Write(dataBytes, 0, dataBytes.Length);
				cs.FlushFinalBlock();
				encryptDataBytes = new byte[msData.Length];
				msData.Position = 0;
				msData.Read(encryptDataBytes, 0, (int)msData.Length);
				msData.Close();
			}
			return encryptDataBytes;
		}
	}
}