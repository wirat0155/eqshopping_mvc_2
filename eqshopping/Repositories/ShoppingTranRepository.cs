using eqshopping.Data;
using eqshopping.Models.DbModel;
using eqshopping.Utility;
using Microsoft.EntityFrameworkCore;
using Starter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static eqshopping.Controllers.CheckController;

namespace eqshopping.Repositories
{
    public class ShoppingTranRepository
    {
        private readonly UICT2EQSDbContext _eqsDb;
        private readonly ShoppingTranSubRepository _shoppingTranSub;
        private readonly ProductionOrderRepository _pos;
        private readonly DapperService _dapper;

        public ShoppingTranRepository(
            UICT2EQSDbContext eqsDb,
            ShoppingTranSubRepository shoppingTranSub,
            ProductionOrderRepository pos,
            DapperService dapper)
        {
            _eqsDb = eqsDb;
            _shoppingTranSub = shoppingTranSub;
            _pos = pos;
            _dapper = dapper;
        }

        public async Task<eqs_shoppingtran> Get(string txt_plantno, string txt_posno, string txt_partno, int? txt_cellid)
        {
            return await BuildSql(txt_plantno, txt_posno, txt_partno, txt_cellid)
                         .FirstOrDefaultAsync();
        }

        public async Task<long> GetTranId(string txt_plantno, string txt_posno, string txt_partno, int? txt_cellid)
        {
            return await BuildSql(txt_plantno, txt_posno, txt_partno, txt_cellid)
                         .Select(e => e.tranid)
                         .FirstOrDefaultAsync();
        }

        private IQueryable<eqs_shoppingtran> BuildSql(string plantNo, string posNo, string partNo, int? cellId)
        {
            return _eqsDb.eqs_shoppingtran.Where(e =>
                e.plantno == plantNo.Trim() &&
                e.posno == posNo.Trim() &&
                e.partno == partNo.Trim() &&
                e.cellid == cellId
            );
        }

        public async Task<long> Create(string txt_plantno, string txt_posno, string txt_partno, int? txt_cellid, string jwt_user)
        {
            var shoppingtran = new eqs_shoppingtran
            {
                plantno = txt_plantno,
                posno = txt_posno,
                partno = txt_partno,
                cellid = (int)txt_cellid,
                shoppingflag = false,
                samelastpartnoflag = false,
                checkingfinishflag = false,
                creuser = jwt_user,
                credate = DateTime.Now,
                lastuser = jwt_user,
                lastupdate = DateTime.Now,
                system = "EQS2"
            };

            await _eqsDb.eqs_shoppingtran.AddAsync(shoppingtran);
            await _eqsDb.SaveChangesAsync();
            return shoppingtran.tranid;
        }

        public async Task<SaveFinishRS> Check(long txt_tranid, string txt_posno, string txt_user)
        {
            SaveFinishRS obj_finish = new SaveFinishRS();
            bool finishflag = false;
            var obj_next = await _shoppingTranSub.GetNextToScan(txt_tranid);
            var obj_tran = await _eqsDb.eqs_shoppingtran
                                       .FirstOrDefaultAsync(e => e.tranid == txt_tranid);

            if (obj_tran != null)
            {
                // 20250903 Check complete discheckingfinishflag
                if (txt_user.ToUpper() == "PLT3DIS")
                {
                    if (obj_next == null)
                    {
                        obj_tran.discheckingfinishflag = true;
                    }
                }
                //
                else
                {
                    if (obj_next == null)
                    {
                        // Check if part exists in eqs_productequipment (PLT3)
                        bool isPartInEquip = await IsPartInProductEquipment(obj_tran.partno);
                        if (isPartInEquip)
                        {
                            obj_tran.checkingfinishflag = false;
                        }
                        else
                        {
                            obj_tran.checkingfinishflag = true;
                        }
                        
                        obj_tran.checkingfinishdate = DateTime.Now;
                        finishflag = true;
                    }
                    else
                    {
                        obj_tran.checkingfinishflag = false;
                        obj_tran.checkingfinishdate = null;
                    }
                }
                

                _eqsDb.Update(obj_tran);
                await _eqsDb.SaveChangesAsync();
            }

            if (finishflag)
            {
                string dbCharacter = DBUtility.ChooseDb(txt_posno);
                if (dbCharacter != "B")
                {
                    await _pos.UpdateStartStage2Date(dbCharacter, txt_posno);
                }
            }
            obj_finish.finishflag = finishflag;
            return obj_finish;
        }

        public async Task<bool> UpdateLastUser(long txt_tranid, string jwt_user)
        {
            var obj_tran = await _eqsDb.eqs_shoppingtran
                                        .FirstOrDefaultAsync(e => e.tranid == txt_tranid);
            if (obj_tran != null)
            {
                obj_tran.lastuser = jwt_user;
                obj_tran.lastupdate = DateTime.Now;
                _eqsDb.eqs_shoppingtran.Update(obj_tran);
                await _eqsDb.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> DeleteByPos(string txt_pos)
        {
            eqs_shoppingtran obj_tran = await _eqsDb.eqs_shoppingtran.Where(e => e.posno == txt_pos).FirstOrDefaultAsync();
            if (obj_tran != null)
            {
                List<eqs_shoppingtransub> ls_sub = await _eqsDb.eqs_shoppingtransub.Where(e => e.tranid == obj_tran.tranid).ToListAsync();
                if (ls_sub.Count > 0)
                {
                    _eqsDb.eqs_shoppingtransub.RemoveRange(ls_sub);
                    _eqsDb.SaveChanges();
                }
                _eqsDb.eqs_shoppingtran.Remove(obj_tran);
                _eqsDb.SaveChanges();
            }
            return true;
        }

        public async Task<bool> UpdateDistoUser(long txt_tranid, string jwt_user)
        {
            var obj_tran = await _eqsDb.eqs_shoppingtran
                                        .FirstOrDefaultAsync(e => e.tranid == txt_tranid);
            if (obj_tran != null)
            {
                obj_tran.checkingfinishflag = false;
                obj_tran.checkingfinishdate = null;
                obj_tran.lastuser = jwt_user;
                obj_tran.lastupdate = DateTime.Now;
                _eqsDb.eqs_shoppingtran.Update(obj_tran);
                await _eqsDb.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> IsShoppingFinished(string posNo)
        {
            var result = await _eqsDb.eqs_shoppingtran
                .Where(x => x.posno == posNo && x.checkingfinishdate != null)
                .FirstOrDefaultAsync();

            return result != null;
        }

        public async Task<bool> CheckUserIsShopper(string posNo, string user)
        {
            var count = await (from s in _eqsDb.eqs_shoppingtransub
                               join t in _eqsDb.eqs_shoppingtran on s.tranid equals t.tranid
                               where t.posno == posNo && s.checkinguser == user
                               select s.tranid).CountAsync();
            
            return count > 0;
        }

        public async Task<(List<eqshopping.Models.DbView.ShoppingHistoryDTO> Data, int TotalCount)> GetHistory(
            string plantno,
            List<string> posNos,
            List<string> partNos,
            List<string> cellNos,
            DateTime? startDate,
            DateTime? endDate,
            int skip,
            int take)
        {
            var query = from t in _eqsDb.eqs_shoppingtran
                        join s in _eqsDb.eqs_shoppingtransub on t.tranid equals s.tranid
                        join c in _eqsDb.pd_cell on t.cellid equals c.cellid
                        select new { t, s, c };

            if (!string.IsNullOrEmpty(plantno))
            {
                query = query.Where(x => x.t.plantno == plantno.Trim());
            }

            if (posNos != null && posNos.Any())
            {
                query = query.Where(x => posNos.Contains(x.t.posno));
            }
            if (partNos != null && partNos.Any())
            {
                query = query.Where(x => partNos.Contains(x.t.partno));
            }
            if (cellNos != null && cellNos.Any())
            {
                query = query.Where(x => cellNos.Contains(x.c.cellno));
            }
            if (startDate.HasValue)
            {
                query = query.Where(x => x.t.credate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(x => x.t.credate <= endDate.Value);
            }

            int totalCount = await query.CountAsync();

            var result = await query.OrderBy(x => x.t.posno).ThenBy(x => x.s.seqno)
                .Skip(skip)
                .Take(take)
                .Select(x => new eqshopping.Models.DbView.ShoppingHistoryDTO
                {
                    tranid = x.t.tranid,
                    plantno = x.t.plantno,
                    posno = x.t.posno,
                    partno = x.t.partno,
                    cellno = x.c.cellno,
                    checkingfinishdate = x.t.checkingfinishdate,
                    StartCheckUser = x.t.creuser,
                    seqno = x.s.seqno,
                    equipmentno = x.s.equipmentno,
                    checkingdate = x.s.checkingdate,
                    checkinguser = x.s.checkinguser
                }).ToListAsync();

            return (result, totalCount);
        }

        public async Task<bool> UpdateFinalAssyFlag(string posNo)
        {
            string sql = @"UPDATE eqs_shoppingtran 
                           SET checkingfinishflag = 1 
                           WHERE posno = @posNo 
                           AND checkingfinishdate IS NOT NULL 
                           AND checkingfinishflag = 0";
                           
            await _dapper.Execute(sql, "U2", new { posNo = posNo });
            return true;
        }

        public async Task<bool> IsPartInProductEquipment(string partNo)
        {
            try
            {
                string sql = @"
                  SELECT count(*)
                  FROM [eqs_productcell]
                  inner join [eqs_productequipment]
                  ON [eqs_productcell].partno = [eqs_productequipment].partno
                  WHERE [eqs_productequipment].plantno = 'PLT3'
                  AND [eqs_productcell].partno = @partNo";

                int count = await _dapper.QueryFirst<int>(sql, "U2", new { partNo = partNo });
                return count > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
