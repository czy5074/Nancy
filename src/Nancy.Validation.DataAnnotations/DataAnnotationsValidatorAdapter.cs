﻿namespace Nancy.Validation.DataAnnotations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// A default implementation of an <see cref="IDataAnnotationsValidatorAdapter"/>.
    /// </summary>
    public abstract class DataAnnotationsValidatorAdapter : IDataAnnotationsValidatorAdapter
    {
        protected readonly string ruleType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAnnotationsValidatorAdapter"/> class.
        /// </summary>
        /// <param name="ruleType">Type of the rule.</param>
        protected DataAnnotationsValidatorAdapter(string ruleType)
        {
            this.ruleType = ruleType;
        }

        /// <summary>
        /// Gets a boolean that indicates if the adapter can handle the
        /// provided <paramref name="attribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="ValidationAttribute"/> that should be handled.</param>
        /// <returns><see langword="true" /> if the attribute can be handles, otherwise <see langword="false" />.</returns>
        public abstract bool CanHandle(ValidationAttribute attribute);

        /// <summary>
        /// Gets the rules the adapter provides.
        /// </summary>
        /// <param name="attribute">The <see cref="ValidationAttribute"/> that should be handled.</param>
        /// <param name="descriptor">A <see cref="PropertyDescriptor"/> instance for the property that is being validated.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ModelValidationRule"/> instances.</returns>
        public virtual IEnumerable<ModelValidationRule> GetRules(ValidationAttribute attribute, PropertyDescriptor descriptor)
        {
            yield return new ModelValidationRule(ruleType, attribute.FormatErrorMessage, new [] { descriptor == null ? string.Empty : descriptor.Name });
        }

        /// <summary>
        /// Validates the given instance.
        /// </summary>
        /// <param name="instance">The instance that should be validated.</param>
        /// <param name="attribute">The <see cref="ValidationAttribute"/> that should be handled.</param>
        /// <param name="descriptor">A <see cref="PropertyDescriptor"/> instance for the property that is being validated.</param>
        /// <param name="context">The <see cref="NancyContext"/> of the current request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ModelValidationRule"/> instances.</returns>
        public virtual IEnumerable<ModelValidationError> Validate(object instance, ValidationAttribute attribute, PropertyDescriptor descriptor, NancyContext context)
        {
            var validationContext = 
                new ValidationContext(instance, null, null)
                {
                    MemberName = descriptor == null ? null : descriptor.Name
                };

            if (descriptor != null)
            {
                // Display(Name) will auto populate the context, while DisplayName() needs to be manually set
                if (validationContext.MemberName == validationContext.DisplayName && !string.IsNullOrEmpty(descriptor.DisplayName))
                {
                    validationContext.DisplayName = descriptor.DisplayName;
                }

                instance = descriptor.GetValue(instance);
            }

            var result = 
                attribute.GetValidationResult(instance, validationContext);

            if (result != null)
            {
                yield return this.GetValidationError(result, validationContext, attribute);
            }
        }

        /// <summary>
        /// Gets a <see cref="ModelValidationError"/> instance based on the supplied <see cref="ValidationResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="ValidationResult"/> to create a <see cref="ModelValidationError"/> for.</param>
        /// <param name="context">The <see cref="ValidationContext"/> of the supplied <see cref="ValidationResult"/>.</param>
        /// <param name="attribute">The <see cref="ValidationAttribute"/> being validated.</param>
        /// <returns>A <see cref="ModelValidationError"/> of member names.</returns>
        protected virtual ModelValidationError GetValidationError(ValidationResult result, ValidationContext context, ValidationAttribute attribute)
        {
            return new ModelValidationError(result.MemberNames, result.ErrorMessage);
        }
    }
}