using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace E_Commerce.Repository.Specifications.Interfaces;
public class ISpecifications<T> where T : BaseEntity
{
    public ISpecifications()
    {
        WhereCriteria = null!;
        IncludesCriteria = [];
        OrderBy = null!;
        OrderByDesc = null!;
        Skip = 0;
        Take = 0;
        IsPaginationEnabled = false;
    }
    public List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> NestedIncludes { get; }
 = new List<Func<IQueryable<T>, IIncludableQueryable<T, object>>>();

    public Expression<Func<T, bool>> WhereCriteria { get; set; }
    public List<Expression<Func<T, object>>> IncludesCriteria { get; set; }
    public List<Func<IQueryable<T>, IQueryable<T>>> IncludeExpressions { get; set; } = new();
    public List<Expression<Func<T, object>>> ThenIncludes { get; set; } = new();

    public Expression<Func<T, object>> OrderBy { get; set; }
    public Expression<Func<T, object>> OrderByDesc { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public bool IsPaginationEnabled { get; set; }
}



