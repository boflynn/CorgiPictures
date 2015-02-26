using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using CorgiPictures.Model;

namespace CorgiPictures.Web.Controllers
{
    public class HomeController : Controller
    {
        private CorgiPicturesContext db = new CorgiPicturesContext();

        public async Task<ActionResult> Index()
        {
            var pictures = db.Pictures.OrderByDescending(p => p.Created).Take(10).AsQueryable();

            return View(await pictures.ToListAsync());
        }
    }
}