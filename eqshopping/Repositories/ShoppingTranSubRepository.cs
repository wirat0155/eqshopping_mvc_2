using eqshopping.Controllers;
using eqshopping.Data;
using eqshopping.Models.DbModel;
using eqshopping.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class ShoppingTranSubRepository
    {
        private readonly UICT2EQSDbContext _eqsDb;
        private readonly ProductEquipmentRepository _productEquipment;

        public ShoppingTranSubRepository(
            UICT2EQSDbContext eqsDb,
            ProductEquipmentRepository productEquipment)
        {
            _eqsDb = eqsDb;
            _productEquipment = productEquipment;
        }

        public async Task<bool> Create(long txt_tranid, string txt_plantno, string txt_partno)
        {
            List<vm_product_equipment> ls_equipment = await _productEquipment.Get(txt_plantno, txt_partno);

            var insertList = ls_equipment.Select(row => new eqs_shoppingtransub
            {
                tranid = txt_tranid,
                seqno = row.seqno,
                taskcode = row.taskcode,
                equipmentno = row.equipmentno,
                shoppingflag = false,
                checkingflag = false,
                adjustflag = false
            }).ToList();

            await _eqsDb.eqs_shoppingtransub.AddRangeAsync(insertList);
            await _eqsDb.SaveChangesAsync();

            return true;
        }


        public async Task<List<eqs_shoppingtransub>> GetCheck(long tranid)
        {
            return await _eqsDb.eqs_shoppingtransub
                .Where(x => x.tranid == tranid && x.checkingflag == true)
                .OrderBy(x => x.seqno)
                .ToListAsync();
        }

        public async Task<List<eqs_shoppingtransub>> GetByTranId(long tranid)
        {
            return await _eqsDb.eqs_shoppingtransub
                .Where(x => x.tranid == tranid)
                .OrderBy(x => x.seqno)
                .ToListAsync();
        }

        public async Task<bool> CheckEquipmentIsIsList(long txt_tranid, string txt_equipmentno)
        {
            return await _eqsDb.eqs_shoppingtransub
                .AnyAsync(e => e.tranid == txt_tranid && e.equipmentno == txt_equipmentno.Trim());
        }

        public async Task<eqs_shoppingtransub> GetNextToScan(long txt_tranid)
        {
            return await _eqsDb.eqs_shoppingtransub
                .Where(e => e.tranid == txt_tranid && e.checkingflag == false)
                .OrderBy(e => e.seqno)
                .Select(e => new eqs_shoppingtransub()
                {
                    tranid = e.tranid,
                    transubid = e.transubid,
                    equipmentno = e.equipmentno,
                    checkingflag = e.checkingflag
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> Check(long txt_transubid, string jwt_user)
        {
            eqs_shoppingtransub obj_sub = await _eqsDb.eqs_shoppingtransub.Where(e => e.transubid == txt_transubid).FirstOrDefaultAsync();
            if (obj_sub != null)
            {
                obj_sub.checkingflag = true;
                obj_sub.checkingdate = DateTime.Now;
                obj_sub.checkinguser = jwt_user;
                _eqsDb.Update(obj_sub);
                await _eqsDb.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> Adjust(List<DataManagementController.SaveSubFM> ls_form, string txt_user)
        {
            foreach (var row in ls_form)
            {
                var obj = await _eqsDb.eqs_shoppingtransub.Where(e => e.transubid == row.txt_transubid).FirstOrDefaultAsync();
                if (obj != null)
                {
                    if (string.IsNullOrEmpty(row.txt_checkingflag))
                    {
                        obj.checkingflag = false;
                        obj.checkingdate = null;
                    }
                    else
                    {
                        obj.checkingflag = true;
                        obj.checkingdate = DateTime.Now;
                    }
                    obj.adjustflag = true;
                    obj.adjustdate = DateTime.Now;
                    obj.adjustuser = txt_user;
                    _eqsDb.eqs_shoppingtransub.Update(obj);
                    await _eqsDb.SaveChangesAsync();
                }
            }
            return true;
        }

        public async Task<bool> UpdateDistoUser(long txt_tranid, string jwt_user)
        {
            List<eqs_shoppingtransub> ls_sub = await _eqsDb.eqs_shoppingtransub.Where(e => e.tranid == txt_tranid).ToListAsync();
            if (ls_sub != null && ls_sub.Count > 0)
            {
                foreach (var row in ls_sub)
                {
                    row.checkingflag = false;
                    row.checkingdate = null;
                    row.checkinguser = jwt_user;
                }
                _eqsDb.UpdateRange(ls_sub);
                await _eqsDb.SaveChangesAsync();
            }
            return true;
        }
    }
}
