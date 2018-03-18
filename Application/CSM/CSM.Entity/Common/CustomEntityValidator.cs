using CSM.Common.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Entity.Common
{
    public static class CustomEntityValidator
    {
        public static ValidationResult ValidateEmptyList(object inputList, ValidationContext context)
        {
            ValidationResult result = null;
            var list = (inputList as IEnumerable<object>);
            if (inputList == null || list.Count() == 0)
                result = new ValidationResult(string.Format(Resource.ValErrParam_PleaseInsertAtLeastOneRecord, context.DisplayName));
            return result;
        }

        public static ValidationResult ValidateRequireInt(int input, ValidationContext context)
        {
            ValidationResult result = null;

            if (input == 0)
            {
                result = new ValidationResult(string.Format(Resource.ValErr_RequiredField, context.DisplayName));
            }
            return result;
        }

        public static ValidationResult ValidateRequireString(string input, ValidationContext context)
        {
            ValidationResult result = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                result = new ValidationResult(string.Format(Resource.ValErr_RequiredField, context.DisplayName));
            }
            return result;
        }
    }
}
