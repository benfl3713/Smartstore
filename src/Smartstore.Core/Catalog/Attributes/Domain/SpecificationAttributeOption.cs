﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Smartstore.Core.Localization;
using Smartstore.Core.Search;
using Smartstore.Domain;

namespace Smartstore.Core.Catalog.Attributes
{
    public class SpecificationAttributeOptionMap : IEntityTypeConfiguration<SpecificationAttributeOption>
    {
        public void Configure(EntityTypeBuilder<SpecificationAttributeOption> builder)
        {
            builder.Property(c => c.NumberValue).HasPrecision(18, 4);

            builder.HasOne(c => c.SpecificationAttribute)
                .WithMany(c => c.SpecificationAttributeOptions)
                .HasForeignKey(c => c.SpecificationAttributeId);
        }
    }

    /// <summary>
    /// Represents a specification attribute option.
    /// </summary>
    public partial class SpecificationAttributeOption : BaseEntity, ILocalizedEntity, IDisplayOrder, ISearchAlias
    {
        private readonly ILazyLoader _lazyLoader;

        public SpecificationAttributeOption()
        {
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private member.", Justification = "Required for EF lazy loading")]
        private SpecificationAttributeOption(ILazyLoader lazyLoader)
        {
            _lazyLoader = lazyLoader;
        }

        /// <summary>
        /// Gets or sets the specification attribute identifier.
        /// </summary>
        public int SpecificationAttributeId { get; set; }

        private SpecificationAttribute _specificationAttribute;
        /// <summary>
        /// Gets or sets the specification attribute.
        /// </summary>
        public SpecificationAttribute SpecificationAttribute
        {
            get => _lazyLoader?.Load(this, ref _specificationAttribute) ?? _specificationAttribute;
            set => _specificationAttribute = value;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required, StringLength(4000)]
        public string Name { get; set; }

        /// <inheritdoc/>
        [StringLength(30)]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the display order.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the number value for range filtering.
        /// </summary>
        public decimal NumberValue { get; set; }

        /// <summary>
        /// Gets or sets the media file identifier.
        /// </summary>
        public int MediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the color RGB value.
        /// </summary>
        [StringLength(100)]
        public string Color { get; set; }

        private ICollection<ProductSpecificationAttribute> _productSpecificationAttributes;
        /// <summary>
        /// Gets or sets the product specification attribute mappings.
        /// </summary>
        public ICollection<ProductSpecificationAttribute> ProductSpecificationAttributes
        {
            get => _lazyLoader?.Load(this, ref _productSpecificationAttributes) ?? (_productSpecificationAttributes ??= new HashSet<ProductSpecificationAttribute>());
            protected set => _productSpecificationAttributes = value;
        }
    }
}