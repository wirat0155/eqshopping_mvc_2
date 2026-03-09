using eqshopping.Data;
using eqshopping.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Repositories
{
    public class ProductEquipmentRepository
    {
        private readonly UICT2EQSDbContext _eqsDb;

        public ProductEquipmentRepository(
            UICT2EQSDbContext eqsDb)
        {
            _eqsDb = eqsDb;
        }

        private IQueryable<vm_product_equipment> BuildProductEquipmentQuery(string plantNo, string partNo)
        {
            plantNo = plantNo.Trim();

            return _eqsDb.eqs_productequipment
                .Join(_eqsDb.eqs_equipment,
                      productEquipment => productEquipment.equipmentid,
                      equipment => equipment.equipmentid,
                      (productEquipment, equipment) => new vm_product_equipment
                      {
                          partno = productEquipment.partno,
                          taskcode = productEquipment.taskcode,
                          seqno = productEquipment.seqno,
                          equipmentno = equipment.equipmentno,
                          plantno = productEquipment.plantno
                      })
                .Where(pe => pe.partno == partNo && pe.plantno == plantNo);
        }

        public async Task<bool> CheckExist(string txt_plantno, string partNo)
        {
            return await BuildProductEquipmentQuery(txt_plantno, partNo).AnyAsync();
        }

        public async Task<List<vm_product_equipment>> Get(string txt_plantno, string txt_partno)
        {
            return await BuildProductEquipmentQuery(txt_plantno, txt_partno).ToListAsync();
        }

    }
}
