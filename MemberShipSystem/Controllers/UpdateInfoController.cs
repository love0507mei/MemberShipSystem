using AuthFilter.Filters;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Configuration;
using System.Web.Mvc;
using UpdateInfo.Models;

namespace UpdateInfo.Controllers
{
    [CustomAuthenticationFilter]
    public class UpdateInfoController : Controller
    {
        public ActionResult UpdateInfo()
        {
			return View();
        }

		/// <summary>
		/// 執行註冊
		/// </summary>
		/// <param name="datas"></param>
		/// <returns></returns>
		public ActionResult DoUpdateInfo(DoUpdateInfo datas)
		{
			DoOut outModel = new DoOut();

			if (string.IsNullOrEmpty(datas.OldPWD) || string.IsNullOrEmpty(datas.NewPWD) || string.IsNullOrEmpty(datas.CheckPWD))
			{
				outModel.ErrMsg = "請輸入必填資料";
			}
			else if (datas.NewPWD != datas.CheckPWD)
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
					AccountList = Conn.Query<AccountList>(strSql, new { UserID = HttpContext.Session["UserID"] }).ToList();

					if (AccountList.Count > 0)
					{
						byte[] noAesBytes = data_Decrypt(Convert.FromBase64String(AccountList[0].Password));
						string OriPwd = System.Text.Encoding.UTF8.GetString(noAesBytes);

                        if (datas.OldPWD != OriPwd)
                        {
							outModel.ErrMsg = "原密碼輸入錯誤，請重新嘗試";
						}
						else if (datas.NewPWD == OriPwd)
                        {
							outModel.ErrMsg = "原密碼與新密碼相同";
						}
						else
                        {
							byte[] aesBytes = data_encryption(System.Text.Encoding.UTF8.GetBytes(datas.NewPWD));
							string NewPwd = Convert.ToBase64String(aesBytes);

							// 更新至資料庫
							strSql = "UPDATE [dbo].[MemberInfo] SET [Password] = @Password WHERE [UserID] = @UserID;";
							//新增參數
							Conn.Execute(strSql, new { UserID = HttpContext.Session["UserID"], Password = NewPwd });

							outModel.ResultMsg = "更改密碼成功";
						}
					}
					else
					{
						outModel.ErrMsg = "資料有異常，請聯繫開發者";
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
	}
}