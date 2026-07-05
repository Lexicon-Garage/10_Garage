using Microsoft.AspNetCore.Mvc.Rendering;

namespace Garage.Web.Helper
{
    public static class EnumHelper
    {
        public static IEnumerable<SelectListItem> ToSelectList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString()
                });
        }
    }
}
