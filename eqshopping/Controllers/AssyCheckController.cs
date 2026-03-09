using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;

namespace eqshopping.Controllers
{
    [Authorize]
    public class AssyCheckController : BaseController
    {
        private readonly Repositories.ProductionOrderRepository _poRepo;
        private readonly Repositories.ShoppingTranRepository _shopRepo;
        private readonly JWTRegen.Interfaces.IClaimsHelper _claimsHelper;
        private readonly Repositories.AuthRepository _authRepo;
        private readonly Repositories.UserMenuRepository _userMenuRepo;

        public AssyCheckController(
            Repositories.ProductionOrderRepository poRepo,
            Repositories.ShoppingTranRepository shopRepo,
            JWTRegen.Interfaces.IClaimsHelper claimsHelper,
            Repositories.AuthRepository authRepo,
            Repositories.UserMenuRepository userMenuRepo,
            ICompositeViewEngine viewEngine) : base(viewEngine)
        {
            _poRepo = poRepo;
            _shopRepo = shopRepo;
            _claimsHelper = claimsHelper;
            _authRepo = authRepo;
            _userMenuRepo = userMenuRepo;
        }

        public IActionResult vCheck()
        {
            return View();
        }

        public class AssyLoginModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> VerifyAssyLogin([FromBody] AssyLoginModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                {
                    return Json(new { success = false, message = "กรุณากรอกรหัสพนักงานและรหัสผ่าน" });
                }

                // 1. Verify Login (Username/Password)
                var user = await _authRepo.Login(model.Username, model.Password);
                if (user == null)
                {
                     return Json(new { success = false, message = "รหัสพนักงาน หรือ รหัสผ่าน ไม่ถูกต้อง" });
                }

                // 2. Check Permission
                bool hasPermission = await _userMenuRepo.CheckAssyMenuPermission(model.Username);
                if (!hasPermission)
                {
                     return Json(new { success = false, message = "คุณไม่มีสิทธิ์เข้าใช้เมนู Assy Check" });
                }

                // 3. Success
                return Json(new { success = true, username = user.username }); 
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAssy(string txt_assylabel, string assy_user = "")
        {
            try
            {
                // substring 10 digit first
                string productionOrderNo = txt_assylabel.Length >= 10 ? txt_assylabel.Substring(0, 10) : txt_assylabel;
                
                if (string.IsNullOrEmpty(assy_user))
                {
                    return Json(new { success = false, message = "Session หมดอายุ กรุณา Login เข้าเมนู Assy Check ใหม่" });
                }
                string jwt_user = assy_user;

                bool isFinished = await _shopRepo.IsShoppingFinished(productionOrderNo);
                if (!isFinished)
                {
                    return Json(new { success = false, message = "ยังสแกน EQ Shopping ไม่ครบ" });
                }

                bool isShopper = await _shopRepo.CheckUserIsShopper(productionOrderNo, jwt_user);
                if (isShopper)
                {
                     return Json(new { success = false, message = "User นี้เป็นผู้ Shopping ไม่สามารถทำการ Assy Check ได้" });
                }

                var result = await _poRepo.GetCheckAssy(txt_assylabel);
                if (result == null)
                {
                    return Json(new { success = false, message = "ไม่พบข้อมูล POS นี้" });
                }

                // Check duplicate for Type S removed as per new requirement

                var history = await _poRepo.GetScanHistory(productionOrderNo);
                
                // Get counts for display (Progress)
                int quantity = await _poRepo.GetOrderQuantity(txt_assylabel); // NMT (Target)
                int numofscan = await _poRepo.GetAssyScanCount(txt_assylabel); // Default (Progress prefix 10)

                return Json(new { success = true, data = result, history = history, scanCount = numofscan, targetCount = quantity });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckPartNo(string txt_assylabel, string txt_partno, string assy_user = "")
        {
            try
            {
                if (string.IsNullOrEmpty(assy_user))
                {
                    return Json(new { success = false, message = "Session หมดอายุ กรุณา Login เข้าเมนู Assy Check ใหม่" });
                }
                string jwt_user = assy_user;
                string productionOrderNo = txt_assylabel.Length >= 10 ? txt_assylabel.Substring(0, 10) : txt_assylabel;

                if (!txt_partno.ToUpper().StartsWith("SL-"))
                {
                    return Json(new { success = false, message = "กรุณาสแกน Sticker ที่ชิ้นงาน" });
                }

                // Process Part No for validation: Remove "SL-" and last 2 chars
                // Example: SL-12345-AB -> 12345
                string processedPartNo = txt_partno.Substring(3); // Remove "SL-"
                if (processedPartNo.Length > 2)
                {
                    processedPartNo = processedPartNo.Substring(0, processedPartNo.Length - 2);
                }
                
                var matchResult = await _poRepo.CheckPartNo(txt_assylabel, processedPartNo);
                if (string.IsNullOrEmpty(matchResult))
                {
                    // Case: Part No check False
                    // Insert False
                    await _poRepo.InsertFinalAssy(txt_assylabel, txt_partno, jwt_user, false);
                    return Json(new { success = false, message = "Part No. ไม่ถูกต้อง" });
                }

                // Insert True
                await _poRepo.InsertFinalAssy(txt_assylabel, txt_partno, jwt_user, true);

                // Get updated counts
                int quantity = await _poRepo.GetOrderQuantity(txt_assylabel); // NMT
                int numofscan = await _poRepo.GetAssyScanCount(txt_assylabel); // Default
                var history = await _poRepo.GetScanHistory(productionOrderNo);

                // Check if complete (For ALL Product Types)
                if (numofscan == quantity)
                {
                    await _shopRepo.UpdateFinalAssyFlag(productionOrderNo);
                    return Json(new { success = true, message = "สแกน Final Assy ครบแล้ว", history = history, scanCount = numofscan, targetCount = quantity });
                }

                // If no specific message or not complete, return success and updated history
                return Json(new { success = true, history = history, scanCount = numofscan, targetCount = quantity });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
