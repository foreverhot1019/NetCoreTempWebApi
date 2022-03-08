using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Anotar.Log4Net;
using MethodDecorator.Fody.Interfaces;
using MethodTimer;
using Microsoft.Extensions.Logging;
using NetCoreTemp.WebApi.Services;
using NetCoreTemp.WebApi.Services.AOP;
using NetCoreTemp.WebApi.Services.FodyTest;
using PropertyChanged;

// Attribute should be "registered" by adding as module or assembly custom attribute
[module: FodyLogAttribute]
//对多个自定义Attribute，拦截织入切片代码（IntersectMethodsMarkedBy名称不能改）
[module: IntersectMethodsMarkedBy(typeof(MyAopLogAttr), typeof(MyAopLogAttrA))]
//静态类重写读取哪个程序集下的（如：PropertyChangedNotificationInterceptor 分别在Services与Services.FodyTest中，将读取Services中的）
//[assembly: PropertyChanged.FilterType("NetCoreTemp.WebApi.Services")]
namespace NetCoreTemp.WebApi.Services
{
    /// <summary>
    /// 指定方法或构造函数上的日志属性
    /// 只有Method、Constructor才可以被截获
    /// Module和Assembly可配制多种Attribute截获
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Module | AttributeTargets.Assembly)]
    public class FodyLogAttribute : Attribute, IMethodDecorator
    {
        object _instance;
        MethodBase _method;
        object[] _args;
        ILogger _logger;
        DateTime dtElapsed = DateTime.Now;

        // Required
        public FodyLogAttribute() { }

        /// <summary>
        /// AOP初始化
        /// </summary>
        /// <param name="instance">调用实例</param>
        /// <param name="method">实例方法</param>
        /// <param name="args">参数</param>
        public void Init(object instance, MethodBase method, object[] args)
        {
            _logger = FodyDIService.GetILogger<FodyLogAttribute>();
            _instance = instance;
            _method = method;
            _args = args;
        }

        /// <summary>
        /// 进入方法事件
        /// </summary>
        public void OnEntry()
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} Start Excute";
            _logger.LogInformation(MsgStr);
        }

        /// <summary>
        /// 出错事件
        /// </summary>
        /// <param name="exception"></param>
        public void OnException(Exception exception)
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} Error";
            _logger.LogError(exception, MsgStr);
        }

        /// <summary>
        /// 方法结束事件
        /// </summary>
        public void OnExit()
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} End Excute(Elapsed:{DateTime.Now.Subtract(dtElapsed).TotalSeconds}s)";
            _logger.LogInformation(MsgStr);
        }

        // Optional
        //public void OnTaskContinuation(Task task) {}
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class MyAopLogAttr : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class MyAopLogAttrA : Attribute
    {

    }

    /// <summary>
    /// 指定方法或构造函数上的日志属性
    /// 拦截多个自定义Attrbute（AttributeTargets只能是Method或Constructor）
    /// IntersectMethodsMarkedByAttribute不能改变名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Module | AttributeTargets.Assembly)]
    public class IntersectMethodsMarkedByAttribute : Attribute
    {
        object _instance;
        MethodBase _method;
        object[] _args;
        ILogger _logger;
        DateTime dtElapsed = DateTime.Now;

        // Required
        public IntersectMethodsMarkedByAttribute()
        {
        }

        /// <summary>
        /// 使用模式为 Module、Assembly时
        /// </summary>
        /// <param name="types"></param>
        public IntersectMethodsMarkedByAttribute(params Type[] types)
        {
            if (types.All(x => typeof(Attribute).IsAssignableFrom(x)))
            {
                throw new Exception("Meaningfull configuration exception");
            }
        }

        /// <summary>
        /// AOP初始化
        /// </summary>
        /// <param name="instance">调用实例</param>
        /// <param name="method">实例方法</param>
        /// <param name="args">参数</param>
        public void Init(object instance, MethodBase method, object[] args)
        {
            _logger = FodyDIService.GetILogger<IntersectMethodsMarkedByAttribute>();
            _instance = instance;
            _method = method;
            _args = args;
        }

        /// <summary>
        /// 进入方法事件
        /// </summary>
        public void OnEntry()
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} Start Excute";
            _logger.LogInformation(MsgStr);
        }

        /// <summary>
        /// 出错事件
        /// </summary>
        public void OnException(Exception exception)
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} Error";
            _logger.LogError(exception, MsgStr);
        }

        /// <summary>
        /// 方法结束事件
        /// </summary>
        public void OnExit()
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} End Excute(Elapsed:{DateTime.Now.Subtract(dtElapsed).TotalSeconds}s)";
            _logger.LogInformation(MsgStr);
        }
        // Optional
        //public void OnTaskContinuation(Task task) {}
    }

}
