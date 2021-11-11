using JankiCards.Janki;
using Microsoft.AspNetCore.Identity;
using System;

namespace JankiWebCards.Janki
{
    public class JankiUser : IdentityUser
    {
        public Guid BundleId { get; set; }

        public Bundle Bundle { get; set; }
    }
}