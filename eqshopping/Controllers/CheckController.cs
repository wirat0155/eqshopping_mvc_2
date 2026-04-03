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
    public class CheckController : BaseController
    {
        private readonly PdCellRepository _cell;
        private readonly UserManagementRepository _userMng;
        private readonly JWTRegen.Interfaces.IClaimsHelper _claimsHelper;
        private readonly CellLockRepository _cellLock;
        private readonly ProductionOrderRepository _pos;
        private readonly CellProductRepository _cellProduct;
        private readonly ProductEquipmentRepository _productEquip;
        private readonly ShoppingTranRepository _shoppingTran;
        private readonly ShoppingTranSubRepository _shoppingTranSub;
        private readonly IConfiguration _config;

        public CheckController(
            PdCellRepository cell,
            UserManagementRepository userMng,
            JWTRegen.Interfaces.IClaimsHelper claimsHelper,
            CellLockRepository cellLock,
            ProductionOrderRepository pos,
            CellProductRepository cellProduct,
            ProductEquipmentRepository productEquip,
            ShoppingTranRepository shoppingTran,
            ShoppingTranSubRepository shoppingTranSub,
            IConfiguration config,
            ICompositeViewEngine viewEngine) : base(viewEngine)
        {
            _cell = cell;
            _userMng = userMng;
            _claimsHelper = claimsHelper;
            _cellLock = cellLock;
            _pos = pos;
            _cellProduct = cellProduct;
            _productEquip = productEquip;
            _shoppingTran = shoppingTran;
            _shoppingTranSub = shoppingTranSub;
            _config = config;
        }

        public IActionResult vIndex()
        {
            return View();
        }


        public IActionResult vIndexTest()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetAssy(string txt_plantno, string txt_posno)
        {
            try
            {
                string result = "H";
                if (txt_plantno == "PLT3" || txt_plantno == "BRAZING")
                {
                    result = "H";
                }
                else
                {
                    string dbCharacter = DBUtility.ChooseDb(txt_posno);
                    vw_pdr_pos obj_pos = await _pos.GetFromNM(dbCharacter, txt_posno);
                    if (obj_pos != null)
                    {
                        result = obj_pos.ProductType;
                    }
                }
                return Json(new { success = true, result = result });
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
            

        public class vFormFM
        {
            public string txt_plantno { get; set; } = "";
            public string txt_cellno { get; set; } = "";
            public bool txt_cellno_disabled { get; set; } = false;
            public string txt_posno { get; set; } = "";
            public bool txt_pos_disabled { get; set; } = false;
            public string txt_pos_placeholder { get; set; } = "";
            public string txt_assy { get; set; } = "";
            public bool txt_assy_disabled { get; set; } = false;
            public string txt_assy_placeholder { get; set; } = "";
        }

        public class CellFlagRS
        {
            public bool txt_pos_disabled { get; set; } = false;
            public string txt_pos_placeholder { get; set; } = "";
            public bool txt_assy_disabled { get; set; } = false;
            public string txt_assy_placeholder { get; set; } = "";
        }
        public async Task<IActionResult> GetFormView(vFormFM form)
        {
            CellFlagRS obj_result = await _cell.GetScanningResult(form.txt_plantno, form.txt_cellno, form.txt_posno, form.txt_assy);
            form.txt_pos_disabled = obj_result.txt_pos_disabled;
            form.txt_pos_placeholder = obj_result.txt_pos_placeholder;
            form.txt_assy_disabled = obj_result.txt_assy_disabled;
            form.txt_assy_placeholder = obj_result.txt_assy_placeholder;

            return PartialView("_Form", form);
        }

        public class SearchFM
        {
            [Required]
            public string txt_plantno { get; set; }
            [Required(ErrorMessage = "กรุณากรอกข้อมูล Cell No. ก่อนการค้นหา")]
            public string txt_cellno { get; set; }
            [Required(ErrorMessage = "กรุณากรอกข้อมูล POS No. ก่อนการค้นหา")]
            public string txt_posno { get; set; }
            [Required(ErrorMessage = "กรุณากรอกข้อมูล Lot Assy ก่อนการค้นหา")]
            public string txt_assylabel { get; set; }
            public string txt_role { get; set; }
        }

        public class SearchRS
        {
            public long txt_tranid { get; set; }
            public string txt_partno { get; set; }
            public List<eqs_shoppingtransub> ls_sub { get; set; } = new List<eqs_shoppingtransub>();
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

                SearchRS obj_return = new SearchRS() {
                    txt_partno = obj_validate.txt_partno
                };
                eqs_shoppingtran obj_shoppingtran =  await _shoppingTran.Get(form.txt_plantno, obj_validate.txt_posno, obj_validate.txt_partno, obj_validate.txt_cellid);
                long? txt_tranid = null;
                string jwt_user = _claimsHelper.GetUserId(User) ?? "admin";

                // 20250901 Wirat Sakorn ถ้า PLT3 สแกนจาก Distributor ครบแล้ว ให้ Reset สถานะ และเริ่มยิงของ Operator
                if (form.txt_plantno == "PLT3")
                {
                    string txt_disuser = obj_shoppingtran?.lastuser ?? "";
                    //var isDistributor = await _userMng.CheckIsDistributor(form.txt_plantno, txt_disuser);
                    var isDistributor = txt_disuser.ToUpper() == "PLT3DIS";
                    if (obj_shoppingtran != null && obj_shoppingtran.discheckingfinishflag == true && isDistributor)
                    {
                        await _shoppingTran.UpdateDistoUser(obj_shoppingtran.tranid, jwt_user);
                        await _shoppingTranSub.UpdateDistoUser(obj_shoppingtran.tranid, jwt_user);
                    }
                }
                //

                if (obj_shoppingtran != null)
                {
                    txt_tranid = obj_shoppingtran.tranid;
                }
                else
                {
                    txt_tranid = await _shoppingTran.Create(form.txt_plantno, obj_validate.txt_posno, obj_validate.txt_partno, obj_validate.txt_cellid, jwt_user);

                    await _shoppingTranSub.Create((long)txt_tranid, form.txt_plantno, obj_validate.txt_partno);
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
            public int? txt_cellid { get; set; }
        }

        private async Task<ValSearchRS> ValSearch(SearchFM form)
        {
            ValSearchRS obj_validate = new ValSearchRS();
            bool result = false;
            string txt_partno = "";
            string txt_posno = "";
            int? txt_cellid = null;
            string jwt_user = _claimsHelper.GetUserId(User) ?? "240002";

            if (!ModelState.IsValid)
            {
                obj_validate.isError = true;
                return obj_validate;
            }
            string dbCharacter = DBUtility.ChooseDb(form.txt_posno);

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

            if (!string.IsNullOrEmpty(form.txt_posno) && !string.IsNullOrEmpty(form.txt_assylabel) && !CheckMatchingPOSAssy(form.txt_posno, form.txt_assylabel))
            {
                ModelState.AddModelError($"txt_posno", "ข้อมูลไม่ถูกต้อง Lot. POS. กับ Lot. Assy. ไม่ตรงกัน");
                obj_validate.isError = true;
                return obj_validate;
            }
            if (!string.IsNullOrEmpty(form.txt_assylabel) && form.txt_assylabel.Length > 2)
            {
                txt_posno = form.txt_assylabel.Substring(0, form.txt_assylabel.Length - 2); // remove 2 last character
            }
            if (!await _userMng.CheckPermissionByPlant(form.txt_plantno, jwt_user))
            {
                ModelState.AddModelError($"txt_cell", "Username นี้ไม่มีสิทธิ์เข้า Plant นี้");
                obj_validate.isError = true;
                return obj_validate;
            }
            if (await _cellLock.CheckCellLock((int)txt_cellid))
            {
                ModelState.AddModelError($"txt_cell", "Cell No. ถูกล็อค กรุณาแจ้งหัวหน้างานเพื่อปลดล็อค");
                obj_validate.isError = true;
                return obj_validate;
            }

            vw_pdr_pos obj_pos = new vw_pdr_pos();
            if (_config["Environment"].ToUpper() == "DEVELOPMENT")
            {
                obj_pos = await _pos.GetFromNM(dbCharacter, txt_posno);
            }
            else
            {
                obj_pos = await _pos.Get(form.txt_plantno, txt_posno);
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

            if (!await _cellProduct.CheckValidPart(form.txt_plantno, txt_partno, (int)txt_cellid))
            {
                string txt_lockreason = "Part: (" + txt_partno.Trim().ToUpper() + ") ไม่อยู่ในลิสที่จะผลิตใน Cell: (" + form.txt_cellno.Trim() + ")";
                ModelState.AddModelError($"txt_posno", "Part No. นี้ไม่อยู่ในลิสที่จะผลิตใน Cell No. นี้ กรุณาแจ้งหัวหน้างานเพื่อปลดล็อค");
                await _cellLock.Lock(form.txt_plantno, (int)txt_cellid, txt_lockreason, jwt_user);
                obj_validate.isError = true;
                return obj_validate;
            }
            if (!await _productEquip.CheckExist(form.txt_plantno, txt_partno))
            {
                ModelState.AddModelError($"txt_posno", "ยังไม่มีการกำหนดข้อมูล Product Epuipment");
                obj_validate.isError = true;
                return obj_validate;
            }

            // 20250901 Wirat ถ้า PLT3 แล้ว เริ่มยิงโดยยังไม่มี DISTRIBUTOR หรือเริ่มยิงโดย DISTRIBUTOR ยังยิงไม่ครบ ให้ Lock Cell
            if (form.txt_plantno == "PLT3")
            {
                eqs_shoppingtran obj_shoppingtran = await _shoppingTran.Get(form.txt_plantno, txt_posno, txt_partno, txt_cellid);
                if ((obj_shoppingtran == null || obj_shoppingtran.discheckingfinishflag == false) && form.txt_role != "DISTRIBUTOR")
                {
                    ModelState.AddModelError($"txt_cell", "ต้องให้ Distributor สแกนก่อน กรุณาแจ้งหัวหน้างานให้ปลดล็อก");
                    string txt_lockreason = "Pos " + txt_posno + " ไม่ผ่านการสแกนจาก Distributor";
                    obj_validate.isError = true;
                    await _cellLock.Lock(form.txt_plantno, (int)txt_cellid, txt_lockreason, jwt_user);
                    return obj_validate;
                }
            }
            // 

            return new ValSearchRS()
            {
                isError = result,
                txt_posno = txt_posno,
                txt_partno = txt_partno,
                txt_cellid = txt_cellid
            };
        }

        private bool CheckMatchingPOSAssy(string txt_posno, string txt_assylabel)
        {
            string assyLabelTrimmed = txt_assylabel.Substring(0, txt_assylabel.Length - 2);
            return txt_posno == assyLabelTrimmed;
        }

        public class vDetailFM
        {
            public vFormFM obj_header { get; set; } = new vFormFM();
            public string txt_partno { get; set; } = "";
            public int txt_cellid { get; set; }
            public eqs_shoppingtran obj_tran { get; set; } = new eqs_shoppingtran();
            public List<eqs_shoppingtransub> ls_sub { get; set; } = new List<eqs_shoppingtransub>();
            public string txt_user { get; set; }
        }
        public async Task<IActionResult> vDetail(string pt, string c = "", string p = "", string a = "")
        {
            vDetailFM form = new vDetailFM() { };

            form.obj_header.txt_cellno = c;
            form.obj_header.txt_cellno_disabled = true;
            form.obj_header.txt_posno = p;
            form.obj_header.txt_pos_disabled = true;
            form.obj_header.txt_assy = a;
            form.obj_header.txt_assy_disabled = true;
            form.txt_user = _claimsHelper.GetUserId(User) ?? "240002";

            string dbCharacter = DBUtility.ChooseDb(p);
            var obj_pos = await _pos.GetFromNM(dbCharacter, p);
            if (obj_pos != null)
            {
                form.txt_partno = obj_pos.PartNo;
            }
            form.txt_cellid = await _cell.GetCellId(pt, c);
            form.obj_tran = await _shoppingTran.Get(pt, p, form.txt_partno, form.txt_cellid);

            form.ls_sub = await _shoppingTranSub.GetCheck(form.obj_tran.tranid);
            return View(form);
        }

        public class SaveFM
        {
            [Required(ErrorMessage = "ไม่พบข้อมูล Plant ติดต่อผู้ดูแลระบบ")]
            public string txt_plantno { get; set; }
            [Required(ErrorMessage = "ไม่พบข้อมูล Cell No. ติดต่อผู้ดูแลระบบ")]
            public string txt_cellno { get; set; }
            [Required(ErrorMessage = "ไม่พบข้อมูล POS No. ติดต่อผู้ดูแลระบบ")]
            public string txt_posno { get; set; }
            [Required(ErrorMessage = "ไม่พบข้อมูล Part No. ติดต่อผู้ดูแลระบบ")]
            public string txt_partno { get; set; }
            [Required(ErrorMessage = "กรุณากรอกข้อมูล Equipment No. ก่อนการบันทึก")]
            public string txt_equipmentno { get; set; }
            public long txt_tranid { get; set; }
        }

        public class SaveFinishRS
        {
            public bool finishflag { get; set; } = false;
        }
        
        [HttpPost]
        public async Task<IActionResult> Save(SaveFM form)
        {
            try
            {
                string jwt_user = _claimsHelper.GetUserId(User) ?? "240002";
                var obj_validate = await ValSave(form);
                if (obj_validate.isError)
                {
                    return GenerateErrorResponse();
                }

                #region บันทึก shoppingtransub
                await _shoppingTranSub.Check(obj_validate.transubid, jwt_user);
                #endregion

                #region เช็คและ stamp finish
                
                SaveFinishRS obj_save = await _shoppingTran.Check(form.txt_tranid, form.txt_posno, jwt_user);
                #endregion

                return Json(new { success = true, text = "การบันทึกเสร็จสมบูรณ์", finishflag = obj_save.finishflag });
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

        public class ValSaveRS
        {
            public bool isError { get; set; } = false;
            public long tranid { get; set; }
            public long transubid { get; set; }
        }

        private async Task<ValSaveRS> ValSave(SaveFM form)
        {
            ValSaveRS obj_validate = new ValSaveRS();
            int? txt_cellid = null;
            string jwt_user = _claimsHelper.GetUserId(User) ?? "240002";

            if (!ModelState.IsValid)
            {
                obj_validate.isError = true;
                return obj_validate;
            }

            #region พบ Cell หรือไม่
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
            #endregion

            #region Cell ถูกล็อกหรือไม่
            if (await _cellLock.CheckCellLock((int)txt_cellid))
            {
                ModelState.AddModelError($"txt_cellno", "Cell No. ถูกล็อค กรุณาแจ้งหัวหน้างานเพื่อปลดล็อค");
                obj_validate.isError = true;
                return obj_validate;
            }
            #endregion

            #region equipmentno อยู่ใน List ที่จะต้องสแกน
            if (!await _shoppingTranSub.CheckEquipmentIsIsList(form.txt_tranid, form.txt_equipmentno))
            {
                ModelState.AddModelError($"txt_equipmentno", "Cell ล็อคเนื่องจาก EQ.No.นี้ไม่อยู่ในลิส แจ้งหัวหน้างานเพื่อปลดล็อค");
                string txt_lockreason = "EQ.No.(" + form.txt_equipmentno + ") ไม่อยู่ในสิส [POS No.(" + form.txt_posno + ")/Part No.(" + form.txt_partno + ")]";
                await _cellLock.Lock(form.txt_plantno, (int)txt_cellid, txt_lockreason, jwt_user);
                obj_validate.isError = true;
                return obj_validate;
            }
            #endregion

            #region สแกนไปครบทุก equipment หรือยัง + ดึง equipment ถัดไปที่ต้องสแกนต่อ
            eqs_shoppingtransub obj_next = await _shoppingTranSub.GetNextToScan(form.txt_tranid);
            if (obj_next == null)
            {
                ModelState.AddModelError($"txt_equipmentno", "Cell ล็อคเนื่องจาก EQ.No. นี้ซ้ำกับที่แสกนไปแล้ว แจ้งหัวหน้างานเพื่อปลดล็อค");
                string txt_lockreason = "EQ.No.(" + form.txt_equipmentno + ") ซ้ำกับที่แสกนไปแล้ว [POS No.(" + form.txt_posno + ")/Part No.(" + form.txt_partno + ")]";
                await _cellLock.Lock(form.txt_plantno, (int)txt_cellid, txt_lockreason, jwt_user);
                obj_validate.isError = true;
                return obj_validate;
            }
            #endregion

            #region equipmentno ที่สแกน ตรงกับรายการถัดไปหรือไม่
            if (form.txt_equipmentno.ToUpper().Trim() != obj_next.equipmentno.ToUpper().Trim())
            {
                ModelState.AddModelError($"txt_equipmentno", "Cell ล็อคเนื่องจาก EQ.No. นี้ไม่ตรงกับลำดับการแสกน แจ้งหัวหน้างานเพื่อปลดล็อค");
                string txt_lockreason = "EQ.No.(" + form.txt_equipmentno + ")  นี้ไม่ตรงกับลำดับการแสกน [POS No.(" + form.txt_posno + ")/Part No.(" + form.txt_partno + ")]";
                await _cellLock.Lock(form.txt_plantno, (int)txt_cellid, txt_lockreason, jwt_user);
                obj_validate.isError = true;
                return obj_validate;
            }
            obj_validate.tranid = obj_next.tranid;
            obj_validate.transubid = obj_next.transubid;
            #endregion

            return obj_validate;
        }
    }
}
