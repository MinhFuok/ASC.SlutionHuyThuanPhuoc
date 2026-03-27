using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class FakeSession : ISession
{
    private Dictionary<string, byte[]> data = new();

    public IEnumerable<string> Keys => data.Keys;
    public string Id => Guid.NewGuid().ToString();
    public bool IsAvailable => true;

    public void Clear() => data.Clear();

    public Task CommitAsync(CancellationToken token = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken token = default) => Task.CompletedTask;

    public void Remove(string key) => data.Remove(key);

    public void Set(string key, byte[] value) => data[key] = value;

    public bool TryGetValue(string key, out byte[] value)
        => data.TryGetValue(key, out value);
}