using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using SmartFaceAligner.Processor.Entity;
using XamlingCore.NET.Implementations;
using XamlingCore.Portable.Contract.Entities;

namespace SmartFaceAligner.Processor.Services
{
    public class FaceDataService : IFaceDataService
    {
        private readonly IEntityCache _cache;

        public FaceDataService(IEntityCache cache)
        {
            _cache = cache;
        }

        public async Task SetFaceData(FaceData f)
        {
            await _cache.SetEntity(_getKey(f.FileName), f);
        }

        public async Task<FaceData> GetFaceData(string fileName)
        {
            var cache = await _cache.GetEntity<FaceData>(_getKey(fileName));
            if (cache != null)
            {
                return cache;
            }

            return new FaceData { FileName = fileName };
        }

        string _getKey(string fileName)
        {
            return HashHelper.CreateSHA1(fileName + Constants.Cache.FaceData);
        }
    }
}
