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

/// <summary>
///
/// </summary>
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

/// <summary>
/// 
/// </summary>
public enum PairStatuses
{
    Working,
    Completed,
    NothingDone,
}

/// <summary>
/// 
/// </summary>
public class PairsTraderParams
{
    public int MaxPairDuration { get; set; }
    public int PairWidth { get; set; }
}

/// <summary>
/// 
/// </summary>
public class PairsTrader : IDisposable
{
    public List<OrderPair> OrderPairs { get; }
    private int pairCounter = 0;
    public PairsTraderParams PairsParams { get; set; }
    private int processingIntervalMilliseconds = 1000;
    private Nbbo currentNbbo = null;
    private DateTime lastProcessingCycle = DateTime.MinValue;
    private readonly IGateway gateway;

    public PairsTrader(NbboPublisher marketData,
    IGateway exchangeGateway,
    PairsTraderParams pairsParams)
    {
        this.gateway = gateway;
        this.gateway.OnOrderUpdated += Gateway_OnOrderUpdated;

        this.marketData = marketData;
        marketData.OnNbboUpdated += OnTickReceived;
        marketData.OnNbboUpdated += this.gateway.OnMarketDataTick;

        PairsParams = pairsParams;
        OrderPairs = new List<OrderPair>();
    }
}

/// <summary>
/// 
/// </summary>
private bool ShouldCreatePair()
{
    if (currentNbbo == null) return false; //Cant create a pair without market data

    if (!OrderPairs.Any()) return true;
    var workingOrderCount = OrderPairs.Count(p => p.PairStatus == OrderPair.PairStatuses.Working);
    if (workingOrderCount == 0) return true;

    return false;
}

/// <summary>
/// 
/// </summary>
private void CreatePair(Nbbo nbbo)
{
    pairCounter++; //Used to assign an ID for each pair

    int pairWidth = PairsParams.PairWidth;

    var pair = new OrderPair(nbbo.Time, pairCounter.ToString())
    {
        OpenPrice = nbbo.Midpoint,
        Width = pairWidth,
        Buy = new Order() { OrderQty = .1m, OrderId = $"{pairCounter}_B", Price = nbbo.Midpoint - pairWidth, Side = "B" },
        Sell = new Order() { OrderQty = .1m, OrderId = $"{pairCounter}_S", Price = nbbo.Midpoint + pairWidth, Side = "S" }
    };

    OrderPairs.Add(pair);

    gateway.SendOrder(pair.Buy);
    gateway.SendOrder(pair.Sell);
}
