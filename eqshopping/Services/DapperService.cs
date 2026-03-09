using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper.Contrib.Extensions;

namespace Starter.Services
{
    public class DapperService
    {
        private readonly IConfiguration _configuration;

        public DapperService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IDbConnection CreateConnection(string dbCharacter = "")
        {
            if (dbCharacter == "")
            {
                return new SqlConnection(_configuration["ConnectionStrings:UICT2"]);
            }
            else if(dbCharacter == "U")
            {
                return new SqlConnection(_configuration["ConnectionStrings:UICT"]);
            }
            else if (dbCharacter == "U2")
            {
                return new SqlConnection(_configuration["ConnectionStrings:UICT2"]);
            }
            else if (dbCharacter == "EQS")
            {
                return new SqlConnection(_configuration["ConnectionStrings:UICT2_EQS"]);
            }
            else if (dbCharacter == "NMU")
            {
                return new SqlConnection(_configuration["ConnectionStrings:USUI"]);
            }
            else if (dbCharacter == "NMB")
            {
                return new SqlConnection(_configuration["ConnectionStrings:BRAZING"]);
            }
            else if (dbCharacter == "NMT") // PLT3
            {
                return new SqlConnection(_configuration["ConnectionStrings:PLT3"]);
            }
            return new SqlConnection(_configuration["ConnectionStrings:ERP"]);
        }

        internal object Query<T>()
        {
            throw new NotImplementedException();
        }

        public async Task Execute(string query, string dbCharacter = "", object param = null)
        {
            using var connection = CreateConnection(dbCharacter);
            await connection.ExecuteAsync(query, param);
        }
        public async Task<dynamic> ExecuteScalar(string query, string dbCharacter = "", object param = null)
        {
            using var connection = CreateConnection(dbCharacter);
            return await connection.ExecuteScalarAsync(query, param);
        }
        public async Task<T> ExecuteScalar<T>(string query, string dbCharacter = "", object param = null)
        {
            using var connection = CreateConnection(dbCharacter);
            return await connection.ExecuteScalarAsync<T>(query, param);
        }
        public async Task<IEnumerable<dynamic>> Query(string query, string dbCharacter = "", object param = null)
        {
            using var connection = CreateConnection(dbCharacter);
            return await connection.QueryAsync<dynamic>(query, param);
        }
        public async Task<IEnumerable<T>> Query<T>(string query, string dbCharacter = "", object param = null)
        {
            using var connection = CreateConnection(dbCharacter);
            return await connection.QueryAsync<T>(query, param);
        }
        public async Task<dynamic> QueryFirst(string query, string dbCharacter = "", object param = null)
        {
            using var connection = CreateConnection(dbCharacter);
            return await connection.QueryFirstOrDefaultAsync<dynamic>(query, param);
        }
        public async Task<T> QueryFirst<T>(string query, string dbCharacter = "", object param = null)
        {
            using var connection = CreateConnection(dbCharacter);
            return await connection.QueryFirstOrDefaultAsync<T>(query, param);
        }
        public async Task<long> Insert<T>(T model, string dbCharacter = "") where T : class
        {
            using var connection = CreateConnection(dbCharacter);
            var insertedId = await connection.InsertAsync(model);
            return Convert.ToInt64(insertedId);
        }

        public async Task<bool> InsertRange<T>(IEnumerable<T> model, string dbCharacter = "") where T : class
        {
            using var connection = CreateConnection(dbCharacter);
            await connection.InsertAsync(model);
            return true;
        }
        public async Task<bool> Update<T>(T model, string dbCharacter = "") where T : class
        {
            using var connection = CreateConnection(dbCharacter);
            await connection.UpdateAsync(model);
            return true;
        }
        public async Task<bool> Delete<T>(T model, string dbCharacter = "") where T : class
        {
            using var connection = CreateConnection(dbCharacter);
            await connection.DeleteAsync(model);
            return true;
        }

        public async Task<T> GetN<T>(int id, string dbCharacter = "") where T : class
        {
            using var connection = CreateConnection(dbCharacter);
            return await connection.GetAsync<T>(id);
        }
        public async Task<T> GetN2<T>(short id, string dbCharacter = "") where T : class
        {
            using var connection = CreateConnection(dbCharacter);
            return await connection.GetAsync<T>(id);
        }
        public async Task<T> GetN3<T>(long id, string dbCharacter = "") where T : class
        {
            using var connection = CreateConnection(dbCharacter);
            return await connection.GetAsync<T>(id);
        }
        public async Task<T> Get2<T>(string id, string dbCharacter = "") where T : class
        {
            using var connection = CreateConnection(dbCharacter);
            return await connection.GetAsync<T>(id);
        }

    }
}
