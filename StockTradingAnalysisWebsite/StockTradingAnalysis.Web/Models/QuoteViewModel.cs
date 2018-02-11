using System;
using System.ComponentModel.DataAnnotations;

namespace StockTradingAnalysis.Web.Models
{
    /// <summary>
    /// The QuoteViewModel defines one quote for a given time.
    /// </summary>
    public class QuoteViewModel
    {
        /// <summary>
        /// Gets/sets the Date
        /// </summary>
        [DataType(DataType.DateTime)]
        [DisplayFormat(NullDisplayText = "", DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        [Display(Name = "Display_QuoteDate", ResourceType = typeof(Resources))]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets/sets the Open
        /// </summary>
        [Display(Name = "Display_QuoteOpen", ResourceType = typeof(Resources))]
        public decimal Open { get; set; }

        /// <summary>
        /// Gets/sets the Close
        /// </summary>
        [Display(Name = "Display_QuoteClose", ResourceType = typeof(Resources))]
        public decimal Close { get; set; }

        /// <summary>
        /// Gets/sets the High
        /// </summary>
        [Display(Name = "Display_QuoteHigh", ResourceType = typeof(Resources))]
        public decimal High { get; set; }

        /// <summary>
        /// Gets/sets the Low
        /// </summary>
        [Display(Name = "Display_QuoteLow", ResourceType = typeof(Resources))]
        public decimal Low { get; set; }
    }
}