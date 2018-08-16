using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Wutnu.Data;

namespace Wutnu.Common.ErrorMgr
{
	/// <summary>
	/// Generic repository
	/// </summary>
	/// <typeparam name="TObject">
	/// </typeparam>
	public class GenericDocNetRepo<TObject> where TObject : class
	{
		/// <summary>
		/// The _context.
		/// </summary>
        public WutNuContext Context;

	    /// <summary>
	    /// Initializes a new instance of the <see cref="GenericDocNetRepo{TObject}"/> class.
	    /// </summary>
	    /// <param name="entities"></param>
	    /// <param name="lazyLoadingEnabled">
	    /// Lazy Load
	    /// </param>
	    public GenericDocNetRepo(WutNuContext entities, bool lazyLoadingEnabled = false)
		{
		    Context = entities;

			Context.Configuration.LazyLoadingEnabled = lazyLoadingEnabled;
			Context.Configuration.ProxyCreationEnabled = false;
		}

        public void GenericAdd<TEntity>(TEntity newEntityItem) where TEntity : class
        {
            try
            {
                Context.Set<TEntity>().Add(newEntityItem);
                Context.SaveChanges();
            }
            catch (Exception)
            {
                // log database object that does not have insert function like DATABASE VIEWS.
               
            }
        }

		/// <summary>
		/// Gets the db set.
		/// </summary>
		protected DbSet<TObject> DbSet
		{
			get
			{
				return Context.Set<TObject>();
			}
		}

		/// <summary>
		/// The all.
		/// </summary>
		/// <returns>
		/// The <see cref="IQueryable"/>.
		/// </returns>
		public virtual IQueryable<TObject> All()
		{
			return DbSet.AsQueryable();
		}

		public virtual TObject Find(params object[] keys)
		{
			return DbSet.Find(keys);
		}

		/// <summary>
		/// The delete.
		/// </summary>
		/// <param name="TObject">
		/// The t object.
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		public virtual int Delete(TObject TObject)
		{
			DbSet.Remove(TObject);
			return Context.SaveChanges();
		}

		/// <summary>
        /// This deletes based on the predicate search passed in.
		/// </summary>
		/// <param name="predicate">
		/// The predicate.
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		public int Delete(Expression<Func<TObject, bool>> predicate)
		{

            var objectToRemoveList = DbSet.Where(predicate).ToList();

            foreach (var objectToRemove in objectToRemoveList)
		    {
                DbSet.Remove(objectToRemove);
		    }

            return Context.SaveChanges();
        }
	}
}

