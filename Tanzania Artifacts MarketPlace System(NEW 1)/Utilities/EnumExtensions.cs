using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Utilities
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());
            var displayAttribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}
