using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;

namespace PromoCodeFactory.DataAccess.Repositories;

public class InMemoryRepository<T>: IRepository<T> where T: BaseEntity
{
    protected List<T> Data { get; set; }


    public InMemoryRepository(IEnumerable<T> data)
    {
        Data = data.ToList();
    }


    public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(Data.AsEnumerable());
    }

    public Task<T> GetByIdAsync(Guid id)
    {
        return Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
    }

    public Task<bool> DeleteByIdAsync(Guid id)
    {
        var item = Data.FirstOrDefault(x => x.Id == id);

        if (item == default)
        {
            return Task.FromResult(false);
        }
        
        Data.Remove(item);

        return Task.FromResult(true);
    }

    public Task<Guid> AddAsync(T newItem)
    {
        var item = Data.FirstOrDefault(x => x.Id == newItem.Id);

        if (item != default)
            return Task.FromResult(item.Id);

        Data.Add(newItem);

        return Task.FromResult(newItem.Id);
    }

    public async Task UpdateAsync(T item)
    {
        await this.DeleteByIdAsync(item.Id);

        await this.AddAsync(item);
    }
}