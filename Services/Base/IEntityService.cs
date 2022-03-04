using System;
using System.Collections.Generic;
using System.Text;
using NetCoreTemp.WebApi.Models.DatabaseContext;

namespace NetCoreTemp.WebApi.Services.Base
{
    public interface IEntityService
    {
        /// <summary>
        /// 返回DynamoDBRepository 仓库
        /// </summary>
        /// <returns></returns>
        AppDbContext GetDBContext();
    }
}
