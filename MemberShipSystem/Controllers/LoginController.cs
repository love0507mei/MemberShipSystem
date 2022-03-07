using Dapper;
using Login.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Login.Controllers
{
    public class LoginController : Controller
    {
		// GET: Login
		public ActionResult Login()
        {
            return View();
		}

		/// <summary>
		/// 執行登入
		/// </summary>
		/// <param name="datas"></param>
		/// <returns></returns>
		public ActionResult DoLogin(DoLogin datas)
		{
			DoLoginOut outModel = new DoLoginOut();

			if (string.IsNullOrEmpty(datas.UserID) || string.IsNullOrEmpty(datas.Password))
			{
				outModel.ErrMsg = "請輸入帳號密碼";
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

					if (AccountList.Count == 0)
					{
						outModel.ErrMsg = "帳號不存在";
					}
					else
					{
                        byte[] noAesBytes = data_Decrypt(Convert.FromBase64String(AccountList[0].Password));
                        string OriPwd = System.Text.Encoding.UTF8.GetString(noAesBytes);

                        if (datas.Password == OriPwd)
                        {
							Session["UserName"] = AccountList[0].UserName;
							Session["UserID"] = AccountList[0].UserID;
						}
                        else
                        {
							outModel.ErrMsg = "密碼錯誤";
						}
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

		//AES解密
		public byte[] data_Decrypt(byte[] encryptBytes)
		{
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

			using (MemoryStream ms = new MemoryStream())
			{
				using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
				{
					cs.Write(encryptBytes, 0, encryptBytes.Length);
					cs.FlushFinalBlock();
					return ms.ToArray();
				}
			}
		}


		/// <summary>
		/// 執行登出
		/// </summary>
		/// <returns></returns>
		public ActionResult DoLogout()
		{
			Session.Abandon();
			return RedirectToAction("Login", "Login", null);
		}

	}
}