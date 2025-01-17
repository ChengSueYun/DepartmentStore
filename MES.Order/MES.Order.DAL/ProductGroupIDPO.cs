﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using MES.Order.DAL.EntityFramework;
using MES.Order.DAL.ViewModel;

namespace MES.Order.DAL
{
    public class ProductGroupIDPO
    {
        internal ProductsDbContext ProductsDbContext
        {
            get
            {
                if (this._productsDbContext == null)
                {
                    this._productsDbContext = new ProductsDbContext();
                }

                return this._productsDbContext;
            }
            set => this._productsDbContext = value;
        }

        private ProductsDbContext _productsDbContext;

        public List<KeyAndNameForCombo> QueryAllProductsGroupID()
        {
            var filter    = this.ProductsDbContext.ProductGroupIDs.ToListAsync().Result;
            var result = (from a in filter
                          select new KeyAndNameForCombo
                          {
                              Code             = a.ProductGroupID,
                              LocalDescription = a.ProductGroupName
                          }).Distinct().ToList();

            return result;
        }
    }
}