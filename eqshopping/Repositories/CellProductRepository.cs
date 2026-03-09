using eqshopping.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class CellProductRepository
    {
        private readonly UICT2EQSDbContext _eqsDb;

        public CellProductRepository(
            UICT2EQSDbContext eqsDb)
        {
            _eqsDb = eqsDb;
        }

        public async Task<bool> CheckValidPart(string txt_plantno, string partNo, int cellid)
        {
            return await _eqsDb.pd_cellproduct
            .AnyAsync(p => p.plantno == txt_plantno
                     && p.partno == partNo
                     && p.cellid == cellid);
        }
    }
}
