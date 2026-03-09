using eqshopping.Models.DbModel;
using eqshopping.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Controllers
{
    [Authorize]
    public class UnlockCellController : BaseController
    {
        private readonly PdCellRepository _cell;
        private readonly JWTRegen.Interfaces.IClaimsHelper _claimsHelper;
        private readonly CellLockRepository _cellLock;
        private readonly UserManagementRepository _userMng;

        public UnlockCellController(
            PdCellRepository cell,
            JWTRegen.Interfaces.IClaimsHelper claimsHelper,
            CellLockRepository cellLock,
            UserManagementRepository userMng,
            ICompositeViewEngine viewEngine) : base(viewEngine)
        {
            _cell = cell;
            _claimsHelper = claimsHelper;
            _cellLock = cellLock;
            _userMng = userMng;
        }

        public class vFormFM
        {
            public string txt_plantno { get; set; } = "";
            public string txt_cellno { get; set; } = "";
            public bool txt_cellno_disabled { get; set; } = false;
        }

        public async Task<IActionResult> vIndex()
        {
            return View();
        }

        public class SearchFM
        {
            [Required]
            public string txt_plantno { get; set; }
            [Required(ErrorMessage = "กรุณากรอกข้อมูล Cell No. ก่อนการค้นหา")]
            public string txt_cellno { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Search(SearchFM form)
        {
            try
            {
                var obj_validate = await ValSearch(form);
                if (obj_validate.isError)
                {
                    return GenerateErrorResponse();
                }

                return Json(new { success = true, text = "OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    text = $"An unexpected error occurred: {ex.Message} {ex.InnerException?.Message ?? ""}"
                });
            }
        }

        public class ValSearchRS
        {
            public bool isError { get; set; } = false;
            public int? txt_cellid { get; set; }
        }

        private async Task<ValSearchRS> ValSearch(SearchFM form)
        {
            ValSearchRS obj_validate = new ValSearchRS();
            bool result = false;
            int? txt_cellid = null;

            if (!ModelState.IsValid)
            {
                obj_validate.isError = true;
                return obj_validate;
            }

            pd_cell obj_cell = await _cell.GetByCellNo(form.txt_plantno, form.txt_cellno);
            if (obj_cell == null)
            {
                ModelState.AddModelError($"txt_cellno", "ไม่พบข้อมูล Cell No. นี้");
                obj_validate.isError = true;
                return obj_validate;
            }
            else
            {
                txt_cellid = obj_cell.cellid;
            }
            if (!await _cellLock.CheckCellLock((int)txt_cellid))
            {
                ModelState.AddModelError($"txt_cellno", "Cell No. นี้ไม่ได้ถูกล็อค");
                obj_validate.isError = true;
                return obj_validate;
            }

            return new ValSearchRS()
            {
                isError = result,
                txt_cellid = txt_cellid
            };
        }

        public class vDetailFM
        {
            public vFormFM obj_header { get; set; } = new vFormFM();
            public int txt_cellid { get; set; }
            public eqs_celllock obj_celllock { get; set; } = new eqs_celllock();
        }

        public async Task<IActionResult> vDetail(string pt, string c = "")
        {
            vDetailFM form = new vDetailFM() { };

            form.obj_header.txt_cellno = c;
            form.obj_header.txt_cellno_disabled = true;
            form.txt_cellid = await _cell.GetCellId(pt, c);
            form.obj_celllock = await _cellLock.Get(pt, form.txt_cellid);
            return View(form);
        }

        public class vSaveFM
        {
            public long txt_locktranid { get; set; }
            public int txt_cellid { get; set; }
            [Required(ErrorMessage = "กรุณากรอกเหตุผลปลดล็อค")]
            public string txt_unlockreason { get; set; }
        }

        [HttpPost]

        public async Task<IActionResult> Save(vSaveFM form)
        {
            try
            {
                if (await ValSave(form))
                {
                    return GenerateErrorResponse();
                }

                await _cellLock.Unlock(form.txt_locktranid, form.txt_unlockreason);

                return Json(new { success = true, text = "การปลดล็อคเสร็จสมบูรณ์" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    text = $"An unexpected error occurred: {ex.Message} {ex.InnerException?.Message ?? ""}"
                });
            }
        }

        private async Task<bool> ValSave(vSaveFM form)
        {
            bool result = false;
            if (!ModelState.IsValid)
            {
                return true;
            }

            string jwt_user = _claimsHelper.GetUserId(User) ?? "admin";
            if (!await _userMng.CheckPermissionUnlockCell(form.txt_cellid, jwt_user))
            {
                ModelState.AddModelError("txt_form", "คุณไม่มีสิทธิ์ปลดล็อค Cell No. นี้");
                return true;
            }

            return result;
        }
    }
}
