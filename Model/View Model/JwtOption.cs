using System;
using System.Collections.Generic;
using System.Text;

namespace NetCoreTemp.WebApi.Models.View_Model
{
    public class JwtOption
    {
        /// <summary>
        /// 服务端密钥
        /// </summary>
        public string ServerSecret { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Audience { get; set; }
    }
}
