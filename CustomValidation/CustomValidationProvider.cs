﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace LocalizerCustomValidation
{
    /// <summary>
    /// 数据显示-适配器
    /// </summary>
    public class My_ModelMetadataProvider : DefaultModelMetadataProvider
    {
        protected IOptions<MvcOptions> _optionsAccessor;

        /// <summary>
        ///  Creates a new Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadataProvider.
        /// </summary>
        /// <param name="detailsProvider">The Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.ICompositeMetadataDetailsProvider.</param> 
        public My_ModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider)
            : base(detailsProvider)
        {
        }

        /// <summary>
        /// Creates a new Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadataProvider.
        /// </summary>
        /// <param name="detailsProvider">The Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.ICompositeMetadataDetailsProvider.</param>
        /// <param name="optionsAccessor">The accessor for Microsoft.AspNetCore.Mvc.MvcOptions.</param>
        public My_ModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor)
            : base(detailsProvider, optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        protected override ModelMetadata CreateModelMetadata(DefaultMetadataDetails entry)
        {
            //if (entry.ModelAttributes?.TypeAttributes?.Any(x => x.GetType().IsAssignableFrom(typeof(MetadataTypeAttribute))) == true)
            //{
            //    if (entry.Key.ModelType?.BaseType == typeof(Models.BaseModel.BaseEntity))
            //    {
            //        if (entry.Key.ModelType?.Name == "User")
            //        {
            //            if (entry.ValidationMetadata == null)
            //                entry.ValidationMetadata = new ValidationMetadata { };
            //            entry.ValidationMetadata.ValidatorMetadata.Add(new RequiredAttribute()
            //            {
            //                ErrorMessageResourceName = "RequiredAttribute_ValidationError",
            //                ErrorMessageResourceType = typeof(CommonLanguage.Language)
            //            });
            //        }
            //        //var modelType = entry.Key.ModelType;
            //        //var metadataType = modelType.GetCustomAttributes<MetadataTypeAttribute>(true).FirstOrDefault();
            //        ////netCore.MVC
            //        ////var metadataType = modelType.GetCustomAttributes<ModelMetadataTypeAttribute>(true).FirstOrDefault();
            //        //if (metadataType?.MetadataClassType != null)
            //        //{
            //        //    var ArrProp = metadataType?.MetadataClassType.GetProperties();
            //        //    List<object> ArrValidatorMetadata = new List<object>();
            //        //    if (ArrProp?.Any() == true)
            //        //    {
            //        //        foreach (var item in ArrProp)
            //        //        {
            //        //            var QArrProp = item.GetCustomAttributes().OfType<ValidationAttribute>();
            //        //            ArrValidatorMetadata.AddRange(QArrProp);
            //        //        }
            //        //    }
            //        //    //entry.BindingMetadata = new BindingMetadata { BinderType = metadataType?.MetadataClassType };

            //        //    entry.DisplayMetadata = new DisplayMetadata { DisplayName = () => "DisplayName" };
            //        //    //if (ArrValidatorMetadata.Any())
            //        //    //{
            //        //    //    if (entry.ValidationMetadata == null)
            //        //    //        entry.ValidationMetadata = new ValidationMetadata { };

            //        //    //    foreach (var item in ArrValidatorMetadata)
            //        //    //        entry.ValidationMetadata.ValidatorMetadata.Add(item);
            //        //    //}
            //        //}
            //    }
            //}
            if (entry.Key.ModelType?.Name == "User")
            {
                //var re = new RequiredAttribute
                //{
                //    ErrorMessage = "自定义ErrorMessage"
                //};
                //////为类增加 特性
                ////System.ComponentModel.TypeDescriptor.AddAttributes(entry.Key.ModelType, re);

                //System.ComponentModel.PropertyDescriptorCollection appSetingAttributes = System.ComponentModel.TypeDescriptor.GetProperties(entry.Key.ModelType);
                //var prop = appSetingAttributes["Test"];
                //var attributeArrayPropInfo = prop.GetType().GetProperty("AttributeArray", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                //var attributeArray = attributeArrayPropInfo.GetValue(prop, null) as Attribute[];
                //var length = attributeArray.Length;
                //var newArray = new Attribute[length + 1];
                //attributeArray.CopyTo(newArray, 0);
                //newArray[length] = re;
                //attributeArrayPropInfo.SetValue(prop, newArray, null);

                //var identity = ModelMetadataIdentity.ForType(entry.Key.ModelType);
                //DefaultMetadataDetails details = CreateTypeDetails(identity);
                //var a = ModelAttributes.GetAttributesForType(entry.Key.ModelType);
                //entry = new DefaultMetadataDetails(entry.Key, a);
            }
            var modelMeta = base.CreateModelMetadata(entry);
            return modelMeta;
        }

        public override ModelMetadata GetMetadataForProperty(PropertyInfo propertyInfo, Type modelType)
        {
            if (modelType.Name == "User")
            {
                var identity = ModelMetadataIdentity.ForType(modelType);
                DefaultMetadataDetails details = CreateTypeDetails(identity);
            }
            var meta = base.GetMetadataForProperty(propertyInfo, modelType);
            return meta;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public override ModelMetadata GetMetadataForType(Type modelType)
        {
            //  Optimization for intensively used System.Object
            if (modelType == typeof(object))
            {
                return base.GetMetadataForType(modelType);
            }
            if (modelType.Name == "User")
            {
                var identity = ModelMetadataIdentity.ForType(modelType);
                DefaultMetadataDetails details = CreateTypeDetails(identity);
            }

            //var identity = ModelMetadataIdentity.ForType(modelType);
            //DefaultMetadataDetails details = CreateTypeDetails(identity);

            ////  This part contains the same logic as DefaultModelMetadata.DisplayMetadata property
            ////  See https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNetCore.Mvc.Core/ModelBinding/Metadata/DefaultModelMetadata.cs

            //var context = new DisplayMetadataProviderContext(identity, details.ModelAttributes);
            ////  Here your implementation of IDisplayMetadataProvider will be called
            //DetailsProvider.CreateDisplayMetadata(context);
            //details.DisplayMetadata = context.DisplayMetadata;
            //return CreateModelMetadata(details);

            return base.GetMetadataForType(modelType);
        }

    }

    /// <summary>
    /// 自定义 验证器
    /// </summary>
    public class MyModelValidator : IModelValidator
    {
        private static readonly object _emptyValidationContextInstance = new object();
        public ValidationAttribute Attribute { get; }
        private readonly IStringLocalizer _stringLocalizer;
        IStringLocalizerFactory _stringLocalizerFactory;
        private readonly IValidationAttributeAdapterProvider _validationAttributeAdapterProvider = new ValidationAttributeAdapterProvider();
        IMemoryCache _memoryCache;

        public MyModelValidator(IMemoryCache memoryCache, IStringLocalizer stringLocalizer, IStringLocalizerFactory stringLocalizerFactory)
        {
            _memoryCache = memoryCache;
            _stringLocalizer = stringLocalizer;
            _stringLocalizerFactory = stringLocalizerFactory;
        }

        public MyModelValidator(IMemoryCache memoryCache, ValidationAttribute attribute, IStringLocalizer stringLocalizer, IStringLocalizerFactory stringLocalizerFactory)
            : this(memoryCache, stringLocalizer, stringLocalizerFactory)
        {
            Attribute = attribute;
        }

        public MyModelValidator(ValidationAttribute attribute, IStringLocalizer stringLocalizer, IStringLocalizerFactory stringLocalizerFactory, IValidationAttributeAdapterProvider validationAttributeAdapterProvider = null)
        {
            if (validationAttributeAdapterProvider != null)
                _validationAttributeAdapterProvider = validationAttributeAdapterProvider;
            Attribute = attribute;
            _stringLocalizer = stringLocalizer;
            _stringLocalizerFactory = stringLocalizerFactory;
        }

        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="context">模型验证上下文</param>
        /// <returns></returns>
        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            List<ModelValidationResult> modelValidationResults = new List<ModelValidationResult>();
            var metadata = context.ModelMetadata;
            if (metadata == null)
                return modelValidationResults;
            var metadataKind = metadata.MetadataKind;
            var memberName = metadata.Name;
            var container = context.Container;
            Type modelType = context.ModelMetadata.ModelType;
            if (!modelType.IsClass || modelType.IsPrimitive || modelType == typeof(string) || modelType.Name == "Object")
                modelType = context.Container.GetType();
            #region 获取MetadataType class 属性

            var ArrMetadataProp = MyModelValidatorProvider.GetTypeProp(modelType, _memoryCache);

            #endregion
            //类所在程序集
            var asm = Assembly.GetAssembly(modelType);
            var tt = asm?.GetType(modelType.FullName.Replace("NetCoreTemp.WebApi.Models.", "NetCoreTemp.WebApi.Models.Resources."));
            switch (metadataKind)
            {
                case ModelMetadataKind.Parameter:
                case ModelMetadataKind.Type:
                    //基类 属性
                    var ArrProp = ArrMetadataProp.Where(x => !x.IsMeta);
                    //基类元数据 属性
                    var ArrMetaProp = ArrMetadataProp.Where(x => x.IsMeta);
                    foreach (var prop in ArrProp)
                    {
                        //基类 验证属性
                        var PropValidationAttrs = prop.ArrCustomAttr.OfType<ValidationAttribute>();
                        var metaProp = ArrMetaProp.Where(x => x.prop.Name == prop.prop.Name)?.FirstOrDefault();
                        //基类元数据 验证属性
                        var MetaValidationAttrs = metaProp?.ArrCustomAttr?.OfType<ValidationAttribute>();
                        //合并属性
                        var ValidationAttrs = PropValidationAttrs;
                        if (MetaValidationAttrs != null)
                            ValidationAttrs = ValidationAttrs.Union(MetaValidationAttrs);
                        var _memberName = prop.prop.Name;

                        //显示名称，读取国际化配置
                        var _displayName = _memberName;
                        var _memberLocalized = _stringLocalizerFactory.Create(tt ?? modelType).GetString(_memberName);
                        if (_memberLocalized.ResourceNotFound)
                        {
                            var _metadataDisplay = metaProp?.ArrCustomAttr?.OfType<DisplayAttribute>().FirstOrDefault();
                            _displayName = _metadataDisplay?.Name ?? _memberName;
                        }
                        else
                        {
                            _displayName = _memberLocalized.Value;
                        }

                        //基类 属性值
                        var modelValue = prop.prop.GetValue(container ?? context.Model);
                        var _validcontext = new ValidationContext(
                            instance: container ?? context.Model ?? _emptyValidationContextInstance,
                            serviceProvider: context.ActionContext?.HttpContext?.RequestServices,
                            items: null)
                        {
                            DisplayName = _displayName,
                            MemberName = _memberName//$"{metadata.GetDisplayName()}.{_memberName}"
                        };
                        foreach (var attr in ValidationAttrs)
                        {
                            var _result = attr.GetValidationResult(modelValue, _validcontext);
                            addModelValidationResult(context, _memberName, _result, modelValidationResults);
                        }
                    }
                    break;
                case ModelMetadataKind.Property:
                    var _attribute = Attribute;
                    //显示名称，读取国际化配置
                    var displayName = memberName;
                    var memberLocalized = _stringLocalizerFactory.Create(tt ?? modelType).GetString(memberName);
                    if (memberLocalized.ResourceNotFound)
                    {
                        var metadataDisplay = ArrMetadataProp?.Where(x => x.IsMeta && x.prop.Name == memberName)?.FirstOrDefault().ArrCustomAttr?.OfType<DisplayAttribute>().FirstOrDefault();
                        displayName = metadataDisplay?.Name ?? memberName;
                    }
                    else
                    {
                        displayName = memberLocalized.Value;
                    }
                    var validcontext = new ValidationContext(
                        instance: container ?? context.Model ?? _emptyValidationContextInstance,
                        serviceProvider: context.ActionContext?.HttpContext?.RequestServices,
                        items: null)
                    {
                        DisplayName = displayName,
                        MemberName = memberName
                    };
                    var result = _attribute.GetValidationResult(context.Model, validcontext);
                    addModelValidationResult(context, memberName, result, modelValidationResults);
                    break;
            }
            return modelValidationResults;
        }

        /// <summary>
        /// 增加错误信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="memberName"></param>
        /// <param name="result"></param>
        /// <param name="modelValidationResults"></param>
        private void addModelValidationResult(ModelValidationContext context, string memberName, ValidationResult result, List<ModelValidationResult> modelValidationResults)
        {
            if (result != null)
            {
                string? errorMessage;
                if (_stringLocalizer != null &&
                    !string.IsNullOrEmpty(Attribute.ErrorMessage) &&
                    string.IsNullOrEmpty(Attribute.ErrorMessageResourceName) &&
                    Attribute.ErrorMessageResourceType == null)
                {
                    errorMessage = GetErrorMessage(context) ?? result.ErrorMessage;
                }
                else
                {
                    errorMessage = result.ErrorMessage;
                }
                if (result.MemberNames != null)
                {
                    foreach (var resultMemberName in result.MemberNames)
                    {
                        // ModelValidationResult.MemberName is used by invoking validators (such as ModelValidator) to
                        // append construct the ModelKey for ModelStateDictionary. When validating at type level we
                        // want the returned MemberNames if specified (e.g. "person.Address.FirstName"). For property
                        // validation, the ModelKey can be constructed using the ModelMetadata and we should ignore
                        // MemberName (we don't want "person.Name.Name"). However the invoking validator does not have
                        // a way to distinguish between these two cases. Consequently we'll only set MemberName if this
                        // validation returns a MemberName that is different from the property being validated.
                        var newMemberName = string.Equals(resultMemberName, memberName, StringComparison.Ordinal) ?
                            null :
                            resultMemberName;
                        var validationResult = new ModelValidationResult(newMemberName, errorMessage);

                        modelValidationResults.Add(validationResult);
                    }
                }

                if (!modelValidationResults.Any())
                {
                    // result.MemberNames was null or empty.
                    modelValidationResults.Add(new ModelValidationResult(memberName: null, message: errorMessage));
                }
            }
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        private string? GetErrorMessage(ModelValidationContextBase validationContext)
        {
            var adapter = _validationAttributeAdapterProvider.GetAttributeAdapter(Attribute, _stringLocalizer);
            return adapter?.GetErrorMessage(validationContext);
        }
    }

    /// <summary>
    /// 自定义模型验证提供者
    /// 想要实现 自定义的全局验证，实现IModelValidator
    /// 并添加到ModelValidatorProviderContext-Results里
    /// </summary>
    public class MyModelValidatorProvider : IMetadataBasedModelValidatorProvider, IModelValidatorProvider
    {
        //验证特性type
        static Type _type = typeof(ValidationAttribute);
        //默认错误信息
        static FieldInfo _defaultErrorMessageFieldInfo = _type.GetField("_defaultErrorMessage", BindingFlags.Instance | BindingFlags.NonPublic);
        //错误信息（为设置的话，读取默认错误信息）
        static FieldInfo _errMsgFieldInfo = _type.GetField("_errorMessage", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// CommonLanguage.Language
        /// </summary>
        private readonly IStringLocalizer _stringLocalizer;
        /// <summary>
        /// 国际化工厂
        /// </summary>
        private readonly IStringLocalizerFactory _sharedLocalizerFac;
        /// <summary>
        /// 缓存
        /// </summary>
        IMemoryCache _memoryCache;

        public MyModelValidatorProvider(IMemoryCache memoryCache, IStringLocalizer stringLocalizer, IStringLocalizerFactory sharedLocalizer)
        {
            _memoryCache = memoryCache;
            _stringLocalizer = stringLocalizer;

            _sharedLocalizerFac = sharedLocalizer;
        }

        public void CreateValidators(ModelValidatorProviderContext context)
        {
            if (context.ModelMetadata?.HasValidators != true)
                return;
            //var MyVldAttrAdp = new MyValidationAttributeAdapterProvider();
            var propName = context.ModelMetadata.PropertyName;
            var modelType = context.ModelMetadata.ModelType;
            if (!modelType.IsClass || modelType.IsPrimitive || modelType == typeof(string) || modelType.Name == "Object")
                modelType = context.ModelMetadata.ContainerType;

            #region 获取MetaDataType类的属性&缓存属性

            var ArrMetadataProp = GetTypeProp(modelType, _memoryCache);

            #endregion

            switch (context.ModelMetadata.MetadataKind)
            {
                case ModelMetadataKind.Parameter:
                case ModelMetadataKind.Type:
                    var ArrModelValidtion = modelType.GetCustomAttributes().OfType<ValidationAttribute>();
                    foreach (var mvld in ArrModelValidtion)
                    {
                        var __myModelValidator = new MyModelValidator(_memoryCache, mvld, _stringLocalizer, _sharedLocalizerFac);
                        context.Results.Add(new ValidatorItem(context.ModelMetadata)
                        {
                            IsReusable = true,
                            Validator = __myModelValidator
                        });
                    }

                    #region 类型验证 注入自定义IModelValidator

                    var _myModelValidator = new MyModelValidator(_memoryCache, _stringLocalizer, _sharedLocalizerFac);
                    context.Results.Add(new ValidatorItem(context.ModelMetadata)
                    {
                        IsReusable = true,
                        Validator = _myModelValidator
                    });

                    #endregion

                    break;
                case ModelMetadataKind.Property:
                    #region 属性验证

                    var QArrMetadataProp = ArrMetadataProp.Where(x => x.prop.Name == propName);
                    var resultsCount = context.Results.Count;

                    if (!context.Results.Any())
                    {
                        //增加 基类ValidationAttribute
                        var ArrValidAttr = QArrMetadataProp.Where(n => !n.IsMeta).SelectMany(x => x.ArrCustomAttr.OfType<ValidationAttribute>());
                        foreach (var _validAttr in ArrValidAttr)
                        {
                            var _MyModelValidator = new MyModelValidator(_memoryCache, _validAttr, _stringLocalizer, _sharedLocalizerFac);
                            context.Results.Add(new ValidatorItem(_validAttr)
                            {
                                IsReusable = true,
                                Validator = _MyModelValidator
                            });
                        }
                    }
                    //元数据验证属性
                    var ArrValidMetadata = QArrMetadataProp.Where(n => n.IsMeta).SelectMany(x => x.ArrCustomAttr.OfType<ValidationAttribute>());
                    for (var i = 0; i < resultsCount; i++)
                    {
                        var validitem = context.Results[i];
                        IEnumerable<ValidationAttribute> QArrValidMetadata = ArrValidMetadata?.Where(x => x.GetType() == validitem.ValidatorMetadata.GetType());
                        if (QArrValidMetadata?.Any() != true)
                        {
                            QArrValidMetadata = QArrMetadataProp.Where(n => !n.IsMeta).SelectMany(x => x.ArrCustomAttr.OfType<ValidationAttribute>()).Where(n => n.GetType() == validitem.ValidatorMetadata.GetType());
                        }
                        var validationAttr = QArrValidMetadata.FirstOrDefault();
                        var _MyModelValidator = new MyModelValidator(_memoryCache, validationAttr, _stringLocalizer, _sharedLocalizerFac);
                        validitem.Validator = _MyModelValidator;
                        context.Results.Remove(validitem);
                        context.Results.Insert(0, validitem);
                    }
                    var QAddMetadataValidator = ArrValidMetadata?.Where(x => !context.Results.Any(n => n.ValidatorMetadata?.GetType() == x.GetType()));
                    if (QAddMetadataValidator?.Any() == true)
                    {
                        foreach (var vldMeta in QAddMetadataValidator)
                        {
                            var _MyModelValidator = new MyModelValidator(_memoryCache, vldMeta, _stringLocalizer, _sharedLocalizerFac);
                            context.Results.Add(new ValidatorItem(vldMeta)
                            {
                                IsReusable = true,
                                Validator = _MyModelValidator
                            });
                        }
                    }

                    #endregion
                    break;
            }
        }

        /// <summary>
        /// 是否有Validator
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="validatorMetadata"></param>
        /// <returns></returns>
        public bool HasValidators(Type modelType, IList<object> validatorMetadata)
        {
            if (validatorMetadata.Any())
                return true;
            var has = false;
            if (modelType.IsClass && !modelType.IsPrimitive && modelType != typeof(string) && modelType.Name != "Object")
            {
                //获取 Type类的属性和自定义属性(包括元数据的)
                var ArrMetadataProp = GetTypeProp(modelType, _memoryCache);

                has = ArrMetadataProp?.Any(x => x.ArrCustomAttr.OfType<ValidationAttribute>().Any()) == true;
            }
            return has;
        }

        /// <summary>
        /// 读取Metadata,设置验证错误信息
        /// 存在ErrorMessage或存在 Resource设置，则不在自动设置(手动实现多语言验证错误信息)
        /// </summary>
        /// <param name="validMetadata"></param>
        public static void SetErrMsgByResource(ValidationAttribute validMetadata)
        {
            var needResourceSet = true;
            var vldAttr = validMetadata;
            //默认错误信息
            //var _defaultErrorMessage = _defaultErrorMessageFieldInfo.GetValue(validMetadata)?.ToString() ?? "";
            //自定义的错误信息
            var _erroeMessage = _errMsgFieldInfo.GetValue(validMetadata)?.ToString() ?? "";
            if (!string.IsNullOrEmpty(_erroeMessage))
                needResourceSet = false;
            if (!string.IsNullOrEmpty(vldAttr.ErrorMessageResourceName) ||
            vldAttr.ErrorMessageResourceType != null)
                needResourceSet = false;

            switch (validMetadata)
            {
                case RequiredAttribute attr:
                    if (needResourceSet)
                        attr.ErrorMessageResourceName = "RequiredAttribute_ValidationError";
                    break;
                case RangeAttribute attr:
                    if (needResourceSet)
                        attr.ErrorMessageResourceName = "RangeAttribute_ValidationError";
                    break;
                case MaxLengthAttribute attr:
                    if (needResourceSet)
                        attr.ErrorMessageResourceName = "MaxLengthAttribute_ValidationError";
                    break;
                case EmailAddressAttribute attr:
                    if (needResourceSet)
                        attr.ErrorMessageResourceName = "EmailAddressAttribute_Invalid";
                    break;
                case StringLengthAttribute attr:
                    if (needResourceSet)
                    {
                        if (attr.MinimumLength > 0)
                            attr.ErrorMessageResourceName = "StringLengthAttribute_ValidationErrorIncludingMinimum";
                        else
                            attr.ErrorMessageResourceName = "StringLengthAttribute_ValidationError";
                    }
                    break;
                case CompareAttribute attr:
                    if (needResourceSet)
                        attr.ErrorMessageResourceName = "CompareAttribute_MustMatch";
                    break;
                default:
                    needResourceSet = false;
                    break;
            }
            if (needResourceSet)
            {
                vldAttr.ErrorMessage = null;
                vldAttr.ErrorMessageResourceType = typeof(CommonLanguage.Language);
            }
        }

        /// <summary>
        /// 获取 Type类的属性和自定义属性(包括元数据的)
        /// </summary>
        /// <param name="modelType">类</param>
        /// <param name="_memoryCache">缓存</param>
        /// <returns></returns>
        public static IEnumerable<(PropertyInfo prop, bool IsMeta, IEnumerable<Attribute> ArrCustomAttr)> GetTypeProp(Type modelType, IMemoryCache _memoryCache = null)
        {
            List<(PropertyInfo prop, bool IsMeta, IEnumerable<Attribute> ArrCustomAttr)> ArrModelMetadataProp = new List<(PropertyInfo prop, bool IsMeta, IEnumerable<Attribute> ArrCustomAttr)>();
            var cacheName = modelType.FullName + "Prop_Metadata";
            Object ObjMetadataProp = null;

            System.Runtime.Caching.MemoryCache cache = null;
            if (_memoryCache == null)
            {
                cache = System.Runtime.Caching.MemoryCache.Default;
                ObjMetadataProp = cache.Get(cacheName);
            }
            else
                ObjMetadataProp = _memoryCache.Get(cacheName);

            if (ObjMetadataProp != null)
            {
                ArrModelMetadataProp = (List<(PropertyInfo prop, bool IsMeta, IEnumerable<Attribute> ArrCustomAttr)>)ObjMetadataProp;
            }
            else
            {
                //基类 属性&自定义属性
                var ArrModelTypeProp = modelType.GetProperties().Select(x => (prop: x, IsMeta: false, ArrCustomAttr: x.GetCustomAttributes()));
                ArrModelMetadataProp.AddRange(ArrModelTypeProp);
                #region 根据基类，获取元数据类型 asp.NetCore.MVC MetadataTypeAttribute 已启用，使用ModelMetadataTypeAttribute

                Type _metadataType;
                var _metadataTypeAttr = modelType.GetCustomAttributes<MetadataTypeAttribute>(true).FirstOrDefault();
                _metadataType = _metadataTypeAttr?.MetadataClassType;
                var _modelmetadataTypeAttr = modelType.GetCustomAttributes<ModelMetadataTypeAttribute>(true).FirstOrDefault();
                if (_modelmetadataTypeAttr != null)
                    _metadataType = _modelmetadataTypeAttr?.MetadataType;

                #endregion

                if (_metadataType != null)
                {
                    ArrModelMetadataProp.AddRange(_metadataType.GetProperties().Select(x => (prop: x, IsMeta: true, ArrCustomAttr: x.GetCustomAttributes())));
                }
                //设置缓存
                if (_memoryCache == null)
                    cache.Set(cacheName, ArrModelMetadataProp, DateTimeOffset.Now.AddHours(8));
                else
                    _memoryCache.Set(cacheName, ArrModelMetadataProp);

                //设置验证属性国际化
                var ArrCusAttr = ArrModelMetadataProp.SelectMany(x => x.ArrCustomAttr.OfType<ValidationAttribute>());
                foreach (var vldAttr in ArrCusAttr)
                {
                    SetErrMsgByResource(vldAttr);
                }
            }
            return ArrModelMetadataProp;
        }
    }

    #region 无效 无法注入，注入后未调用

    /// <summary>
    /// 验证属性适配器提供者
    /// <remark>
    /// webapi下无效，注入后未调用
    /// var ArrValidAttrAdp = services.Where(f => f.ServiceType == typeof(IValidationAttributeAdapterProvider));
    ///         if (ArrValidAttrAdp.Any())
    ///         {
    ///             services.Remove(ArrValidAttrAdp.First());
    ///         }
    /// services.AddSingleton<IValidationAttributeAdapterProvider, MyValidationAttributeAdapterProvider>();
    /// </remark>
    /// </summary>
    public class MyValidationAttributeAdapterProvider
            : ValidationAttributeAdapterProvider, IValidationAttributeAdapterProvider
    {
        public MyValidationAttributeAdapterProvider()
        {

        }

        private readonly IValidationAttributeAdapterProvider _baseProvider =
            new ValidationAttributeAdapterProvider();

        IAttributeAdapter IValidationAttributeAdapterProvider.GetAttributeAdapter(
            ValidationAttribute attribute,
            IStringLocalizer stringLocalizer)
        {
            IAttributeAdapter adapter;
            switch (attribute)
            {
                case EmailAddressAttribute emailAttr:
                    attribute.ErrorMessage = "{0}邮箱格式不正确.";
                    adapter = _baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
                    break;
                case RequiredAttribute rqAttr:
                    attribute.ErrorMessage = "字段 {0} 是必填项.";
                    adapter = _baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
                    break;

                case StringLengthAttribute slenAttr:
                case MaxLengthAttribute mlenAttr:
                    attribute.ErrorMessage = "字段 {0} 是必填 介于{1}-{2}之间.";
                    adapter = _baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
                    break;
                case CompareAttribute cmpAttr:
                //attribute.ErrorMessageResourceName = "InvalidCompare";
                //attribute.ErrorMessageResourceType = typeof(System.Resources.ValidationMessages);
                //adapter = new Compa CompareAttributeAdapter(cmpAttr, stringLocalizer);
                //break;
                default:
                    adapter = _baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
                    break;
            }

            return adapter;
        }
    }

    public class IPAddressOrHostnameAttributeAdapter : AttributeAdapterBase<RequiredAttribute>
    {
        public IPAddressOrHostnameAttributeAdapter(RequiredAttribute attribute, IStringLocalizer stringLocalizer)
            : base(attribute, stringLocalizer)
        { }

        public override void AddValidation(ClientModelValidationContext context) { }

        public override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
        }
    }

    public class IPAddressOrHostnameAttributeAdapterProvider : IValidationAttributeAdapterProvider
    {
        private readonly IValidationAttributeAdapterProvider fallback = new ValidationAttributeAdapterProvider();

        public IAttributeAdapter GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer stringLocalizer)
        {
            var attr = attribute as RequiredAttribute;
            return attr == null ?
                this.fallback.GetAttributeAdapter(attribute, stringLocalizer) :
                new IPAddressOrHostnameAttributeAdapter(attr, stringLocalizer);
        }
    }

    #endregion
}