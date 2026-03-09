using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Controllers
{
    [Authorize]
    public class MainMenuController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly JWTRegen.Interfaces.IClaimsHelper _claimsHelper;
        private readonly Repositories.AuthRepository _auth;
        private readonly Repositories.UserMenuRepository _userMenuRepo;

        public MainMenuController(
            JWTRegen.Interfaces.IClaimsHelper claimsHelper,
            IConfiguration config,
            Repositories.AuthRepository auth,
            Repositories.UserMenuRepository userMenuRepo,
            ICompositeViewEngine viewEngine) : base(viewEngine)
        {
            _config = config;
            _claimsHelper = claimsHelper;
            _auth = auth;
            _userMenuRepo = userMenuRepo;
        }

        public class vListVM
        {
            public bool txt_unlockdis { get; set; } = false;
            public bool showAssyCheck { get; set; } = false;
        }
        public async Task<IActionResult> vList()
        {
            string jwt_user = _claimsHelper.GetUserId(User) ?? "admin";
            vListVM model = new vListVM();

            // อ่านค่า Empno จาก appsettings.json
            var empnos = _config
                .GetSection("PLT3UnlockDis:Empno")
                .Get<string[]>();

            if (empnos != null && empnos.Contains(jwt_user))
            {
                model.txt_unlockdis = true;
            }

            // Server-side permission check skipped to allow client-side visibility control based on localStorage Plant
            model.showAssyCheck = true;

            return View(model);
        }
    }
}
