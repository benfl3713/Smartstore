﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Checkout.Cart;
using Smartstore.Core.Data;

namespace Smartstore.Core.Identity
{
    public static partial class CustomerQueryExtensions
    {
        /// <summary>
        /// Includes the complete cart graph for eager loading (including bundle items, applied discounts and rule sets).
        /// </summary>
        public static IQueryable<Customer> IncludeShoppingCart(this IQueryable<Customer> query)
        {
            Guard.NotNull(query, nameof(query));

            return query
                .Include(x => x.ShoppingCartItems.Select(y => y.BundleItem))
                .Include(x => x.ShoppingCartItems.Select(y => y.Product.AppliedDiscounts.Select(z => z.RuleSets)));
        }

        /// <summary>
        /// Selects a customer by <see cref="Customer.Email"/>, <see cref="Customer.Username"/> or <see cref="Customer.CustomerNumber"/> (in that particular order).
        /// </summary>
        /// <param name="exactMatch">Whether to perform an exact or partial field match.</param>
        public static IQueryable<Customer> ApplyIdentFilter(this IQueryable<Customer> query,
            string email = null, 
            string userName = null, 
            string customerNumber = null,
            bool exactMatch = false)
        {
            Guard.NotNull(query, nameof(query));

            if (email.HasValue())
            {
                query = exactMatch ? query.Where(c => c.Email == email) : query.Where(c => c.Email.Contains(email));
            }

            if (userName.HasValue())
            {
                query = exactMatch ? query.Where(c => c.Username == userName) : query.Where(c => c.Username.Contains(userName));
            }

            if (customerNumber.HasValue())
            {
                query = exactMatch ? query.Where(c => c.CustomerNumber == customerNumber) : query.Where(c => c.CustomerNumber.Contains(customerNumber));
            }

            return query;
        }

        /// <summary>
        /// Selects customers by <see cref="Customer.FullName"/> or <see cref="Customer.Company"/>.
        /// </summary>
        /// <param name="exactMatch">Whether to perform an exact or partial field match.</param>
        public static IQueryable<Customer> ApplySearchTermFilter(this IQueryable<Customer> query, string term, bool exactMatch = false)
        {
            Guard.NotNull(query, nameof(query));
            Guard.NotEmpty(term, nameof(term));

            query = exactMatch 
                ? query.Where(c => c.FullName == term || c.Company == term)
                : query.Where(c => c.FullName.Contains(term) || c.Company.Contains(term));

            return query;
        }

        /// <summary>
        /// Selects customers by birthdate comparing the date parts for year, month and day of month.
        /// </summary>
        /// <param name="year">Year of birth. Pass <c>null</c> for any year.</param>
        /// <param name="month">Month of year part (1-12). Pass <c>null</c> for any month.</param>
        /// <param name="day">Day of month part (1-31). Pass <c>null</c> for any day.</param>
        public static IQueryable<Customer> ApplyBirthDateFilter(this IQueryable<Customer> query, int? year, int? month, int? day)
        {
            Guard.NotNull(query, nameof(query));

            if (day > 0)
            {
                query = query.Where(c => c.BirthDate.Value.Day == day.Value);
            }

            if (month > 0)
            {
                query = query.Where(c => c.BirthDate.Value.Month == month.Value);
            }

            if (year > 0)
            {
                query = query.Where(c => c.BirthDate.Value.Year == year.Value);
            }

            return query;
        }

        /// <summary>
        /// Selects customers who have registered within a given time period and orders by <see cref="Customer.CreatedOnUtc"/> descending.
        /// </summary>
        /// <param name="fromUtc">Earliest (inclusive)</param>
        /// <param name="toUtc">Latest (inclusive)</param>
        public static IOrderedQueryable<Customer> ApplyRegistrationFilter(this IQueryable<Customer> query, DateTime? fromUtc, DateTime? toUtc)
        {
            Guard.NotNull(query, nameof(query));

            if (fromUtc.HasValue)
            {
                query = query.Where(c => fromUtc.Value <= c.CreatedOnUtc);
            }

            if (toUtc.HasValue)
            {
                query = query.Where(c => toUtc.Value >= c.CreatedOnUtc);
            }

            return query.OrderByDescending(x => x.CreatedOnUtc);
        }

        /// <summary>
        /// Selects customers who have been active within a given time period and orders by <see cref="Customer.LastActivityDateUtc"/> descending.
        /// </summary>
        /// <param name="fromUtc">Earliest (inclusive)</param>
        /// <param name="toUtc">Latest (inclusive)</param>
        public static IOrderedQueryable<Customer> ApplyLastActivityFilter(this IQueryable<Customer> query, DateTime? fromUtc, DateTime? toUtc)
        {
            Guard.NotNull(query, nameof(query));

            if (fromUtc.HasValue)
            {
                query = query.Where(c => fromUtc.Value <= c.LastActivityDateUtc);
            }

            if (toUtc.HasValue)
            {
                query = query.Where(c => toUtc.Value >= c.LastActivityDateUtc);
            }

            return query.OrderByDescending(x => x.LastActivityDateUtc);
        }

        /// <summary>
        /// Selects customers who are assigned to given customer roles.
        /// </summary>
        public static IQueryable<Customer> ApplyRolesFilter(this IQueryable<Customer> query, params int[] roleIds)
        {
            Guard.NotNull(query, nameof(query));

            if (roleIds.Length > 0)
            {
                query = query.Where(c => c.CustomerRoleMappings.Select(x => x.CustomerRoleId).Intersect(roleIds).Any());
            }

            return query;
        }

        /// <summary>
        /// Selects customers who are currently online since <paramref name="minutes"/> and orders by <see cref="Customer.LastActivityDateUtc"/> descending.
        /// </summary>
        /// <param name="minutes"></param>
        public static IOrderedQueryable<Customer> ApplyOnlineCustomersFilter(this IQueryable<Customer> query, int minutes = 20)
        {
            Guard.NotNull(query, nameof(query));

            var fromUtc = DateTime.UtcNow.AddMinutes(-minutes);

            return query
                .Where(c => c.IsSystemAccount == false)
                .ApplyLastActivityFilter(fromUtc, null);
        }

        /// <summary>
        /// Selects customers who use given password <paramref name="format"/>.
        /// </summary>
        public static IQueryable<Customer> ApplyPasswordFormatFilter(this IQueryable<Customer> query, PasswordFormat format)
        {
            Guard.NotNull(query, nameof(query));

            int passwordFormatId = (int)format;
            return query.Where(c => c.PasswordFormatId == passwordFormatId);
        }

        /// <summary>
        /// Selects customers by telephone number (partial match)
        /// </summary>
        public static IQueryable<Customer> ApplyPhoneFilter(this IQueryable<Customer> query, string phone)
        {
            Guard.NotNull(query, nameof(query));
            Guard.NotEmpty(phone, nameof(phone));

            var db = query.GetDbContext<SmartDbContext>();

            query = query
                .Join(db.GenericAttributes, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                    z.Attribute.Key == SystemCustomerAttributeNames.Phone &&
                    z.Attribute.Value.Contains(phone))
                .Select(z => z.Customer);

            return query;
        }

        /// <summary>
        /// Selects customers by ZIP postal code (partial match)
        /// </summary>
        public static IQueryable<Customer> ApplyZipPostalCodeFilter(this IQueryable<Customer> query, string zip)
        {
            Guard.NotNull(query, nameof(query));
            Guard.NotEmpty(zip, nameof(zip));

            var db = query.GetDbContext<SmartDbContext>();

            query = query
                .Join(db.GenericAttributes, x => x.Id, y => y.EntityId, (x, y) => new { Customer = x, Attribute = y })
                .Where(z => z.Attribute.KeyGroup == nameof(Customer) &&
                    z.Attribute.Key == SystemCustomerAttributeNames.ZipPostalCode &&
                    z.Attribute.Value.Contains(zip))
                .Select(z => z.Customer);

            return query;
        }

        /// <summary>
        /// Selects only customers with shopping carts and sorts by <see cref="Customer.CreatedOnUtc"/> descending.
        /// </summary>
        /// <param name="cartType">Type of cart to match. <c>null</c> to match any type.</param>
        public static IQueryable<Customer> ApplyHasCartFilter(this IQueryable<Customer> query, ShoppingCartType? cartType = null)
        {
            Guard.NotNull(query, nameof(query));

            var cartItemQuery = query
                .GetDbContext<SmartDbContext>()
                .ShoppingCartItems
                .AsNoTracking()
                .Include(x => x.Customer)
                .AsQueryable();

            if (cartType.HasValue)
            {
                cartItemQuery = cartItemQuery.Where(x => x.ShoppingCartTypeId == (int)cartType.Value);
            }

            var groupQuery =
                from sci in cartItemQuery
                group sci by sci.CustomerId into grp
                select grp
                    .OrderByDescending(x => x.CreatedOnUtc)
                    .Select(x => new
                    {
                        x.Customer,
                        x.CreatedOnUtc
                    })
                    .FirstOrDefault();

            // We have to sort again because of paging.
            query = groupQuery
                .OrderByDescending(x => x.CreatedOnUtc)
                .Select(x => x.Customer);

            return query;
        }
    }
}