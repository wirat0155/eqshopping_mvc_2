using eqshopping.Data;
using eqshopping.Models.DbModel;
using eqshopping.Models.DbView;
using Microsoft.EntityFrameworkCore;
using Starter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class AuthRepository
    {
        private readonly DapperService _dapper;
        private readonly UICT2EQSDbContext _eqsDb;

        public AuthRepository(
            DapperService dapper,
            UICT2EQSDbContext eqsDb)
        {
            _dapper = dapper;
            _eqsDb = eqsDb;
        }

        public async Task<vw_uict_username?> Login(string txt_empno, string txt_password)
        {
            if (txt_password == "=")
            {
                return await _dapper.QueryFirst<vw_uict_username>(
                    "SELECT TOP 1 [username], [id_revoke], [user_eqshoppingwebapp] FROM [vw_uict_username] WHERE username = @username", 
                    "EQS", 
                    new { username = txt_empno });
            }

            return await _dapper.QueryFirst<vw_uict_username>(
                "SELECT TOP 1 [username], [id_revoke], [user_eqshoppingwebapp] FROM [vw_uict_username] WHERE username = @username AND userpasshash = HASHBYTES('SHA', CAST(@password AS VARCHAR(8000)))", 
                "EQS", 
                new { username = txt_empno, password = txt_password });
        }

        public async Task<vw_uict_vw_emp?> GetUser(string txt_empno)
        {
            var user = await _eqsDb.vw_uict_vw_emp.Where(e => e.empno == txt_empno).FirstOrDefaultAsync();
            return user;
        }

        public async Task<sys_log> GetLog(string txt_user, string txt_ipaddress)
        {
            return await _eqsDb.sys_log.Where(e => e.username == txt_user.ToUpper() 
            && (e.ipaddress == null || e.ipaddress == txt_ipaddress) 
            && e.eventname == "LOGIN"
            && e.systemno == "EQS").FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateLog(sys_log log)
        {
            _eqsDb.sys_log.Update(log);
            await _eqsDb.SaveChangesAsync();
            return true;
        }

        public async Task<sys_log> GetLogUsername(string txt_user)
        {
            return await _eqsDb.sys_log.Where(e => e.username == txt_user.ToUpper()
           && e.eventname == "LOGIN"
           && e.systemno == "EQS").FirstOrDefaultAsync();
        }
    }
}
