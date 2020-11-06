using Microsoft.Azure.Cosmos.Table;
using System;

namespace ContentModeratorFunction.Models
{
    public class BeerReview : TableEntity
    {
        public BeerReview()
        {
            PartitionKey = "Reviews";
        }

        public string MediaUrl { get; set; }

        public string ReviewText { get; set; }

        public bool? IsApproved { get; set; }

        public string Caption { get; set; }

        public DateTime Created { get; set; }
    }
}
