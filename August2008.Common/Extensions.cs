﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using August2008.Common.Interfaces;

namespace August2008.Common
{
    public static class Extensions
    {
        public static string ToDbXml(this IEnumerable<IPostedFile> postedFiles)
        {
            if (postedFiles != null)
            {
                var sb = new StringBuilder("<Photos>");
                foreach (var file in postedFiles)
                {
                    sb.Append(file.ToDbXml());
                }
                sb.Append("</Photos>");
                return sb.ToString();
            }
            return string.Empty;
        }
        public static string ToDbXml(this IPostedFile file)
        {
            return string.Format("<Photo PhotoUri=\"{0}\" ContentType=\"{1}\" {2}/>", 
                Path.GetFileName(file.FileName), 
                file.ContentType, 
                file.Attributes.ToXmlAttributes("FileName"));
        }   
        public static string ToXmlAttributes(this IDictionary<string, string> dictionary, params string[] ignore)
        {
            if (dictionary != null)
            {
                var sb = new StringBuilder();
                foreach (var item in dictionary.Where(item => !item.Key.Equals(ignore)))
                {
                    sb.AppendFormat("{0}=\"{1}\"", item.Key, item.Value);
                }
                return sb.ToString();
            }
            return string.Empty;
        }
        public static string ToDbXml(this IDictionary<string, string> dictionary, params string[] ignore) 
        {
            if (dictionary != null)
            {
                var sb = new StringBuilder("<Dictionary>");
                foreach (var item in dictionary.Where(item => !item.Key.Equals(ignore)))
                {
                    sb.AppendFormat("<Item Key=\"{0}\" Value=\"{1}\" />", item.Key, item.Value);
                }
                sb.Append("</Dictionary>");
                return sb.ToString();
            }
            return string.Empty;
        }
        public static DateTime? ToFromDate(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                dateTime = dateTime.Value.Date;
            }
            return dateTime;
        }
        public static DateTime? ToToDate(this DateTime? dateTime) 
        {
            if (dateTime.HasValue)
            {
                dateTime = dateTime.Value.Date.AddDays(1).AddMilliseconds(-1);                
            }
            return dateTime;
        }
        public static string SingleOrEmpty(this IEnumerable<string> source)
        {
            return source.SingleOrDefault() ?? string.Empty;
        }
        public static string ToASCIIString(this byte[] value)
        {
            if (value != null && value.Length > 0)
            {
                return Encoding.ASCII.GetString(value);
            }
            return string.Empty;
        }
    }
}
