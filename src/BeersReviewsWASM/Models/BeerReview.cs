using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace BeersReviewWASM.Models
{
    public class BeerReview : TableEntity
    {
        public BeerReview()
        {
            PartitionKey = "Reviews";
        }

        public string MediaUrl { get; set; }

        [Display(Name = "Review")]
        public string ReviewText { get; set; }

        public bool? IsApproved { get; set; }

        [Display(Name = "Caption")]
        public string Caption { get; set; }

        public DateTime Created { get; set; }
    }
}