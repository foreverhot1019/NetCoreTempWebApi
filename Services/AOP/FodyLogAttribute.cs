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

//[module: FodyLogAttribute]
//[assembly: FodyLogAttribute]
[module: IntersectMethodsMarkedBy(typeof(MyAopLogAttr), typeof(MyAopLogAttrA))]
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

        public void Init(object instance, MethodBase method, object[] args)
        {
            _logger = FodyDIService.GetILogger<FodyLogAttribute>();
            _instance = instance;
            _method = method;
            _args = args;
        }

        public void OnEntry()
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} Start Excute";
            _logger.LogInformation(MsgStr);
        }

        public void OnException(Exception exception)
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} Error";
            _logger.LogError(exception, MsgStr);
        }

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

        public void Init(object instance, MethodBase method, object[] args)
        {
            _logger = FodyDIService.GetILogger<IntersectMethodsMarkedByAttribute>();
            _instance = instance;
            _method = method;
            _args = args;
        }

        public void OnEntry()
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} Start Excute";
            _logger.LogInformation(MsgStr);
        }

        public void OnException(Exception exception)
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} Error";
            _logger.LogError(exception, MsgStr);
        }

        public void OnExit()
        {
            var MsgStr = $"{_instance.GetType().Name}- {_method.Name} End Excute(Elapsed:{DateTime.Now.Subtract(dtElapsed).TotalSeconds}s)";
            _logger.LogInformation(MsgStr);
        }
        // Optional
        //public void OnTaskContinuation(Task task) {}
    }

}
