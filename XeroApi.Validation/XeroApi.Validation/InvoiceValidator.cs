﻿using System;
using System.Linq;
using Xero.Api.Core.Model;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using XeroApi.Validation.Helpers;
using Xero.Api.Core.Model.Types;
using Unity;

namespace XeroApi.Validation
{
    public class InvoiceValidator : Validator<Invoice>
    {
        Validator<LineItem> lineItemValidator = null;

        public InvoiceValidator(Validator<LineItem> lineItemValidator)
            : base(null, null)
        {
            this.lineItemValidator = lineItemValidator;
        }

        public InvoiceValidator()
            : base(null, null)
        {
            this.lineItemValidator = ValidationHelper.Container.Resolve<Validator<LineItem>>();
        }

        protected override void DoValidate(Invoice objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
            if (objectToValidate.Contact == null)
            {
                validationResults.AddResult(new ValidationResult("The document has no Contact", currentTarget, key, "Contact", this));
            }

            if (objectToValidate.LineItems == null || !objectToValidate.LineItems.Any())
            {
                validationResults.AddResult(new ValidationResult("The document has no LineItems", currentTarget, key, "LineItems", this));
            }
            else
            {
                ValidationResults vr = new ValidationResults();
                foreach (var item in objectToValidate.LineItems)
                {
                    lineItemValidator.Validate(item, vr);
                }
                if (vr.Any())
                {
                    validationResults.AddResult(new ValidationResult("Invalid LineItems", currentTarget, key, "LineItems", this, vr));
                }

                if (objectToValidate.LineItems.GetLineItemTotal() < 0)
                {
                    validationResults.AddResult(new ValidationResult("The LineItems total must be greater than 0.", currentTarget, key, "LineItems", this));
                }
            }

            if (objectToValidate.Total.HasValue)
            {
                if (objectToValidate.Total.Value != objectToValidate.LineItems.GetLineItemTotal())
                {
                    validationResults.AddResult(new ValidationResult("The document total does not equal the sum of the lines.", currentTarget, key, "Total", this));
                }
                if (objectToValidate.Total.Value < 0)
                {
                    validationResults.AddResult(new ValidationResult("The document total must be greater than 0.", currentTarget, key, "Total", this));
                }
            }

            if (objectToValidate.SubTotal.HasValue)
            {
                if (objectToValidate.SubTotal.Value != objectToValidate.LineItems.GetLineItemSubTotal())
                {
                    validationResults.AddResult(new ValidationResult("The document subtotal does not equal the sum of the lines.", currentTarget, key, "SubTotal", this));
                }
                if (objectToValidate.SubTotal.Value < 0)
                {
                    validationResults.AddResult(new ValidationResult("The document subtotal must be greater than 0.", currentTarget, key, "SubTotal", this));
                }
            }

            if (objectToValidate.TotalTax.HasValue)
            {
                if (objectToValidate.TotalTax.Value != objectToValidate.LineItems.Sum(a => a.TaxAmount))
                {
                    validationResults.AddResult(new ValidationResult("The document totaltax does not equal the sum of the lines.", currentTarget, key, "TotalTax", this));
                }
                if (objectToValidate.TotalTax.Value < 0)
                {
                    validationResults.AddResult(new ValidationResult("The document totaltax must be greater than or equal to 0.", currentTarget, key, "TotalTax", this));
                }
            }

            if (objectToValidate.Type == InvoiceType.AccountsReceivable)
            {
                if (string.IsNullOrEmpty(objectToValidate.Number))
                {
                    validationResults.AddResult(new ValidationResult("Document InvoiceNumber must be specified.", currentTarget, key, "InvoiceNumber", this));
                }
            }

            if (!objectToValidate.DueDate.HasValue)
            {
                validationResults.AddResult(new ValidationResult("DueDate must be specified.", currentTarget, key, "DueDate", this));
            }
        }

        protected override string DefaultMessageTemplate
        {
            get { throw new NotImplementedException(); }
        }
    }
}
