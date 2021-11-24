using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NetCoreTemp.WebApi.WXKF
{
    /// <summary>
    /// 微信客服 应用回调返回值
    /// </summary>
    [XmlRoot("xml")]
    public class WXKF_EventResponse
    {
        /// <summary>
        /// 企业微信CorpID
        /// </summary>
        public string ToUserName { get; set; }

        [XmlIgnore]
        /// <summary>
        /// 消息创建时间，unix时间戳
        /// </summary>
        public DateTime? CreateTime
        {
            get
            {
                return _CreateTimeUnix.toDateTime();
            }
        }

        [XmlElement("CreateTime")]
        public long? _CreateTimeUnix { get; set; }

        /// <summary>
        /// 消息的类型，此时固定为：event
        /// </summary>
        public string MsgType { get; set; }

        /// <summary>
        /// 事件的类型，此时固定为：kf_msg_or_event
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// 调用拉取消息接口时，需要传此token，用于校验请求的合法性
        /// </summary>
        public string Token { get; set; }
    }

    /// <summary>
    /// 微信客服返回消息
    /// </summary>
    public class WXKFResponseBase
    {
        ///<summary>
        /// 返回码
        ///</summary>
        public virtual int errcode { get; set; }

        ///<summary>
        /// 错误码描述
        ///</summary>
        public virtual string errmsg { get; set; }
    }

    /// <summary>
    /// 获取AccessToken
    /// </summary>
    public class GetTokenRes: WXKFResponseBase
    {
        /// <summary>
        /// 授权码
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// 过期时间（秒）
        /// </summary>
        public int expires_in { get; set; }
    }

    #region 消息

    /// <summary>
    /// 读取消息
    /// POST https://qyapi.weixin.qq.com/cgi-bin/kf/sync_msg?access_token=ACCESS_TOKEN
    /// body
    /// {
    ///     "cursor": "4gw7MepFLfgF2VC5npN",
    ///     "token": "ENCApHxnGDNAVNY4AaSJKj4Tb5mwsEMzxhFmHVGcra996NR",
    ///     "limit": 1000
    /// }
    /// </summary>
    public class WXKFMessageResponse : WXKFResponseBase
    {
        ///<summary>
        /// 下次调用带上该值则从该key值往后拉，用于增量拉取
        ///</summary>
        public string next_cursor { get; set; }

        ///<summary>
        /// 是否还有更多数据。0-否；1-是。
        ///</summary>
        public bool has_more { get; set; }

        ///<summary>
        /// 消息列表-不能通过判断msg_list是否空来停止拉取，可能会出现has_more为1，而msg_list为空的情况
        ///</summary>
        public IEnumerable<WXKFMessage> msg_list { get; set; }
    }

    /// <summary>
    /// 微信消息
    /// </summary>
    public class WXKFMessage : ICloneable
    {
        #region 克隆

        /// <summary>
        /// 拷贝
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        /// <summary>
        /// 深度拷贝
        /// 引用类型必须重新赋值，否则指向同一引用地址
        /// </summary>
        /// <returns></returns>
        public WXKFMessage Clone()
        {
            var CloneObj = (WXKFMessage)this.MemberwiseClone();
            CloneObj.text = this.text.Clone();
            CloneObj.image = this.image.Clone();
            CloneObj.video = this.video.Clone();
            CloneObj.file = this.file.Clone();
            CloneObj.location = this.location.Clone();
            CloneObj.link = this.link.Clone();
            CloneObj.business_card = this.business_card.Clone();
            CloneObj.miniprogram = this.miniprogram.Clone();
            CloneObj.@event = this.@event.Clone();
            return CloneObj;
        }

        #endregion

        //private bool _isAnalysis = false;
        /// <summary>
        /// 是否处理过
        /// </summary>
        public bool isAnalysis
        {
            get; set;
            //get
            //{
            //    //5分钟之内的数据
            //    if (!_isAnalysis && send_time < DateTime.Now.AddMinutes(-5).to_Long())
            //        _isAnalysis = true;
            //    else
            //        _isAnalysis = false;
            //    return _isAnalysis;
            //}
            //set
            //{
            //    _isAnalysis = value;
            //}
        } = true;

        /// <summary>
        /// 会话Id
        /// </summary>
        public string DialogId { get; set; }

        ///<summary>
        /// 消息ID
        ///</summary>
        public string msgid { get; set; }

        ///<summary>
        /// 客服帐号ID（msgtype为event，该字段不返回）
        ///</summary>
        public string open_kfid { get; set; }

        ///<summary>
        /// 客户UserID（msgtype为event，该字段不返回）
        ///</summary>
        public string external_userid { get; set; }

        ///<summary>
        /// 消息发送时间
        ///</summary>
        public long send_time { get; set; }

        ///<summary>
        /// 消息来源。3-微信客户发送的消息 4-系统推送的事件消息 5-接待人员在企业微信客户端发送的消息
        ///</summary>
        public int origin { get; set; }

        ///<summary>
        /// 从企业微信给客户发消息的客服人员userid（msgtype为event，该字段不返回）
        ///</summary>
        public string servicer_userid { get; set; }

        ///<summary>
        /// 对不同的msgtype，有相应的结构描述，下面进一步说明
        /// text、image、voice、video、file、location、link、business_card、miniprogram、event
        /// 文本、图片、语音、视频、文件、位置、链接、名片、小程序、事件。
        ///</summary>
        public string msgtype { get; set; }

        ///<summary>
        /// 文本消息text	
        ///</summary>
        public textMsg text { get; set; }

        ///<summary>
        /// 图片消息
        ///</summary>
        public imageMsg image { get; set; }

        ///<summary>
        /// 视频消息
        ///</summary>
        public videoMsg video { get; set; }

        ///<summary>
        /// 文件消息
        ///</summary>
        public fileMsg file { get; set; }

        ///<summary>
        /// 地理位置消息
        ///</summary>
        public locationMsg location { get; set; }

        ///<summary>
        /// 链接消息
        ///</summary>
        public linkMsg link { get; set; }

        ///<summary>
        /// 名片消息
        ///</summary>
        public business_cardMsg business_card { get; set; }

        ///<summary>
        /// 小程序消息
        ///</summary>
        public miniprogramMsg miniprogram { get; set; }

        ///<summary>
        /// 事件消息
        ///</summary>
        public eventMsg @event { get; set; }
    }

    #region 消息类型

    ///<summary>
    /// 菜单消息text	
    ///</summary>
    public class MenuMsg : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public MenuMsg Clone()
        {
            var CloneObj = (MenuMsg)this.MemberwiseClone();
            return CloneObj;
        }

        #endregion

        ///<summary>
        /// id 
        ///</summary>
        public string id { get; set; }

        ///<summary>
        /// content
        ///</summary>
        public string content { get; set; }

    }
    ///<summary>
    /// 文本消息text	
    ///</summary>
    public class textMsg : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public textMsg Clone()
        {
            var CloneObj = (textMsg)this.MemberwiseClone();
            return CloneObj;
        }

        #endregion

        ///<summary>
        /// 文本内 
        ///</summary>
        public string content { get; set; }

        ///<summary>
        /// 客户点击菜单消息，触发的回复消息中附带的菜单ID
        ///</summary>
        public string menu_id { get; set; }

    }

    ///<summary>
    /// 图片消息
    ///</summary>
    public class imageMsg : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public imageMsg Clone()
        {
            var CloneObj = (imageMsg)this.MemberwiseClone();
            return CloneObj;
        }

        #endregion

        ///<summary>
        /// 图片文件id
        ///</summary>
        public string media_id { get; set; }

    }

    ///<summary>
    /// 视频消息
    ///</summary>
    public class videoMsg : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public videoMsg Clone()
        {
            var CloneObj = (videoMsg)this.MemberwiseClone();
            return CloneObj;
        }

        #endregion

        ///<summary>
        /// 文件id
        ///</summary>
        public string media_id { get; set; }

    }

    ///<summary>
    /// 文件消息
    ///</summary>
    public class fileMsg : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public fileMsg Clone()
        {
            var CloneObj = (fileMsg)this.MemberwiseClone();
            return CloneObj;
        }

        #endregion

        ///<summary>
        /// 文件id
        ///</summary>
        public string media_id { get; set; }

    }

    ///<summary>
    /// 地理位置消息
    ///</summary>
    public class locationMsg : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public locationMsg Clone()
        {
            var CloneObj = (locationMsg)this.MemberwiseClone();
            return CloneObj;
        }

        #endregion

        ///<summary>
        /// 纬度
        ///</summary>
        public float latitude { get; set; }

        ///<summary>
        /// 经度
        ///</summary>
        public float longitude { get; set; }

        ///<summary>
        /// 位置名
        ///</summary>t;}
        public string name { get; set; }

        ///<summary>
        /// 地址详情说明
        ///</summary>
        public string address { get; set; }

    }

    ///<summary>
    /// 链接消息
    ///</summary>
    public class linkMsg : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public linkMsg Clone()
        {
            var CloneObj = (linkMsg)this.MemberwiseClone();
            return CloneObj;
        }

        #endregion

        ///<summary>
        /// 标题
        ///</summary>
        public string title { get; set; }

        ///<summary>
        /// 描述
        ///</summary>}
        public string desc { get; set; }

        ///<summary>
        /// 点击后跳转的链接
        ///</summary>}
        public string url { get; set; }

        ///<summary>
        /// 缩略图链接
        ///</summary>
        public string pic_url { get; set; }

        /// <summary>
        /// 缩略图的media_id
        /// 可以通过素材管理接口获得。此处thumb_media_id即上传接口返回的media_id
        /// </summary>
        public string thumb_media_id { get; set; }

    }

    ///<summary>
    /// 名片消息
    ///</summary>
    public class business_cardMsg : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public business_cardMsg Clone()
        {
            var CloneObj = (business_cardMsg)this.MemberwiseClone();
            return CloneObj;
        }

        #endregion

        ///<summary>
        /// 名片userid
        ///</summary>
        public string userid { get; set; }

    }

    ///<summary>
    /// 小程序消息
    ///</summary>
    public class miniprogramMsg : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public miniprogramMsg Clone()
        {
            var CloneObj = (miniprogramMsg)this.MemberwiseClone();
            return CloneObj;
        }

        #endregion

        ///<summary>
        /// 标题
        ///</summary>
        public string title { get; set; }

        ///<summary>
        /// 小程序appid
        ///</summary>
        public string appid { get; set; }

        ///<summary>
        /// 点击消息卡片后进入的小程序页面路径
        ///</summary>
        public string pagepath { get; set; }

        ///<summary>
        /// 小程序消息封面的mediaid
        ///</summary>
        public string thumb_media_id { get; set; }

    }

    ///<summary>
    /// 事件消息
    ///</summary>
    public class eventMsg : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public eventMsg Clone()
        {
            var CloneObj = (eventMsg)this.MemberwiseClone();
            return CloneObj;
        }

        #endregion

        ///<summary>
        /// 事件类型
        /// enter_session：用户进入会话事件
        /// msg_send_fail: 消息发送失败事件
        /// servicer_status_change: 客服人员接待状态变更事件
        /// session_status_change：会话状态变更事件
        ///</summary>
        public string event_type { get; set; }

        ///<summary>
        /// 客服账号ID
        ///</summary>}
        public string open_kfid { get; set; }

        ///<summary>
        /// 客户UserID
        ///</summary>
        public string external_userid { get; set; }

        #region enter_session：用户进入会话事件

        ///<summary>
        /// 进入会话的场景值，获取客服帐号链接开发者自定义的场景值
        ///</summary>
        public string scene { get; set; }

        #endregion

        #region msg_send_fail: 消息发送失败事件

        ///<summary>
        /// 发送失败的消息msgid
        ///</summary>
        public string fail_msgid { get; set; }
        ///<summary>
        /// 失败类型。0-未知原因 1-客服账号已删除 2-应用已关闭 4-会话已过期，超过48小时 5-会话已关闭 6-超过5条限制 7-未绑定视频号 8-主体未验证 9-未绑定视频号且主体未验证 10-用户拒收
        ///</summary>
        public int fail_type { get; set; }

        #endregion

        #region servicer_status_change: 客服人员接待状态变更事件

        ///<summary>
        /// 客服人员userid
        ///</summary>
        public string servicer_userid { get; set; }
        ///<summary>
        /// 状态类型。1-接待中 2-停止接待
        ///</summary>
        public int status { get; set; }

        #endregion

        #region session_status_change：会话状态变更事件

        ///<summary>
        /// 变更类型。1-从接待池接入会话 2-转接会话 3-结束会话
        ///</summary>
        public int change_type { get; set; }
        ///<summary>
        /// 老的客服人员userid。仅change_type为2和3有值
        ///</summary>
        public string old_servicer_userid { get; set; }
        ///<summary>
        /// 新的客服人员userid。仅change_type为1和2有值
        ///</summary>
        public string new_servicer_userid { get; set; }

        ///<summary>
        /// 用于发送事件响应消息的code，仅change_type为1-从接待池接入会话和 3-结束会话时，会返回该字段。可用该msg_code调用发送事件响应消息接口给客户发送回复语或结束语。
        ///</summary>
        public string msg_code { get; set; }

        #endregion
    }

    #endregion

    #endregion

    #region 微信客服接待人员列表

    public class WXKFServicerResponse : WXKFResponseBase
    {
        /// <summary>
        /// 会话状态
        /// 0:未处理 新会话接入。可选择：1.直接用API自动回复消息。2.放进待接入池等待接待人员接待。3.指定接待人员进行接待
        /// 1:由智能助手接待 可使用API回复消息。可选择转入待接入池或者指定接待人员处理。
        /// 2:待接入池排队中 在待接入池中排队等待接待人员接入。可选择转为指定人员接待
        /// 3:由人工接待 人工接待中。可选择结束会话
        /// 4:已结束 会话已经结束。不允许变更会话状态，等待用户重新发起咨询
        /// </summary>
        public int service_state { get; set; }

        /// <summary>
        /// 接待人员的userid
        /// 仅当service_state=3时有效
        /// </summary>
        public string servicer_userid { get; set; }

        ///<summary>
        /// 客服帐号的接待人员列表
        /// 或者是 OpenKFId列表
        ///</summary>
        public List<WXKFServicer> servicer_list { get; set; }

        ///<summary>
        /// 删除、新增 客服帐号的接待人员的结果
        ///</summary>
        public List<WXKFServicer> result_list { get; set; }

        ///<summary>
        /// 所有客服接待人员
        ///</summary>
        public List<WXKFServicer> account_list { get; set; }

        #region 获取外部客户unionId

        /// <summary>
        /// 外部客户基本信息
        /// </summary>
        public List<WXKFCustomer> customer_list { get; set; }

        /// <summary>
        /// 不被允许的客户
        /// </summary>
        public List<string> invalid_external_userid { get; set; }

        #endregion

    }

    /// <summary>
    /// 外部客户基本信息
    /// </summary>
    public class WXKFCustomer
    {
        ///<summary>
        /// 微信客户的external_userid
        ///</summary>
        public string external_userid { get; set; }

        ///<summary>
        /// 微信昵称
        ///</summary>
        public string nickname { get; set; }

        ///<summary>
        /// 微信头像第三方不可获取
        ///</summary>
        public string avatar { get; set; }

        ///<summary>
        /// 性别
        ///</summary>
        public int gender { get; set; }

        ///<summary>
        /// 外部客户unionid需要绑定微信开发者帐号才能获取到
        ///</summary>
        public string unionid { get; set; }

        #region 匹配EmployeeWeChatMapping

        ///<summary>
        /// 姓名
        ///</summary>
        public string DisplayName { get; set; }

        ///<summary>
        /// Email
        ///</summary>
        public string Email { get; set; }

        ///<summary>
        /// 中文姓名
        ///</summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string Department { get; set; }

        #endregion
    }

    public class WXKFServicer
    {
        #region 获取 微信客服 接待人员

        ///<summary>
        /// 接待人员的userid。第三方应用获取到的为密文userid，即open_userid
        ///</summary>
        public string userid { get; set; }

        ///<summary>
        /// 接待人员的接待状态。0:接待中,1:停止接待。第三方应用需具有“管理帐号、分配会话和收发消息”权限才可获取
        ///</summary>
        public int status { get; set; }

        #region 删除或添加时返回的信息

        /// <summary>
        /// 操作该userid的结果
        /// </summary>
        public int errcode { get; set; } 

        /// <summary>
        /// 结果信息
        /// </summary>
        public string errmsg { get; set; }

        #endregion

        #endregion

        #region 获取 所有 客服人员时使用account_list 帐号信息列表 即：客服列表

        /// <summary>
        /// 客服帐号ID
        /// </summary>
        public string open_kfid { get; set; }

        /// <summary>
        /// 客服帐号名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 客服头像URL
        /// </summary>
        public string avatar { get; set; }

        #endregion
    }

    #endregion

    /// <summary>
    /// 微信客服发送消息
    /// </summary>
    public class WXKFMsgResponse : WXKFResponseBase
    {
        ///<summary>
        /// 消息ID。如果请求参数指定了msgid，则原样返回，否则系统自动生成并返回。
        /// 不多于32字节
        /// 字符串取值范围(正则表达式)：[0-9a-zA-Z_-]*
        ///</summary>
        public string msgid { get; set; }
    }

    /// <summary>
    /// 微信客服会话
    /// </summary>
    public class WXKFDialog : ICloneable
    {
        #region 克隆

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public WXKFDialog Clone()
        {
            return (WXKFDialog)this.MemberwiseClone();
        }

        #endregion

        ///<summary>
        /// 会话Id
        ///</summary>}
        public Guid dialogId { get; set; } = Guid.Empty;

        ///<summary>
        /// 客服账号ID
        ///</summary>}
        public string open_kfid { get; set; }

        /// <summary>
        /// 客服接待人ID
        /// </summary>
        public string servicerId { get; set; }

        /// <summary>
        /// 外部客户名称
        /// </summary>
        public string external_userid { get; set; }

        ///<summary>
        /// 客户进入时间
        ///</summary>
        public long StartTime { get; set; }

        ///<summary>
        /// 客服接待时间
        ///</summary>
        public long servicer_pickup_time { get; set; }

        ///<summary>
        /// 结束时间
        ///</summary>
        public long EndTime { get; set; }
    }

    /// <summary>
    /// 上传媒体文件
    /// </summary>
    public class WXKFMediaResponse : WXKFResponseBase
    {
        /// <summary>
        /// 媒体文件类型
        /// 分别有图片（image）、语音（voice）、视频（video），普通文件(file)
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 媒体文件上传后获取的唯一标识，3天内有效
        /// </summary>
        public string media_id { get; set; }

        /// <summary>
        /// 媒体文件上传时间戳
        /// </summary>
        public long created_at { get; set; }

    }

    /// <summary>
    /// 事件类型 枚举
    /// </summary>
    public enum WXKFeventType {
        // enter_session：用户进入会话事件
        enter_session,
        // session_status_change：会话状态变更事件  
        session_status_change,
        // msg_send_fail: 消息发送失败事件
        msg_send_fail,
        // servicer_status_change: 客服人员接待状态变更事件
        servicer_status_change
    }
    
    /// <summary>
    /// 发送消息类型 枚举
    /// </summary>
    public enum WXKFSendMsgType {
        // 用户进入会话
        Userenter,
        // 结束会话状发送消息  
        Endsession
    }
    
    /// <summary>
    /// 发送消息类型 枚举
    /// </summary>
    public enum WXKFCacheKey {
        // 客服 接待人员 用户id
        kfid_servicerId_userid_mapping,
        GetWXKFAccountServicerList

    }
}
