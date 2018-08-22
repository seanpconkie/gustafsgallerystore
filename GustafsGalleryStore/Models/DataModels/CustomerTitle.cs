using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GustafsGalleryStore.Models.DataModels
{
    public class CustomerTitle
    {
        #region Public Properties
        public long Id { get; set; }
        public string Value { get; set; }
        #endregion

        #region Public Methods
        public static List<SelectListItem> GetTitles(List<CustomerTitle> resultList)
        {

            List<SelectListItem> titles = new List<SelectListItem>();

            foreach (var item in resultList)
            {
                titles.Add(new SelectListItem { Value = item.Value, Text = item.Value });
            }

            return titles;
        }
        #endregion
    }
}
