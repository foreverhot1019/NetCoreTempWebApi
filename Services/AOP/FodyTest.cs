using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Anotar.Log4Net;
using MethodDecorator.Fody.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NetCoreTemp.WebApi.Models.DatabaseContext;
using NetCoreTemp.WebApi.Services.FodyTest;
using System.Linq;
using System.Text.Json;
using MethodTimer;
using System.ComponentModel;
using PropertyChanged;
using NetCoreTemp.WebApi.Services.AOP;

// Attribute should be "registered" by adding as module or assembly custom attribute
//[module: FodyMethodInspect]
//静态类重写读取哪个程序集下的（如：PropertyChangedNotificationInterceptor 分别在Services与Services.FodyTest中，将读取Services中的）
//[assembly: PropertyChanged.FilterType("NetCoreTemp.WebApi.Services")]

namespace NetCoreTemp.WebApi.Services.FodyTest
{
    /// <summary>
    /// 属性
    /// </summary>
    //https://github.com/Fody/MethodDecorator
    // Any attribute which provides OnEntry/OnExit/OnException with proper args
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
    public class FodyMethodInspectAttribute : Attribute, IMethodDecorator
    {
        AppDbContext _appDbContext;
        UserService _userService;
        ILogger _logger;
        object _instance;
        MethodBase _method;
        object[] _args;

        public string Modual { get; set; }

        // instance, method and args can be captured here and stored in attribute instance fields
        // for future usage in OnEntry/OnExit/OnException
        public void Init(object instance, MethodBase method, object[] args)
        {
            _instance = instance;
            _method = method;
            _args = args;

            _userService = FodyDIService.GetIService<UserService>();
            _appDbContext = _userService?.GetDBContext();
            _logger = FodyDIService.GetILogger<FodyMethodInspectAttribute>();
            _logger.LogInformation("Microsoft.Extension.ILogger-Init: {0} [{1}]", method.DeclaringType.FullName + "." + method.Name, args.Length);
            LogTo.Info(string.Format("Init: {0} [{1}]", method.DeclaringType.FullName + "." + method.Name, args.Length));
        }

        public void OnEntry()
        {
            var num = _userService.Query(x => x.Status > Models.EnumType.EnumRepo.UseStatusEnum.Draft).Count();
            _logger.LogInformation($"Microsoft.Extension.ILogger-Init: OnEntry-User：Count-{num}");
            LogTo.Info("OnEntry");
        }

        public void OnExit()
        {
            _logger.LogInformation("Microsoft.Extension.ILogger-Init: OnExit");
            LogTo.Info("OnExit");
        }

        public void OnException(Exception exception)
        {
            _logger.LogError(exception, "Microsoft.Extension.ILogger-OnException: {0}: {1}", exception.GetType(), exception.InnerException?.Message ?? exception.Message);
            LogTo.ErrorException($"OnException: {exception.GetType()}: {exception.InnerException?.Message ?? exception.Message}", exception);
        }
    }

    /// <summary>
    /// 方法事件计算日志实例
    /// </summary>
    public static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
        {
            LogTo.Info("MethodTimeLogger:{0}-{1}-{2}", methodBase.Name, elapsed.TotalSeconds, message);
        }
    }

    /// <summary>
    /// 属性改变通知实例
    /// </summary>
    public static class PropertyChangedNotificationInterceptor
    {
        public static void Intercept(object target, Action onPropertyChangedAction,
                                     string propertyName, object before, object after)
        {
            onPropertyChangedAction();

            LogTo.Info("PropertyChangedNotificationInterceptor:{0}-{1}-{2}", propertyName, before, after);
        }
    }

}

namespace NetCoreTemp.WebApi.Services
{
    /// <summary>
    /// 属性改变通知实例
    /// </summary>
    public static class PropertyChangedNotificationInterceptor
    {
        public static void Intercept(object target, Action onPropertyChangedAction,
                                     string propertyName, object before, object after)
        {
            onPropertyChangedAction();

            LogTo.Info("PropertyChangedNotificationInterceptor:{0}-{1}-{2}", propertyName, before, after);
        }
    }

    #region 测试类

    public delegate void PropertyValueChanedEventHandler(object sender, PropertyChangedEventArgs args, object oldVal, object newVal);

    /// <summary>
    /// https://github.com/Fody/PropertyChanged/wiki/Attributes#donotnotifyattribute
    /// <remark>
    /// AddINotifyPropertyChangedInterface 会自动添加 且无需添加 继承INotifyPropertyChanged接口
    /// public event PropertyChangedEventHandler PropertyChanged 事件
    /// [assembly: PropertyChanged.FilterType("My.Specific.OptIn.Namespace.")] 会自动把命名空间下 所有类加上PropertyChanged通知
    /// </remark>
    /// </summary>
    public class SampleA : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //public event PropertyValueChanedEventHandler PropertyValueChaned;

        public string A { get; set; }

        public string AA { get; set; }

        public string A_AA
        {
            get
            {
                return $"{(A ?? "").Replace("-", "__")}-{AA}";
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var arr = value.Split("-").ToList();
                    A = arr[0].Replace("__", "-");
                    AA = arr?.Count > 1 ? string.Join("", arr.Skip(1)) : "";
                }
            }
        }

        public int B { get; set; }
        private void OnBChanged(int oldValue, int newValue)
        {
            LogTo.Info("B Changed:" + oldValue + " => " + newValue);
            //PropertyValueChaned?.Invoke(this, new PropertyChangedEventArgs("B"), oldValue, newValue);
        }

        [OnChangedMethod(nameof(OnChangedC))]
        public decimal C { get; set; }
        private void OnChangedC(object oldValue, object newValue)
        {
            LogTo.Info("C Changed:" + oldValue + " => " + newValue);
            //PropertyValueChaned?.Invoke(this, new PropertyChangedEventArgs("C"), oldValue, newValue);
        }

        [DoNotNotify]
        public bool D { get; set; }

        public int? BB { get; set; }

        public decimal? CC { get; set; }

        public bool? DD { get; set; }

        [FodyMethodInspect]
        public void MethodA()
        {
            LogTo.Info(JsonSerializer.Serialize(this));
        }

        [Time("MethodTime-Time Field Arg:{TimeField}")]
        public void MethodTime(string TimeField)
        {
            LogTo.Info($"MethodTime：{TimeField}");
        }
    }

    /// <summary>
    /// 自动加PropertityChangeEvent
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class SampleB
    {
        public string A { get; set; }

        public string AA { get; set; }

        public string A_AA
        {
            get
            {
                return $"{(A ?? "").Replace("-", "__")}-{AA}";
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var arr = value.Split("-").ToList();
                    A = arr[0].Replace("__", "-");
                    AA = arr?.Count > 1 ? string.Join("", arr.Skip(1)) : "";
                }
            }
        }

        public int B { get; set; }
        private void OnBChanged(int oldValue, int newValue)
        {
            LogTo.Info("B Changed:" + oldValue + " => " + newValue);
            //PropertyValueChaned?.Invoke(this, new PropertyChangedEventArgs("B"), oldValue, newValue);
        }

        [OnChangedMethod(nameof(OnChangedC))]
        public decimal C { get; set; }
        private void OnChangedC(object oldValue, object newValue)
        {
            LogTo.Info("C Changed:" + oldValue + " => " + newValue);
            //PropertyValueChaned?.Invoke(this, new PropertyChangedEventArgs("C"), oldValue, newValue);
        }

        [DoNotNotify]
        public bool D { get; set; }

        public int? BB { get; set; }

        public decimal? CC { get; set; }

        public bool? DD { get; set; }

        [FodyMethodInspect]
        public void MethodB()
        {
            LogTo.Info(JsonSerializer.Serialize(this));
        }

        [Time("MethodTimeB-Time Field Arg:{TimeField}")]
        public void MethodTimeB(string TimeField)
        {
            LogTo.Info($"MethodTime：{TimeField}");
        }
    }

    #endregion
}