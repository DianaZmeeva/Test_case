using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Test_case.Models;
using Test_case.Models.ViewModels;

namespace Test_case.Controllers
{
    [Authorize]
    public class FilmsController : Controller
    {
        private static readonly HashSet<String> AllowedFormats = new HashSet<String> { "jpg", "jpeg", "png", "gif" };

        private ApplicationDbContext db = new ApplicationDbContext();

        [AllowAnonymous]
        // GET: Films
        public ActionResult Index(int? page)
        {
            var pageNumber = page ?? 1;
            //var films = db.Films.Include(f => f.Creator);
            var onePageOfFilms = db.Films.Include(f => f.Creator).OrderByDescending(f => f.Year).ToPagedList(pageNumber, 3);
            ViewBag.Page = pageNumber;
            return View(onePageOfFilms);
        }

        [AllowAnonymous]
        // GET: Films/Details/5
        public ActionResult Details(Guid? id, int? page)
        {
            var pageNumber = page ?? 1;
            ViewBag.Page = pageNumber;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Film film = db.Films.Find(id);
            film.Creator = db.Users.Find(film.CreatorId);
            if (film == null)
            {
                return HttpNotFound();
            }
            return View(film);
        }

        // GET: Films/Create
        public ActionResult Create(int? page)
        {
            var pageNumber = page ?? 1;
            ViewBag.Page = pageNumber;
            return this.View(new CreateFilmModel());
        }

        // POST: Films/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateFilmModel model, int? page)
        {
            var pageNumber = page ?? 1;
            ViewBag.Page = pageNumber;
            var user = User.Identity.GetUserId();
            if (ModelState.IsValid && user != null)
            {
                var film = new Film
                {
                    CreatorId = user,
                    Name = model.Name,
                    Description = model.Description,
                    Year = model.Year,
                    Producer = model.Producer
                };

                if (model.ImageFile != null && (model.ImageFile.ContentLength > 0))
                {
                    var fileName = loadImage(model.ImageFile);
                    if (fileName != null)
                    {
                        film.Path = $"/Resourses/{fileName}";
                    }
                }

                db.Films.Add(film);
                db.SaveChanges();
                return this.RedirectToAction("Index", new { page = pageNumber });
            }
            return this.View(model);
        }


        // GET: Films/Edit/5
        public ActionResult Edit(Guid? id, int? page)
        {
            var pageNumber = page ?? 1;
            ViewBag.Page = pageNumber;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Film film = db.Films.Find(id);
            if (film == null)
            {
                return HttpNotFound();
            }
            if (!CanEditorDelete(film))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var model = new EditFilmModel
            {
                Name = film.Name,
                Description = film.Description,
                Year = film.Year,
                Producer = film.Producer,
                ImageName=film.Path
            };
            return this.View(model);
        }

        // POST: Films/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Guid? id, EditFilmModel model, int? page)
        {
            var pageNumber = page ?? 1;
            ViewBag.Page = pageNumber;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Film film = db.Films.Find(id);
            if (film == null)
            {
                return HttpNotFound();
            }

            if (!CanEditorDelete(film))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            if (film.Path != null)
            {
                model.ImageName = film.Path;
            }

            if (ModelState.IsValid)
            {
                if (model.DeletePoster && model.ImageName != null)
                {
                    deleteFile(model.ImageName);
                    model.ImageName = null;
                    film.Path = null;

                }

                if (model.ImageFile != null && (model.ImageFile.ContentLength > 0))
                {
                    if (model.ImageName != null)
                    {
                        deleteFile(model.ImageName);
                        model.ImageName = null;
                        film.Path = null;
                    }

                    var fileName = loadImage(model.ImageFile);
                    if (fileName != null)
                    {
                        film.Path = $"/Resourses/{fileName}";
                    }
                }

                film.Name = model.Name;
                film.Description = model.Description;
                film.Year = model.Year;
                film.Producer = model.Producer;

                db.Entry(film).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { page = pageNumber });
            }
            return this.View(model);
        }


        // GET: Films/Delete/5
        public ActionResult Delete(Guid? id, int? page)
        {
            var pageNumber = page ?? 1;
            ViewBag.Page = pageNumber;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Film film = db.Films.Find(id);
            film.Creator = db.Users.Find(film.CreatorId);
            if (film == null)
            {
                return HttpNotFound();
            }

            if (!CanEditorDelete(film))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id, int? page)
        {
            var pageNumber = page ?? 1;
            ViewBag.Page = pageNumber;
            Film film = db.Films.Find(id);
            film.Creator = db.Users.Find(film.CreatorId);
            if (film.Path != null)
            {
                deleteFile(film.Path);
            }
            db.Films.Remove(film);
            db.SaveChanges();
            return RedirectToAction("Index", new { page = pageNumber });
        }

        private bool CanEditorDelete(Film film)
        {
            return User.Identity.GetUserId() == film.CreatorId;
        }

        private object loadImage(HttpPostedFileBase imageFile)
        {
            string directory = HostingEnvironment.MapPath(@"~/") + "Resourses";
            string fileExt = Path.GetExtension(imageFile.FileName).Substring(1);
            if (!FilmsController.AllowedFormats.Contains(fileExt))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var fileName = Guid.NewGuid() + "." + fileExt;
            var uploadPath = Path.Combine(directory, fileName);
            imageFile.SaveAs(uploadPath);
            return fileName;
        }

        private void deleteFile(string imageName)
        {
            string image = HostingEnvironment.MapPath(@"~/") + imageName;

            if (System.IO.File.Exists(image))
            {
                System.IO.File.Delete(image);
            }
        }
    }
}
