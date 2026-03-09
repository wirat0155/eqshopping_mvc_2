//using eqshopping.Repositories;
using eqshopping.Repositories;
using JWTRegen.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IJwtTokenService = JWTRegen.Interfaces.IJwtTokenService;
using IClaimsHelper = JWTRegen.Interfaces.IClaimsHelper;
using eqshopping.Models.FormModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using eqshopping.Utility;
using eqshopping.Models.DbView;
using System.Net;

namespace eqshopping.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IClaimsHelper _claimsHelper;
        private readonly AuthRepository _auth;
        private readonly DropdownUtility _dd;
        private readonly UserManagementRepository _userMng;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ILogger<AuthController> logger,
            IConfiguration configuration,
            IJwtTokenService jwtTokenService,
            IClaimsHelper claimsHelper,
            AuthRepository auth,
            DropdownUtility dd,
            UserManagementRepository userMng,
            ICompositeViewEngine viewEngine) : base(viewEngine)
        {
            _logger = logger;
            _configuration = configuration;
            _jwtTokenService = jwtTokenService;
            _claimsHelper = claimsHelper;
            _auth = auth;
            _dd = dd;
            _userMng = userMng;
        }

        public class vLoginFM
        {
            public List<SelectListItem> op_plant { get; set; }
        }

        public async Task<IActionResult> vLogin()
        {
            vLoginFM form = new vLoginFM()
            {
                op_plant = await _dd.GetPlantList()
            };
            return View(form);
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginFM form)
        {
            try
            {
                if (await ValLogin(form))
                {
                    return GenerateErrorResponse();
                }

                var token = _jwtTokenService.GenerateToken(form.txt_empno, "user");

                Response.Cookies.Append("eqshopping_jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    //Secure = true, disable when use http
                    SameSite = SameSiteMode.Strict,
                    Path = "/", // Set cookie available across the entire site
                    Expires = DateTime.UtcNow.AddHours(13)
                });

                //var isDistributor = await _userMng.CheckIsDistributor(form.txt_plantno, form.txt_empno);
                var isDistributor = form.txt_empno.ToUpper() == "PLT3DIS";
                string txt_role = isDistributor ? "DISTRIBUTOR" : "USER";

                return Json(new { success = true, text = txt_role });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errors = ex.Message });
            }
        }

        private async Task<bool> ValLogin(LoginFM form)
        {
            bool result = false;
            if (!ModelState.IsValid)
            {
                return true;
            }

            // 20250903 Wirat Sakorn Bypass for PLT3 Distributor
            if (form.txt_empno.ToUpper() == "PLT3DIS" && form.txt_plantno == "PLT3")
            {
                if (form.txt_password.ToUpper() != "PLT3DIS")
                {
                    ModelState.AddModelError($"txt_empno", "รหัสพนักงาน และ รหัสผ่านไม่ถูกต้อง กรุณาตรวจสอบอีกครั้ง");
                    return true;
                }

                // เช็คว่าเป็นเครื่องเดียวกัน Login ต่อไหม ถ้าคนละเครื่อง ต้องให้ Foreman ปลดล็อก
                string ipaddress = this.GetIP();
                var log = await _auth.GetLog(form.txt_empno, ipaddress);
                if (log != null && !string.IsNullOrEmpty(ipaddress))
                {
                    log.ipaddress = ipaddress;
                    await _auth.UpdateLog(log);
                }
                else
                {
                    ModelState.AddModelError($"txt_empno", "มีการ Login ด้วยเครื่องอื่นแล้ว");
                    return true;
                }
                return result;
            }
            //
            if (!await _userMng.CheckPermissionByPlant(form.txt_plantno, form.txt_empno))
            {
                ModelState.AddModelError($"txt_plantno", "รหัสพนักงานนี้ ยังไม่ถูกกำหนดสิทธิ์ให้เข้าระบบ");
                return true;
            }

            vw_uict_username user = await _auth.Login(form.txt_empno, form.txt_password);
            if (user == null)
            {
                ModelState.AddModelError($"txt_empno", "รหัสพนักงาน และ รหัสผ่านไม่ถูกต้อง กรุณาตรวจสอบอีกครั้ง");
                return true;
            }
            else if (user.id_revoke == true)
            {
                ModelState.AddModelError($"txt_empno", "รหัสพนักงานนี้ นี้ถูกยกเลิกสิทธิ์");
                return true;
            }
            else if (user.user_eqshoppingwebapp == false)
            {
                ModelState.AddModelError($"txt_empno", "รหัสพนักงานนี้ ไม่มีสิทธิ์ในการเข้าใช้งานระบบ กรุณาตรวจสอบอีกครั้ง");
                return true;
            }
            vw_uict_vw_emp userinfo = await _auth.GetUser(form.txt_empno);
            if (userinfo == null)
            {
                ModelState.AddModelError($"txt_empno", "ไม่พบรหัสพนักงานของท่าน กรุณาตรวจสอบอีกครั้ง");
                return true;
            }
            else if (userinfo.empstatusno.Trim().ToUpper() == "R")
            {
                ModelState.AddModelError($"txt_empno", "รหัสพนักงานท่านนี้ได้ลาออกไปแล้ว");
                return true;
            }

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            string jwt_user = _claimsHelper.GetUserId(User) ?? "admin";
            string ipaddress = this.GetIP();
            var log = await _auth.GetLog(jwt_user, ipaddress);
            if (log != null)
            {
                log.ipaddress = null;
                await _auth.UpdateLog(log);
            }

            Response.Cookies.Append("eqshopping_jwt", string.Empty, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Path = "/", // Set cookie available across the entire site
                Expires = DateTime.UtcNow.AddDays(-1)
            });

            

            return RedirectToAction(nameof(vLogin));
        }
        public string GetIP()
        {
            string ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // ถ้าเป็น IPv6 localhost (::1) แปลงเป็น IPv4 127.0.0.1
            if (ipaddress == "::1")
            {
                ipaddress = Dns.GetHostEntry(Dns.GetHostName())
                              .AddressList
                              .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                              .ToString();
            }

            return ipaddress;
        }

        public async Task<IActionResult> ClearIP()
        {
            var log = await _auth.GetLogUsername("PLT3DIS");
            if (log != null)
            {
                log.ipaddress = null;
                await _auth.UpdateLog(log);
            }
            return RedirectToAction("vList", "MainMenu");
        }
    }
}
