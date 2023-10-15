using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GenAITech.Data;
using GenAITech.Models;
using Microsoft.AspNetCore.Authorization;

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
            if (_context.GenAISites != null)
            {
                // Sort the data by SomeProperty and then convert it to a list
                var sortedData = await _context.GenAISites.OrderByDescending(item => item.Like).ToListAsync();
                return View(sortedData);
            }
            else
            {
                return Problem("Entity set 'ApplicationDbContext.GenAISites' is null.");
            }
        }


        // GET: GenAISites/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: GenAISites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [Authorize]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GenAIName,Summary,AnchorLink,Like")] GenAISite genAISite, IFormFile ImageFilename)
        {
            if (id != genAISite.Id)
            {
                return NotFound();
            }

            var existingGenAISite = await _context.GenAISites.FindAsync(id);

            if (existingGenAISite != null)
            {
                if (ImageFilename != null)
                {
                    // Delete the old image if a new image is being uploaded
                    if (!string.IsNullOrEmpty(existingGenAISite.ImageFilename))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", existingGenAISite.ImageFilename);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    string uniqueFileName = GetUniqueFileName(ImageFilename.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", uniqueFileName);

                    string directoryPath = Path.GetDirectoryName(imagePath);
                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await ImageFilename.CopyToAsync(stream);
                    }
                    existingGenAISite.ImageFilename = uniqueFileName;
                }

                // Update other properties
                existingGenAISite.GenAIName = genAISite.GenAIName;
                existingGenAISite.Summary = genAISite.Summary;
                existingGenAISite.AnchorLink = genAISite.AnchorLink;
                existingGenAISite.Like = genAISite.Like;

                ModelState.Clear();
                TryValidateModel(existingGenAISite);

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(existingGenAISite);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!GenAISiteExists(existingGenAISite.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return View(genAISite);
        }
        [Authorize]
        public async Task<IActionResult> Like(int id, string AnchorLink)
        {
            if (User.Identity.IsAuthenticated)
            {
                var likeKey = $"Like_{id}";

                if (!Request.Cookies.ContainsKey(likeKey))
                {
                    var genAISite = await _context.GenAISites.FindAsync(id);
                    if (genAISite != null)
                    {
                        genAISite.Like++;
                        await _context.SaveChangesAsync();
                        Response.Cookies.Append(likeKey, "true");
                        string anchorLink = genAISite.AnchorLink;
                        return Redirect($"/GenAISites/#{anchorLink}");
                    }
                }
            }

            // If the user is not authenticated or has already liked the item, redirect back to the view
            return Redirect($"/GenAISites/#{AnchorLink}");
        }

        // GET: GenAISites/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
