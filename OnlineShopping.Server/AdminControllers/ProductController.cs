using OnlineShopping.DataAccess.Data;
using OnlineShopping.DataAccess.Repository.IRepository;
using OnlineShopping.Models;
using OnlineShopping.Models.ViewModels;
using OnlineShopping.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineShopping.Models;

//using Microsoft.DotNet.Scaffolding.Shared.Messaging;
//using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace OnlineShopping.Server.AdminControllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objCatagoryList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            {
                new ProductImage { Id = 1, ProductId = 1, ImageUrl = "C:\\Users\\DELL\\Source\\Repos\\ASP.NET-Core-MVC-NET-8\\Bulky\\BulkyWeb\\wwwroot\\images\\sports.jpg" };
                new ProductImage { Id = 2, ProductId = 2, ImageUrl = "images/dark_skies.jpg" };
                new ProductImage { Id = 3, ProductId = 3, ImageUrl = "images/vanish_in_the_sunset.jpg" };
                new ProductImage { Id = 4, ProductId = 4, ImageUrl = "images/cotton_candy.jpg" };
                new ProductImage { Id = 5, ProductId = 5, ImageUrl = "images/rock_in_the_ocean.jpg" };
                new ProductImage { Id = 6, ProductId = 6, ImageUrl = "images/leaves_and_wonders.jpg" };
            }

            return View(objCatagoryList);
        }
        //Update and Insert  
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {

                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()

            };
            if (id == null || id == 0)
            {
                //Create
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "ProductImages");
                return View(productVM);
            }

        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {



            //if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
            //{
            //    //Delete Old Image 
            //    var oldImagePath = 
            //        Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

            //    if (System.IO.File.Exists(oldImagePath))
            //    {
            //        System.IO.File.Delete(oldImagePath);
            //    }

            //}
            //using (var fileStream = new FileStream(Path.Combine(productPath,fileName), FileMode.Create))
            //{
            //    file.CopyTo(fileStream);
            //}
            //productVM.Product.ImageUrl = @"\images\product\" + fileName;

            if (ModelState.IsValid)
            {
                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);

                    TempData["success"] = "Product Created Successfully";
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);

                    TempData["success"] = "Product Updated Successfully";
                }
                _unitOfWork.Save();
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (files != null)
                {

                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = productVM.Product.Id,
                        };

                        if (productVM.Product.ProductImages == null)
                            productVM.Product.ProductImages = new List<ProductImage>();

                        productVM.Product.ProductImages.Add(productImage);

                    }

                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();

                }

                TempData["success"] = "Product created/updated successfully";

                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
        }

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath =
                                   Path.Combine(_webHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }


    }
}
