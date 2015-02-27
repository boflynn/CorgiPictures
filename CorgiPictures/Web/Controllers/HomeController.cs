using System.Linq;
using System.Web.Mvc;

using CorgiPictures.Model;

namespace CorgiPictures.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly CorgiPicturesContext db = new CorgiPicturesContext();

        public ActionResult Index()
        {
            var pictures = db.Pictures.OrderByDescending(p => p.Created).AsQueryable();

            return View(pictures);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}