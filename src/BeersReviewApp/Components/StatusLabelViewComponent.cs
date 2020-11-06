using Microsoft.AspNetCore.Mvc;

namespace BeersReviewApp.Components
{
    public class StatusLabelViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(bool? isApproved)
        {
            return View(isApproved);
        }
    }
}
