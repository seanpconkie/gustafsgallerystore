using System.Linq;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GustafsGalleryStore.Models.DataModels;
using IEmailSender = GustafsGalleryStore.Services.IEmailSender;
using Microsoft.AspNetCore.Authorization;
using GustafsGalleryStore.Helpers;
using System.Collections.Generic;
using System;

namespace GustafsGalleryStore.Helpers
{
    public static class ProductHelper
    {
        #region SQL Methods
        public static bool CheckSQL(string sql)
        {
            if (sql.ToLower().IndexOf("delete", System.StringComparison.CurrentCulture) > -1 ||
                sql.ToLower().IndexOf("update", System.StringComparison.CurrentCulture) > -1 ||
                sql.ToLower().IndexOf("insert", System.StringComparison.CurrentCulture) > -1 ||
                sql.ToLower().IndexOf("create", System.StringComparison.CurrentCulture) > -1 ||
                sql.ToLower().IndexOf("drop", System.StringComparison.CurrentCulture) > -1
               )
            {
                return false;
            }

            return true;
        }

        public static string CreateFilterSQL(string column, List<string> list, GustafsGalleryStoreContext context)
        {
            var where = "";

            foreach (var item in list)
            {
                if (where.Length == 0)
                {
                    where = string.Format(" {0} in ( ", column);
                }
                else
                {
                    where += ",";
                }

                switch (column)
                {
                    case "productbrandid":
                        where += context.ProductBrands.Where(x => x.Brand == item).SingleOrDefault().Id;
                        break;
                    case "departmentid":
                        where += context.Departments.Where(x => x.DepartmentName == item).SingleOrDefault().Id;
                        break;
                    default:
                        return "";
                }

            }

            return where += ")";

        }

        public static string CreateSearchSQL(string searchTerm)
        {
            var searchString = "(";

            //add title search terms
            searchString += string.Format("title like '%{0}%' or ", searchTerm.Replace('+',' '));
            foreach (var word in searchTerm.Split(' '))
            {
                searchString += string.Format("title like '%{0}%' or ", word.Replace('+', ' '));
            }

            //add brand search terms
            searchString += string.Format(" productbrandid in (select id from productbrands where brand like '%{0}%') or ", searchTerm.Replace('+', ' '));
            foreach (var word in searchTerm.Split(' '))
            {
                searchString += string.Format(" productbrandid in (select id from productbrands where brand like '%{0}%') or ", word.Replace('+', ' '));
            }

            //add brand search terms
            foreach (var word in searchTerm.Split(' '))
            {
                searchString += string.Format(" departmentid in (select id from departments where departmentname like '%{0}%') or ", word.Replace('+', ' '));
            }
            searchString += string.Format(" departmentid in (select id from departments where departmentname like '%{0}%')", searchTerm.Replace('+', ' '));

            searchString += ")";

            return searchString;
        }
        #endregion

        #region GetProducts
        public static List<Product> GetProducts(GustafsGalleryStoreContext context, string where = null, string orderBy = "createdate", string orderByModifier = "desc")
        {
            if (string.IsNullOrWhiteSpace(where)) 
            { 
                if (string.IsNullOrWhiteSpace(orderBy))
                {
                    return GetProductsBasic(context);
                }
                else
                {
                    return GetProductsOrderBy(context, orderBy, orderByModifier);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(orderBy))
                {
                    return GetProductsWhere(context, where);
                }
                else
                {
                    return GetProductsWhereOrderBy(context, where, orderBy, orderByModifier);
                }

            }
        }

        private static List<Product> GetProductsBasic (GustafsGalleryStoreContext context)
        {
            return context.Products.
                Include(p => p.Department).
                Include(p => p.ProductBrand).
                Include(p => p.ProductSizes).
                Include(p => p.ProductImages).
                Include(p => p.ProductColours).
                Where(x => x.Stock > 0).ToList();
        }

        private static List<Product> GetProductsOrderBy (GustafsGalleryStoreContext context, string orderBy = "createdate", string orderByModifier = "desc")
        {
            if (orderByModifier.ToLower() == "asc" || 
                orderByModifier.ToLower() == "ascending" ||
                orderByModifier == null)
            {
                switch (orderBy)
                {
                    case "title":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.Title).
                                Where(x => x.Stock > 0).ToList();
                    case "price":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.Price).
                                Where(x => x.Stock > 0).ToList();
                    case "department":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.Department.DepartmentName).
                                Where(x => x.Stock > 0).ToList();
                    case "brand":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.ProductBrand.Brand).
                                Where(x => x.Stock > 0).ToList();
                    case "createdate":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.CreateDate).
                                Where(x => x.Stock > 0).ToList();
                    default:
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.CreateDate).
                                Where(x => x.Stock > 0).ToList();
                }
            }
            else
            {
                switch (orderBy)
                {
                    case "title":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.Title).
                                Where(x => x.Stock > 0).ToList();
                    case "price":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.Price).
                                Where(x => x.Stock > 0).ToList();
                    case "department":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.Department.DepartmentName).
                                Where(x => x.Stock > 0).ToList();
                    case "brand":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.ProductBrand.Brand).
                                Where(x => x.Stock > 0).ToList();
                    case "createdate":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.CreateDate).
                                Where(x => x.Stock > 0).ToList();
                    default:
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.CreateDate).
                                Where(x => x.Stock > 0).ToList();
                }
            }
        }

        private static List<Product> GetProductsWhere(GustafsGalleryStoreContext context, string where = null)
        {
            return context.Products.
                FromSql(where).
                Include(p => p.Department).
                Include(p => p.ProductBrand).
                Include(p => p.ProductSizes).
                Include(p => p.ProductImages).
                Include(p => p.ProductColours).
                Where(x => x.Stock > 0).ToList();
        }

        private static List<Product> GetProductsWhereOrderBy(GustafsGalleryStoreContext context, string where = null, string orderBy = "createdate", string orderByModifier = "desc")
        {
            if (orderByModifier.ToLower() == "asc" ||
                orderByModifier.ToLower() == "ascending" ||
                orderByModifier == null)
            {
                switch (orderBy)
                {
                    case "title":
                        return context.Products.
                                FromSql(where).
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.Title).
                                Where(x => x.Stock > 0).ToList();
                    case "price":
                        return context.Products.
                                FromSql(where).
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.Price).
                                Where(x => x.Stock > 0).ToList();
                    case "department":
                        return context.Products.
                                FromSql(where).
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.Department.DepartmentName).
                                Where(x => x.Stock > 0).ToList();
                    case "brand":
                        return context.Products.
                                FromSql(where).
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.ProductBrand.Brand).
                                Where(x => x.Stock > 0).ToList();
                    case "createdate":
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.CreateDate).
                                Where(x => x.Stock > 0).ToList();
                    default:
                        return context.Products.
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderBy(p => p.CreateDate).
                                Where(x => x.Stock > 0).ToList();
                }
            }
            else
            {
                switch (orderBy)
                {
                    case "title":
                        return context.Products.
                                FromSql(where).
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.Title).
                                Where(x => x.Stock > 0).ToList();
                    case "price":
                        return context.Products.
                                FromSql(where).
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.Price).
                                Where(x => x.Stock > 0).ToList();
                    case "department":
                        return context.Products.
                                FromSql(where).
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.Department.DepartmentName).
                                Where(x => x.Stock > 0).ToList();
                    case "brand":
                        return context.Products.
                                FromSql(where).
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.ProductBrand.Brand).
                                Where(x => x.Stock > 0).ToList();
                    case "createdate":
                        return context.Products.
                                FromSql(where).
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.CreateDate).
                                Where(x => x.Stock > 0).ToList();
                    default:
                        return context.Products.
                                FromSql(where).
                                Include(p => p.Department).
                                Include(p => p.ProductBrand).
                                Include(p => p.ProductSizes).
                                Include(p => p.ProductImages).
                                Include(p => p.ProductColours).
                                OrderByDescending(p => p.CreateDate).
                                Where(x => x.Stock > 0).ToList();
                }
            }
        }
        #endregion
    }

}
