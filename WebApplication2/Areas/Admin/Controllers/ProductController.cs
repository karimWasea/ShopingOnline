using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BulkyBook.Areas.Admin.Controllers
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

        // GET: Categries
        public async Task<IActionResult> Index()
        {
            List<Product> ObjProduct = _unitOfWork.product.GetAll(includeProperties: "categry").ToList();
            
                
                return View(ObjProduct);
         }

        // GET: Categries/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            var Product = _unitOfWork.product.Get(m => m.Id == id);

            if (Product == null)
            {
                return NotFound();
            }

            return View(Product);
        }

        // GET: Categries/Create
        public IActionResult UpSert(int? id)
        {
            ProductVM productVM = new()
            {
                CategryList = _unitOfWork.categry.GetAll().Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                }),
                product = new Product()
                 
            };
            if(id==null || id == 0)
            {
                //create
                return View(productVM);

            }
            else
            {
                //update    
                productVM.product=_unitOfWork.product.Get(a=>a.Id==id, includeProperties: "ProductImages"); 
                return View(productVM);
               
            }


        }

        // POST: Categries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public  IActionResult UpSert( ProductVM productVM, List<IFormFile> files )
        {
			if (ModelState.IsValid)
			{
				if (productVM.product.Id == 0)
				{
					_unitOfWork.product.add(productVM.product);
				}
				else
				{
					_unitOfWork.product.update(productVM.product);
				}

				_unitOfWork.save();


				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (files != null)
				{

					foreach (IFormFile file in files)
					{
						string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
						string productPath = @"Images\Products\product-" + productVM.product.Id;
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
							ProductId = productVM.product.Id,
						};

						if (productVM.product.ProductImages == null)
							productVM.product.ProductImages = new List<ProductImage>();

						productVM.product.ProductImages.Add(productImage);
                        _unitOfWork.ProductImage.add(productImage);
                    }

					_unitOfWork.product.update(productVM.product);
					_unitOfWork.save();




				}


				TempData["success"] = "Product created/updated successfully";
				return RedirectToAction("Index");
			}
			else
			{
				productVM.CategryList = _unitOfWork.categry.GetAll().Select(u => new SelectListItem
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

                _unitOfWork.ProductImage.remove(imageToBeDeleted);
                _unitOfWork.save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(UpSert), new { id = productId });
        }
        // GET: Categries/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{


        //    var Product = _unitOfWork.product.Get(m => m.Id == id);
        //    if (Product == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(Product);
        //}

        //// POST: Categries/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id,Product Product)
        //{
        //    if (id != Product.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _unitOfWork.product.update(Product);
        //            _unitOfWork.save();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ProductExists(Product.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        TempData["success"] = "product Modified Successfully";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(Product);
        //}

        // GET: Categries/Delete/5
        [HttpDelete]
		public IActionResult Delete(int? id)
		{
			var productToBeDeleted = _unitOfWork.product.Get(u => u.Id == id);
			if (productToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			string productPath = @"Images\Products\product-" + id;
			string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

			if (Directory.Exists(finalPath))
			{
				string[] filePaths = Directory.GetFiles(finalPath);
				foreach (string filePath in filePaths)
				{
					System.IO.File.Delete(filePath);
				}

				Directory.Delete(finalPath);
			}


			_unitOfWork.product.remove(productToBeDeleted);
			_unitOfWork.save();

			return Json(new { success = true, message = "Delete Successful" });
		}

		// POST: Categries/Delete/5
		//[HttpPost, ActionName("Delete")]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> DeleteConfirmed(int id)
		//{
		//    Product? Product = _unitOfWork.product.Get(m => m.Id == id);

		//    if (Product != null)
		//    {
		//        _unitOfWork.product.remove(Product);
		//        _unitOfWork.save();
		//        TempData["success"] = "product Deleted Successfully";
		//    }

		//    return RedirectToAction(nameof(Index));
		//}

		private bool ProductExists(int id)
        {
            return (_unitOfWork.product.GetAll()?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        #region API Call
        [HttpGet]
        public IActionResult GetAll() {
            List<Product> ObjProduct = _unitOfWork.product.GetAll(includeProperties: "categry").ToList();
            return Json(new { data = ObjProduct });
        }
        #endregion
    }
}
 