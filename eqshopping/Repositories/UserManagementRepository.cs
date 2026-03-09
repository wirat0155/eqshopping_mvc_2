using eqshopping.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class UserManagementRepository
    {
        private readonly UICT2EQSDbContext _eqsDb;

        public UserManagementRepository(UICT2EQSDbContext eqsDb)
        {
            _eqsDb = eqsDb;
        }
        public async Task<bool> CheckPermissionByPlant(string txt_plantno, string txt_empno)
        {
            return await _eqsDb.eqs_usermanagement.Where(e => e.plantno == txt_plantno && e.username == txt_empno).AnyAsync();
        }

        public async Task<bool> CheckPermissionUnlockCell(int txt_cellid, string txt_user)
        {
            return await _eqsDb.eqs_usermanagement
                .Where(u => u.unlockcellflag == true &&
                            u.cellid == txt_cellid &&
                            u.username == txt_user)
                .AnyAsync();
        }

        public async Task<bool> CheckPermissionDataManagement(int txt_cellid, string txt_user)
        {
            return await _eqsDb.eqs_usermanagement
                .Where(u => u.datamanagementflag == true &&
                            u.cellid == txt_cellid &&
                            u.username == txt_user)
                .AnyAsync();
        }

        public async Task<bool> CheckIsDistributor(string txt_plantno, string txt_user)
        {
            return await _eqsDb.eqs_usermanagement
                .Where(u => u.plantno == txt_plantno &&
                            u.username == txt_user &&
                            u.distributorflag == true)
                .AnyAsync();
        }
    }
}
