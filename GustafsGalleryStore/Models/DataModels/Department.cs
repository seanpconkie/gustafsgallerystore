using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GustafsGalleryStore
{
    public class Department
    {
        public long Id { get; set; }

        [Required]
        [StringLength(250)]
        public string DepartmentName { get; set; }

        [Required]
        public string DepartmentCode { get; set; }

        #region Public Methods
        public static List<SelectListItem> GetList(List<Department> resultList)
        {

            List<SelectListItem> dept = new List<SelectListItem>();

            foreach (var item in resultList)
            {
                dept.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.DepartmentName });
            }

            return dept;
        }
        #endregion

    }
}
