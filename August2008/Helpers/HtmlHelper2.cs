﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;

namespace August2008.Helpers
{
    public static class HtmlHelper2 
    {
        public static IHtmlString LabelFor2<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return LabelHelper(html, 
                ModelMetadata.FromLambdaExpression(expression, html.ViewData), 
                ExpressionHelper.GetExpressionText(expression), "");
        }
        private static IHtmlString LabelHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string labelText)
        {
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            }
            if (string.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }
            var isRequired = false;
            if (metadata.ContainerType != null && metadata.PropertyName != null)
            {
                isRequired = metadata.ContainerType.GetProperty(metadata.PropertyName) 
                    .GetCustomAttributes(typeof (RequiredAttribute), false)
                    .Length == 1;
            }
            var tag = new TagBuilder("label");
            tag.Attributes.Add("for", TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
            if (isRequired)
            {
                tag.Attributes.Add("class", "label-required");
            }
            tag.SetInnerText(labelText);
            var output = tag.ToString(TagRenderMode.Normal);
            if (isRequired)
            {
                var asterisk = new TagBuilder("span");
                asterisk.Attributes.Add("class", "required");
                asterisk.SetInnerText("*");
                output += asterisk.ToString(TagRenderMode.Normal);
            }
            return MvcHtmlString.Create(output);
        }
        public static IHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty[]>> expression, MultiSelectList multiSelectList, object htmlAttributes = null)
        {
            //Derive property name for checkbox name
            MemberExpression body = expression.Body as MemberExpression;
            string propertyName = body.Member.Name;

            //Get currently select values from the ViewData model
            TProperty[] props = expression.Compile().Invoke(html.ViewData.Model);

            //Convert selected value list to a List<string> for easy manipulation
            List<string> selectedValues = new List<string>();

            if (props != null)
            {
                selectedValues = new List<TProperty>(props).ConvertAll<string>(delegate(TProperty i) { return i.ToString(); });
            }

            //Create div
            TagBuilder divTag = new TagBuilder("div");
            divTag.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            //Add checkboxes
            foreach (SelectListItem item in multiSelectList)
            {
                divTag.InnerHtml += String.Format(
                    "<div><input type=\"checkbox\" name=\"{0}\" id=\"{0}_{1}\" value=\"{1}\" {2} /><label for=\"{0}_{1}\">{3}</label></div>",
                                                    propertyName,
                                                    item.Value,
                                                    selectedValues.Contains(item.Value) ? "checked=\"checked\"" : "",
                                                    item.Text);
            }
            return MvcHtmlString.Create(divTag.ToString());
        }
        public static IHtmlString DisplayTextFor2<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)   
        {
            var metadata = ModelMetadata.FromLambdaExpression<TModel, TProperty>(expression, htmlHelper.ViewData);
            string value = metadata.DisplayName ?? (metadata.PropertyName ?? ExpressionHelper.GetExpressionText(expression));
            return MvcHtmlString.Create(value);
        }
        public static IHtmlString ActionMenuItem(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName)
        {
            if (!htmlHelper.ViewContext.RequestContext.IsCurrentRoute(null, controllerName, actionName))
            {
                return htmlHelper.ActionLink(linkText, actionName, controllerName);
            }
            else
            {
                return htmlHelper.ActionLink(linkText, actionName, controllerName, new { @class = "selected" });
            }
        }
        public static IHtmlString Img(this HtmlHelper helper, string fileName, object htmlAttributes = null)
        {
            var urlHelper = ((Controller)helper.ViewContext.Controller).Url;
            var src = urlHelper.ImageUrl(fileName);
            var img = new TagBuilder("img");
            img.Attributes["src"] = src;
            if (htmlAttributes != null)
            {
                img.MergeAttributes<string, object>(new RouteValueDictionary(htmlAttributes));
            }
            return MvcHtmlString.Create(img.ToString(TagRenderMode.SelfClosing));
        }
        private static RouteValueDictionary Merge(this RouteValueDictionary original, RouteValueDictionary other)
        {
            if (other != null && original != null)
            {
                foreach (var item in other)
                {
                    if (original.ContainsKey(item.Key))
                    {
                        original[item.Key] = item.Value;
                    }
                    else
                    {
                        original.Add(item.Key, item.Value);
                    }
                }
            }
            return original;
        }
    }
}