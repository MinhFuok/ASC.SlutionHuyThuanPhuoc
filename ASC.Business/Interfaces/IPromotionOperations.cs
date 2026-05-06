using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Model.Models;

namespace ASC.Business.Interfaces
{
    public interface IPromotionOperations
    {
        Task CreatePromotionAsync(Promotion promotion);
        Task<Promotion> UpdatePromotionAsync(string rowKey, Promotion promotion);
        Task<List<Promotion>> GetAllPromotionsAsync();
    }
}