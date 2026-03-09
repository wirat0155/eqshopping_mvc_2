using eqshopping.Models.DbModel;
using Starter.Services;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class UserMenuRepository
    {
        private readonly DapperService _dapper;

        public UserMenuRepository(DapperService dapper)
        {
            _dapper = dapper;
        }

        public async Task<bool> CheckAssyMenuPermission(string username)
        {
            var result = await _dapper.QueryFirst<UserMenu>(
                "select * from usermenu where username = @username and mnuid = '1516' and id_enabled = 1",
                "U", 
                new { username });
            return result != null;
        }
    }
}
