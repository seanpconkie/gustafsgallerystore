using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GustafsGalleryStore.Areas.Identity.Data
{
	public class ApplicationUser : IdentityUser
    {

        [PersonalData]
        public string Forename { get; set; }
        [PersonalData]
        public string Surname { get; set; }
        [PersonalData]
        public string Title { get; set; }

    }
}
