using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Wimbley.AppleWatch.HeartRate.Site.Core.Exceptions;

namespace Wimbley.AppleWatch.HeartRate.Site.Core
{
    public class HeartRateXml
    {
        private string[] AllowedExtensions { get; set; }
        private readonly ILogger _log = ApplicationLogging.CreateLogger<HeartRateXml>();

        /// <summary>
        /// Instantiates 
        /// </summary>
        public HeartRateXml()
        {
            AllowedExtensions = new string[] { ".xml", ".XML" };
        }

        /// <summary>
        /// Parse an imported XML structure of apple watch heart rate data
        /// </summary>
        /// <param name="xmlstring">Apple Watch heart rate xml</param>
        /// <returns><list type="HeartRateData"></list></returns>
        public List<HeartRateData> Parse(string xmlstring)
        {
            _log.LogDebug("Parsing hr xml data");
            
            var doc = XDocument.Parse(xmlstring);
            var html = this.ParseDocument(doc);

            return html;
        }

        /// <summary>
        /// Load and parse XML file of apple watch heart rate data
        /// </summary>
        /// <param name="filename">File of XML data to load/parse</param>
        /// <returns><list type="HeartRateData"></list></returns>
        public List<HeartRateData> Load(string filename)
        {
            var isExtGood = this.VerifyExtension(filename);

            if (!isExtGood)
            {
                throw new FileExtensionNotSupportedException(string.Format(@"File format {0} is not supported. Please upload XML data only at this time", Path.GetExtension(filename)));
            }

            var doc = XDocument.Load(filename);
            var html = this.ParseDocument(doc);

            return html;
        }

        /// <summary>
        /// Parse XML document and load minimum required elements from heart rate data to List
        /// </summary>
        /// <param name="doc">Heart rate XML Document</param>
        /// <returns><list type="HeartRateData"></list></returns>
        private List<HeartRateData> ParseDocument(XDocument doc)
        {
            var hrdata = new List<HeartRateData>();

            XNamespace ns = "urn:hl7-org:v3";
            var components = doc.Descendants(ns + "component");
            
            foreach (var component in components)
            {
                var observations = component.Descendants(ns + "observation");
                
                foreach (var obs in observations)
                {
                    var hrelement = new HeartRateData();

                    var sourcelement = obs.Element(ns + "text").Element(ns + "sourceName");
                    var sourcename = sourcelement == null ? "Apple Watch" : sourcelement.Value;

                    var datatypeelement = obs.Element(ns + "text").Element(ns + "type");
                    var datatype = datatypeelement == null ? "NotHeartRateData" : datatypeelement.Value;

                    var hrvalueelement = obs.Element(ns + "text").Element(ns + "value");
                    var hrvalue = hrvalueelement == null ? "-1" : hrvalueelement.Value;

                    var unitelement = obs.Element(ns + "text").Element(ns + "unit");
                    var unitvalue = unitelement == null ? "count/min" : unitelement.Value;

                    var timeelement = obs.Element(ns + "effectiveTime").Element(ns + "high").Attribute("value");
                    var timevalue = timeelement == null ? DateTime.MinValue : this.ConvertHrTime(timeelement.Value);
                    
                    hrelement.SourceName = sourcename;
                    hrelement.Value = string.Format($"{hrvalue} {unitvalue}");
                    hrelement.RecordedDate = timevalue;
                    hrelement.DataType = datatype;

                    hrdata.Add(hrelement);
                }
            }
            
            return hrdata.Where(m => m.DataType == "HKQuantityTypeIdentifierHeartRate").OrderByDescending(m => m.RecordedDate).ToList();
        }
        
        /// <summary>
        /// Allow specified file extensions for parsing heart rate data
        /// </summary>
        /// <param name="filename">File to verify extensions</param>
        /// <returns>Bool</returns>
        public bool VerifyExtension(string filename)
        {
            return this.AllowedExtensions.Contains(Path.GetExtension(filename));
        }
        
        /// <summary>
        /// Apple has a strange date format, convert it to date object
        /// </summary>
        /// <param name="time">Apple custom date format</param>
        /// <returns>DateTime</returns>
        private DateTime ConvertHrTime(string time)
        {
            var year = time.Length > 4 ? time.Substring(0, 4) : "";
            var month = time.Length > 6 ? time.Substring(4, 2) : "";
            var day = time.Length > 8 ? time.Substring(6, 2) : "";
            var hour = time.Length > 10 ? time.Substring(8, 2) : "";
            var minute = time.Length > 12 ? time.Substring(10, 2) : "";
            var seconds = time.Length > 14 ? time.Substring(12, 2) : "";

            DateTime parsedDate;
            DateTime.TryParse($@"{year}/{month}/{day} {hour}:{minute}:{seconds}", out parsedDate);
            
            return parsedDate;
        }
    }
}
