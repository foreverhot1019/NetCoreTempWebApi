using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace NetCoreTemp.WebApi
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
            if (entry.ModelAttributes?.TypeAttributes?.Any(x => x.GetType().IsAssignableFrom(typeof(MetadataTypeAttribute))) == true)
            {
                if (entry.Key.ModelType?.BaseType == typeof(Models.BaseModel.BaseEntity))
                {
                    var modelType = entry.Key.ModelType;
                    var metadataType = modelType.GetCustomAttributes<MetadataTypeAttribute>(true).FirstOrDefault();
                    //netCore.MVC
                    //var metadataType = modelType.GetCustomAttributes<ModelMetadataTypeAttribute>(true).FirstOrDefault();
                    if (metadataType?.MetadataClassType != null)
                    {
                        var ArrProp = metadataType?.MetadataClassType.GetProperties();
                        List<object> ArrValidatorMetadata = new List<object>();
                        if (ArrProp?.Any() == true)
                        {
                            foreach (var item in ArrProp)
                            {
                                var QArrProp = item.GetCustomAttributes().OfType<ValidationAttribute>();
                                ArrValidatorMetadata.AddRange(QArrProp);
                            }
                        }
                        //entry.BindingMetadata = new BindingMetadata { BinderType = metadataType?.MetadataClassType };

                        entry.DisplayMetadata = new DisplayMetadata { DisplayName = () => "DisplayName" };
                        //if (ArrValidatorMetadata.Any())
                        //{
                        //    if (entry.ValidationMetadata == null)
                        //        entry.ValidationMetadata = new ValidationMetadata { };

                        //    foreach (var item in ArrValidatorMetadata)
                        //        entry.ValidationMetadata.ValidatorMetadata.Add(item);
                        //}
                    }
                }
            }
            var modelMeta = base.CreateModelMetadata(entry);
            return modelMeta;
        }

        //public override ModelMetadata GetMetadataForProperty(PropertyInfo propertyInfo, Type modelType)
        //{
        //    var identity = ModelMetadataIdentity.ForType(modelType);
        //    DefaultMetadataDetails details = CreateTypeDetails(identity);
        //    var meta = base.GetMetadataForProperty(propertyInfo, modelType);
        //    return meta;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="modelType"></param>
        ///// <returns></returns>
        //public override ModelMetadata GetMetadataForType(Type modelType)
        //{
        //    //  Optimization for intensively used System.Object
        //    if (modelType == typeof(object))
        //    {
        //        return base.GetMetadataForType(modelType);
        //    }

        //    var identity = ModelMetadataIdentity.ForType(modelType);
        //    DefaultMetadataDetails details = CreateTypeDetails(identity);

        //    //  This part contains the same logic as DefaultModelMetadata.DisplayMetadata property
        //    //  See https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNetCore.Mvc.Core/ModelBinding/Metadata/DefaultModelMetadata.cs

        //    var context = new DisplayMetadataProviderContext(identity, details.ModelAttributes);
        //    //  Here your implementation of IDisplayMetadataProvider will be called
        //    DetailsProvider.CreateDisplayMetadata(context);
        //    details.DisplayMetadata = context.DisplayMetadata;

        //    return CreateModelMetadata(details);
        //}

    }

    /// <summary>
    /// 自定义 验证器
    /// </summary>
    public class MyModelValidator : IModelValidator
    {
        private static readonly object _emptyValidationContextInstance = new object();
        public ValidationAttribute Attribute { get; }
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IValidationAttributeAdapterProvider _validationAttributeAdapterProvider = new ValidationAttributeAdapterProvider();
        IMemoryCache _memoryCache;

        public MyModelValidator(IMemoryCache memoryCache, IStringLocalizer stringLocalizer)
        {
            _memoryCache = memoryCache;
            _stringLocalizer = stringLocalizer;
        }

        public MyModelValidator(IMemoryCache memoryCache, ValidationAttribute attribute, IStringLocalizer stringLocalizer)
            : this(memoryCache, stringLocalizer)
        {
            Attribute = attribute;
        }

        public MyModelValidator(ValidationAttribute attribute, IStringLocalizer stringLocalizer, IValidationAttributeAdapterProvider validationAttributeAdapterProvider = null)
        {
            if (validationAttributeAdapterProvider != null)
                _validationAttributeAdapterProvider = validationAttributeAdapterProvider;
            Attribute = attribute;
            _stringLocalizer = stringLocalizer;
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
            //var  lang = context.ActionContext.HttpContext.Request.Headers["Content-Language"]?.ToString() ?? "zh-cn";
            //_stringLocalizer.WithCulture
            switch (metadataKind)
            {
                case ModelMetadataKind.Type:
                    var ValidationAttrs = container.GetType().GetCustomAttributes().OfType<ValidationAttribute>();
                    foreach (var attr in ValidationAttrs)
                    {
                        var _memberName = container.GetType().Name;
                        var _validcontext = new ValidationContext(
                        instance: container ?? context.Model ?? _emptyValidationContextInstance,
                        serviceProvider: context.ActionContext?.HttpContext?.RequestServices,
                        items: null)
                        {
                            DisplayName = metadata.GetDisplayName(),
                            MemberName = _memberName
                        };
                        var _result = Attribute.GetValidationResult(context.Model, _validcontext);
                        addModelValidationResult(context, _memberName, _result, modelValidationResults);
                    }
                    break;
                case ModelMetadataKind.Property:
                    var _attribute = Attribute;
                    IEnumerable<PropertyInfo> ArrMetadataProp = null;
                    var modelType = context.Container.GetType();
                    var cacheName = modelType.FullName + "Metadata";
                    var ObjMetadataProp = _memoryCache.Get(cacheName);

                    if (ObjMetadataProp != null)
                    {
                        ArrMetadataProp = (IEnumerable<PropertyInfo>)ObjMetadataProp;
                    }
                    else
                    {
                        Type _metadataType;
                        var _metadataTypeAttr = modelType.GetCustomAttributes<MetadataTypeAttribute>(true).FirstOrDefault();
                        _metadataType = _metadataTypeAttr?.MetadataClassType;
                        var _modelmetadataTypeAttr = modelType.GetCustomAttributes<ModelMetadataTypeAttribute>(true).FirstOrDefault();
                        if (_modelmetadataTypeAttr != null)
                            _metadataType = _modelmetadataTypeAttr?.MetadataType;

                        if (_metadataType != null)
                        {
                            ArrMetadataProp = _metadataType.GetProperties();
                            _memoryCache.Set(cacheName, ArrMetadataProp);
                        }
                    }
                    var metadataDisplay = ArrMetadataProp?.Where(x => x.Name == memberName)?.FirstOrDefault().GetCustomAttribute<DisplayAttribute>();
                    var validcontext = new ValidationContext(
                        instance: container ?? context.Model ?? _emptyValidationContextInstance,
                        serviceProvider: context.ActionContext?.HttpContext?.RequestServices,
                        items: null)
                    {
                        DisplayName = metadataDisplay?.Name ?? metadata.GetDisplayName(),
                        MemberName = memberName
                    };
                    var result = _attribute.GetValidationResult(context.Model, validcontext);
                    addModelValidationResult(context, memberName, result, modelValidationResults);
                    break;
                case ModelMetadataKind.Parameter:
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
            var c = System.Globalization.CultureInfo.CurrentCulture;
            var c2 = System.Globalization.CultureInfo.DefaultThreadCurrentCulture;
            var c3 = System.Globalization.CultureInfo.DefaultThreadCurrentUICulture;
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
        static Type _type = typeof(ValidationAttribute);
        static FieldInfo _defaultErrorMessageFieldInfo = _type.GetField("_defaultErrorMessage", BindingFlags.Instance | BindingFlags.NonPublic);
        static FieldInfo _errMsgFieldInfo = _type.GetField("_errorMessage", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly IStringLocalizer _stringLocalizer;
        private readonly IStringLocalizer<CommonLanguage.Language> _sharedLocalizer;
        IMemoryCache _memoryCache;

        public MyModelValidatorProvider(IMemoryCache memoryCache, IStringLocalizer stringLocalizer, IStringLocalizer<CommonLanguage.Language> sharedLocalizer)
        {
            _memoryCache = memoryCache;
            _stringLocalizer = stringLocalizer;
            _sharedLocalizer = sharedLocalizer;
        }

        public void CreateValidators(ModelValidatorProviderContext context)
        {
            if (context.ModelMetadata?.HasValidators != true)
                return;
            //var MyVldAttrAdp = new MyValidationAttributeAdapterProvider();
            var propName = context.ModelMetadata.PropertyName;
            var modelType = context.ModelMetadata.ContainerType;

            #region 获取MetaDataType类的属性&缓存属性

            IEnumerable<PropertyInfo> ArrMetadataProp = null;
            var cacheName = modelType.FullName + "Metadata";
            var ObjMetadataProp = _memoryCache.Get(cacheName);

            if (ObjMetadataProp != null)
            {
                ArrMetadataProp = (IEnumerable<PropertyInfo>)ObjMetadataProp;
            }
            else
            {
                Type _metadataType;
                var _metadataTypeAttr = modelType.GetCustomAttributes<MetadataTypeAttribute>(true).FirstOrDefault();
                _metadataType = _metadataTypeAttr?.MetadataClassType;
                var _modelmetadataTypeAttr = modelType.GetCustomAttributes<ModelMetadataTypeAttribute>(true).FirstOrDefault();
                if (_modelmetadataTypeAttr != null)
                    _metadataType = _modelmetadataTypeAttr?.MetadataType;

                if (_metadataType != null)
                {
                    ArrMetadataProp = _metadataType.GetProperties();
                    _memoryCache.Set(cacheName, ArrMetadataProp);
                }
            }

            #endregion

            #region MetaDataType类里的验证属性

            List<ValidationAttribute> ArrValidMetadata = new List<ValidationAttribute>();

            var QArrMetadataProp = ArrMetadataProp?.Where(x => x.Name == propName);
            if (QArrMetadataProp?.Any() == true)
            {
                var ArrCusAttr = QArrMetadataProp.FirstOrDefault().GetCustomAttributes();
                var _ValidationAttrs = ArrCusAttr.OfType<ValidationAttribute>();
                ArrValidMetadata.AddRange(_ValidationAttrs);
            }

            #endregion

            switch (context.ModelMetadata.MetadataKind)
            {
                case ModelMetadataKind.Type:
                    #region 注入自定义IModelValidator

                    var _myModelValidator = new MyModelValidator(_memoryCache, _stringLocalizer);
                    context.Results.Add(new ValidatorItem(null)
                    {
                        IsReusable = true,
                        Validator = _myModelValidator
                    });

                    #endregion
                    break;
                case ModelMetadataKind.Property:
                    var resultsCount = context.Results.Count;
                    for (var i = 0; i < resultsCount; i++)
                    {
                        var validitem = context.Results[i];
                        var QArrValidMetadata = ArrValidMetadata?.Where(x => x.GetType() == validitem.ValidatorMetadata.GetType());
                        if (QArrValidMetadata?.Any() == true)
                        {
                            var validationAttr = QArrValidMetadata.FirstOrDefault();
                            SetErrMsgByResource(validationAttr);
                            var _MyModelValidator = new MyModelValidator(_memoryCache, validationAttr, _stringLocalizer);
                            validitem.Validator = _MyModelValidator;
                            context.Results.Remove(validitem);
                            context.Results.Insert(0, validitem);
                        }
                        else
                        {
                            if (validitem.ValidatorMetadata is ValidationAttribute validationAttr)
                                SetErrMsgByResource(validationAttr);
                        }
                    }
                    var QAddMetadataValidator = ArrValidMetadata?.Where(x => !context.Results.Any(n => n.ValidatorMetadata?.GetType() == x.GetType()));
                    if (QAddMetadataValidator?.Any() == true)
                    {
                        foreach (var vldMeta in QAddMetadataValidator)
                        {
                            SetErrMsgByResource(vldMeta);
                            var _MyModelValidator = new MyModelValidator(_memoryCache, vldMeta, _stringLocalizer);
                            context.Results.Add(new ValidatorItem(vldMeta)
                            {
                                IsReusable = true,
                                Validator = _MyModelValidator
                            });
                        }
                    }
                    break;
                case ModelMetadataKind.Parameter:
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
            var cacheName = modelType.FullName + "Metadata";
            var ObjMetadataProp = _memoryCache.Get(cacheName);
            IEnumerable<PropertyInfo> ArrMetadataProp = null;
            if (ObjMetadataProp != null)
            {
                ArrMetadataProp = (IEnumerable<PropertyInfo>)ObjMetadataProp;
            }
            else
            {
                var metadataType = modelType.GetCustomAttributes<MetadataTypeAttribute>(true).FirstOrDefault();
                if (metadataType?.MetadataClassType != null)
                {
                    _memoryCache.Set(cacheName, metadataType.MetadataClassType.GetProperties());
                }
            }
            var Arr = ArrMetadataProp?.Where(x => x.GetCustomAttributes<ValidationAttribute>(true)?.Any() == true);
            has = Arr?.Any() == true;
            return has;
        }

        /// <summary>
        /// 读取Metadata,设置验证错误信息
        /// 存在ErrorMessage或存在 Resource设置，则不在自动设置(手动实现多语言验证错误信息)
        /// </summary>
        /// <param name="validMetadata"></param>
        private void SetErrMsgByResource(ValidationAttribute validMetadata)
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
                #region MyRegion

                //case RequiredAttribute validator:
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        validator.ErrorMessageResourceName = "RequiredAttribute_ValidationError";
                //    }
                //    break;
                //case EnumDataTypeAttribute validator:
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        validator.ErrorMessageResourceName = "EnumDataTypeAttribute_TypeNeedsToBeAnEnum";
                //    }
                //    break;

                //case MinLengthAttribute validator:
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        //StringLengthAttribute_ValidationError
                //        validator.ErrorMessageResourceName = "MinLengthAttribute_ValidationError";
                //    }
                //    break;
                //case MaxLengthAttribute validator:
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        //StringLengthAttribute_ValidationError
                //        validator.ErrorMessageResourceName = "MaxLengthAttribute_ValidationError";
                //    }
                //    break;
                //case RangeAttribute validator:
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        validator.ErrorMessageResourceName = "RangeAttribute_ValidationError";
                //        //if (metadata.ModelType == typeof(int) || metadata.ModelType == typeof(long))
                //        //else
                //        //    validator.ErrorMessageResourceName = "ValidationDefault_FloatRange";
                //    }
                //    break;
                //case StringLengthAttribute validator:
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        StringLengthAttribute StrLenAttr = validator as StringLengthAttribute;
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        if (StrLenAttr.MinimumLength > 0)
                //            validator.ErrorMessageResourceName = "StringLengthAttribute_ValidationErrorIncludingMinimum";
                //        else
                //            validator.ErrorMessageResourceName = "StringLengthAttribute_ValidationError";
                //    }
                //    break;
                //case EmailAddressAttribute validator:
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        validator.ErrorMessageResourceName = "EmailAddressAttribute_Invalid";
                //    }
                //    break;
                //case RegularExpressionAttribute validator:
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        validator.ErrorMessageResourceName = "RegexAttribute_ValidationError";
                //    }
                //    break;
                //case System.ComponentModel.DataAnnotations.CompareAttribute validator:
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        //var CompareAttr = validator as CompareAttribute;
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        validator.ErrorMessageResourceName = "CompareAttribute_MustMatch";
                //    }
                //    break;
                //case RemoteAttribute validator:
                //    if (validator.ErrorMessageResourceType != typeof(CommonLanguage.Language))
                //    {
                //        //var RemoteAttr = validator as System.Web.Mvc.RemoteAttribute;
                //        validator.ErrorMessageResourceType = typeof(CommonLanguage.Language);
                //        validator.ErrorMessageResourceName = "RemoteAttribute_NoUrlFound";
                //    }
                //    break;

                #endregion
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
