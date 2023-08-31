using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
 
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository 
        {
        private ApplicationDBContext _db;
        public ProductImageRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        

        public void Update(ProductImage obj)
        {
            _db.Update(obj);
        }
    }
}
