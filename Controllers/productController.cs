using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryBeginners.Data;
using InventoryBeginners.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using InventoryBeginners.Interfaces;
using CodesByAniz.Tools;
using Microsoft.AspNetCore.Authorization;
using Inventory.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace InventoryBeginners.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {

        private readonly IWebHostEnvironment _webHost;
        private readonly IBrand _brandRepo;
        private readonly ICategory _categoryRepo;
        private readonly IProductGroup _productgroupRepo;
        private readonly IProductProfile _productprofileRepo;
        private readonly IUnit _unitRepo;
        private readonly IProduct _productRepo;
        public ProductController(IProduct productrepo, IUnit unitrepo, IBrand brandrepo, ICategory categoryrepo, IProductGroup productgrouprepo, IProductProfile productprofilerepo, IWebHostEnvironment webHost) // here the repository will be passed by the dependency injection.
        {

            //set them as readonly
            _webHost = webHost;
            _productRepo = productrepo;
            _unitRepo = unitrepo;
            _brandRepo = brandrepo;
            _categoryRepo = categoryrepo;
            _productgroupRepo = productgrouprepo;
            _productprofileRepo = productprofilerepo;
        }


        public IActionResult Index(string sortExpression = "", string SearchText = "", int pg = 1, int pageSize = 5)
        {
            SortModel sortModel = new SortModel();
            sortModel.AddColumn("Code");
            sortModel.AddColumn("Name");
            sortModel.AddColumn("Cost");
            sortModel.AddColumn("Price");
            sortModel.AddColumn("Description");
            sortModel.AddColumn("Unit");
            sortModel.ApplySort(sortExpression);
            ViewData["sortModel"] = sortModel;

            ViewBag.SearchText = SearchText;

            PaginatedList<Product> products = _productRepo.GetItems(sortModel.SortedProperty, sortModel.SortedOrder, SearchText, pg, pageSize);


            var pager = new PagerModel(products.TotalRecords, pg, pageSize);
            pager.SortExpression = sortExpression;
            this.ViewBag.Pager = pager;


            TempData["CurrentPage"] = pg;


            return View(products);
        }


        //public IActionResult Create()
        //{
        //    Product product = new Product();
        //    ViewBag.Units = GetUnits();
        //    ViewBag.Brands = GetBrands();
        //    ViewBag.ProductGroups = GetProductGroups();
        //    ViewBag.ProductProfiles = GetProductProfiles();
        //    ViewBag.Categories = GetCategories();
        //    return View(product);
        //}

       

        private void populateViewbags()
        {
            ViewBag.Units=GetUnits();
            ViewBag.Brands=GetBrands();
            ViewBag.Categories=GetCategories();
            ViewBag.ProductProfiles=GetProductProfiles();
            ViewBag.ProductGroups=GetProductGroups();
        }


        public IActionResult Create()
        {
            Product product = new Product();
            populateViewbags();

            return View(product);
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            bool bolret = false;
            string errMessage = "";
            try
            {
                if (product.Description.Length < 4 || product.Description == null)
                    errMessage = "Product Description Must be atleast 4 Characters";

                if (_productRepo.IsItemExists(product.Name) == true)
                    errMessage = errMessage + " " + " Product Name " + product.Name + " Exists Already";

                if (errMessage == "")
                { 

                    string uniqueFileName = GetUploadedFileName(product);
                    product.PhotoUrl = uniqueFileName;



                    product = _productRepo.Create(product);
                    bolret = true;
                }
            }
            catch (Exception ex)
            {
                errMessage = errMessage + " " + ex.Message;
            }
            if (bolret == false)
            {
                TempData["ErrorMessage"] = errMessage;
                ModelState.AddModelError("", errMessage);

                populateViewbags();
                return View(product);

                
            }
            else
            {
                TempData["SuccessMessage"] = "Product " + product.Name + " Created Successfully";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Details(string id) //Read
        {
            Product product = _productRepo.GetItem(id);
            ViewBag.Units = GetUnits();
            ViewBag.Brands = GetBrands();
            ViewBag.ProductGroups = GetProductGroups();
            ViewBag.ProductProfiles = GetProductProfiles();
            ViewBag.Categories = GetCategories();
            return View(product);
        }


        public IActionResult Edit(string id)
        {
            Product product = _productRepo.GetItem(id);
            ViewBag.Units = GetUnits();
            ViewBag.Brands = GetBrands();
            ViewBag.ProductGroups = GetProductGroups();
            ViewBag.ProductProfiles = GetProductProfiles();
            ViewBag.Categories = GetCategories();
            TempData.Keep();
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            bool bolret = false;
            string errMessage = "";

            try
            {
                if (product.Description.Length < 4 || product.Description == null)
                    errMessage = "Product Description Must be atleast 4 Characters";

                if (_productRepo.IsItemExists(product.Name, product.Code) == true)
                    errMessage = errMessage + "Product Name " + product.Name + " Already Exists";

                if(product.ProductPhoto != null)
                {
                    string uniqueFileName = GetUploadedFileName(product);
                    product.PhotoUrl = uniqueFileName;
                }

                if (errMessage == "")
                {
                    product = _productRepo.Edit(product);
                    TempData["SuccessMessage"] = product.Name + ", product Saved Successfully";
                    bolret = true;
                }
            }
            catch (Exception ex)
            {
                errMessage = errMessage + " " + ex.Message;
            }



            int currentPage = 1;
            if (TempData["CurrentPage"] != null)
                currentPage = (int)TempData["CurrentPage"];


            if (bolret == false)
            {
                TempData["ErrorMessage"] = errMessage;
                ModelState.AddModelError("", errMessage);
                return View(product);
            }
            else
                return RedirectToAction(nameof(Index), new { pg = currentPage });
        }

        public IActionResult Delete(string id)
        {
            Product product = _productRepo.GetItem(id);
            TempData.Keep();
            return View(product);
        }


        [HttpPost]
        public IActionResult Delete(Product product)
        {
            try
            {
                product = _productRepo.Delete(product);
            }
            catch (Exception ex)
            {
                string errMessage = ex.Message;
                TempData["ErrorMessage"] = errMessage;
                if (ex.InnerException != null)
                    errMessage = ex.InnerException.Message;
                ModelState.AddModelError("", errMessage);
                return View(product);
            }

            int currentPage = 1;
            if (TempData["CurrentPage"] != null)
                currentPage = (int)TempData["CurrentPage"];

            TempData["SuccessMessage"] = "Product " + product.Name + " Deleted Successfully";
            return RedirectToAction(nameof(Index), new { pg = currentPage });


        }

        private List<SelectListItem> GetUnits()
        {
            var Istunits = new List<SelectListItem>();

            PaginatedList<Unit> units = _unitRepo.GetItems("Name", SortOrder.Ascending, "", 1, 1000);
            Istunits = units.Select(ut => new SelectListItem()
            {
                Value = ut.Id.ToString(),
                Text = ut.Name

            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Unit -----"
            };
            Istunits.Insert(0, defItem);

            return Istunits;


        }
        private List<SelectListItem> GetCategories()
        {
            var Istcategories = new List<SelectListItem>();

            PaginatedList<Category> categories = _categoryRepo.GetItems("Name", SortOrder.Ascending, "", 1, 1000);
            Istcategories = categories.Select(ut => new SelectListItem()
            {
                Value = ut.Id.ToString(),
                Text = ut.Name

            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Category -----"
            };
            Istcategories.Insert(0, defItem);

            return Istcategories;


        }
        private List<SelectListItem> GetProductProfiles()
        {
            var Istproductprofiles = new List<SelectListItem>();

            PaginatedList<ProductProfile> productProfiles = _productprofileRepo.GetItems("Name", SortOrder.Ascending, "", 1, 1000);
            Istproductprofiles = productProfiles.Select(ut => new SelectListItem()
            {
                Value = ut.Id.ToString(),
                Text = ut.Name

            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Productprofile -----"
            };
            Istproductprofiles.Insert(0, defItem);

            return Istproductprofiles;


        }
        private List<SelectListItem> GetProductGroups()
        {
            var Istproductgroups = new List<SelectListItem>();

            PaginatedList<ProductGroup> productGroups = _productgroupRepo.GetItems("Name", SortOrder.Ascending, "", 1, 1000);
            Istproductgroups = productGroups.Select(ut => new SelectListItem()
            {
                Value = ut.Id.ToString(),
                Text = ut.Name

            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select ProductGroup -----"
            };
            Istproductgroups.Insert(0, defItem);

            return Istproductgroups;


        }
        private List<SelectListItem> GetBrands()
        {
            var Istbrands = new List<SelectListItem>();

            PaginatedList<Brand> brands = _brandRepo.GetItems("Name", SortOrder.Ascending, "", 1, 1000);
            Istbrands = brands.Select(ut => new SelectListItem()
            {
                Value = ut.Id.ToString(),
                Text = ut.Name

            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Brand -----"
            };
            Istbrands.Insert(0, defItem);

            return Istbrands;


        }
          private string GetUploadedFileName(Product product)
        {
            string uniqueFileName = null;
            if (product.ProductPhoto != null)
            {
                string uploadsFolder = Path.Combine(_webHost.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString()+"_"+ product.ProductPhoto.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    product.ProductPhoto.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        


    }
}
