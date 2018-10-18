using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;
using GustafsGalleryStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
namespace GustafsGalleryStore.Models.ViewModels
{
    public class DeliveryViewModel
    {

        public string StatusMessage { get; set; }

        public List<DeliveryType> DeliveryTypes { get; set; }
        public List<DeliveryCompany> DeliveryCompanies { get; set; }

        // new delivery company
        public string Company { get; set; }
        // new delivery type
        public string Type { get; set; }
        public decimal Price { get; set; }
        public string Time { get; set; }


    }
}
