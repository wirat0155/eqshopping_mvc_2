using eqshopping.Data;
using eqshopping.Models.DbModel;
using eqshopping.Models.DbView;
using eqshopping.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Starter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Utility
{
    public class DropdownUtility
    {
        private readonly DapperService _dapper;
        private readonly IConfiguration _configuration;
        private readonly UICT2EQSDbContext _eqsDb;

        public DropdownUtility(DapperService dapper,
            IConfiguration configuration,
            UICT2EQSDbContext eqsDb)
        {
            _dapper = dapper;
            _configuration = configuration;
            _eqsDb = eqsDb;
        }

        public async Task<List<SelectListItem>> GetPlantList(string placeholder = "เลือก Plant", bool requirePlaceholder = true)
        {
            var plants = await _eqsDb.pd_plant
                .Where(p => new[] { "USUI", "PLT3", "BRAZING" }.Contains(p.plantno))
                .OrderBy(p => p.plantname)
                .ToListAsync();

            var selectList = plants.Select(e => new SelectListItem
            {
                Value = e.plantno,
                Text = e.plantname
            });

            if (requirePlaceholder)
            {
                return new List<SelectListItem>
                    {
                        new SelectListItem { Value = "", Text = placeholder }
                    }.Concat(selectList).ToList();
            }
            return selectList.ToList();
        }
    }
}
