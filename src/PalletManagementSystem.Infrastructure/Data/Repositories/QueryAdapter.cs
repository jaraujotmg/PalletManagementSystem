using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.Interfaces.Repositories;

namespace PalletManagementSystem.Infrastructure.Data.Repositories
{
    public class QueryAdapter<T> : IQuery<T> where T : class
    {
        private readonly IQueryable<T> _query;

        public QueryAdapter(IQueryable<T> query)
        {
            _query = query;
        }

        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            return new QueryAdapter<T>(_query.Where(predicate));
        }

        public IQuery<T> Include(Expression<Func<T, object>> path)
        {
            return new QueryAdapter<T>(_query.Include(path));
        }

        public IQuery<T> Include(string path)
        {
            return new QueryAdapter<T>(_query.Include(path));
        }

        public IReadOnlyList<T> ToList()
        {
            return _query.ToList();
        }

        public T FirstOrDefault()
        {
            return _query.FirstOrDefault();
        }
    }
}