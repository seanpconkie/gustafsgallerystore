using System;
using System.Collections.Generic;
using System.Linq;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;
using GustafsGalleryStore.Models.ViewModels;

namespace GustafsGalleryStore.Helpers
{
    public static class ManageSiteHelper
    {

        public static UpdateResult AddBrand(BrandViewModel model, GustafsGalleryStoreContext context)
        {
            try
            {

                var brand = new ProductBrand()
                {
                    Brand = model.Brand
                };

                string brandCode = model.Brand.ToUpper().Substring(0, 3).Trim();
                List<ProductBrand> brands = new List<ProductBrand>();

                brands = context.ProductBrands.Where(x => x.Brand == model.Brand).ToList();
                if (brands.Count > 0)
                {
                    return UpdateResult.Duplicate;
                }

                brands = context.ProductBrands.Where(x => x.BrandCode == brandCode).ToList();
                int i = 0;
                while (brands.Count > 0)
                {
                    i++;
                    brandCode = model.Brand.ToUpper().Substring(0, 3).Trim() + i;
                    brands = context.ProductBrands.Where(x => x.BrandCode == brandCode).ToList();
                }

                brand.BrandCode = brandCode;

                context.Add(brand);
                context.SaveChanges();

                return UpdateResult.Success;

            }
            catch
            {
                return UpdateResult.Error;
            }
        }

        public static UpdateResult AddColour(ColourViewModel model, GustafsGalleryStoreContext context)
        {
            try
            {

                var colour = new Colour()
                {
                    Value = model.Colour
                };

                List<Colour> colours = new List<Colour>();

                colours = context.Colours.Where(x => x.Value == model.Colour).ToList();
                if (colours.Count > 0)
                {
                    return UpdateResult.Duplicate;
                }

                context.Add(colour);
                context.SaveChanges();

                return UpdateResult.Success;

            }
            catch
            {
                return UpdateResult.Error;
            }
        }

        public static UpdateResult AddDepartment(DepartmentViewModel model, GustafsGalleryStoreContext context)
        {
            try
            {



                var dept = new Department()
                {
                    DepartmentName = model.DepartmentName
                };

                string deptCode = model.DepartmentName.ToUpper().Substring(0, 3).Trim();
                List<Department> depts = new List<Department>();

                depts = context.Departments.Where(x => x.DepartmentName == model.DepartmentName).ToList();
                if (depts.Count > 0)
                {
                    return UpdateResult.Duplicate;
                }

                depts = context.Departments.Where(x => x.DepartmentCode == deptCode).ToList();
                int i = 0;
                while(depts.Count > 0)
                {
                    i ++;
                    deptCode = model.DepartmentName.ToUpper().Substring(0, 3).Trim() + i;
                    depts = context.Departments.Where(x => x.DepartmentCode == deptCode).ToList();
                }

                dept.DepartmentCode = deptCode;

                context.Add(dept);
                context.SaveChanges();

                return UpdateResult.Success;

            }
            catch
            {
                return UpdateResult.Error;
            }
        }

        public static UpdateResult AddSize(SizeViewModel model, GustafsGalleryStoreContext context)
        {
            try
            {
                var size = new Size()
                {
                    Value = model.Size
                };

                List<Size> sizes = new List<Size>();

                sizes = context.Sizes.Where(x => x.Value == model.Size).ToList();
                if (sizes.Count > 0)
                {
                    return UpdateResult.Duplicate;
                }

                context.Add(size);
                context.SaveChanges();

                return UpdateResult.Success;

            }
            catch
            {
                return UpdateResult.Error;
            }
        }
    }
}
