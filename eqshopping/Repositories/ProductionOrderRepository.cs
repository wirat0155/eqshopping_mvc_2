using Dapper;
using eqshopping.Data;
using eqshopping.Models.DbView;
using Starter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class ProductionOrderRepository
    {
        private readonly UICT2EQSDbContext _eqsDb;
        private readonly DapperService _dapper;
        private readonly string _tableName;

        public ProductionOrderRepository(
            UICT2EQSDbContext eqsDb,
            DapperService dapper)
        {
            _eqsDb = eqsDb;
            _dapper = dapper;
            _tableName = "ProductionOrder";
        }

        public async Task<vw_pdr_pos> Get(string txt_plantno, string txt_posno)
        {
            var sql = BuildSql(txt_plantno, "[ProductionOrderNo], [PartNo]");
            return await _dapper.QueryFirst<vw_pdr_pos>(sql, "EQS", new { p = txt_posno });
        }

        public async Task<vw_pdr_pos> GetFromNM(string txt_plantno, string txt_posno)
        {
            string sql = $@"SELECT [ProductionOrderNo], p.[PartNo], [ProductType] FROM [ProductionOrder] p
            INNER JOIN [Product] pt ON pt.PartNo = p.PartNo 
            WHERE [ProductionOrderNo] = @p";
            return await _dapper.QueryFirst<vw_pdr_pos>(sql, txt_plantno, new { p = txt_posno });
        }

        public async Task<string> GetPartNo(string txt_plantno, string txt_posno)
        {
            var sql = BuildSql(txt_plantno, "[PartNo]");
            return await _dapper.QueryFirst<string>(sql, "EQS", new { p = txt_posno });
        }

        private string BuildSql(string plantNo, string selectFields)
        {
            return $"SELECT TOP 1 {selectFields} FROM [vw_pdr_pos_{plantNo}] WHERE [ProductionOrderNo] = @p";
        }

        public async Task<bool> UpdateStartStage2Date(string dbCharacter, string txt_posno)
        {
            string sql = $@"UPDATE [{_tableName}] SET [StartStage2Date] = GETDATE() WHERE [ProductionOrderNo] = @p";
            await _dapper.Execute(sql, dbCharacter, new { p = txt_posno });
            return true;
        }

        public async Task<vw_pdr_pos> GetCheckAssy(string assyLabel)
        {
            // substring 10 digit first
            string productionOrderNo = assyLabel.Length >= 10 ? assyLabel.Substring(0, 10) : assyLabel;
            string dbCharacter = eqshopping.Utility.DBUtility.ChooseDb(productionOrderNo);

            string sql = $@"select productionorderno, po.partno, productType from productionorder po
                inner join product p on po.partno = p.partno
                where productionorderno = @pono";

            return await _dapper.QueryFirst<vw_pdr_pos>(sql, dbCharacter, new { pono = productionOrderNo });
        }

        public async Task<string> CheckPartNo(string assyLabel, string partNo)
        {
            try
            {
                string productionOrderNo = assyLabel.Length >= 10 ? assyLabel.Substring(0, 10) : assyLabel;
                string dbCharacter = eqshopping.Utility.DBUtility.ChooseDb(productionOrderNo);

                string sql = $@"SELECT TOP 1 [ProductionOrderNo] FROM [ProductionOrder]
                            WHERE [PartNo] = @partno AND [ProductionOrderNo] = @pono";

                return await _dapper.QueryFirst<string>(sql, dbCharacter, new { partno = partNo, pono = productionOrderNo });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<int> GetOrderQuantity(string assyLabel)
        {
            string productionOrderNo = assyLabel.Length >= 10 ? assyLabel.Substring(0, 10) : assyLabel;
            string sql = "select OrderQuantity from productionorder where ProductionOrderNo = @pono";
            return await _dapper.QueryFirst<int>(sql, "NMT", new { pono = productionOrderNo });
        }

        public async Task<int> GetAssyQtyLimit(string assyLabel)
        {
            string productionOrderNo = assyLabel.Length >= 10 ? assyLabel.Substring(0, 10) : assyLabel;
            string sql = "SELECT ProductPackQuantity from productionorder where ProductionOrderNo = @pono";
            return await _dapper.QueryFirst<int>(sql, "NMT", new { pono = productionOrderNo });
        }

        public async Task<int> GetAssyScanCount(string assyLabel)
        {
            string prefix = assyLabel.Length >= 10 ? assyLabel.Substring(0, 10) : assyLabel;
            string sql = "select Count(id) AS numofscan from eqs_finalassy where LEFT(assylabel, 10) = @prefix and checkflag = 1";
            return await _dapper.QueryFirst<int>(sql, "", new { prefix = prefix });
        }

        public async Task<int> GetAssyLabelScanCount(string assyLabel)
        {
            string sql = "select Count(id) AS numofscan from eqs_finalassy where assylabel = @label and checkflag = 1";
            return await _dapper.QueryFirst<int>(sql, "", new { label = assyLabel });
        }

        public async Task<bool> InsertFinalAssy(string assyLabel, string partNo, string user, bool checkFlag)
        {
            try
            {
                string sql = $@"
                    DECLARE @MaxSeq TINYINT;
                    SELECT @MaxSeq = ISNULL(MAX(sequenceno), 0) FROM eqs_finalassy WHERE assylabel = @label;
                    
                    INSERT INTO eqs_finalassy (assylabel, sequenceno, checkuser, checkdate, actual_partno, checkflag)
                    VALUES (@label, @MaxSeq + 1, @user, GETDATE(), @partno, @flag);
                ";
                
                await _dapper.Execute(sql, "", new { label = assyLabel, user = user, partno = partNo, flag = checkFlag });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<dynamic>> GetScanHistory(string productionOrderNo)
        {
            string sql = $@"
                SELECT [assylabel]
                      ,[actual_partno]
                      ,[checkuser]
                      ,[checkdate]
                  FROM [eqs_finalassy]
                WHERE assylabel LIKE @pono + '%'
                AND checkflag = 1
                ORDER BY checkdate DESC";

            return await _dapper.Query<dynamic>(sql, "", new { pono = productionOrderNo });
        }

        public async Task<bool> CheckDuplicateAssy(string assyLabel)
        {
            string sql = "SELECT COUNT(id) FROM eqs_finalassy WHERE assylabel = @label AND checkflag = 1";
            int count = await _dapper.QueryFirst<int>(sql, "", new { label = assyLabel });
            return count > 0;
        }
        
        public async Task<(List<dynamic> Data, int TotalCount)> GetFinalAssyHistory(
            string plantno,
            List<string> posNos,
            List<string> partNos,
            DateTime? startDate,
            DateTime? endDate,
            int skip,
            int take,
            string sortColumn,
            string sortDirection)
        {
            var p = new DynamicParameters();
            
            string where = " WHERE 1=1 ";
            
            if (startDate.HasValue)
            {
                where += " AND t.[checkdate] >= @startDate ";
                p.Add("startDate", startDate.Value);
            }
            
            if (endDate.HasValue)
            {
                where += " AND t.[checkdate] <= @endDate ";
                p.Add("endDate", endDate.Value.Date.AddDays(1).AddTicks(-1));
            }
            
            if (posNos != null && posNos.Any())
            {
                where += " AND LEFT(t.assylabel, 10) IN @posNos ";
                p.Add("posNos", posNos);
            }
            
            // For Part we might need to check Shopping Part No or Actual Part No
            // Usually we filter by target part no in Shopping history.
            if (partNos != null && partNos.Any())
            {
                where += " AND s.partno IN @partNos ";
                p.Add("partNos", partNos);
            }
            
            // Base sql with join
            string baseSql = @"
                FROM [eqs_finalassy] t
                INNER JOIN [eqs_shoppingtran] s
                ON LEFT(t.[assylabel], 10) = s.[posno] ";
                
            if (!string.IsNullOrEmpty(plantno))
            {
                baseSql += " AND s.plantno = @plantno ";
                p.Add("plantno", plantno);
            }
            
            string countSql = "SELECT COUNT(*) " + baseSql + where;
            int totalCount = await _dapper.QueryFirst<int>(countSql, "", p);
            
            // Handle Sorting
            string orderBy = " ORDER BY t.checkdate DESC "; // Default
            if (!string.IsNullOrEmpty(sortColumn))
            {
                string dir = (sortDirection?.ToLower() == "asc") ? "ASC" : "DESC";
                // Prevent SQL injection by whitelisting columns or validating
                // Assuming internal call, simple switch
                switch (sortColumn.ToLower())
                {
                    case "id": orderBy = $" ORDER BY t.id {dir} "; break;
                    case "assylabel": orderBy = $" ORDER BY t.assylabel {dir} "; break;
                    case "sequenceno": orderBy = $" ORDER BY t.sequenceno {dir} "; break;
                    case "checkuser": orderBy = $" ORDER BY t.checkuser {dir} "; break;
                    case "checkdate": orderBy = $" ORDER BY t.checkdate {dir} "; break;
                    case "partno": orderBy = $" ORDER BY s.partno {dir} "; break;
                    case "actual_partno": orderBy = $" ORDER BY t.actual_partno {dir} "; break;
                    case "checkflag": orderBy = $" ORDER BY t.checkflag {dir} "; break;
                    default: orderBy = $" ORDER BY t.checkdate DESC "; break;
                }
            }

            string selectSql = @"
                SELECT t.[id]
                      ,t.[assylabel]
                      ,t.[sequenceno]
                      ,t.[checkuser]
                      ,t.[checkdate]
                      ,s.[partno]
                      ,t.[actual_partno]
                      ,t.[checkflag]
            " + baseSql + where + orderBy + " OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
            
            p.Add("skip", skip);
            p.Add("take", take);
            
            var result = await _dapper.Query<dynamic>(selectSql, "", p);
            return (result.ToList(), totalCount);
        }
    }
}
