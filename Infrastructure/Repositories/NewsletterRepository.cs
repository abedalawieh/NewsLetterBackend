using Microsoft.EntityFrameworkCore;
using NewsletterApp.Domain.Entities;
using NewsletterApp.Domain.Interfaces;
using NewsletterApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsletterApp.Infrastructure.Repositories
{
    public class NewsletterRepository : BaseRepository<Newsletter>, INewsletterRepository
    {
        public NewsletterRepository(NewsletterDbContext context) : base(context)
        {
        }
    }
}
