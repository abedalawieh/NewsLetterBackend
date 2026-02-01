using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewsletterApp.Domain.Entities;

namespace NewsletterApp.Domain.Interfaces
{
    public interface INewsletterRepository : IAsyncRepository<Newsletter>
    {
    }
}
