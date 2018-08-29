using System;
using System.ComponentModel.DataAnnotations;
namespace GustafsGalleryStore
{
    public class Department
    {
        public long Id { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
    }
}
