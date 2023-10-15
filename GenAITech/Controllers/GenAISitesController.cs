using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GenAITech.Data;
using GenAITech.Models;

namespace GenAITech.Controllers
{
    public class GenAISitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public GenAISitesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        private string GetUniqueFileName(string fileName)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fileName);
            return uniqueFileName;
        }
        private string GenerateUniqueAnchorLink(string genAIName)
        {
            string sanitizedName = genAIName.Replace(" ", "").ToLower();
            string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 6);
            string anchorLink = $"{sanitizedName}-{uniqueId}";

            return anchorLink;
        }

        // GET: GenAISites
        public async Task<IActionResult> Index()
        {
            return _context.GenAISites != null ?
                        View(await _context.GenAISites.ToListAsync()) :
                        Problem("Entity set 'ApplicationDbContext.GenAISites'  is null.");
        }


        // GET: GenAISites/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GenAISites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GenAIName,Summary")] GenAISite genAISite, IFormFile ImageFilename)
        {
            if (ImageFilename != null)
            {
                string uniqueFileName = GetUniqueFileName(ImageFilename.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", uniqueFileName);

                string? directoryPath = Path.GetDirectoryName(imagePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await ImageFilename.CopyToAsync(stream);
                }
                genAISite.ImageFilename = uniqueFileName;
            }


            genAISite.AnchorLink = GenerateUniqueAnchorLink(genAISite.GenAIName);
            genAISite.Like = 0;

            ModelState.Clear();
            TryValidateModel(genAISite);

            if (ModelState.IsValid)
            {
                _context.Add(genAISite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(genAISite);
        }

        // GET: GenAISites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.GenAISites == null)
            {
                return NotFound();
            }

            var genAISite = await _context.GenAISites.FindAsync(id);
            if (genAISite == null)
            {
                return NotFound();
            }
            return View(genAISite);
        }

        // POST: GenAISites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GenAIName,Summary,AnchorLink,Like")] GenAISite genAISite, IFormFile ImageFilename)
        {
            if (id != genAISite.Id)
            {
                return NotFound();
            }
            var newGenAISite = await _context.GenAISites.FindAsync(id);
            if (genAISite?.ImageFilename != null)
            {
                string imageFileName = genAISite.ImageFilename;
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", imageFileName);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            if (ImageFilename != null)
            {
                string uniqueFileName = GetUniqueFileName(ImageFilename.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", uniqueFileName);

                string? directoryPath = Path.GetDirectoryName(imagePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await ImageFilename.CopyToAsync(stream);
                }
                genAISite.ImageFilename = uniqueFileName;
            }
            ModelState.Clear();
            TryValidateModel(genAISite);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(genAISite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenAISiteExists(genAISite.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(genAISite);
        }

        // GET: GenAISites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.GenAISites == null)
            {
                return NotFound();
            }

            var genAISite = await _context.GenAISites
                .FirstOrDefaultAsync(m => m.Id == id);
            if (genAISite == null)
            {
                return NotFound();
            }

            return View(genAISite);
        }

        // POST: GenAISites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.GenAISites == null)
            {
                return Problem("Entity set 'ApplicationDbContext.GenAISites'  is null.");
            }
            var genAISite = await _context.GenAISites.FindAsync(id);
            if (genAISite != null)
            {
                string imageFileName = genAISite.ImageFilename;
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", imageFileName);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _context.GenAISites.Remove(genAISite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool GenAISiteExists(int id)
        {
            return (_context.GenAISites?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
