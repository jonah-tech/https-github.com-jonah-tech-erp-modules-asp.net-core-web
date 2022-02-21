﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryBeginners.Data;
using InventoryBeginners.Interfaces;
using InventoryBeginners.Models;
using Microsoft.EntityFrameworkCore;
using CodesByAniz.Tools;
using Inventory.Models;

namespace InventoryBeginners.Repositories
{
    public class ProductRepo : IProduct
    {
        private readonly InventoryContext _context; // for connecting to efcore.
        public ProductRepo(InventoryContext context) // will be passed by dependency injection.
        {
            _context = context;
        }
        public Product Create(Product Product)
        {
            _context.Products.Add(Product);
            _context.SaveChanges();
            return Product;
        }

        public Product Delete(Product Product)
        {
            Product = pGetItem(Product.Code);
            _context.Products.Attach(Product);
            _context.Entry(Product).State = EntityState.Deleted;
            _context.SaveChanges();
            return Product;
        }

        public Product Edit(Product Product)
        {
            _context.Products.Attach(Product);
            _context.Entry(Product).State = EntityState.Modified;
            _context.SaveChanges();
            return Product;
        }


        private List<Product> DoSort(List<Product> items, string SortProperty, SortOrder sortOrder)
        {

            if (SortProperty.ToLower() == "name")
            {
                if (sortOrder == SortOrder.Ascending)
                    items = items.OrderBy(n => n.Name).ToList();
                else
                    items = items.OrderByDescending(n => n.Name).ToList();
            }
            else
            {
                if (sortOrder == SortOrder.Ascending)
                    items = items.OrderBy(d => d.Description).ToList();
                else
                    items = items.OrderByDescending(d => d.Description).ToList();
            }

            return items;
        }

        public PaginatedList<Product> GetItems(string SortProperty, SortOrder sortOrder, string SearchText = "", int pageIndex = 1, int pageSize = 5)
        {
            List<Product> items;

            if (SearchText != "" && SearchText != null)
            {
                items = _context.Products.Where(n => n.Name.Contains(SearchText) || n.Description.Contains(SearchText))
                    .Include(u => u.Units)
                    .ToList();
            }
            else
                items = _context.Products.Include(u=>u.Units).ToList();

            items = DoSort(items, SortProperty, sortOrder);

            PaginatedList<Product> retItems = new PaginatedList<Product>(items, pageIndex, pageSize);

            return retItems;
        }

        public Product GetItem(string Code)
        {
            Product item = _context.Products.Where(u => u.Code == Code)
                .Include(u => u.Units)
                .FirstOrDefault();


            item.BriefPhotoName = GetBriefPhotoName(item.PhotoUrl);
            return item;
        }


        private string GetBriefPhotoName(string fileName)  
        {
            if (fileName == null)
            {
                return string.Empty;
            }


            string[] words = fileName.Split('_');
            return words[words.Length - 1];
        }

        // This method fecth product details without including it's unit details
        public Product pGetItem(string Code)
        {
            Product item = _context.Products.Where(u => u.Code == Code)
               .FirstOrDefault();
            return item;
        }
        public bool IsItemExists(string name)
        {
            int ct = _context.Products.Where(n => n.Name.ToLower() == name.ToLower()).Count();
            if (ct > 0)
                return true;
            else
                return false;
        }

        public bool IsItemExists(string name, string Code)
        {
            int ct = _context.Products.Where(n => n.Name.ToLower() == name.ToLower() && n.Code != Code).Count();
            if (ct > 0)
                return true;
            else
                return false;
        }

    }
}
