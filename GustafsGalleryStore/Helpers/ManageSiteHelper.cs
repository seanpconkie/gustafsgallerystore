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


                if (model.Id == 0)
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
                }
                else
                {
                    var inDb = context.ProductBrands.Where(x => x.Id == model.Id).SingleOrDefault();
                    inDb.Brand = model.Brand;
                    context.Update(inDb);
                }

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
                if (model.Id == 0)
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
                }
                else
                {
                    var inDb = context.Colours.Where(x => x.Id == model.Id).SingleOrDefault();
                    inDb.Value = model.Colour;
                    context.Update(inDb);
                }

                context.SaveChanges();

                return UpdateResult.Success;

            }
            catch
            {
                return UpdateResult.Error;
            }
        }

        public static UpdateResult AddDeliveryCompany (DeliveryViewModel model, GustafsGalleryStoreContext context)
        {
            try
            {
                if (model.Id == 0)
                {

                    var company = new DeliveryCompany()
                    {
                        Company = model.Company
                    };

                    List<DeliveryCompany> companies = new List<DeliveryCompany>();

                    companies = context.DeliveryCompanies.Where(x => x.Company == model.Company).ToList();

                    if (companies.Count > 0)
                    {
                        return UpdateResult.Duplicate;
                    }

                    context.Add(company);
                }
                else
                {
                    var inDb = context.DeliveryCompanies.Where(x => x.Id == model.Id).SingleOrDefault();
                    inDb.Company = model.Company;
                    context.Update(inDb);
                }

                context.SaveChanges();

                return UpdateResult.Success;

            }
            catch
            {
                return UpdateResult.Error;
            }
        }

        public static UpdateResult AddDeliveryType(DeliveryViewModel model, GustafsGalleryStoreContext context)
        {
            try
            {
                if (model.Id == 0)
                {
                    var company = context.DeliveryCompanies.Where(c => c.Company == model.Company).SingleOrDefault();
                    var type = new DeliveryType()
                    {
                        Type = model.Type,
                        Price = model.Price,
                        Time = model.Time,
                        DeliveryCompany = company
                    };

                    List<DeliveryType> types = new List<DeliveryType>();

                    types = context.DeliveryTypes.
                                   Where(x => x.DeliveryCompanyId == type.DeliveryCompany.Id).
                                   Where(x => x.Type == type.Type).
                                   ToList();

                    if (types.Count > 0)
                    {
                        return UpdateResult.Duplicate;
                    }

                    context.Add(type);
                }
                else
                {
                    var inDb = context.DeliveryTypes.Where(x => x.Id == model.Id).SingleOrDefault();
                    inDb.Type = model.Type;
                    inDb.Price = model.Price;
                    inDb.Time = model.Time;
                    context.Update(inDb);
                }

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
                if (model.Id == 0)
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
                    while (depts.Count > 0)
                    {
                        i++;
                        deptCode = model.DepartmentName.ToUpper().Substring(0, 3).Trim() + i;
                        depts = context.Departments.Where(x => x.DepartmentCode == deptCode).ToList();
                    }

                    dept.DepartmentCode = deptCode;

                    context.Add(dept);
                }
                else
                {
                    var inDb = context.Departments.Where(x => x.Id == model.Id).SingleOrDefault();
                    inDb.DepartmentName = model.DepartmentName;
                    context.Update(inDb);
                }
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
                if (model.Id == 0)
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
                }
                else
                {
                    var inDb = context.Sizes.Where(x => x.Id == model.Id).SingleOrDefault();
                    inDb.Value = model.Size;
                    context.Update(inDb);
                }
                context.SaveChanges();

                return UpdateResult.Success;

            }
            catch
            {
                return UpdateResult.Error;
            }
        }

        public static UpdateResult AddDiscount(DiscountViewModel model, GustafsGalleryStoreContext context)
        {
            try
            {
                if (model.Id == 0)
                {
                    var discount = new Discount()
                    {
                        Code = model.Code,
                        IsLive = model.IsLive,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        Value = model.Value,
                        Percentage = model.Percentage

                    };

                    List<Discount> discounts = new List<Discount>();

                    discounts = context.Discounts.Where(x => x.Code == model.Code).ToList();
                    if (discounts.Count > 0)
                    {
                        return UpdateResult.Duplicate;
                    }

                    context.Add(discount);
                }
                else
                {
                    var inDb = context.Discounts.Where(x => x.Id == model.Id).SingleOrDefault();

                    inDb.Code = model.Code;
                    inDb.IsLive = model.IsLive;
                    inDb.StartDate = model.StartDate;
                    inDb.EndDate = model.EndDate;
                    inDb.Value = model.Value;
                    inDb.Percentage = model.Percentage;

                    context.Update(inDb);
                }

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
