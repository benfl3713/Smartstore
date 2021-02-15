﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Catalog.Products;
using Smartstore.Core.Data;
using Smartstore.Core.Identity;
using Smartstore.Core.Rules;

namespace Smartstore.Core.Checkout.Rules.Impl
{
    public class ProductReviewCountRule : IRule
    {
        private readonly SmartDbContext _db;

        public ProductReviewCountRule(SmartDbContext db)
        {
            _db = db;
        }

        public async Task<bool> MatchAsync(CartRuleContext context, RuleExpression expression)
        {
            var reviewsCount = await _db.CustomerContent
                .ApplyCustomerFilter(context.Customer.Id, true)
                .OfType<ProductReview>()
                .CountAsync();

            return expression.Operator.Match(reviewsCount, expression.Value);
        }
    }
}