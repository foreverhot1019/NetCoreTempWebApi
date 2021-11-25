using System;
using System.Collections.Generic;
using System.Text;

namespace NetCoreTemp.WebApi.Models.View_Model
{
    /// <summary>
    /// 翻页返回集合
    /// </summary>
    public sealed class PpagenationResult
    {
        public PpagenationResult()
        {

        }
        
        /// <summary>
        /// 当前页
        /// </summary>
        public int pageIndex { get; set; }

        /// <summary>
        /// 每页显示
        /// </summary>
        public int pagelimit { get; set; }

        /// <summary>
        /// 页码token
        /// </summary>
        public string PageNationToken { get; set; }

        /// <summary>
        /// 数据集
        /// </summary>
        public IEnumerable<Object> ArrData { get; set; }

        /// <summary>
        /// 行数
        /// </summary>
        public long RowsCount { get; set; }
    }
}
