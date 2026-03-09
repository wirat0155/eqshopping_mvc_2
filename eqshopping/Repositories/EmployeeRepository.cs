using eqshopping.Data;
using eqshopping.Models.DbView;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class EmployeeRepository
    {
        private readonly UICTDbContext _dbUICT;

        public EmployeeRepository(UICTDbContext dbUICT)
        {
            _dbUICT = dbUICT;
        }

        public async Task<vw_emp_general> GetEmpno(string empno)
        {
            return await _dbUICT.vw_emp_general.Where(e => e.empno == empno && e.empstatusno == "N").FirstOrDefaultAsync();
        }
    }
}
