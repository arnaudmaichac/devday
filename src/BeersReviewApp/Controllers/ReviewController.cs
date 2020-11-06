using BeersReviewApp.Models;
using BeersReviewApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BeersReviewApp.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ReviewProvider reviewProvider;

        public ReviewController(ReviewProvider reviewProvider)
        {
            this.reviewProvider = reviewProvider;
        }

        // GET: Review
        public IActionResult Index()
        {
            return this.View(this.reviewProvider.GetReviewsAsync().OrderByDescending(r => r.Created));
        }

        // GET: Review/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            return this.View(await this.reviewProvider.GetReviewAsync(id));
        }

        // GET: Review/Create
        public ActionResult Create()
        {
            return this.View();
        }

        // POST: Review/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateBeerReview newBeerReview)
        {
            try
            {
                var id = await this.reviewProvider.CreateReviewAsync(newBeerReview.Image.OpenReadStream(), newBeerReview.ReviewText);

                return this.RedirectToAction("Details", new { Id = id });
            }
            catch
            {
                return this.View();
            }
        }
    }
}
