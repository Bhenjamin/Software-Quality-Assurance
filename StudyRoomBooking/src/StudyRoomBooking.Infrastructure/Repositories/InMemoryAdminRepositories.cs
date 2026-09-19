using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Interfaces;

namespace StudyRoomBooking.Infrastructure.Repositories;

public sealed class InMemoryAccessRuleRepository : IAccessRuleRepository
{
    private readonly List<AccessRule> _items = new();
    private int _nextId = 1;
    public Task<AccessRule?> GetByIdAsync(int id) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    public Task<List<AccessRule>> GetAllAsync() => Task.FromResult(_items.ToList());
    public Task AddAsync(AccessRule item) { item.Id = _nextId++; _items.Add(item); return Task.CompletedTask; }
    public Task UpdateAsync(AccessRule item) { var old = _items.FirstOrDefault(x => x.Id == item.Id); if (old is not null) _items[_items.IndexOf(old)] = item; return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _items.RemoveAll(x => x.Id == id); return Task.CompletedTask; }
}

public sealed class InMemoryBookingOverrideRepository : IBookingOverrideRepository
{
    private readonly List<BookingOverride> _items = new();
    private int _nextId = 1;
    public Task<BookingOverride?> GetByIdAsync(int id) => Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    public Task<List<BookingOverride>> GetAllAsync() => Task.FromResult(_items.ToList());
    public Task AddAsync(BookingOverride item) { item.Id = _nextId++; _items.Add(item); return Task.CompletedTask; }
    public Task UpdateAsync(BookingOverride item) { var old = _items.FirstOrDefault(x => x.Id == item.Id); if (old is not null) _items[_items.IndexOf(old)] = item; return Task.CompletedTask; }
    public Task DeleteAsync(int id) { _items.RemoveAll(x => x.Id == id); return Task.CompletedTask; }
}
