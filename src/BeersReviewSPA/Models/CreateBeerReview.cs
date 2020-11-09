using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BeersReviewSPA.Models
{
    public class CreateBeerReview : BeerReview
    {
        [Display(Name = "Image File")]
        public IFormFile Image { get; set; }
    }
}