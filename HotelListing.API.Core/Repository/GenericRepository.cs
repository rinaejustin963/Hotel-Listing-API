using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Core.Models;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Core.Exceptions;
using System.Diagnostics.Metrics;


namespace HotelListing.API.Core.Repository
{
    // :Inherits from
    //This is the actual implementation of our contract
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;

        public GenericRepository(HotelListingDbContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }
        public async Task<T> AddAsync(T entity)
        {
            await _context.AddAsync(entity); 
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<TResult> AddAsync<TSource, TResult>(TSource source)
        {
            var entity = _mapper.Map<T>(source);

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            //Mapback to the DTO that we want
            return _mapper.Map<TResult>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetAsync (id);   
            if(entity is null)
            {
                throw new NotFoundException(typeof(T).Name, id);
            }
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }


        public async Task<bool> Exists(int id)
        {
            //get the entity and return the entity if its not null(true if exists)
            var entity = await GetAsync(id);
            return entity != null; 
        }

        public async Task<List<T>> GetAllAsync()
        {

            var results=_context.Set<T>().ToList();
            //The reason why we had to wait is because we have asynchronised method.  
            //Go to the Db and get the Dbset that is associated with T 
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters)
        {
            var totalSize = await _context.Set<T>().CountAsync();
            var items = await _context.Set<T>()//This is more like SELECT * in Sql
                .Skip(queryParameters.StartIndex)
                .Take(queryParameters.PageSize)
                .ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync(); //The use of mapping

            return new PagedResult<TResult> { //Returns the list of items(More like an SELECT query in sql)
                
                Items = items,
                PageNumber = queryParameters.PageNumber,
                RecordNumber = queryParameters.PageSize,
                TotalCount = totalSize 
                
            };
        }

        public async Task<List<TResult>> GetAllAsync<TResult>()
        {
            return await _context.Set<T>().
                ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync<TResult>();
        }

        public async Task<T> GetAsync(int? id)
        {
            if(id is null)
            {
                return null;
            }
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<TResult> GetAsync<TResult>(int? id)
        {
            var result = await _context.Set<T>().FindAsync(id);
            if(result is null)
            {
                //HasValue ? check if it has an Id or NOT, if not hten "No key Provided"
                throw new NotFoundException(typeof(T).Name, id.HasValue ? id : "No Key Provided");
            }
                           
            return _mapper.Map<TResult>(result);
        }

        public async Task UpdateAsync(T entity)
        {
            //update the context then save the changes
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

         public async Task UpdateAsync<TSource>(int id, TSource source)
        {
            //finding the entity
            var entity = await GetAsync(id);

            if(entity is null)
            {
                throw new NotFoundException(typeof(T).Name, id);
            }

            _mapper.Map(source, entity);
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
