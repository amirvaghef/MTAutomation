using System;
using System.ComponentModel.DataAnnotations;

namespace MTAutomation
{
    public class PositionsDTO
    {
        [Key]
        public long ID { get; set; }

        public int RowNO { get; set; }

        public int TicketNO { get; set; }

        public string SymbolName { get; set; }

        public double Volume { get; set; }

        public string UserName { get; set; }

        public bool PositionStatus { get; set; }

        public string PositionStateStr { get; set; }

        public bool IsClose { get; set; }

        public bool PositionState { get; set; }

        [Display(Name = "Deal Price")]
        public double? Price { get; set; }

        public double? ClosePrice { get; set; }

        public DateTime ExpireDate { get; set; }

        private double? profit;
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public double? Profit { get { return profit; } set { profit = Math.Round(value.GetValueOrDefault(), 2); } }

        private double? profitPercent;
        [DisplayFormat(DataFormatString = "{0:N}", ApplyFormatInEditMode = true)]
        public double? ProfitPercent { get { return profitPercent; } set { profitPercent = Math.Round(value.GetValueOrDefault(), 2); } }

        public double? MarketPrice { get; set; }

        public short Leverage { get; set; }
    }
}
