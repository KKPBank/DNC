using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using BatchSelectService.Message.SMG;

namespace BatchSelectService.Common
{
    public class CustomDataContractResolver : DefaultContractResolver
    {
        public static readonly CustomDataContractResolver Instance = new CustomDataContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.DeclaringType == typeof(SMGRequest))
            {
                if (property.PropertyName.Equals("Params", StringComparison.OrdinalIgnoreCase))
                {
                    property.PropertyName = "params";
                }
            }
            return property;
        }
    }
}