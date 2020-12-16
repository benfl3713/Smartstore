﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smartstore.Core.Localization;
using Smartstore.Core.Search;
using Smartstore.Domain;

namespace Smartstore.Core.Catalog.Attributes
{
    public class ProductVariantAttributeValueMap : IEntityTypeConfiguration<ProductVariantAttributeValue>
    {
        public void Configure(EntityTypeBuilder<ProductVariantAttributeValue> builder)
        {
            builder.HasQueryFilter(c => !c.ProductVariantAttribute.Product.Deleted);

            builder.Property(c => c.PriceAdjustment).HasPrecision(18, 4);
            builder.Property(c => c.WeightAdjustment).HasPrecision(18, 4);

            builder.HasOne(c => c.ProductVariantAttribute)
                .WithMany(c => c.ProductVariantAttributeValues)
                .HasForeignKey(c => c.ProductVariantAttributeId);
        }
    }

    /// <summary>
    /// Represents a product variant attribute value.
    /// </summary>
    [Index(nameof(Name), Name = "IX_Name")]
    [Index(nameof(ValueTypeId), Name = "IX_ValueTypeId")]
    [Index(nameof(ProductVariantAttributeId), nameof(DisplayOrder), Name = "IX_ProductVariantAttributeValue_ProductVariantAttributeId_DisplayOrder")]
    public partial class ProductVariantAttributeValue : BaseEntity, ILocalizedEntity, ISearchAlias, IDisplayOrder
    {
        private readonly ILazyLoader _lazyLoader;

        public ProductVariantAttributeValue()
        {
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private member.", Justification = "Required for EF lazy loading")]
        private ProductVariantAttributeValue(ILazyLoader lazyLoader)
        {
            _lazyLoader = lazyLoader;
        }

        /// <summary>
        /// Gets or sets the product variant attribute mapping identifier.
        /// </summary>
        public int ProductVariantAttributeId { get; set; }

        private ProductVariantAttribute _productVariantAttribute;
        /// <summary>
        /// Gets or sets the product variant attribute mapping.
        /// </summary>
        public ProductVariantAttribute ProductVariantAttribute
        {
            get => _lazyLoader?.Load(this, ref _productVariantAttribute) ?? _productVariantAttribute;
            set => _productVariantAttribute = value;
        }

        /// <summary>
        /// Gets or sets the product variant attribute name.
        /// </summary>
        [StringLength(4000)]
        public string Name { get; set; }

        /// <inheritdoc/>
        [StringLength(100)]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the media file identifier.
        /// </summary>
        public int MediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the color RGB value (used with "Boxes" attribute type).
        /// </summary>
        [StringLength(100)]
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the price adjustment.
        /// </summary>
        public decimal PriceAdjustment { get; set; }

        /// <summary>
        /// Gets or sets the weight adjustment.
        /// </summary>
        public decimal WeightAdjustment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the option is pre-selected.
        /// </summary>
        public bool IsPreSelected { get; set; }

        /// <summary>
        /// Gets or sets the display order.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the type identifier.
        /// </summary>
        public int ValueTypeId { get; set; }

        /// <summary>
        /// Gets or sets the product attribute value type.
        /// </summary>
        [NotMapped]
        public ProductVariantAttributeValueType ValueType
        {
            get => (ProductVariantAttributeValueType)ValueTypeId;
            set => ValueTypeId = (int)value;
        }

        /// <summary>
        /// Gets or sets the linked product identifier.
        /// </summary>
        public int LinkedProductId { get; set; }

        /// <summary>
        /// Gets or sets the quantity for the linked product.
        /// </summary>
        public int Quantity { get; set; }
    }
}