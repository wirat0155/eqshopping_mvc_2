using eqshopping.Data;
using eqshopping.Models.DbModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class CellLockRepository
    {
        private readonly UICT2EQSDbContext _eqsDb;

        public CellLockRepository(
            UICT2EQSDbContext eqsDb)
        {
            _eqsDb = eqsDb;
        }

        public async Task<bool> CheckCellLock(int txt_cellid)
        {
            return await _eqsDb.eqs_celllock
                .AnyAsync(c => c.cellid == txt_cellid && c.lockflag);
        }

        public async Task<bool> Lock(string txt_plantno, int txt_cellid, string txt_lockreason, string jwt_user)
        {
            eqs_celllock model = new eqs_celllock() {
                plantno = txt_plantno.Trim(),
                cellid = txt_cellid,
                tranid = 0,
                lockreason = txt_lockreason,
                lockflag = true,
                lockdate = DateTime.Now,
                unlockdate = null,
                unlockreason = null,
                creuser = jwt_user.Trim(),
                lastuser = jwt_user.Trim(),
                credate = DateTime.Now,
                lastupdate = DateTime.Now
            };
            // FORTEST
            await _eqsDb.eqs_celllock.AddAsync(model);
            await _eqsDb.SaveChangesAsync();
            // ENDFORTEST
            return true;
        }

        public async Task<eqs_celllock> Get(string txt_plantno, int txt_cellid)
        {
            return await _eqsDb.eqs_celllock
                .Where(e => e.plantno == txt_plantno.Trim() && e.cellid == txt_cellid && e.lockflag == true)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> Unlock(long txt_locktranid, string txt_unlockreason)
        {
            eqs_celllock eqs_Celllock = await _eqsDb.eqs_celllock.Where(e => e.locktranid == txt_locktranid).FirstOrDefaultAsync();
            if (eqs_Celllock != null)
            {
                eqs_Celllock.lockflag = false;
                eqs_Celllock.unlockdate = DateTime.Now;
                eqs_Celllock.unlockreason = txt_unlockreason.Trim();
            }
            _eqsDb.Update(eqs_Celllock);
            await _eqsDb.SaveChangesAsync();
            return true;
        }
    }
}
