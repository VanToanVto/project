using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebShop.Helpper;
using WebShop.Models;
namespace WebShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AdminCategoriesController : Controller
    {
        private readonly dbMarketsContext _context;
        public INotyfService _notyfService { get; }
        public AdminCategoriesController(dbMarketsContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var lsCategorys = _context.Categories.AsNoTracking().OrderBy(x => x.CatId);
            PagedList<Category> models = new PagedList<Category>(lsCategorys, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FirstOrDefaultAsync(m => m.CatId == id);
            if (category == null) return NotFound();
            return View(category);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CatId,CatName,Description,ParentId,Levels,Ordering,Published,Thumb,Title,Alias,MetaDesc,MetaKey,Cover,SchemaMarkup")] Category category, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (ModelState.IsValid)
            {
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string imageName = Utilities.SEOUrl(category.CatName) + extension;
                    category.Thumb = await Utilities.UploadFile(fThumb, @"category", imageName.ToLower());
                }
                if (string.IsNullOrEmpty(category.Thumb)) category.Thumb = "default.jpg";
                category.Alias = Utilities.SEOUrl(category.CatName);
                _context.Add(category);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm mới thành công");
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CatId,CatName,Description,ParentId,Levels,Ordering,Published,Thumb,Title,Alias,MetaDesc,MetaKey,Cover,SchemaMarkup")] Category category, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (id != category.CatId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string imageName = Utilities.SEOUrl(category.CatName) + extension;
                        category.Thumb = await Utilities.UploadFile(fThumb, @"category", imageName.ToLower());
                    }
                    if (string.IsNullOrEmpty(category.Thumb)) category.Thumb = "default.jpg";
					category.Alias = Utilities.SEOUrl(category.CatName);
					_context.Update(category);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Cập nhật thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CatId)) return NotFound();
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FirstOrDefaultAsync(m => m.CatId == id);
            if (category == null) return NotFound();
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            _notyfService.Success("Xóa thành công");
            return RedirectToAction(nameof(Index));
        }
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CatId == id);
        }
    }
}
