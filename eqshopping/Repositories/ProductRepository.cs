using eqshopping.Models.DbModel;
using Starter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class ProductRepository
    {
        private readonly DapperService _dapper;

        public ProductRepository(DapperService dapper)
        {
            _dapper = dapper;
        }

        public async Task<Product?> GetByPos(string txt_plantno, string txt_posno)
        {
            string sql = $@"SELECT TOP 1 p.[partno],
                                         [eqs_scanposflag],
                                         [eqs_scanassyflag]
                            FROM   product p
                                   INNER JOIN productionorder pos
                                           ON p.partno = pos.partno
                                              AND pos.productionorderstatus IN ( 'A', 'P' )
                            WHERE  pos.productionorderno = @p";
            return await _dapper.QueryFirst<Product>(sql, txt_plantno, new { p = txt_posno });
        }
    }
}
