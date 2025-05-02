using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.Models
{
    public class Transaction
    {
        public required Guid Id { get; set; }

        public required DateTime Date { get; set; }

        public required string Description { get; set; }

        public required decimal Amount { get; set; }

        public Guid CategoryId { get; set; }

        public bool IsRecurring { get; set; } = false;
    }
}
