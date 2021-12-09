using System;
using System.Threading.Tasks;

namespace JankiBusiness.Services
{
    public interface ILastSyncTimeAccessor
    {
        Task<DateTime> GetLastSyncTime();

        Task SetLastSyncTime(DateTime time);
    }
}