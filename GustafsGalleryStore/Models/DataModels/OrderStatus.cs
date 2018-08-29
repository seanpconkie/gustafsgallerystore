using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GustafsGalleryStore.Models.DataModels
{
    public class OrderStatus
    {
        public long Id { get; set; }
        public string Status { get; set; }


        #region Public Methods
        public static List<SelectListItem> GetStatus(List<OrderStatus> resultList)
        {

            List<SelectListItem> status = new List<SelectListItem>();

            foreach (var item in resultList)
            {
                status.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.Status });
            }

            return status;
        }
        #endregion

    }
}
