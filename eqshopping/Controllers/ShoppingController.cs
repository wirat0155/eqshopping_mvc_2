using eqshopping.Models.DbModel;
using eqshopping.Models.DbView;
using eqshopping.Repositories;
using eqshopping.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Controllers
{
    [Authorize]
    public class ShoppingController : BaseController
    {
        private readonly JWTRegen.Interfaces.IClaimsHelper _claimsHelper;
        private readonly UserManagementRepository _userMng;
        private readonly PdCellRepository _cell;
        private readonly CellLockRepository _cellLock;
        private readonly ProductionOrderRepository _pos;
        private readonly ShoppingTranRepository _shoppingTran;
        private readonly ShoppingTranSubRepository _shoppingTranSub;
        private readonly ProductEquipmentRepository _productEquip;
        private readonly IConfiguration _config;

        public ShoppingController(
            JWTRegen.Interfaces.IClaimsHelper claimsHelper,
            UserManagementRepository userMng,
            PdCellRepository cell,
            CellLockRepository cellLock,
            ProductionOrderRepository pos,
            ShoppingTranRepository shoppingTran,
            ShoppingTranSubRepository shoppingTranSub,
            ProductEquipmentRepository productEquip,
            IConfiguration config,
            ICompositeViewEngine viewEngine) : base(viewEngine)
        {
            _claimsHelper = claimsHelper;
            _userMng = userMng;
            _cell = cell;
            _cellLock = cellLock;
            _pos = pos;
            _shoppingTran = shoppingTran;
            _shoppingTranSub = shoppingTranSub;
            _productEquip = productEquip;
            _config = config;
        }

        public class vFormFM
        {
            public string txt_plantno { get; set; } = "";
            public string txt_cellno { get; set; } = "";
            public bool txt_cellno_disabled { get; set; } = false;
            public string txt_posno { get; set; } = "";
            public bool txt_pos_disabled { get; set; } = false;
        }

        public IActionResult vIndex()
        {
            return View();
        }

        public class SearchFM
        {
            [Required]
            public string txt_plantno { get; set; }
            [Required(ErrorMessage = "กรุณากรอกข้อมูล Cell No. ก่อนการค้นหา")]
            public string txt_cellno { get; set; }
            [Required(ErrorMessage = "กรุณากรอกข้อมูล POS ก่อนการค้นหา")]
            public string txt_posno { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Search(SearchFM form)
        {
            try
            {
                ValSearchRS obj_validate = await ValSearch(form);
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
            public string txt_posno { get; set; } = "";
            public string txt_partno { get; set; } = "";
        }

        private async Task<ValSearchRS> ValSearch(SearchFM form)
        {
            ValSearchRS obj_validate = new ValSearchRS();
            int? txt_cellid = null;
            string txt_partno = "";
            string jwt_user = _claimsHelper.GetUserId(User) ?? "admin";
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

            if (await _cellLock.CheckCellLock((int)txt_cellid))
            {
                ModelState.AddModelError($"txt_cell", "Cell No. ถูกล็อค กรุณาแจ้งหัวหน้างานเพื่อปลดล็อค");
                obj_validate.isError = true;
                return obj_validate;
            }

            string dbCharacter = DBUtility.ChooseDb(form.txt_posno);
            vw_pdr_pos obj_pos = new vw_pdr_pos();
            if (_config["Environment"].ToUpper() == "DEVELOPMENT")
            {
                obj_pos = await _pos.GetFromNM(dbCharacter, form.txt_posno);
            }
            else
            {
                obj_pos = await _pos.Get(form.txt_plantno, form.txt_posno);
            }
            if (obj_pos == null)
            {
                ModelState.AddModelError($"txt_posno", "ไม่พบข้อมูล POS No. นี้ (POS ต้องอยู่ในสถานะ A หรือ P เท่านั้น)");
                obj_validate.isError = true;
                return obj_validate;
            }
            else
            {
                txt_partno = obj_pos.PartNo;
            }

            eqs_shoppingtran obj_shoppingtran = await _shoppingTran.Get(form.txt_plantno, form.txt_posno, txt_partno, (int)txt_cellid);
            long? txt_tranid = null;

            if (obj_shoppingtran != null)
            {
                txt_tranid = obj_shoppingtran.tranid;
            }
            else
            {
                if (!await _productEquip.CheckExist(form.txt_plantno, txt_partno))
                {
                    ModelState.AddModelError($"txt_posno", "ยังไม่มีการกำหนดข้อมูล Product Epuipment");
                    obj_validate.isError = true;
                    return obj_validate;
                }

                txt_tranid = await _shoppingTran.Create(form.txt_plantno, obj_validate.txt_posno, obj_validate.txt_partno, (int)txt_cellid, jwt_user);

                await _shoppingTranSub.Create((long)txt_tranid, form.txt_plantno, obj_validate.txt_partno);
            }

            obj_validate.txt_partno = txt_partno;
            obj_validate.txt_posno = form.txt_posno;
            return obj_validate;
        }

        public class vDetailFM
        {
            public vFormFM obj_header { get; set; } = new vFormFM();
            public string txt_partno { get; set; } = "";
            public int txt_cellid { get; set; }
            public eqs_shoppingtran obj_tran { get; set; } = new eqs_shoppingtran();
            public List<eqs_shoppingtransub> ls_sub { get; set; } = new List<eqs_shoppingtransub>();
        }
        public async Task<IActionResult> vDetail(string pt, string c = "", string p = "")
        {
            vDetailFM form = new vDetailFM() { };

            form.obj_header.txt_cellno = c;
            form.obj_header.txt_cellno_disabled = true;
            form.obj_header.txt_posno = p;
            form.obj_header.txt_pos_disabled = true;

            string dbCharacter = DBUtility.ChooseDb(p);
            var obj_pos = await _pos.GetFromNM(dbCharacter, p);
            if (obj_pos != null)
            {
                form.txt_partno = obj_pos.PartNo;
            }
            form.txt_cellid = await _cell.GetCellId(pt, c);
            form.obj_tran = await _shoppingTran.Get(pt, p, form.txt_partno, form.txt_cellid);

            form.ls_sub = await _shoppingTranSub.GetByTranId(form.obj_tran.tranid);
            return View(form);
        }
    }
}
