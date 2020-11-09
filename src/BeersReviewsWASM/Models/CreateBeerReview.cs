using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BeersReviewWASM.Models
{
    public class CreateBeerReview : BeerReview
    {
        [Display(Name = "Image File")]
        public IFormFile Image { get; set; }
    }
}