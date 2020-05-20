public class Nbbo
{
    public DateTime Time { get; set; }
    public decimal Bid { get; set; }
    public decimal BidSize { get; set; }
    public decimal Ask { get; set; }
    public decimal AskSize { get; set; }

    public decimal Midpoint
    {
        get
        {
            return (Bid + Ask) / 2;
        }
    }

    public override string ToString()
    {
        return $"{Time.Ticks},{Bid},{BidSize},{Ask},{AskSize}";
    }
}

/// <summary>
/// 
/// </summary>
public delegate void dgNbboChange(Nbbo nbbo);
public event dgNbboChange OnNbboUpdated;

/// <summary>
/// 
/// </summary>
public void RecordNbbos()
{
    var nbboListener = new NbboListener_CPB(); //CBP = CoinbasePro.
    nbboListener.OnNbboUpdated += NbboListener_OnNbboUpdated;

    Task t = new Task(nbboListener.Start);
    t.Start();
}

/// <summary>
/// 
/// </summary>
private void NbboListener_OnNbboUpdated(Nbbo nbbo)
{
    Console.WriteLine(nbbo.ToString());
    System.IO.File.AppendAllText($"nbbo_{nbbo.Time:yyyyMMdd}.cbp.csv", nbbo.ToString() + Environment.NewLine);
}

/// <summary>
/// 
/// </summary>
public void OnTickReceived(Nbbo nbbo)
{
    currentNbbo = nbbo;

    if (nbbo.Time - lastProcessingCycle > TimeSpan.FromMilliseconds(processingIntervalMilliseconds))
    {
        lastProcessingCycle = nbbo.Time;

        if (ShouldCreatePair())
            CreatePair(nbbo);
    }
}

/// <summary>
/// 
/// </summary>
public void OnMarketDataTick(Nbbo nbbo)
{
    //Check the processing interval, and don't bother processing if the prices are the same as the last tick (often times, it's only the order sizes that change from tick to tick).
    if (nbbo.Time - priorNbbo.Time < TimeSpan.FromMilliseconds(1000)
        || (nbbo.Ask == priorNbbo.Ask && nbbo.Bid == priorNbbo.Bid))
        return;

    priorNbbo = nbbo;

    List<Order> filledMakerOrders = new List<Order>();
    List<Order> filledTakerOrders = new List<Order>();

    //Does this tick satisfy a maker order?
    foreach (var order in makerOrders)
    {
        if ((order.Value.Side == "B" && order.Value.Price > nbbo.Ask)
            ||
            (order.Value.Side == "S" && order.Value.Price < nbbo.Bid))
        {
            order.Value.AvgPx = order.Value.Price;
            decimal fillQty = GetFillQty(nbbo, order.Value);
            order.Value.CumQty += fillQty;

            if (order.Value.CumQty == order.Value.OrderQty) filledMakerOrders.Add(order.Value);

            OnOrderUpdated?.Invoke(order.Value, nbbo.Time);
        }
    }

    //Fill any taker orders
    foreach (var order in takerOrders)
    {
        order.Value.AvgPx = order.Value.Side == "B" ? nbbo.Ask : nbbo.Bid;
        order.Value.CumQty += GetFillQty(nbbo, order.Value);
        OnOrderUpdated?.Invoke(order.Value, nbbo.Time);

        if (order.Value.CumQty == order.Value.OrderQty) filledTakerOrders.Add(order.Value);
    }

    private decimal GetFillQty(Nbbo nbbo, Order order)
    {
        if (order.Side == "B")
            return nbbo.AskSize > order.LeavesQty ? order.LeavesQty : nbbo.AskSize;
        else
            return nbbo.BidSize > order.LeavesQty ? order.LeavesQty : nbbo.BidSize;
    }

    filledMakerOrders.ForEach(f => makerOrders.Remove(f.OrderId));
    filledTakerOrders.ForEach(f => takerOrders.Remove(f.OrderId));
}
