using eqshopping.Controllers;
using eqshopping.Data;
using eqshopping.Models.DbModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class PdCellRepository
    {
        private readonly UICT2EQSDbContext _eqsDb;
        private readonly ProductRepository _product;

        public PdCellRepository(
            UICT2EQSDbContext eqsDb,
            ProductRepository product)
        {
            _eqsDb = eqsDb;
            _product = product;
        }
        public async Task<pd_cell?> GetByCellNo(string txt_plantno, string txt_cellno)
        {
            return await BuildSql(txt_plantno, txt_cellno)
                         .FirstOrDefaultAsync();
        }

        public async Task<int> GetCellId(string txt_plantno, string txt_cellno)
        {
            return await BuildSql(txt_plantno, txt_cellno)
                         .Select(e => e.cellid)
                         .FirstOrDefaultAsync();
        }

        public async Task<pd_cell> GetScanFlag(string txt_plantno, string txt_cellno)
        {
            return await BuildSql(txt_plantno, txt_cellno)
               .Select(e => new pd_cell
               {
                   eqs_scanposflag = e.eqs_scanposflag,
                   eqs_scanassyflag = e.eqs_scanassyflag
               })
               .FirstOrDefaultAsync();
        }


        private IQueryable<pd_cell> BuildSql(string plantNo, string cellNo)
        {
            return _eqsDb.pd_cell.Where(e =>
                e.plantno == plantNo &&
                e.cellno == cellNo &&
                e.statusno.Trim().ToUpper() == "A"
            );
        }

        public async Task<CheckController.CellFlagRS> GetScanningResult(string txt_plantno, string txt_cellno, string txt_posno, string txt_assy)
        {
            CheckController.CellFlagRS obj_result = new CheckController.CellFlagRS();
            if (txt_plantno == "PLT3" || txt_plantno == "BRAZING" ||
                (string.IsNullOrEmpty(txt_posno) && string.IsNullOrEmpty(txt_assy)))
            {
                obj_result.txt_pos_disabled = false;
                obj_result.txt_assy_disabled = false;
                return obj_result;
            }

            if (string.IsNullOrEmpty(txt_posno) && !string.IsNullOrEmpty(txt_assy))
            {
                txt_posno = txt_assy.Substring(0, txt_assy.Length - 2);
            }

            var obj_product = await _product.GetByPos(txt_plantno, txt_posno);
            var obj_cell = await GetScanFlag(txt_plantno, txt_cellno);

            obj_result.txt_pos_disabled = !GetEffectiveFlag(obj_product?.eqs_scanposflag, obj_cell?.eqs_scanposflag);
            obj_result.txt_assy_disabled = !GetEffectiveFlag(obj_product?.eqs_scanassyflag, obj_cell?.eqs_scanassyflag);
            return obj_result;
        }

        private bool GetEffectiveFlag(bool? productFlag, bool? cellFlag)
        {
            // Prioritize productFlag if it has a value, else fall back to cellFlag
            return productFlag ?? cellFlag ?? false;
        }
    }
}
