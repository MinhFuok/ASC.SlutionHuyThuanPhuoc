using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Business.Interfaces;
using ASC.DataAccess;
using ASC.Model.Models;

namespace ASC.Business
{
    public class PromotionOperations : IPromotionOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public PromotionOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreatePromotionAsync(Promotion promotion)
        {
            await _unitOfWork.Repository<Promotion>().AddAsync(promotion);
            _unitOfWork.CommitTransaction();
        }

        public async Task<Promotion> UpdatePromotionAsync(string rowKey, Promotion promotion)
        {
            var promotions = await _unitOfWork.Repository<Promotion>()
                .FindAllByQuery(x => x.RowKey == rowKey);

            var existingPromotion = promotions.First();

            existingPromotion.PartitionKey = promotion.PartitionKey;
            existingPromotion.Header = promotion.Header;
            existingPromotion.Content = promotion.Content;
            existingPromotion.IsDeleted = promotion.IsDeleted;

            _unitOfWork.Repository<Promotion>().Update(existingPromotion);
            _unitOfWork.CommitTransaction();

            return existingPromotion;
        }

        public async Task<List<Promotion>> GetAllPromotionsAsync()
        {
            var promotions = await _unitOfWork.Repository<Promotion>().FindAllAsync();

            return promotions.ToList();
        }
    }
}