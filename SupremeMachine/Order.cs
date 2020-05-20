public class Order
{
    public Order()
    {
        OrderCreateTime = DateTime.Now;
    }

    public string OrderId { get; set; }
    public string PublicHandlerId { get; set; }
    public DateTime OrderCreateTime { get; set; }
    public string Side { get; set; }
    public decimal Price { get; set; }
    public decimal OrderQty { get; set; }
    public decimal CumQty { get; set; }

    public decimal Notional
    {
        get
        {
            return CumQty * AvgPx;
        }
    }

    public decimal LeavesQty
    {
        get
        {
            return OrderQty - CumQty;
        }
    }
    public decimal AvgPx { get; set; }
}


public class OrderPair
{
    public OrderPair(DateTime createTime, string pairId)
    {
        PairCreateTime = createTime;
        PairStatus = PairStatuses.Working;
        PairId = pairId;
    }

    public string PairId { get; set; }
    public int Width { get; set; }
    public PairStatuses PairStatus { get; set; }
    public DateTime PairCreateTime { get; set; }

    public Orders.Order Buy { get; set; }
    public Orders.Order Sell { get; set; }
}


public enum PairStatuses
{
    Working,
    Completed,
    NothingDone,
}