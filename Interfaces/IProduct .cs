﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodesByAniz.Tools;
using Inventory.Models;

namespace InventoryBeginners.Interfaces
{
    public interface IProduct
    {
        PaginatedList<Product> GetItems(string SortProperty, SortOrder sortOrder, string SearchText = "", int pageIndex = 1, int pageSize = 5); //read all
        Product GetItem(string Code); // read particular item

        Product Create(Product product);
            
        Product Edit(Product product);

        Product Delete(Product product);

        public bool IsItemExists(string name);
        public bool IsItemExists(string name, string Code);


    }
}

